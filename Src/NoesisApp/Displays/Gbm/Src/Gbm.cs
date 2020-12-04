using System;
using System.Runtime.InteropServices;

namespace NoesisApp
{
    internal class Gbm
    {
        internal const uint BO_USE_SCANOUT = 1 << 0;
        internal const uint BO_USE_RENDERING = 1 << 2;

        [StructLayout(LayoutKind.Explicit)]
        internal struct BoHandle
        {
            [FieldOffset(0)]
            internal uint u32;
            [FieldOffset(0)]
            internal ulong u64;
        }

        [DllImport("libgbm.so.1", EntryPoint = "gbm_create_device")]
        internal static extern IntPtr CreateDevice(int fd);

        [DllImport("libgbm.so.1", EntryPoint = "gbm_surface_create")]
        internal static extern IntPtr SurfaceCreate(IntPtr gbm, uint width, uint height, uint format, uint flags);

        [DllImport("libgbm.so.1", EntryPoint = "gbm_surface_lock_front_buffer")]
        internal static extern IntPtr SurfaceLockFrontBuffer(IntPtr surf);

        [DllImport("libgbm.so.1", EntryPoint = "gbm_surface_release_buffer")]
        internal static extern void SurfaceReleaseBuffer(IntPtr surf, IntPtr bo);

        [DllImport("libgbm.so.1", EntryPoint = "gbm_bo_get_width")]
        internal static extern uint BoGetWidth(IntPtr bo);

        [DllImport("libgbm.so.1", EntryPoint = "gbm_bo_get_height")]
        internal static extern uint BoGetHeight(IntPtr bo);

        [DllImport("libgbm.so.1", EntryPoint = "gbm_bo_get_stride")]
        internal static extern uint BoGetStride(IntPtr bo);

        [DllImport("libgbm.so.1", EntryPoint = "gbm_bo_get_format")]
        internal static extern uint BoGetFormat(IntPtr bo);

        [DllImport("libgbm.so.1", EntryPoint = "gbm_bo_get_handle")]
        internal static extern BoHandle BoGetHandle(IntPtr bo);
    }
}
