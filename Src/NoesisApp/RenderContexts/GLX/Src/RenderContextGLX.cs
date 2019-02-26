using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Noesis;

using OpenGL;
using Khronos;

namespace NoesisApp
{
    public class RenderContextGLX : RenderContext
    {
        public RenderContextGLX()
        {
        }

        static RenderContextGLX()
        {
            Environment.SetEnvironmentVariable("OPENGL_NET_EGL_STATIC_INIT", "NO");
            Gl.Initialize();
        }

        ~RenderContextGLX()
        {
            _device = null;
            Glx.DestroyContext(_display, _context);
            _context = IntPtr.Zero;
            Glx.MakeCurrent(_display, IntPtr.Zero, IntPtr.Zero);
        }

        public override RenderDevice Device
        {
            get { return _device; }
        }

        private IntPtr _display = IntPtr.Zero;
        private IntPtr _window = IntPtr.Zero;
        private IntPtr _context = IntPtr.Zero;
        Glx.Extensions _extensions;
        private RenderDeviceGL _device;

        public override void Init(IntPtr display, IntPtr window, uint samples, bool vsync, bool sRGB)
        {
            _display = display;
            _window = window;

            int[] ma = { 0 }, mi = { 0 };
            Glx.QueryVersion(display, ma, mi);

            int screen = 0;//DefaultScreen(display);

            _extensions = new Glx.Extensions();

            IntPtr config = IntPtr.Zero;
            if (samples > 1 && _extensions.Multisample_ARB)
            {
                do
                {
                    unsafe
                    {
                        int[] attrs =
                        {
                            Glx.X_RENDERABLE, Gl.TRUE,
                            Glx.DRAWABLE_TYPE, (int)Glx.WINDOW_BIT,
                            Glx.RENDER_TYPE, (int)Glx.RGBA_BIT,
                            Glx.DOUBLEBUFFER, Gl.TRUE,
                            Glx.RED_SIZE, 8,
                            Glx.BLUE_SIZE, 8,
                            Glx.GREEN_SIZE, 8,
                            Glx.STENCIL_SIZE, 8,
                            Glx.SAMPLE_BUFFERS, samples > 1 ? 1 : 0,
                            Glx.SAMPLES, (int)(samples > 1 ? samples : 0),
                            Gl.NONE,
                        };

                        int[] num = { 0 };
                        IntPtr* cs = Glx.ChooseFBConfig(display, screen, attrs, num);
                        if (num[0] != 0)
                        {
                            config = cs[0];
                        }
                        else
                        {
                            samples >>= 1;
                        }
                        Glx.XFree((IntPtr)cs);
                    }
                } while (config == IntPtr.Zero && samples != 0);
            }
            else
            {
                unsafe
                {
                    int[] attrs =
                    {
                        Glx.X_RENDERABLE, Gl.TRUE,
                        Glx.DRAWABLE_TYPE, (int)Glx.WINDOW_BIT,
                        Glx.RENDER_TYPE, (int)Glx.RGBA_BIT,
                        Glx.DOUBLEBUFFER, Gl.TRUE,
                        Glx.RED_SIZE, 8,
                        Glx.BLUE_SIZE, 8,
                        Glx.GREEN_SIZE, 8,
                        Glx.STENCIL_SIZE, 8,
                        Gl.NONE,
                    };

                    int[] num = { 0 };
                    IntPtr* cs = Glx.ChooseFBConfig(display, screen, attrs, num);
                    config = cs[0];
                    Glx.XFree((IntPtr)cs);
                }
            }

            Glx.XVisualInfo visual = Glx.GetVisualFromFBConfig(display, config);

            if (_extensions.CreateContext_ARB)
            {
                int[] versions = { 46, 45, 44, 43, 42, 41, 40, 33 };
                int flags = 0;
                if (_extensions.CreateContextNoError_ARB)
                {
                    flags |= Glx.CONTEXT_OPENGL_NO_ERROR_ARB;
                }

                foreach (int version in versions)
                {
                    int[] attribs =
                    {
                            Glx.CONTEXT_FLAGS_ARB, flags,
                            Glx.CONTEXT_PROFILE_MASK_ARB, (int)Glx.CONTEXT_CORE_PROFILE_BIT_ARB,
                            Glx.CONTEXT_MAJOR_VERSION_ARB, version / 10,
                            Glx.CONTEXT_MINOR_VERSION_ARB, version % 10,
                            Gl.NONE
                        };

                    _context = Glx.CreateContextAttribsARB(display, config, IntPtr.Zero, true, attribs);
                    if (_context != IntPtr.Zero)
                        break;
                }
            }

            if (_context == IntPtr.Zero)
            {
                _context = Glx.CreateContext(display, visual, IntPtr.Zero, true);
            }

            Glx.MakeCurrent(display, (IntPtr)window, _context);

            if (_extensions.SwapControl_EXT)
            {
                Glx.SwapIntervalEXT(display, (IntPtr)window, vsync ? 1 : 0);
            }
            else if (_extensions.SwapControl_SGI)
            {
                Glx.SwapIntervalSGI(vsync ? 1 : 0);
            }

            _device = new RenderDeviceGL();
        }

        public override void SetWindow(IntPtr window) { }

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
            Glx.SwapBuffers(_display, _window);
        }

        public override void Resize() { }
    }
}
