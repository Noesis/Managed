using System;
using System.Runtime.InteropServices;

namespace NoesisApp
{
    internal class Drm
    {
        internal const uint MODE_CONNECTED = 1;
        internal const uint MODE_DISCONNECTED = 2;
        internal const uint MODE_UNKNOWNCONNECTION = 3;

        internal const uint MODE_TYPE_PREFERRED = 1 << 3;

        internal const uint FORMAT_ARGB8888 = 0x34325241; // fourcc_code('A', 'R', '2', '4')

        internal const ulong CLIENT_CAP_UNIVERSAL_PLANES = 2;

        internal const uint MODE_PAGE_FLIP_EVENT = 0x01;

        [StructLayout(LayoutKind.Sequential)]
        internal struct ModeRes
        {
            internal int countFbs;
            internal IntPtr fbs;
            internal int countCrtcs;
            internal IntPtr crtcs;
            internal int countConnectors;
            internal IntPtr connectors;
            internal int countEncoders;
            internal IntPtr encoders;
            internal uint minWidth;
            internal uint maxWidth;
            internal uint minHeight;
            internal uint maxHeight;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct EventContext
        {
            internal int version;
            internal IntPtr vblankHandler;
            internal PageFlipHandler pageFlipHandler;
            internal IntPtr pageFlipHandler2;
            internal IntPtr sequenceHandler;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct ModeConnector
        {
            internal uint connectorId;
            internal uint encoderId;
            internal uint connectorType;
            internal uint connectorTypeId;
            internal uint connection;
            internal uint mmWidth;
            internal uint mmHeight;
            internal uint subpixel;
            internal int countModes;
            internal IntPtr modes;
            internal int countProps;
            internal IntPtr props;
            internal IntPtr propValues;
            internal int countEncoders;
            internal IntPtr encoders;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        internal struct ModeModeInfo
        {
            internal uint clock;
            internal ushort hdisplay;
            internal ushort hsyncStart;
            internal ushort hsyncEnd;
            internal ushort htotal;
            internal ushort hskew;
            internal ushort vdisplay;
            internal ushort vsyncStart;
            internal ushort vsyncEnd;
            internal ushort vtotal;
            internal ushort vscan;
            internal uint vrefresh;
            internal uint flags;
            internal uint type;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            internal string name;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct ModeEncoder
        {
            internal uint encoderId;
            internal uint encoderType;
            internal uint crtcId;
            internal uint possibleCrtcs;
            internal uint possibleClones;
        }

        internal delegate void PageFlipHandler(int fd, uint sequence, uint sec, uint usec, IntPtr userData);

        [DllImport("libdrm.so.2", EntryPoint = "drmModeGetResources")]
        internal static extern IntPtr ModeGetResources(int fd);

        [DllImport("libdrm.so.2", EntryPoint = "drmModeFreeResources")]
        internal static extern void ModeFreeResources(IntPtr ptr);

        [DllImport("libdrm.so.2", EntryPoint = "drmModeGetConnector")]
        internal static extern IntPtr ModeGetConnector(int fd, uint connectorId);

        [DllImport("libdrm.so.2", EntryPoint = "drmModeFreeConnector")]
        internal static extern void ModeFreeConnector(IntPtr ptr);

        [DllImport("libdrm.so.2", EntryPoint = "drmModeGetEncoder")]
        internal static extern IntPtr ModeGetEncoder(int fd, uint encoderId);

        [DllImport("libdrm.so.2", EntryPoint = "drmModeFreeEncoder")]
        internal static extern void ModeFreeEncoder(IntPtr ptr);

        [DllImport("libdrm.so.2", EntryPoint = "drmModeAddFB2")]
        internal static extern int ModeAddFB2(int fd, uint width, uint height, uint pixel_format,
            [MarshalAs(UnmanagedType.LPArray, SizeConst = 4)] uint[] bo_handles,
            [MarshalAs(UnmanagedType.LPArray, SizeConst = 4)] uint[] pitches,
            [MarshalAs(UnmanagedType.LPArray, SizeConst = 4)] uint[] offsets,
            ref uint buf_id, uint flags);

        [DllImport("libdrm.so.2", EntryPoint = "drmModeSetCrtc")]
        internal static extern int ModeSetCrtc(int fd, uint crtcId, uint bufferId, uint x, uint y, IntPtr connectors, int count, ref ModeModeInfo mode);

        [DllImport("libdrm.so.2", EntryPoint = "drmModePageFlip")]
        internal static extern int ModePageFlip(int fd, uint crtc_id, uint fb_id, uint flags, IntPtr user_data);

        [DllImport("libdrm.so.2", EntryPoint = "drmHandleEvent")]
        internal static extern int HandleEvent(int fd, ref EventContext evctx);

        [DllImport("libdrm.so.2", EntryPoint = "drmSetClientCap")]
        internal static extern void SetClientCap(int fd, ulong capability, ulong value);
    }
}
