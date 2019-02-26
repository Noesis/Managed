using System;
using System.Runtime.InteropServices;
using Noesis;

using OpenGL;
using Khronos;

namespace NoesisApp
{
    public class RenderContextEGL : RenderContext
    {
        public RenderContextEGL()
        {
        }

        static RenderContextEGL()
        {
            Gl.Initialize();
        }

        ~RenderContextEGL()
        {
            Egl.DestroySurface(_display, _surface);
            _surface = IntPtr.Zero;
            Egl.DestroyContext(_display, _context);
            _context = IntPtr.Zero;
            Egl.MakeCurrent(_display, (IntPtr)Egl.NO_SURFACE, (IntPtr)Egl.NO_SURFACE, (IntPtr)Egl.NO_CONTEXT);
            Egl.Terminate(_display);
        }

        public override RenderDevice Device
        {
            get { return _device; }
        }

        private IntPtr _display = IntPtr.Zero;
        private IntPtr _window = IntPtr.Zero;
        private IntPtr _context = IntPtr.Zero;
        private IntPtr _config = IntPtr.Zero;
        private IntPtr _surface = IntPtr.Zero;
        private RenderDeviceGL _device;

        public override void Init(IntPtr display, IntPtr window, uint samples, bool vsync, bool sRGB)
        {
            _display = Egl.GetDisplay((IntPtr)Egl.DEFAULT_DISPLAY);

            int[] ma = { 0 }, mi = { 0 };
            Egl.Initialize(_display, ma, mi);

            do
            {
                int[] attrs =
                {
                    Egl.RENDERABLE_TYPE, Egl.OPENGL_ES2_BIT,
                    Egl.SURFACE_TYPE, Egl.WINDOW_BIT,
                    Egl.BUFFER_SIZE, 32,
                    Egl.STENCIL_SIZE, 8,
                    Egl.SAMPLE_BUFFERS, samples > 1 ? 1 : 0,
                    Egl.SAMPLES, (int)(samples > 1 ? samples : 0),
                    Egl.NONE
                };

                IntPtr[] cs = { IntPtr.Zero };
                int[] num = { 0 };
                Egl.ChooseConfig(_display, attrs, cs, 1, num);
                if (num[0] != 0)
                {
                    _config = cs[0];
                }
                else
                {
                    samples >>= 1;
                }
            } while (_config == IntPtr.Zero && samples != 0);

            int[] attribs =
            {
                Egl.CONTEXT_CLIENT_VERSION, 2,
                Egl.NONE
            };

            _context = Egl.CreateContext(_display, _config, (IntPtr)Egl.NO_CONTEXT, attribs);

            SetWindow(window);

            Egl.SwapInterval(_display, vsync ? 1 : 0);

            _device = new RenderDeviceGL();
        }

        public override void SetWindow(IntPtr window)
        {
            if (window != _window)
            {
                if (_surface != IntPtr.Zero)
                {
                    Egl.DestroySurface(_display, _surface);
                }

                _window = window;
                _surface = Egl.CreateWindowSurface(_display, _config, window, null);
                Egl.SurfaceAttrib(_display, _surface, Egl.SWAP_BEHAVIOR, Egl.BUFFER_DESTROYED);
                Egl.MakeCurrent(_display, _surface, _surface, _context);
            }
        }

        public override void BeginRender() { }

        public override void EndRender() { }

        public override void SetDefaultRenderTarget(int width, int height, bool doClearColor)
        {
            Gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

            Gl.Viewport(0, 0, width, height);

            Gl.ClearStencil(0);
            ClearBufferMask mask = ClearBufferMask.StencilBufferBit;

            if (doClearColor)
            {
                Gl.ColorMask(true, true, true, true);
                Gl.ClearColor(0.0f, 0.0f, 0.0f, 0.0f);
            }

            Gl.Clear(mask);
        }

        public override void Swap()
        {
            Egl.SwapBuffers(_display, _surface);
        }

        public override void Resize() { }

    }
}
