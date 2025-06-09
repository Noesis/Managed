using System;
using System.Runtime.InteropServices;
using Noesis;

namespace NoesisApp
{
    public class RenderContextWGL : RenderContext
    {
        public RenderContextWGL()
        {
        }

        ~RenderContextWGL()
        {
            wglMakeCurrent(IntPtr.Zero, IntPtr.Zero);
            wglDeleteContext(_context);
            _context = IntPtr.Zero;
        }

        public override RenderDevice Device
        {
            get { return _device; }
        }

        private IntPtr _display = IntPtr.Zero;
        private IntPtr _window = IntPtr.Zero;
        private IntPtr _context = IntPtr.Zero;
        private RenderDeviceGL _device;
        private ImageCapture _imageCapture;
        private byte[] _imageCapturePixels;

        public override void Init(IntPtr display, IntPtr window, uint samples, bool vsync, bool sRGB)
        {
            _display = display;
            _window = window;

            // Dummy window and context to get ARB wgl func pointers
            PIXELFORMATDESCRIPTOR pfd = new PIXELFORMATDESCRIPTOR();
            pfd.nSize = (ushort)System.Runtime.InteropServices.Marshal.SizeOf(pfd);
            pfd.nVersion = 1;
            pfd.dwFlags = PFD_DRAW_TO_WINDOW | PFD_SUPPORT_OPENGL | PFD_DOUBLEBUFFER;
            pfd.iPixelType = PFD_TYPE_RGBA;
            pfd.cColorBits = 24;
            pfd.cStencilBits = 8;

            int pf = ChoosePixelFormat(_display, ref pfd);

            SetPixelFormat(_display, pf, ref pfd);

            IntPtr hglrc = wglCreateContext(_display);

            wglMakeCurrent(_display, hglrc);

            wglGetExtensionsStringARB = GetProcAddress<GetExtensionsStringARB>("wglGetExtensionsStringARB");
            wglChoosePixelFormatARB = GetProcAddress<ChoosePixelFormatARB>("wglChoosePixelFormatARB");
            wglCreateContextAttribsARB = GetProcAddress<CreateContextAttribsARB>("wglCreateContextAttribsARB");
            wglSwapIntervalEXT = GetProcAddress<SwapIntervalEXT>("wglSwapIntervalEXT");

            wglMakeCurrent(IntPtr.Zero, IntPtr.Zero);
            wglDeleteContext(hglrc);

            IntPtr extensionStringPtr = wglGetExtensionsStringARB(_display);
            string extensionString = System.Runtime.InteropServices.Marshal.PtrToStringAnsi(extensionStringPtr);
            string[] extensions = extensionString.Split(' ');

            if (Array.IndexOf(extensions, "WGL_ARB_pixel_format") >= 0 && Array.IndexOf(extensions, "WGL_ARB_multisample") >= 0)
            {
                do
                {
                    int[] attribs =
                    {
                        WGL_DRAW_TO_WINDOW_ARB, GL_TRUE,
                        WGL_SUPPORT_OPENGL_ARB, GL_TRUE,
                        WGL_DOUBLE_BUFFER_ARB, GL_TRUE,
                        WGL_PIXEL_TYPE_ARB, WGL_TYPE_RGBA_ARB,
                        WGL_COLOR_BITS_ARB, 24,
                        WGL_STENCIL_BITS_ARB, 8,
                        WGL_SAMPLE_BUFFERS_ARB, samples > 1 ? 1 : 0,
                        WGL_SAMPLES_ARB, samples > 1 ? (int)samples : 0,
                        0
                    };

                    int[] pixelFormats = { 0 };
                    uint n = 0;
                    if (!wglChoosePixelFormatARB(_display, attribs, null, 1, pixelFormats, ref n) || n == 0)
                    {
                        pf = 0;
                        samples >>= 1;
                    }
                    else
                    {
                        pf = pixelFormats[0];
                    }
                }
                while (pf == 0 && samples != 0);
            }

            if (pf == 0)
            {
                pf = ChoosePixelFormat(_display, ref pfd);
            }

            DescribePixelFormat(_display, pf, (uint)System.Runtime.InteropServices.Marshal.SizeOf(pfd), ref pfd);

            SetPixelFormat(_display, pf, ref pfd);

            if (Array.IndexOf(extensions, "WGL_ARB_create_context") >= 0)
            {
                int[] versions = { 46, 45, 44, 43, 42, 41, 40, 33 };
                int flags = 0;

#if DEBUG
                flags |= WGL_CONTEXT_DEBUG_BIT_ARB;
#else
                if (Array.IndexOf(extensions, "WGL_ARB_create_context_no_error") >= 0)
                {
                    flags |= WGL_CONTEXT_OPENGL_NO_ERROR_ARB;
                }
#endif

                for (int i = 0; i < versions.Length && _context == IntPtr.Zero; i++)
                {
                    int[] attribs =
                    {
                        WGL_CONTEXT_FLAGS_ARB, flags,
                        WGL_CONTEXT_PROFILE_MASK_ARB, WGL_CONTEXT_CORE_PROFILE_BIT_ARB,
                        WGL_CONTEXT_MAJOR_VERSION_ARB, versions[i] / 10,
                        WGL_CONTEXT_MINOR_VERSION_ARB, versions[i] % 10,
                        0
                    };

                    _context = wglCreateContextAttribsARB(_display, IntPtr.Zero, attribs);
                }
            }

            if (_context == IntPtr.Zero)
            {
                _context = wglCreateContext(_display);
            }

            wglMakeCurrent(_display, _context);

            if (Array.IndexOf(extensions, "WGL_EXT_swap_control") >= 0)
            {
                wglSwapIntervalEXT(vsync ? 1 : 0);
            }

            glBindFramebuffer = GetProcAddress<BindFramebuffer>("glBindFramebuffer");

            _device = new RenderDeviceGL();
        }

        public override void SetWindow(IntPtr window) { }

        public override void BeginRender()
        {
            wglMakeCurrent(_display, _context);
        }

        public override void EndRender() { }

        public override void SetDefaultRenderTarget(int width, int height, bool doClearColor)
        {
            glBindFramebuffer(GL_FRAMEBUFFER, 0);

            glViewport(0, 0, width, height);
            glDisable(GL_SCISSOR_TEST);

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
            int srcStride = width * 4;

            if (_imageCapture == null || _imageCapture.Width != width || _imageCapture.Height != height)
            {
                _imageCapture = new ImageCapture((uint)width, (uint)height);
                _imageCapturePixels = new byte[width * height * 4];
            }

            glPixelStorei(GL_PACK_ALIGNMENT, 1);
            glReadPixels(x, y, width, height, GL_RGBA, GL_UNSIGNED_BYTE, _imageCapturePixels);

            byte[] dst = _imageCapture.Pixels;

            for (int i = 0; i < height; ++i)
            {
                int dstRow = i * (int)_imageCapture.Stride;
                int srcRow = (height - i - 1) * srcStride;

                for (int j = 0; j < width; j++)
                {
                    // RGBA -> BGRA
                    dst[dstRow + 4 * j + 2] = _imageCapturePixels[srcRow + 4 * j + 0];
                    dst[dstRow + 4 * j + 1] = _imageCapturePixels[srcRow + 4 * j + 1];
                    dst[dstRow + 4 * j + 0] = _imageCapturePixels[srcRow + 4 * j + 2];
                    dst[dstRow + 4 * j + 3] = _imageCapturePixels[srcRow + 4 * j + 3];
                }
            }

            return _imageCapture;
        }

        public override void Resize() { }

        public override void Swap()
        {
            SwapBuffers(_display);
        }

        public static TDelegate GetProcAddress<TDelegate>(string name) where TDelegate : class
        {
            IntPtr addr = wglGetProcAddress(name);
            if (addr == IntPtr.Zero) return null;
            return (TDelegate)(object)System.Runtime.InteropServices.Marshal.GetDelegateForFunctionPointer<TDelegate>(addr);
        }

        private const int GL_TRUE = 1;
        private const int WGL_DRAW_TO_WINDOW_ARB = 0x2001;
        private const int WGL_SUPPORT_OPENGL_ARB = 0x2010;
        private const int WGL_DOUBLE_BUFFER_ARB = 0x2011;
        private const int WGL_PIXEL_TYPE_ARB = 0x2013;
        private const int WGL_TYPE_RGBA_ARB = 0x202B;
        private const int WGL_COLOR_BITS_ARB = 0x2014;
        private const int WGL_STENCIL_BITS_ARB = 0x2023;
        private const int WGL_SAMPLE_BUFFERS_ARB = 0x2041;
        private const int WGL_SAMPLES_ARB = 0x2042;
        private const int WGL_CONTEXT_DEBUG_BIT_ARB = 0x00000001;
        private const int WGL_CONTEXT_OPENGL_NO_ERROR_ARB = 0x31B3;
        private const int WGL_CONTEXT_FLAGS_ARB = 0x2094;
        private const int WGL_CONTEXT_PROFILE_MASK_ARB = 0x9126;
        private const int WGL_CONTEXT_CORE_PROFILE_BIT_ARB = 0x00000001;
        private const int WGL_CONTEXT_MAJOR_VERSION_ARB = 0x2091;
        private const int WGL_CONTEXT_MINOR_VERSION_ARB = 0x2092;

        public delegate IntPtr GetExtensionsStringARB(IntPtr hdc);
        public GetExtensionsStringARB wglGetExtensionsStringARB;

        public delegate bool ChoosePixelFormatARB(IntPtr hdc, int[] piAttribIList, float[] pfAttribFList, uint nMaxFormats, int[] piFormats, ref uint nNumFormats);
        public ChoosePixelFormatARB wglChoosePixelFormatARB;

        public delegate IntPtr CreateContextAttribsARB(IntPtr hdc, IntPtr hShareContext, int[] attribList);
        public CreateContextAttribsARB wglCreateContextAttribsARB;

        public delegate bool SwapIntervalEXT(int interval);
        public SwapIntervalEXT wglSwapIntervalEXT;

        public delegate void BindFramebuffer(uint target, uint framebuffer);
        public BindFramebuffer glBindFramebuffer;

        private const uint PFD_DRAW_TO_WINDOW = 0x00000004;
        private const uint PFD_SUPPORT_OPENGL = 0x00000020;
        private const uint PFD_DOUBLEBUFFER = 0x00000001;
        private const byte PFD_TYPE_RGBA = 0;


        [StructLayout(LayoutKind.Sequential)]
        public struct PIXELFORMATDESCRIPTOR
        {
            public ushort nSize;
            public ushort nVersion;
            public uint dwFlags;
            public byte iPixelType;
            public byte cColorBits;
            public byte cRedBits;
            public byte cRedShift;
            public byte cGreenBits;
            public byte cGreenShift;
            public byte cBlueBits;
            public byte cBlueShift;
            public byte cAlphaBits;
            public byte cAlphaShift;
            public byte cAccumBits;
            public byte cAccumRedBits;
            public byte cAccumGreenBits;
            public byte cAccumBlueBits;
            public byte cAccumAlphaBits;
            public byte cDepthBits;
            public byte cStencilBits;
            public byte cAuxBuffers;
            public byte iLayerType;
            public byte bReserved;
            public uint dwLayerMask;
            public uint dwVisibleMask;
            public uint dwDamageMask;
        }

        private const string Gdi32LibraryName = "gdi32.dll";

        [DllImport(Gdi32LibraryName)]
        static extern int ChoosePixelFormat(IntPtr hdc, ref PIXELFORMATDESCRIPTOR ppfd);

        [DllImport(Gdi32LibraryName)]
        static extern bool SetPixelFormat(IntPtr hdc, int format, ref PIXELFORMATDESCRIPTOR ppfd);

        [DllImport(Gdi32LibraryName)]
        static extern int DescribePixelFormat(IntPtr hdc, int iPixelFormat, uint nBytes, ref PIXELFORMATDESCRIPTOR ppfd);

        [DllImport(Gdi32LibraryName)]
        static extern bool SwapBuffers(IntPtr hdc);

        private const string User32LibraryName = "user32.dll";

        [DllImport(User32LibraryName)]
        static extern IntPtr GetDC(IntPtr hWnd);

        private const uint GL_FRAMEBUFFER = 0x8D40;
        private const uint GL_STENCIL_BUFFER_BIT = 0x00000400;
        private const uint GL_COLOR_BUFFER_BIT = 0x00004000;
        private const uint GL_SCISSOR_TEST = 0x00000C11;
        private const uint GL_VIEWPORT = 0x00000BA2;
        private const uint GL_PACK_ALIGNMENT = 0x00000D05;
        private const uint GL_UNSIGNED_BYTE = 0x00001401;
        private const uint GL_RGBA = 0x00001908;

        private const string OpenGLLibraryName = "opengl32.dll";

        [DllImport(OpenGLLibraryName)]
        static extern IntPtr wglCreateContext(IntPtr hdc);

        [DllImport(OpenGLLibraryName)]
        static extern bool wglDeleteContext(IntPtr hglrc);

        [DllImport(OpenGLLibraryName)]
        static extern bool wglMakeCurrent(IntPtr hdc, IntPtr hglrc);

        [DllImport(OpenGLLibraryName)]
        static extern IntPtr wglGetProcAddress(string lpszProc);

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
        static extern void glDisable(uint cap);

        [DllImport(OpenGLLibraryName)]
        static extern void glGetIntegerv(uint pname, int[] param);

        [DllImport(OpenGLLibraryName)]
        static extern void glPixelStorei(uint pname, int param);

        [DllImport(OpenGLLibraryName)]
        static extern void glReadPixels(int x, int y, int width, int height,
            uint format, uint type, byte[] pixels);
    }
}
