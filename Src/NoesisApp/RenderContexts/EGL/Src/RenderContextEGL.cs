using System;
using System.Runtime.InteropServices;
using Noesis;

namespace NoesisApp
{
    public class RenderContextEGL : RenderContext
    {
        public RenderContextEGL()
        {
        }

        static RenderContextEGL()
        {
        }

        ~RenderContextEGL()
        {
            eglDestroySurface(_display, _surface);
            _surface = IntPtr.Zero;
            eglDestroyContext(_display, _context);
            _context = IntPtr.Zero;
            eglMakeCurrent(_display, EGL_NO_SURFACE, EGL_NO_SURFACE, EGL_NO_CONTEXT);
            eglTerminate(_display);
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
            _display = eglGetDisplay(display);

            int ma = 0, mi = 0;
            eglInitialize(_display, ref ma, ref mi);

            do
            {
                int[] attrs =
                {
                    EGL_RENDERABLE_TYPE, EGL_OPENGL_ES2_BIT,
                    EGL_SURFACE_TYPE, EGL_WINDOW_BIT,
                    EGL_BUFFER_SIZE, 32,
                    EGL_STENCIL_SIZE, 8,
                    EGL_SAMPLE_BUFFERS, samples > 1 ? 1 : 0,
                    EGL_SAMPLES, (int)(samples > 1 ? samples : 0),
                    EGL_NONE
                };

                IntPtr[] cs = { IntPtr.Zero };
                int num = 0;
                eglChooseConfig(_display, attrs, cs, 1, ref num);
                if (num != 0)
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
                EGL_CONTEXT_CLIENT_VERSION, 2,
                EGL_NONE
            };

            _context = eglCreateContext(_display, _config, EGL_NO_CONTEXT, attribs);

            SetWindow(window);

            eglSwapInterval(_display, vsync ? 1 : 0);

            _device = new RenderDeviceGL();

        }

        public override void SetWindow(IntPtr window)
        {
            if (window != _window)
            {
                if (_surface != IntPtr.Zero)
                {
                    eglDestroySurface(_display, _surface);
                }

                _window = window;
                _surface = eglCreateWindowSurface(_display, _config, window, null);
                eglSurfaceAttrib(_display, _surface, EGL_SWAP_BEHAVIOR, EGL_BUFFER_DESTROYED);
                eglMakeCurrent(_display, _surface, _surface, _context);
            }
        }

        public override void BeginRender()
        {
            eglMakeCurrent(_display, _surface, _surface, _context);
        }

        public override void EndRender() { }

        public override void SetDefaultRenderTarget(int width, int height, bool doClearColor)
        {
            glBindFramebuffer(GL_FRAMEBUFFER, 0);

            glViewport(0, 0, width, height);

            glClearStencil(0);
            uint mask = GL_STENCIL_BUFFER_BIT;

            if (doClearColor)
            {
                mask |= GL_COLOR_BUFFER_BIT;
                glColorMask(true, true, true, true);
                glClearColor(0.0f, 0.0f, 0.0f, 0.0f);
            }

            glClear(mask);
        }

        public override ImageCapture CaptureRenderTarget(RenderTarget surface)
        {
            int[] viewport = new int[4];
            glGetIntegerv(GL_VIEWPORT, viewport);

            int x = viewport[0];
            int y = viewport[1];
            int width = viewport[2];
            int height = viewport[3];

            byte[] src = new byte[width * height * 4];
            int srcStride = width * 4;

            glPixelStorei(GL_PACK_ALIGNMENT, 1);
            glReadPixels(x, y, width, height, GL_RGBA, GL_UNSIGNED_BYTE, src);

            ImageCapture img = new ImageCapture((uint)width, (uint)height);

            byte[] dst = img.Pixels;

            for (int i = 0; i < height; ++i)
            {
                int dstRow = i * (int)img.Stride;
                int srcRow = i * srcStride;

                for (int j = 0; j < width; ++j)
                {
                    // RGBA -> BGRA
                    dst[dstRow + 4 * j + 2] = src[srcRow + 4 * j + 0];
                    dst[dstRow + 4 * j + 1] = src[srcRow + 4 * j + 1];
                    dst[dstRow + 4 * j + 0] = src[srcRow + 4 * j + 2];
                    dst[dstRow + 4 * j + 3] = src[srcRow + 4 * j + 3];
                }
            }

            return img;
        }

        public override void Swap()
        {
            eglSwapBuffers(_display, _surface);
        }

        public override void Resize() { }

        private IntPtr EGL_DEFAULT_DISPLAY = IntPtr.Zero;

        private IntPtr EGL_NO_CONTEXT = IntPtr.Zero;

        private IntPtr EGL_NO_SURFACE = IntPtr.Zero;

        private const int EGL_RENDERABLE_TYPE = 0x3040;

        private const int EGL_OPENGL_ES2_BIT = 0x0004;

        private const int EGL_SURFACE_TYPE = 0x3033;

        private const int EGL_WINDOW_BIT = 0x0004;

        private const int EGL_BUFFER_SIZE = 0x3020;

        private const int EGL_STENCIL_SIZE = 0x3026;

        private const int EGL_SAMPLE_BUFFERS = 0x3032;

        private const int EGL_SAMPLES = 0x3031;

        private const int EGL_NONE = 0x3038;

        private const int EGL_CONTEXT_CLIENT_VERSION = 0x3098;

        private const int EGL_SWAP_BEHAVIOR = 0x3093;

        private const int EGL_BUFFER_DESTROYED = 0x3095;

        [DllImport("EGL")]
        static extern IntPtr eglGetDisplay(IntPtr display_id);

        [DllImport("EGL")]
        static extern bool eglInitialize(IntPtr dpy, ref int major, ref int minor);

        [DllImport("EGL")]
        static extern bool eglChooseConfig(IntPtr dpy, int[] attrib_list, IntPtr[] configs, int config_size, ref int num_config);

        [DllImport("EGL")]
        static extern IntPtr eglCreateContext(IntPtr dpy, IntPtr config, IntPtr share_context, int[] attrib_list);

        [DllImport("EGL")]
        static extern bool eglDestroySurface(IntPtr dpy, IntPtr surface);

        [DllImport("EGL")]
        static extern IntPtr eglCreateWindowSurface(IntPtr dpy, IntPtr config, IntPtr win, int[] attrib_list);

        [DllImport("EGL")]
        static extern bool eglSurfaceAttrib(IntPtr dpy, IntPtr surface, int attribute, int value);

        [DllImport("EGL")]
        static extern bool eglMakeCurrent(IntPtr dpy, IntPtr draw, IntPtr read, IntPtr ctx);

        [DllImport("EGL")]
        static extern bool eglSwapInterval(IntPtr dpy, int interval);

        [DllImport("EGL")]
        static extern bool eglSwapBuffers(IntPtr dpy, IntPtr surface);

        [DllImport("EGL")]
        static extern bool eglDestroyContext(IntPtr dpy, IntPtr ctx);

        [DllImport("EGL")]
        static extern bool eglTerminate(IntPtr dpy);

        private const uint GL_FRAMEBUFFER = 0x8D40;
        private const uint GL_STENCIL_BUFFER_BIT = 0x00000400;
        private const uint GL_COLOR_BUFFER_BIT = 0x00004000;
        private const uint GL_VIEWPORT = 0x00000BA2;
        private const uint GL_PACK_ALIGNMENT = 0x00000D05;
        private const uint GL_UNSIGNED_BYTE = 0x00001401;
        private const uint GL_RGBA = 0x00001908;

        [DllImport("GLESv2")]
        static extern void glBindFramebuffer(uint target, uint framebuffer);

        [DllImport("GLESv2")]
        static extern void glViewport(int x, int y, int width, int height);

        [DllImport("GLESv2")]
        static extern void glClearStencil(int s);

        [DllImport("GLESv2")]
        static extern void glClear(uint s);

        [DllImport("GLESv2")]
        static extern void glColorMask(bool r, bool g, bool b, bool a);

        [DllImport("GLESv2")]
        static extern void glClearColor(float r, float g, float b, float a);

        [DllImport("GLESv2")]
        public static extern void glGetIntegerv(uint pname, int[] param);

        [DllImport("GLESv2")]
        public static extern void glPixelStorei(uint pname, int param);

        [DllImport("GLESv2")]
        public static extern void glReadPixels(int x, int y, int width, int height,
            uint format, uint type, byte[] pixels);
    }
}
