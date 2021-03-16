using System;
using System.Runtime.InteropServices;

namespace NoesisApp
{
    internal class LibC
    {
        internal const short POLLIN = 1;
        internal const short POLLPRI = 2;

        internal const int O_RDONLY = 0;
        internal const int O_RDWR = 2;
        internal const int O_NONBLOCK = 0x0800;

        internal const int EAGAIN = 11;

        private const int F_GETFL = 3;
        private const int F_SETFL = 4;

        [StructLayout(LayoutKind.Sequential)]
        internal struct PollFd
        {
            internal int fd;
            internal short events;
            internal short revents;
        }

        internal static int Poll(PollFd[] fds, int timeout)
        {
            int SizeOfPollFd = Marshal.SizeOf<PollFd>();
            int nfds = fds.Length;
            IntPtr fds_ = Marshal.AllocHGlobal(nfds * SizeOfPollFd);
            for (int i = 0; i < nfds; ++i)
            {
                Marshal.StructureToPtr(fds[i], IntPtr.Add(fds_, i * SizeOfPollFd), false);
            }
            int ret = poll(fds_, (ulong)nfds, timeout);
            for (int i = 0; i < nfds; ++i)
            {
                fds[i] = Marshal.PtrToStructure<PollFd>(IntPtr.Add(fds_, i * SizeOfPollFd));
            }
            Marshal.FreeHGlobal(fds_);
            return ret;
        }

        internal static int SetFdFlags(int fd, int flags)
        {
            int flags_ = fcntl(fd, F_GETFL, 0);
            flags_ |= flags;
            return fcntl(fd, F_SETFL, flags_);
        }

        [DllImport("libc")]
        private static extern int poll(IntPtr fds, ulong nfds, int timeout);

        [DllImport("libc", EntryPoint = "open")]
        public static extern int Open(string pathname, int flags);

        [DllImport("libc", EntryPoint = "fcntl")]
        public static extern int fcntl(int fd, int flags, int arg);

        [DllImport("libc", EntryPoint = "ioctl")]
        internal static extern int Ioctl(int fd, ulong request, IntPtr arg);
    }
}
