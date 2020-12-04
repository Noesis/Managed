using System;
using System.Runtime.InteropServices;

namespace NoesisApp
{
    internal class Fbdev
    {
        internal static uint GetWidth()
        {
            int SizeOfFbVarScreeninfo = Marshal.SizeOf<FbVarScreeninfo>();
            FbVarScreeninfo varScreeninfo = new FbVarScreeninfo();
            IntPtr arg = Marshal.AllocHGlobal(SizeOfFbVarScreeninfo);
            Marshal.StructureToPtr(varScreeninfo, arg, false);
            int fbFd = LibC.Open("/dev/fb", LibC.O_RDWR);
            LibC.Ioctl(fbFd, FBIOGET_VSCREENINFO, arg);
            varScreeninfo = Marshal.PtrToStructure<FbVarScreeninfo>(arg);
            Marshal.FreeHGlobal(arg);
            return varScreeninfo.xres;
        }

        internal static uint GetHeight()
        {
            int SizeOfFbVarScreeninfo = Marshal.SizeOf<FbVarScreeninfo>();
            FbVarScreeninfo varScreeninfo = new FbVarScreeninfo();
            IntPtr arg = Marshal.AllocHGlobal(SizeOfFbVarScreeninfo);
            Marshal.StructureToPtr(varScreeninfo, arg, false);
            int fbFd = LibC.Open("/dev/fb", LibC.O_RDWR);
            LibC.Ioctl(fbFd, FBIOGET_VSCREENINFO, arg);
            varScreeninfo = Marshal.PtrToStructure<FbVarScreeninfo>(arg);
            Marshal.FreeHGlobal(arg);
            return varScreeninfo.yres;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct FbBitfield
        {
            uint offset;
            uint length;
            uint msb_right;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct FbVarScreeninfo
        {
            internal uint xres;
            internal uint yres;
            uint xresVirtual;
            uint yresVirtual;
            uint xoffset;
            uint yoffset;
            uint bitsPerPixel;
            uint grayscale;
            FbBitfield red;
            FbBitfield green;
            FbBitfield blue;
            FbBitfield transp;
            uint nonstd;
            uint activate;
            uint height;
            uint width;
            uint accelFlags;
            uint pixclock;
            uint leftMargin;
            uint rightMargin;
            uint upperMargin;
            uint lowerMargin;
            uint hsyncLen;
            uint vsyncLen;
            uint sync;
            uint vmode;
            uint rotate;
            uint colorspace;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            uint[] reserved;
        }

        private const ulong FBIOGET_VSCREENINFO = 0x4600;
    }
}
