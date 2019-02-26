using System;
using System.Runtime.InteropServices;
using Noesis;

using OpenGL;
using Khronos;

namespace NoesisApp
{
    public class RenderContextWGL : RenderContext
    {
        public RenderContextWGL()
        {
        }

        static RenderContextWGL()
        {
            Gl.Initialize();
        }

        ~RenderContextWGL()
        {
            Wgl.DeleteContext(_context);
            _context = IntPtr.Zero;
            Wgl.MakeCurrent(_display, IntPtr.Zero);
        }

        public override RenderDevice Device
        {
            get { return _device; }
        }

        private IntPtr _display = IntPtr.Zero;
        private IntPtr _window = IntPtr.Zero;
        private IntPtr _context = IntPtr.Zero;
        Wgl.Extensions _extensions;
        private RenderDeviceGL _device;

        public override void Init(IntPtr display, IntPtr window, uint samples, bool vsync, bool sRGB)
        {
            _display = display;
            _window = window;

            _extensions = new Wgl.Extensions();
            int pixelFormat = 0;
            if (_extensions.PixelFormat_ARB && _extensions.Multisample_ARB)
            {
                do
                {
                    int[] attribs =
                    {
                        Wgl.DRAW_TO_WINDOW_ARB, Gl.TRUE,
                        Wgl.SUPPORT_OPENGL_ARB, Gl.TRUE,
                        Wgl.DOUBLE_BUFFER_ARB, Gl.TRUE,
                        Wgl.PIXEL_TYPE_ARB, Wgl.TYPE_RGBA_ARB,
                        Wgl.COLOR_BITS_ARB, 24,
                        Wgl.STENCIL_BITS_ARB, 8,
                        Wgl.SAMPLE_BUFFERS_ARB, samples > 1 ? 1 : 0,
                        Wgl.SAMPLES_ARB, (int)(samples > 1 ? samples : 0),
                        Gl.NONE
                    };

                    float[] fattribs = { };
                    int[] pf = { 0 };
                    uint[] n = { 0 };
                    if (Wgl.ChoosePixelFormatARB(display, attribs, fattribs, 1, pf, n))
                    {
                        pixelFormat = pf[0];
                    }
                    else
                    {
                        samples >>= 1;
                    }
                } while (pixelFormat == 0 && samples != 0);
            }

            Wgl.PIXELFORMATDESCRIPTOR pfd = new Wgl.PIXELFORMATDESCRIPTOR();
            if (pixelFormat == 0)
            {
                pfd.dwFlags = Wgl.PixelFormatDescriptorFlags.DrawToWindow | Wgl.PixelFormatDescriptorFlags.SupportOpenGL | Wgl.PixelFormatDescriptorFlags.Doublebuffer;
                pfd.iPixelType = Wgl.PixelFormatDescriptorPixelType.Rgba;
                pfd.cStencilBits = 8;
                pfd.cColorBits = 24;
                pixelFormat = Wgl.ChoosePixelFormat(display, ref pfd);
            }

            int ir = Wgl.UnsafeNativeMethods.DescribePixelFormat(display, pixelFormat, (uint)Noesis.Marshal.SizeOf<Wgl.PIXELFORMATDESCRIPTOR>(), ref pfd);
            bool r = Wgl.UnsafeNativeMethods.SetPixelFormat(display, pixelFormat, ref pfd);

            if (_extensions.CreateContext_ARB)
            {
                int[] versions = { 46, 45, 44, 43, 42, 41, 40, 33 };
                int flags = 0;
                if (_extensions.CreateContextNoError_ARB)
                {
                    flags |= Wgl.CONTEXT_OPENGL_NO_ERROR_ARB;
                }

                foreach (int version in versions)
                {
                    int[] attribs =
                    {
                        Wgl.CONTEXT_FLAGS_ARB, flags,
                        Wgl.CONTEXT_PROFILE_MASK_ARB, (int)Wgl.CONTEXT_CORE_PROFILE_BIT_ARB,
                        Wgl.CONTEXT_MAJOR_VERSION_ARB, version / 10,
                        Wgl.CONTEXT_MINOR_VERSION_ARB, version % 10,
                        Gl.NONE
                    };

                    _context = Wgl.CreateContextAttribsARB(display, IntPtr.Zero, attribs);
                }
            }

            if (_context == IntPtr.Zero)
            {
                _context = Wgl.CreateContext(display);
            }

            r = Wgl.MakeCurrent(display, _context);

            if (_extensions.SwapControl_EXT)
            {
                Wgl.SwapIntervalEXT(vsync ? 1 : 0);
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

        public override void Resize() { }

        public override void Swap()
        {
            Wgl.UnsafeNativeMethods.GdiSwapBuffersFast(_display);
        }

    }
}
