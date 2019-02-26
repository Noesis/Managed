using System;
using System.Runtime.InteropServices;
using Noesis;

namespace NoesisApp
{
    public abstract class RenderContext
    {
        public abstract RenderDevice Device { get; }

        public abstract void Init(IntPtr display, IntPtr window, uint samples, bool vsync, bool sRGB);

        public abstract void SetWindow(IntPtr window);

        public abstract void BeginRender();

        public abstract void EndRender();

        public abstract void SetDefaultRenderTarget(int width, int height, bool doClearColor);

        public abstract void Swap();

        public abstract void Resize();
    }
}
