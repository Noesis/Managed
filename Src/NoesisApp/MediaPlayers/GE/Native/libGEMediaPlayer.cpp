////////////////////////////////////////////////////////////////////////////////////////////////////
// NoesisGUI - http://www.noesisengine.com
// Copyright (c) 2013 Noesis Technologies S.L. All Rights Reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////

#include <GLES2/gl2.h>
#include <GLES2/gl2ext.h>
#include <GLES3/gl3.h>
#include <GLES3/gl3ext.h>
#include <EGL/egl.h>
#include <EGL/eglext.h>
#include <libdrm/drm_fourcc.h>

#include <sys/socket.h>
#include <sys/un.h>
#include <sys/prctl.h>
#include <sys/wait.h>

#include <assert.h>
#include <stdlib.h>
#include <unistd.h>
#include <fcntl.h>
#include <errno.h>
#include <signal.h>
#include <pthread.h>

#include <stdio.h>

#include "MediaPlayerCommand.h"

static const GLchar* VertexShaderSource =
    "#version 100\n"
    "attribute vec4 a_position;\n"
    "attribute vec2 a_tex_coord;\n"
    "varying vec2 v_tex_coord;\n"
    "void main()\n"
    "{\n"
    "   gl_Position = a_position;\n"
    "   v_tex_coord = a_tex_coord;\n"
    "}\n";

static const GLchar* FragmentShaderSource =
    "#version 100\n"
    "#extension GL_OES_EGL_image_external : require\n"
    "precision mediump float;\n"
    "varying vec2 v_tex_coord;\n"
    "vec3 rgb;\n"
    "uniform samplerExternalOES s_yuyv_texture;\n"
    "void main()\n"
    "{\n"
    "    rgb = texture2D(s_yuyv_texture, v_tex_coord).rgb;\n"
    "    gl_FragColor = vec4(rgb,1);\n"
    "}\n";

static const GLchar* BlankFragmentShaderSource =
    "#version 100\n"
    "void main()\n"
    "{\n"
    "    gl_FragColor = vec4(0, 0, 0,1);\n"
    "}\n";

GLuint mVertexShader;
GLuint mFragmentShader;
GLuint mBlankFragmentShader;
GLuint mProgram;
GLuint mBlankProgram;
GLuint mVertexBuffer;
GLuint mIndexBuffer;
GLint mYuyvSamplerLocation;
PFNEGLCREATEIMAGEKHRPROC eglCreateImageKHR;
PFNEGLDESTROYIMAGEKHRPROC eglDestroyImageKHR;
PFNGLEGLIMAGETARGETTEXTURE2DOESPROC glEGLImageTargetTexture2DOES;

typedef unsigned int (*ReadStream)(const void* streamPtr, void* buffer, unsigned int size);
typedef unsigned int (*SeekStream)(const void* streamPtr, unsigned int offset);

typedef unsigned int (*MediaOpened)();
typedef unsigned int (*MediaEnded)();
typedef unsigned int (*MediaFailed)();


struct GstMediaPlayerState
{
    int serverSocket;
    sockaddr_un clientSockaddr;
    int videoServerSocket;
    int child;
    int fd;
    uint64_t duration;
    uint64_t time;
    uint64_t lastRenderTime;
    uint32_t width;
    uint32_t height;
    float volume;
    float balance;
    float speedRatio;
    bool isMuted;
    bool scrubbingEnabled;
    char tmpDir[256];
    pthread_t videoThread;
    const void* streamPtr;
    ReadStream readFn;
    SeekStream seekFn;
    MediaOpened mediaOpenedFn;
    MediaEnded mediaEndedFn;
    MediaFailed mediaFailedFn;
    bool isValid;
};

void* VideoThreadFunc(void* state)
{
    GstMediaPlayerState* st = (GstMediaPlayerState*)state;

    while (1)
    {
        sockaddr_un videoClientSockaddr;
        socklen_t socklen = sizeof(sockaddr_un);
        int videoSocket = accept(st->videoServerSocket, (struct sockaddr *) &videoClientSockaddr, &socklen);

        const unsigned int BUFFER_SIZE = 4 * 1024;
        char buffer[BUFFER_SIZE];
        unsigned int read_size;
        do
        {
            MediaPlayerCommand command;
            if (recv(videoSocket, &command, sizeof(command), 0) == -1)
                break;

            if (command.cmd == MPC_Play)
            {
                uint32_t size = (uint32_t)command.arg[0];
                while (size > 0)
                {
                    uint32_t batchSize = size > BUFFER_SIZE ? BUFFER_SIZE : size;
                    read_size = st->readFn(st->streamPtr, buffer, batchSize);
                    send(videoSocket, buffer, read_size, 0);
                    size -= read_size;
                    if (read_size == 0)
                        break;
                }

                if (size > 0)
                    break;
            }
            else if (command.cmd == MPC_Seek)
            {
                uint64_t offset = command.arg[0];
                st->seekFn(st->streamPtr, (uint32_t)offset);
            }
        }
        while (true);

        close(videoSocket);
    }
    return nullptr;
}

extern "C" void InitMediaPlayer()
{
    if (glEGLImageTargetTexture2DOES == 0)
    {
        mVertexShader = glCreateShader(GL_VERTEX_SHADER);
        glShaderSource(mVertexShader, 1, &VertexShaderSource, nullptr);
        glCompileShader(mVertexShader);

        mFragmentShader = glCreateShader(GL_FRAGMENT_SHADER);
        glShaderSource(mFragmentShader, 1, &FragmentShaderSource, nullptr);
        glCompileShader(mFragmentShader);

        mBlankFragmentShader = glCreateShader(GL_FRAGMENT_SHADER);
        glShaderSource(mBlankFragmentShader, 1, &BlankFragmentShaderSource, nullptr);
        glCompileShader(mBlankFragmentShader);

        mProgram = glCreateProgram();
        glAttachShader(mProgram, mVertexShader);
        glAttachShader(mProgram, mFragmentShader);
        glLinkProgram(mProgram);

        glBindAttribLocation(mProgram, 0, "a_position");
        glBindAttribLocation(mProgram, 1, "a_tex_coord");

        mYuyvSamplerLocation = glGetUniformLocation(mProgram, "s_yuyv_texture");

        mBlankProgram = glCreateProgram();
        glAttachShader(mBlankProgram, mVertexShader);
        glAttachShader(mBlankProgram, mBlankFragmentShader);
        glLinkProgram(mBlankProgram);

        GLfloat vertices[] = {
            -1.0f, 1.0f, 0.0f, 0.0f, 0.0f,
            -1.0f, -1.0f, 0.0f, 0.0f, 1.0f,
            1.0f, -1.0f, 0.0f, 1.0f, 1.0f,
            1.0f, 1.0f, 0.0f, 1.0f, 0.0f
        };
        glGenBuffers(1, &mVertexBuffer);
        glBindBuffer(GL_ARRAY_BUFFER, mVertexBuffer);
        glBufferData(GL_ARRAY_BUFFER, sizeof(vertices), vertices, GL_STATIC_DRAW);

        GLushort indices[] = {0, 1, 2, 0, 2, 3};
        glGenBuffers(1, &mIndexBuffer);
        glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, mIndexBuffer);
        glBufferData(GL_ELEMENT_ARRAY_BUFFER, sizeof(indices), indices, GL_STATIC_DRAW);

        eglCreateImageKHR = (PFNEGLCREATEIMAGEKHRPROC)eglGetProcAddress("eglCreateImageKHR");
        eglDestroyImageKHR = (PFNEGLDESTROYIMAGEKHRPROC)eglGetProcAddress("eglDestroyImageKHR");
        glEGLImageTargetTexture2DOES = (PFNGLEGLIMAGETARGETTEXTURE2DOESPROC)eglGetProcAddress("glEGLImageTargetTexture2DOES");
    }
}

extern "C" void* CreateState()
{
    GstMediaPlayerState* st = new GstMediaPlayerState();
    st->duration = 0xffffffffffffffff;
    st->time = 0xffffffffffffffff;
    st->lastRenderTime = 0xffffffffffffffff;
    st->width = 2;
    st->height = 2;
    st->volume = 0.5f;
    st->balance = 0.5f;
    st->speedRatio = 1.0f;
    st->isMuted = false;
    st->scrubbingEnabled = false;
    st->isValid = false;
    st->fd = -1;
    return st;
}

extern "C" void DestroyState(void* state)
{
    GstMediaPlayerState* st = (GstMediaPlayerState*)state;
    kill(st->child, SIGKILL);
    waitpid(st->child, NULL, 0);

    char tmpVideoPath[256];
    strcpy(tmpVideoPath, st->tmpDir);
    strcat(tmpVideoPath, "/video");
    unlink(tmpVideoPath);

    char serverSocketPath[256];
    strcpy(serverSocketPath, st->tmpDir);
    strcat(serverSocketPath, "/server_socket");
    unlink(serverSocketPath);

    char clientSocketPath[256];
    strcpy(clientSocketPath, st->tmpDir);
    strcat(clientSocketPath, "/client_socket");
    unlink(clientSocketPath);

    rmdir(st->tmpDir);

    delete st;
}

extern "C" bool OpenMedia(void* state, const void* streamPtr, const char* streamName, int64_t streamSize,
    ReadStream readFn, SeekStream seekFn, MediaOpened mediaOpenedFn, MediaEnded mediaEndedFn, MediaFailed mediaFailedFn)
{
    GstMediaPlayerState* st = (GstMediaPlayerState*)state;

    st->streamPtr = streamPtr;
    st->readFn = readFn;
    st->seekFn = seekFn;

    st->mediaOpenedFn = mediaOpenedFn;
    st->mediaEndedFn = mediaEndedFn;
    st->mediaFailedFn = mediaFailedFn;

    strcpy(st->tmpDir, "/tmp/mpXXXXXX");
    mkdtemp(st->tmpDir);

    st->serverSocket = socket(AF_UNIX, SOCK_DGRAM, 0);
    char serverSocketPath[256];
    strcpy(serverSocketPath, st->tmpDir);
    strcat(serverSocketPath, "/server_socket");

    sockaddr_un serverSockaddr;
    memset(&serverSockaddr, 0, sizeof(sockaddr_un));
    serverSockaddr.sun_family = AF_UNIX;
    strcpy(serverSockaddr.sun_path, serverSocketPath);
    unlink(serverSocketPath);
    bind(st->serverSocket, (sockaddr*)&serverSockaddr, sizeof(sockaddr_un));

    st->videoServerSocket = socket(AF_UNIX, SOCK_STREAM, 0);
    char videoServerSocketPath[256];
    strcpy(videoServerSocketPath, st->tmpDir);
    strcat(videoServerSocketPath, "/video_socket");

    sockaddr_un videoServerSockaddr;
    memset(&videoServerSockaddr, 0, sizeof(sockaddr_un));
    videoServerSockaddr.sun_family = AF_UNIX;
    strcpy(videoServerSockaddr.sun_path, videoServerSocketPath);
    unlink(videoServerSocketPath);
    bind(st->videoServerSocket, (sockaddr*)&videoServerSockaddr, sizeof(sockaddr_un));

    listen(st->videoServerSocket, 1);

    pthread_attr_t attr;
    pthread_attr_init(&attr);
    pthread_create(&st->videoThread, &attr, VideoThreadFunc, state);

    st->child = fork();
    if (st->child == 0)
    {
        prctl(PR_SET_PDEATHSIG, SIGKILL);
        char streamSizeStr[64];
        sprintf(streamSizeStr, "%ld", streamSize);
        execlp("./mp", "mp", st->tmpDir, streamSizeStr, (char*)NULL);
    }

    MediaPlayerCommand command;
    socklen_t socklen = sizeof(sockaddr_un);
    recvfrom(st->serverSocket, &command, sizeof(MediaPlayerCommand), 0, (sockaddr*)&st->clientSockaddr, &socklen);

    int flags = fcntl(st->serverSocket, F_GETFL, 0);
    flags |= O_NONBLOCK;
    fcntl(st->serverSocket, F_SETFL, flags);

    return true;
}

extern "C" uint32_t GetWidth(void* state)
{
    GstMediaPlayerState* st = (GstMediaPlayerState*)state;

    return st->width;
}

extern "C" uint32_t GetHeight(void* state)
{
    GstMediaPlayerState* st = (GstMediaPlayerState*)state;

    return st->height;
}

extern "C" bool GetCanPause(void* state)
{
    GstMediaPlayerState* st = (GstMediaPlayerState*)state;

    return true;
}

extern "C" bool GetHasAudio(void* state)
{
    GstMediaPlayerState* st = (GstMediaPlayerState*)state;

    return true;
}

extern "C" bool GetHasVideo(void* state)
{
    GstMediaPlayerState* st = (GstMediaPlayerState*)state;

    return true;
}
extern "C" float GetBufferingProgress(void* state)
{
    GstMediaPlayerState* st = (GstMediaPlayerState*)state;

    return 1.0f;
}

extern "C" float GetDownloadProgress(void* state)
{
    GstMediaPlayerState* st = (GstMediaPlayerState*)state;

    return 1.0f;
}

extern "C" double GetDuration(void* state)
{
    GstMediaPlayerState* st = (GstMediaPlayerState*)state;

    return (double)st->duration * 1e-9;
}

extern "C" double GetTime(void* state)
{
    GstMediaPlayerState* st = (GstMediaPlayerState*)state;

    return (double)st->time * 1e-9;
}

extern "C" float GetSpeedRatio(void* state)
{
    GstMediaPlayerState* st = (GstMediaPlayerState*)state;

    return st->speedRatio;
}

extern "C" void SetSpeedRatio(void* state, float speedRatio)
{
    GstMediaPlayerState* st = (GstMediaPlayerState*)state;

    st->speedRatio = speedRatio;
}

extern "C" float GetVolume(void* state)
{
    GstMediaPlayerState* st = (GstMediaPlayerState*)state;

    return st->volume;
}

extern "C" void SetVolume(void* state, float volume)
{
    GstMediaPlayerState* st = (GstMediaPlayerState*)state;

    st->volume = volume;
    union FU64
    {
        float f;
        uint64_t u;
    } cast;
    cast.f = volume;

    MediaPlayerCommand command;
    command.cmd = MPC_Volume;
    command.arg[0] = cast.u;
    command.arg[1] = 0;

    sendto(st->serverSocket, &command, sizeof(MediaPlayerCommand), 0, (sockaddr*)&st->clientSockaddr, sizeof(struct sockaddr_un));
}

extern "C" float GetBalance(void* state)
{
    GstMediaPlayerState* st = (GstMediaPlayerState*)state;

    return st->balance;
}

extern "C" void SetBalance(void* state, float balance)
{
    GstMediaPlayerState* st = (GstMediaPlayerState*)state;

    st->balance = balance;
}

extern "C" bool GetIsMuted(void* state)
{
    GstMediaPlayerState* st = (GstMediaPlayerState*)state;

    return st->isMuted;
}

extern "C" void SetIsMuted(void* state, bool isMuted)
{
    GstMediaPlayerState* st = (GstMediaPlayerState*)state;

    st->isMuted = isMuted;
}

extern "C" bool GetScrubbingEnabled(void* state)
{
    GstMediaPlayerState* st = (GstMediaPlayerState*)state;

    return st->scrubbingEnabled;
}

extern "C" void SetScrubbingEnabled(void* state, bool scrubbingEnabled)
{
    GstMediaPlayerState* st = (GstMediaPlayerState*)state;

    st->scrubbingEnabled = scrubbingEnabled;
}

extern "C" void Play(void* state)
{
    GstMediaPlayerState* st = (GstMediaPlayerState*)state;

    MediaPlayerCommand command;
    command.cmd = MPC_Play;
    command.arg[0] = 0;
    command.arg[1] = 0;

    sendto(st->serverSocket, &command, sizeof(MediaPlayerCommand), 0, (sockaddr*)&st->clientSockaddr, sizeof(struct sockaddr_un));
}

extern "C" void Pause(void* state)
{
    GstMediaPlayerState* st = (GstMediaPlayerState*)state;

    MediaPlayerCommand command;
    command.cmd = MPC_Pause;
    command.arg[0] = 0;
    command.arg[1] = 0;

    sendto(st->serverSocket, &command, sizeof(MediaPlayerCommand), 0, (sockaddr*)&st->clientSockaddr, sizeof(struct sockaddr_un));
}

extern "C" void Seek(void* state, double position)
{
    GstMediaPlayerState* st = (GstMediaPlayerState*)state;

    MediaPlayerCommand command;
    command.cmd = MPC_Seek;
    command.arg[0] = (uint64_t)(position * 1e9);
    command.arg[1] = 0;

    sendto(st->serverSocket, &command, sizeof(MediaPlayerCommand), 0, (sockaddr*)&st->clientSockaddr, sizeof(struct sockaddr_un));
}

extern "C" void Stop(void* state)
{
    GstMediaPlayerState* st = (GstMediaPlayerState*)state;

    MediaPlayerCommand command;
    command.cmd = MPC_Stop;
    command.arg[0] = 0;
    command.arg[1] = 0;

    sendto(st->serverSocket, &command, sizeof(MediaPlayerCommand), 0, (sockaddr*)&st->clientSockaddr, sizeof(struct sockaddr_un));
}

extern "C" bool HasNewFrame(void* state)
{
    GstMediaPlayerState* st = (GstMediaPlayerState*)state;

    return st->time != st->lastRenderTime;
}

extern "C" void RenderFrame(void* state)
{
    GstMediaPlayerState* st = (GstMediaPlayerState*)state;

    if (st->fd == -1)
    {
        glDisable(GL_SCISSOR_TEST);
        glUseProgram(mBlankProgram);

        glBindBuffer(GL_ARRAY_BUFFER, mVertexBuffer);
        glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, mIndexBuffer);

        glEnableVertexAttribArray(0);
        glEnableVertexAttribArray(1);

        glVertexAttribPointer(0, 3, GL_FLOAT, GL_FALSE, 5 * sizeof(GLfloat), 0);
        glVertexAttribPointer(1, 2, GL_FLOAT, GL_FALSE, 5 * sizeof(GLfloat), (const void*)(3 * sizeof(GLfloat)));

        glDrawElements(GL_TRIANGLES, 6, GL_UNSIGNED_SHORT, 0);
        return;
    }

    glDisable(GL_SCISSOR_TEST);
    glUseProgram(mProgram);

    glBindBuffer(GL_ARRAY_BUFFER, mVertexBuffer);
    glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, mIndexBuffer);

    glEnableVertexAttribArray(0);
    glEnableVertexAttribArray(1);

    glVertexAttribPointer(0, 3, GL_FLOAT, GL_FALSE, 5 * sizeof(GLfloat), 0);
    glVertexAttribPointer(1, 2, GL_FLOAT, GL_FALSE, 5 * sizeof(GLfloat), (const void*)(3 * sizeof(GLfloat)));

    EGLDisplay display = eglGetCurrentDisplay();
    glActiveTexture(GL_TEXTURE0);
    EGLint attribs[] = {
        EGL_WIDTH, (int)st->width,
        EGL_HEIGHT, (int)st->height,
        EGL_LINUX_DRM_FOURCC_EXT, DRM_FORMAT_NV12,
        EGL_DMA_BUF_PLANE0_FD_EXT, st->fd,
        EGL_DMA_BUF_PLANE0_OFFSET_EXT, 0,
        EGL_DMA_BUF_PLANE0_PITCH_EXT, (int)st->width,
        EGL_DMA_BUF_PLANE1_FD_EXT, st->fd,
        EGL_DMA_BUF_PLANE1_OFFSET_EXT, (int)(st->width * st->height),
        EGL_DMA_BUF_PLANE1_PITCH_EXT, (int)st->width,
        EGL_NONE
    };

    EGLImageKHR image = eglCreateImageKHR (display, EGL_NO_CONTEXT, EGL_LINUX_DMA_BUF_EXT, NULL, attribs);
    glEGLImageTargetTexture2DOES(GL_TEXTURE_EXTERNAL_OES, image);
    // eglDestroyImageKHR(display, image);

    glUniform1i(mYuyvSamplerLocation, 0);

    glDrawElements(GL_TRIANGLES, 6, GL_UNSIGNED_SHORT, 0);
    glEGLImageTargetTexture2DOES(GL_TEXTURE_EXTERNAL_OES, EGL_NO_IMAGE_KHR);
    eglDestroyImageKHR(display, image);
    close(st->fd);
    // st->fd = -1;
    // st->fd = 0;

    st->lastRenderTime = st->time;
}

extern "C" bool IsValid(void* state)
{
    GstMediaPlayerState* st = (GstMediaPlayerState*)state;

    return st->isValid;
}

extern "C" bool Update(void* state)
{
    GstMediaPlayerState* st = (GstMediaPlayerState*)state;

    msghdr msg;
    iovec iov;
    cmsghdr *cmsg;
    char cmsg_buffer[sizeof(cmsghdr) + sizeof(int)];
    MediaPlayerCommand command;
    memset(&msg, 0, sizeof(msghdr));
    memset(&iov, 0, sizeof(iovec));
    iov.iov_base = &command;
    iov.iov_len = sizeof(MediaPlayerCommand);
    msg.msg_name = nullptr;
    msg.msg_namelen = 0;
    msg.msg_iov = &iov;
    msg.msg_iovlen = 1;
    msg.msg_control = cmsg_buffer;
    msg.msg_controllen = sizeof(cmsg_buffer);
    cmsg = CMSG_FIRSTHDR(&msg);
    cmsg->cmsg_len = sizeof(cmsg_buffer);
    cmsg->cmsg_level = SOL_SOCKET;
    cmsg->cmsg_type = SCM_RIGHTS;

    while (recvmsg(st->serverSocket, &msg, 0) != -1)
    {
        if (command.cmd == MPC_NewFrame)
        {
            // close(st->fd);
            st->fd = *((int *)CMSG_DATA(cmsg));
            st->time = command.arg[0];
            st->width = (uint32_t)(command.arg[1] >> 32);
            st->height = (uint32_t)(command.arg[1] & 0xffffffff);
            return true;
        }
        else if (command.cmd == MPC_MediaEnded)
        {
            st->time = st->duration;
            st->mediaEndedFn();
            return true;
        }
        else if (command.cmd == MPC_MediaFailed)
        {
            st->isValid = false;
            st->mediaFailedFn();
            return false;
        }
        else if (command.cmd == MPC_MediaLoaded)
        {
            st->isValid = true;
            st->duration = command.arg[0];
            st->width = (uint32_t)(command.arg[1] >> 32);
            st->height = (uint32_t)(command.arg[1] & 0xffffffff);
            st->mediaOpenedFn();
            return true;
        }
    }

    assert(errno == EAGAIN || errno == EWOULDBLOCK);
    return true;
}
