////////////////////////////////////////////////////////////////////////////////////////////////////
// NoesisGUI - http://www.noesisengine.com
// Copyright (c) 2013 Noesis Technologies S.L. All Rights Reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////

#include <sys/types.h>
#include <sys/stat.h>
#include <sys/ioctl.h>
#include <sys/mman.h>
#include <fcntl.h>
#include <unistd.h>
#include <stdio.h>
#include <pthread.h>
#include <cstdint>
#include <poll.h>

#include <sys/socket.h>
#include <sys/un.h>

#include <gst/gst.h>
#include <gst/app/gstappsrc.h>
#include <gst/allocators/gstdmabuf.h>

#include "MediaPlayerCommand.h"

sockaddr_un serverSockaddr;
int videoSocket;
int64_t streamSize;
GstElement* pipeline;

struct frame
{
    gint64 time;
    GstSample* sample;
    GstBuffer* buffer;
    GstMapInfo map;
    int fd;
};

const uint MaxFrames = 64; // Way more than wee need so we don't have to bother checking for wraparound
frame storedFrames[MaxFrames];
uint firstFrame = 0;
uint lastFrame = 0;

static GstFlowReturn Sample(GstElement* sink, void* data, const char* signal)
{
    uint idx = lastFrame;
    lastFrame = (lastFrame + 1) % MaxFrames;

    g_signal_emit_by_name (sink, signal, &storedFrames[idx].sample);
    if (storedFrames[idx].sample)
    {
        GstCaps* caps = gst_sample_get_caps (storedFrames[idx].sample);
        GstStructure* s = gst_caps_get_structure (caps, 0);
        gint width;
        gint height;
        gst_structure_get_int (s, "width", &width);
        gst_structure_get_int (s, "height", &height);

        storedFrames[idx].buffer = gst_sample_get_buffer(storedFrames[idx].sample);
        GstSegment* segment = gst_sample_get_segment(storedFrames[idx].sample);
        storedFrames[idx].time = storedFrames[idx].buffer->pts;
        if (gst_buffer_map(storedFrames[idx].buffer, &storedFrames[idx].map, GST_MAP_READ))
        {
            GstMemory* mem = gst_buffer_peek_memory(storedFrames[idx].buffer, 0);

            int gst_fd = gst_dmabuf_memory_get_fd(mem);
            storedFrames[idx].fd = dup(gst_fd);
            msghdr msg;
            iovec iov;
            cmsghdr *cmsg;
            char cmsg_buffer[sizeof(cmsghdr) + sizeof(int)];
            MediaPlayerCommand command;
            command.cmd = MPC_NewFrame;
            command.arg[0] = storedFrames[idx].time;
            command.arg[1] = (((uint64_t)width) << 32) | height;
            memset(&msg, 0, sizeof(msghdr));
            memset(&iov, 0, sizeof(iovec));
            iov.iov_base = &command;
            iov.iov_len = sizeof(MediaPlayerCommand);
            msg.msg_name = &serverSockaddr;
            msg.msg_namelen = sizeof(sockaddr_un);
            msg.msg_iov = &iov;
            msg.msg_iovlen = 1;
            msg.msg_control = cmsg_buffer;
            msg.msg_controllen = sizeof(cmsg_buffer);
            cmsg = CMSG_FIRSTHDR(&msg);
            cmsg->cmsg_len = sizeof(cmsg_buffer);
            cmsg->cmsg_level = SOL_SOCKET;
            cmsg->cmsg_type = SCM_RIGHTS;
            *((int *)CMSG_DATA(cmsg)) = storedFrames[idx].fd;
            int clientSocket = *(int*)data;
            sendmsg(clientSocket, &msg, 0);
        }

        return GST_FLOW_OK;
    }

    return GST_FLOW_ERROR;
}

static GstFlowReturn NewSample(GstElement* sink, void* data)
{
    return Sample(sink, data, "pull-sample");
}

static GstFlowReturn NewPreroll(GstElement* sink, void* data)
{
    return Sample(sink, data, "pull-preroll");
}

static const gint64 Status_EOS = 1;
static const gint64 Status_ERROR = 2;
static const gint64 Status_READY = 3;

gboolean BusCall(GstBus* bus, GstMessage* msg, gpointer data)
{
    gint64* status = (gint64*) data;
    switch (GST_MESSAGE_TYPE (msg)) {
        case GST_MESSAGE_ASYNC_DONE:
        {
            *status = Status_READY;
            break;
        }
        case GST_MESSAGE_EOS:
        {
            *status = Status_EOS;
            break;
        }
        case GST_MESSAGE_WARNING:
        {
            GError *err;
            gchar *dbg = NULL;

            gst_message_parse_warning (msg, &err, &dbg);
            printf ("WARNING %s\n", err->message);
            if (dbg != NULL)
                printf ("WARNING debug information: %s\n", dbg);
            g_clear_error (&err);
            g_free (dbg);
            break;
        }
        case GST_MESSAGE_ERROR:
        {
            *status = Status_ERROR;
            GError *err;
            gchar *dbg;
            gst_message_parse_error (msg, &err, &dbg);
            printf ("ERROR %s\n", err->message);
            if (dbg != NULL)
                printf ("ERROR debug information: %s\n", dbg);
            g_clear_error (&err);
            g_free (dbg);

            /* flush any other error messages from the bus and clean up */
            gst_element_set_state (pipeline, GST_STATE_NULL);
            break;
        }
        case GST_MESSAGE_CLOCK_LOST:
        {
            gst_element_set_state (pipeline, GST_STATE_PAUSED);
            gst_element_set_state (pipeline, GST_STATE_PLAYING);
            break;
        }
        case GST_MESSAGE_BUFFERING:{
            gint percent;
            gst_message_parse_buffering (msg, &percent);
            break;
        }
        case GST_MESSAGE_LATENCY:
        {
            gst_bin_recalculate_latency (GST_BIN (pipeline));
            break;
        }
        case GST_MESSAGE_REQUEST_STATE:
        {
            GstState state;
            gst_message_parse_request_state (msg, &state);
            gst_element_set_state (pipeline, state);
            break;
        }
        default:
            break;
    }

    return TRUE;
}

static void NeedData(GstElement* element, guint size, void* data)
{
    MediaPlayerCommand command;
    command.cmd = MPC_Play;
    command.arg[0] = size;
    send(videoSocket, &command, sizeof(command), 0);

    gint64* status = (gint64*)data;
    GstBuffer* buffer = gst_buffer_new();
    GstMemory* memory = gst_allocator_alloc(NULL, size, NULL);
    gst_buffer_insert_memory (buffer, -1, memory);

    GstMapInfo map;
    gst_memory_map(memory, &map, GST_MAP_WRITE);
    ssize_t read_size = recv(videoSocket, map.data, size, 0);
    while (read_size < size)
    {
        ssize_t s = recv(videoSocket, map.data + read_size, size - read_size, 0);
        read_size += s;
        if (s <= 0)
            break;
    }
    gst_memory_unmap(memory, &map);

    GstFlowReturn ret;
    g_signal_emit_by_name(element, "push-buffer", buffer, &ret);
  
    if (ret != GST_FLOW_OK)
    {
        *status = Status_ERROR;
    }
  
    if (read_size != size)
    {
        // We don't set the status to EOS here.
        // This data still needs to go through the pipeline
        gst_app_src_end_of_stream(GST_APP_SRC(element));
    }

    gst_buffer_unref(buffer);
}

static gboolean SeekData(GstElement* element, guint64 offset, void* data)
{
    MediaPlayerCommand command;
    command.cmd = MPC_Seek;
    command.arg[0] = offset;
    send(videoSocket, &command, sizeof(command), 0);
    return TRUE;
}

GstElement *source;

static void SourceSetup(GstElement *pipeline, GstElement *src, gpointer data)
{
    source = src;
    g_object_set (source, "size", streamSize, NULL);
    g_object_set (source, "stream-type", GST_APP_STREAM_TYPE_RANDOM_ACCESS, NULL);
    g_object_set (source, "emit-signals", TRUE, NULL);
    g_signal_connect (source, "need-data", G_CALLBACK (NeedData), data);
    g_signal_connect (source, "seek-data", G_CALLBACK (SeekData), data);
}

static void VideoChanged(GstElement * playbin, gpointer udata)
{
}

void SignalHandler(int)
{
    exit(0);
}

int main(int argc, char** argv)
{
    if (argc != 3)
    {
        exit(-1);
    }

    signal(SIGTERM, &SignalHandler);

    const char* tmpDir = argv[1];
    const char* streamSizeStr = argv[2];
    streamSize = atoll(streamSizeStr);
    
    int clientSocket = socket(AF_UNIX, SOCK_DGRAM, 0);
    
    char clientSocketPath[256];
    strcpy(clientSocketPath, tmpDir);
    strcat(clientSocketPath, "/client_socket");

    sockaddr_un clientSockaddr;
    memset(&clientSockaddr, 0, sizeof(sockaddr_un));
    clientSockaddr.sun_family = AF_UNIX;        
    strcpy(clientSockaddr.sun_path, clientSocketPath);
    unlink(clientSocketPath);
    bind(clientSocket, (sockaddr*)&clientSockaddr, sizeof(sockaddr_un));
    
    char serverSocketPath[256];
    strcpy(serverSocketPath, tmpDir);
    strcat(serverSocketPath, "/server_socket");

    memset(&serverSockaddr, 0, sizeof(sockaddr_un));
    serverSockaddr.sun_family = AF_UNIX;        
    strcpy(serverSockaddr.sun_path, serverSocketPath);
    
    char videoServerSocketPath[256];
    strcpy(videoServerSocketPath, tmpDir);
    strcat(videoServerSocketPath, "/video_socket");
    
    sockaddr_un videoServerSockaddr;
    memset(&videoServerSockaddr, 0, sizeof(sockaddr_un));
    videoServerSockaddr.sun_family = AF_UNIX;        
    strcpy(videoServerSockaddr.sun_path, videoServerSocketPath);
    
    videoSocket = socket(AF_UNIX, SOCK_STREAM, 0);
    connect(videoSocket, (sockaddr*)&videoServerSockaddr, sizeof(sockaddr_un));

    gst_init(NULL, NULL);

    const gchar* descr = "playbin uri=appsrc:// video-sink=\"appsink name=sink\"";
    GError *error = NULL;
    pipeline = gst_parse_launch (descr, &error);
    
    gint64 status = 0;
    g_signal_connect (pipeline, "source-setup", G_CALLBACK (SourceSetup), &status);
    g_signal_connect (pipeline, "video-changed", G_CALLBACK (VideoChanged), &status);

    GstBus* bus = gst_pipeline_get_bus(GST_PIPELINE (pipeline));
    int bus_watch_id = gst_bus_add_watch (bus, BusCall, &status);
    gst_object_unref (bus);

    gst_element_set_state (pipeline, GST_STATE_PAUSED);

    // Wait for the pipeline to preroll
    gst_element_get_state (pipeline, NULL, NULL, GST_CLOCK_TIME_NONE);

    GstElement* sink = gst_bin_get_by_name (GST_BIN (pipeline), "sink");
    g_object_set (sink, "emit-signals", TRUE, NULL);
    g_signal_connect (sink, "new-sample", G_CALLBACK (NewSample), &clientSocket);
    g_signal_connect (sink, "new-preroll", G_CALLBACK (NewPreroll), &clientSocket);
    gst_object_unref(sink);

    gint64 dimensions = 0;    
    GstPad *videopad = NULL;
    g_signal_emit_by_name (pipeline, "get-video-pad", 0, &videopad); 
    GstCaps *caps;
    if ((caps = gst_pad_get_current_caps (videopad)))
    {
        int width = -1, height = -1;
        GstStructure *s = gst_caps_get_structure (caps, 0);
        gst_structure_get_int (s, "width", &width);
        gst_structure_get_int (s, "height", &height);
        gst_caps_unref (caps);
        dimensions = (((uint64_t)width) << 32) | height;
    }
    gst_object_unref (videopad);
    
    MediaPlayerCommand command;
    sendto(clientSocket, &command, sizeof(MediaPlayerCommand), 0, (sockaddr*)&serverSockaddr, sizeof(struct sockaddr_un));

    if (status != Status_ERROR)
    {
        gint64 duration;
        gst_element_query_duration(pipeline, GST_FORMAT_TIME, &duration);

        //MediaPlayerCommand command;
        command.cmd = MPC_MediaLoaded;
        command.arg[0] = duration;
        command.arg[1] = dimensions;

        sendto(clientSocket, &command, sizeof(MediaPlayerCommand), 0, (sockaddr*)&serverSockaddr, sizeof(struct sockaddr_un));
    }
    
    int flags = fcntl(clientSocket, F_GETFL, 0);
    flags |= O_NONBLOCK;
    fcntl(clientSocket, F_SETFL, flags);

    while(true)
    {
        MediaPlayerCommand command;
        if (status != 0)
        {
            if (status == Status_EOS)
            {
                int flags = fcntl(clientSocket, F_GETFL, 0);
                flags &= ~O_NONBLOCK;
                fcntl(clientSocket, F_SETFL, flags);
                
                command.cmd = MPC_MediaEnded;
                command.arg[0] = 0;
                command.arg[1] = 0;
                close(videoSocket);
                videoSocket = socket(AF_UNIX, SOCK_STREAM, 0);
                connect(videoSocket, (sockaddr*)&videoServerSockaddr, sizeof(sockaddr_un));
                sendto(clientSocket, &command, sizeof(MediaPlayerCommand), 0, (sockaddr*)&serverSockaddr, sizeof(struct sockaddr_un));
            }
            else if (status == Status_ERROR)
            {
                int flags = fcntl(clientSocket, F_GETFL, 0);
                flags &= ~O_NONBLOCK;
                fcntl(clientSocket, F_SETFL, flags);
                
                command.cmd = MPC_MediaFailed;
                command.arg[0] = 0;
                command.arg[1] = 0;

                sendto(clientSocket, &command, sizeof(MediaPlayerCommand), 0, (sockaddr*)&serverSockaddr, sizeof(struct sockaddr_un));
            }
            else if (status == Status_READY)
            {
            }
            
            status = 0;
        }

        sockaddr_un clientSockaddr;
        socklen_t socklen = sizeof(sockaddr_un);
        pollfd fds;
        fds.fd = clientSocket;
        fds.events = POLLIN;
        if (poll(&fds, 1, 10) != -1)
        {
            if (recvfrom(clientSocket, &command, sizeof(MediaPlayerCommand), 0, (sockaddr*)&serverSockaddr, &socklen) != -1)
            {
                if (command.cmd == MPC_Play)
                {
                    int flags = fcntl(clientSocket, F_GETFL, 0);
                    flags |= O_NONBLOCK;
                    fcntl(clientSocket, F_SETFL, flags);

                    gst_element_set_state (pipeline, GST_STATE_PLAYING);
                    gst_element_get_state (pipeline, NULL, NULL, GST_CLOCK_TIME_NONE);
                }
                else if (command.cmd == MPC_Pause)
                {
                    int flags = fcntl(clientSocket, F_GETFL, 0);
                    flags &= ~O_NONBLOCK;
                    fcntl(clientSocket, F_SETFL, flags);

                    gst_element_set_state (pipeline, GST_STATE_PAUSED);
                }
                else if (command.cmd == MPC_Stop)
                {
                    for (; firstFrame != lastFrame; firstFrame = (firstFrame + 1) % MaxFrames)
                    {
                        close(storedFrames[firstFrame].fd);
                        gst_buffer_unmap (storedFrames[firstFrame].buffer, &storedFrames[firstFrame].map);
                        gst_sample_unref (storedFrames[firstFrame].sample);
                    }
                    int flags = fcntl(clientSocket, F_GETFL, 0);
                    flags &= ~O_NONBLOCK;
                    fcntl(clientSocket, F_SETFL, flags);

                    gst_element_set_state (pipeline, GST_STATE_PAUSED);
                    // Wait for the pipeline to preroll
                    gst_element_get_state (pipeline, NULL, NULL, GST_CLOCK_TIME_NONE);

                    gst_element_seek_simple (pipeline, GST_FORMAT_TIME, (GstSeekFlags)(GST_SEEK_FLAG_ACCURATE | GST_SEEK_FLAG_FLUSH), 0);

                    gst_element_set_state (pipeline, GST_STATE_PAUSED);

                    // Wait for the pipeline to preroll
                    gst_element_get_state (pipeline, NULL, NULL, GST_CLOCK_TIME_NONE);
                }
                else if (command.cmd == MPC_Seek)
                {
                    if (((int64_t)command.arg[0]) >= 0)
                    {
                        for (; firstFrame != lastFrame; firstFrame = (firstFrame + 1) % MaxFrames)
                        {
                            close(storedFrames[firstFrame].fd);
                            gst_buffer_unmap (storedFrames[firstFrame].buffer, &storedFrames[firstFrame].map);
                            gst_sample_unref (storedFrames[firstFrame].sample);
                        }
                        gst_element_seek_simple (pipeline, GST_FORMAT_TIME, (GstSeekFlags)(GST_SEEK_FLAG_ACCURATE | GST_SEEK_FLAG_FLUSH), command.arg[0]);
                        gst_element_get_state (pipeline, NULL, NULL, GST_CLOCK_TIME_NONE);
                    }
                }
                else if (command.cmd == MPC_Volume)
                {
                    union FU64
                    {
                        float f;
                        uint64_t u;
                    } cast;
                    cast.u = command.arg[0];
                    
                    g_object_set (pipeline, "volume", (double)cast.f, NULL);
                }
                else if (command.cmd == MPC_FrameAck)
                {
                    gint64 lastFrameAck = command.arg[0];
                    for (; firstFrame != lastFrame; firstFrame = (firstFrame + 1) % MaxFrames)
                    {
                        if (storedFrames[firstFrame].time == lastFrameAck)
                            break;
                        close(storedFrames[firstFrame].fd);
                        gst_buffer_unmap (storedFrames[firstFrame].buffer, &storedFrames[firstFrame].map);
                        gst_sample_unref (storedFrames[firstFrame].sample);
                    }
                }
            }
        }
        
        if (g_main_context_pending(nullptr))
        {
            g_main_context_iteration(nullptr, FALSE);
        }
    }
    
    g_source_remove(bus_watch_id);
    gst_object_unref(GST_OBJECT(pipeline));
    
    close(clientSocket);
    return 0;
}
