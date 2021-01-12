using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Noesis;

namespace NoesisApp
{
    public class RenderContextGLX : RenderContext
    {
        public RenderContextGLX()
        {
        }

        ~RenderContextGLX()
        {
            glXMakeCurrent(_display, IntPtr.Zero, IntPtr.Zero);
            glXDestroyContext(_display, _context);
        }

        public override RenderDevice Device
        {
            get { return _device; }
        }

        private IntPtr _display = IntPtr.Zero;
        private IntPtr _window = IntPtr.Zero;
        private IntPtr _context = IntPtr.Zero;
        private RenderDeviceGL _device;

        public override void Init(IntPtr display, IntPtr window, uint samples, bool vsync, bool sRGB)
        {
            _display = display;
            _window = window;

            int ma = 0, mi = 0;
            glXQueryVersion(display, ref ma, ref mi);

            int screen = 0;//DefaultScreen(display);

            IntPtr extensionStringPtr = glXQueryExtensionsString(display, screen);
            string extensionString = System.Runtime.InteropServices.Marshal.PtrToStringAnsi(extensionStringPtr);
            string[] extensions = extensionString.Split(' ');

            IntPtr config = IntPtr.Zero;
            if (samples > 1 && Array.IndexOf(extensions, "GLX_ARB_multisample") >= 0)
            {
                do
                {
                    int[] attrs =
                    {
                        GLX_X_RENDERABLE, GL_TRUE,
                        GLX_DRAWABLE_TYPE, GLX_WINDOW_BIT,
                        GLX_RENDER_TYPE, GLX_RGBA_BIT,
                        GLX_DOUBLEBUFFER, GL_TRUE,
                        GLX_RED_SIZE, 8,
                        GLX_GREEN_SIZE, 8,
                        GLX_BLUE_SIZE, 8,
                        GLX_STENCIL_SIZE, 8,
                        GLX_SAMPLE_BUFFERS, samples > 1 ? 1 : 0,
                        GLX_SAMPLES, (int)(samples > 1 ? samples : 0),
                        GL_NONE,
                    };

                    int num = 0;
                    IntPtr ptr = glXChooseFBConfig(display, screen, attrs, ref num);
                    if (num != 0)
                    {
                        IntPtr[] cs = new IntPtr[num];
                        System.Runtime.InteropServices.Marshal.Copy(ptr, cs, 0, num);
                        config = cs[0];
                    }
                    else
                    {
                        samples >>= 1;
                    }
                    XFree(ptr);
                } while (config == IntPtr.Zero && samples != 0);
            }
            else
            {
                int[] attrs =
                {
                    GLX_X_RENDERABLE, GL_TRUE,
                    GLX_DRAWABLE_TYPE, GLX_WINDOW_BIT,
                    GLX_RENDER_TYPE, GLX_RGBA_BIT,
                    GLX_DOUBLEBUFFER, GL_TRUE,
                    GLX_RED_SIZE, 8,
                    GLX_GREEN_SIZE, 8,
                    GLX_BLUE_SIZE, 8,
                    GLX_STENCIL_SIZE, 8,
                    GL_NONE,
                };

                int num = 0;
                IntPtr ptr = glXChooseFBConfig(display, screen, attrs, ref num);
                IntPtr[] cs = new IntPtr[num];
                System.Runtime.InteropServices.Marshal.Copy(ptr, cs, 0, num);
                config = cs[0];
                XFree(ptr);
            }

            IntPtr visual = glXGetVisualFromFBConfig(display, config);

            IntPtr c = glXCreateContext(display, visual, IntPtr.Zero, true);

            glXCreateContextAttribsARB = GetProcAddress<CreateContextAttribsARB>("glXCreateContextAttribsARB");
            glXSwapIntervalEXT = GetProcAddress<SwapIntervalEXT>("glXSwapIntervalEXT");
            glXSwapIntervalMESA = GetProcAddress<SwapIntervalMESA>("glXSwapIntervalMESA");
            glXSwapIntervalSGI = GetProcAddress<SwapIntervalSGI>("glXSwapIntervalSGI");

            glXDestroyContext(display, c);

            XSetErrorHandler((a, b) => { return 0; });
            if (Array.IndexOf(extensions, "GLX_ARB_create_context") >= 0)
            {
                int[] versions = { 46, 45, 44, 43, 42, 41, 40, 33 };
                int flags = 0;

#if DEBUG
                flags |= GLX_CONTEXT_DEBUG_BIT_ARB;
#endif

                for (int i = 0; i < versions.Length && _context == IntPtr.Zero; i++)
                {
                    int[] attribs =
                    {
                        GLX_CONTEXT_FLAGS_ARB, flags,
                        GLX_CONTEXT_PROFILE_MASK_ARB, GLX_CONTEXT_CORE_PROFILE_BIT_ARB,
                        GLX_CONTEXT_MAJOR_VERSION_ARB, versions[i] / 10,
                        GLX_CONTEXT_MINOR_VERSION_ARB, versions[i] % 10,
                        0
                    };

                    _context = glXCreateContextAttribsARB(display, config, IntPtr.Zero, true, attribs);
                }
            }
            XSetErrorHandler(null);

            if (_context == IntPtr.Zero)
            {
                _context = glXCreateContext(display, visual, IntPtr.Zero, true);
            }

            glXMakeCurrent(display, window, _context);

            if (Array.IndexOf(extensions, "GLX_EXT_swap_control") >= 0)
            {
                glXSwapIntervalEXT(display, window, vsync ? 1 : 0);
            }
            else if (Array.IndexOf(extensions, "GLX_MESA_swap_control") >= 0)
            {
                glXSwapIntervalMESA((uint)(vsync ? 1 : 0));
            }
            else if (Array.IndexOf(extensions, "GLX_SGI_swap_control") >= 0)
            {
                glXSwapIntervalSGI(vsync ? 1 : 0);
            }

            _device = new RenderDeviceGL();
        }

        public override void SetWindow(IntPtr window) { }

        public override void BeginRender()
        {
            glXMakeCurrent(_display, _window, _context);
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
            glXSwapBuffers(_display, _window);
        }

        public override void Resize() { }

        public static TDelegate GetProcAddress<TDelegate>(string name) where TDelegate : class
        {
            IntPtr addr = glXGetProcAddress(name);
            if (addr == IntPtr.Zero) return null;
            return (TDelegate)(object)System.Runtime.InteropServices.Marshal.GetDelegateForFunctionPointer(addr, typeof(TDelegate));
        }

        private const int GL_TRUE = 1;
        private const int GL_NONE = 0;
        private const int GLX_X_RENDERABLE = 0x8012;
        private const int GLX_DRAWABLE_TYPE = 0x8010;
        private const int GLX_WINDOW_BIT = 0x00000001;
        private const int GLX_RENDER_TYPE = 0x8011;
        private const int GLX_RGBA_BIT = 0x00000001;
        private const int GLX_DOUBLEBUFFER = 5;
        private const int GLX_RED_SIZE = 8;
        private const int GLX_GREEN_SIZE = 9;
        private const int GLX_BLUE_SIZE = 10;
        private const int GLX_STENCIL_SIZE = 13;
        private const int GLX_SAMPLE_BUFFERS = 0x186a0;
        private const int GLX_SAMPLES = 0x186a1;
        private const int GLX_CONTEXT_DEBUG_BIT_ARB = 0x00000001;
        private const int GLX_CONTEXT_FLAGS_ARB = 0x2094;
        private const int GLX_CONTEXT_PROFILE_MASK_ARB = 0x9126;
        private const int GLX_CONTEXT_CORE_PROFILE_BIT_ARB = 0x00000001;
        private const int GLX_CONTEXT_MAJOR_VERSION_ARB = 0x2091;
        private const int GLX_CONTEXT_MINOR_VERSION_ARB = 0x2092;

        public delegate int XErrorHandler(IntPtr dpy, IntPtr error);

        private const string X11LibraryName = "X11";

        [DllImport(X11LibraryName)]
        public static extern int XFree(IntPtr data);

        [DllImport(X11LibraryName)]
        public extern static IntPtr XSetErrorHandler(XErrorHandler error_handler);

        public delegate IntPtr CreateContextAttribsARB(IntPtr dpy, IntPtr config, IntPtr share_context, bool direct, int[] attrib_list);
        public CreateContextAttribsARB glXCreateContextAttribsARB;

        public delegate void SwapIntervalEXT(IntPtr dpy, IntPtr drawable, int interval);
        public SwapIntervalEXT glXSwapIntervalEXT;

        public delegate int SwapIntervalMESA(uint interval);
        public SwapIntervalMESA glXSwapIntervalMESA;

        public delegate int SwapIntervalSGI(int interval);
        public SwapIntervalSGI glXSwapIntervalSGI;

        private const string GLXLibraryName = "GLX";

        [DllImport(GLXLibraryName)]
        static extern bool glXQueryVersion(IntPtr dpy, ref int major, ref int minor);

        [DllImport(GLXLibraryName)]
        static extern IntPtr glXQueryExtensionsString(IntPtr dpy, int screen);

        [DllImport(GLXLibraryName)]
        static extern IntPtr glXChooseFBConfig(IntPtr dpy, int screen, int[] attrib_list, ref int nelements);

        [DllImport(GLXLibraryName)]
        static extern IntPtr glXGetVisualFromFBConfig(IntPtr dpy, IntPtr config);

        [DllImport(GLXLibraryName)]
        static extern IntPtr glXCreateContext(IntPtr dpy, IntPtr vis, IntPtr shareList, bool direct);

        [DllImport(GLXLibraryName)]
        static extern void glXDestroyContext(IntPtr dpy, IntPtr ctx);

        [DllImport(GLXLibraryName)]
        static extern bool glXMakeCurrent(IntPtr dpy, IntPtr drawable, IntPtr ctx);

        [DllImport(GLXLibraryName)]
        static extern void glXSwapBuffers(IntPtr dpy, IntPtr drawable);

        [DllImport(OpenGLLibraryName)]
        static extern IntPtr glXGetProcAddress(string lpszProc);

        private const uint GL_FRAMEBUFFER = 0x8D40;
        private const uint GL_STENCIL_BUFFER_BIT = 0x00000400;
        private const uint GL_COLOR_BUFFER_BIT = 0x00004000;
        private const uint GL_VIEWPORT = 0x00000BA2;
        private const uint GL_PACK_ALIGNMENT = 0x00000D05;
        private const uint GL_UNSIGNED_BYTE = 0x00001401;
        private const uint GL_RGBA = 0x00001908;

        private const string OpenGLLibraryName = "GL";

        [DllImport(OpenGLLibraryName)]
        static extern void glBindFramebuffer(uint target, uint framebuffer);

        [DllImport(OpenGLLibraryName)]
        static extern void glViewport(int x, int y, int width, int height);

        [DllImport(OpenGLLibraryName)]
        static extern void glClearStencil(int s);

        [DllImport(OpenGLLibraryName)]
        static extern void glClear(uint s);

        [DllImport(OpenGLLibraryName)]
        static extern void glColorMask(bool r, bool g, bool b, bool a);

        [DllImport(OpenGLLibraryName)]
        static extern void glClearColor(float r, float g, float b, float a);

        [DllImport(OpenGLLibraryName)]
        public static extern void glGetIntegerv(uint pname, int[] param);

        [DllImport(OpenGLLibraryName)]
        public static extern void glPixelStorei(uint pname, int param);

        [DllImport(OpenGLLibraryName)]
        public static extern void glReadPixels(int x, int y, int width, int height,
            uint format, uint type, byte[] pixels);
    }
}
