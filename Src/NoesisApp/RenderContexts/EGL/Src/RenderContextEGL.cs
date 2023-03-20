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
        private ImageCapture _imageCapture;
        private byte[] _imageCapturePixels;

        public override void Init(IntPtr display, IntPtr window, uint samples, bool vsync, bool sRGB)
        {
            _display = eglGetDisplay(display);
            if (_display == IntPtr.Zero)
            {
                int errorCode = eglGetError();
                string error = GetErrorString(errorCode);
                throw new Exception($"eglGetDisplay failed with {error}");
            }

            int ma = 0, mi = 0;
            uint initialized = eglInitialize(_display, ref ma, ref mi);
            if (initialized == EGL_FALSE)
            {
                int errorCode = eglGetError();
                string error = GetErrorString(errorCode);
                throw new Exception($"eglInitialize failed with {error}");
            }

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
            if (_context == IntPtr.Zero)
            {
                int errorCode = eglGetError();
                string error = GetErrorString(errorCode);
                throw new Exception($"eglCreateContext failed with {error}");
            }

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
                if (_surface == IntPtr.Zero)
                {
                    int errorCode = eglGetError();
                    string error = GetErrorString(errorCode);
                    throw new Exception($"eglCreateWindowSurface failed with {error}");
                }
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

        public override void Swap()
        {
            eglSwapBuffers(_display, _surface);
        }

        public override void Resize() { }

        private string GetErrorString(int errorCode)
        {
            switch (errorCode)
            {
                case EGL_SUCCESS:
                    return "EGL_SUCCESS";
                case EGL_NOT_INITIALIZED:
                    return "EGL_NOT_INITIALIZED";
                case EGL_BAD_ACCESS:
                    return "EGL_BAD_ACCESS";
                case EGL_BAD_ALLOC:
                    return "EGL_BAD_ALLOC";
                case EGL_BAD_ATTRIBUTE:
                    return "EGL_BAD_ATTRIBUTE";
                case EGL_BAD_CONFIG:
                    return "EGL_BAD_CONFIG";
                case EGL_BAD_CONTEXT:
                    return "EGL_BAD_CONTEXT";
                case EGL_BAD_CURRENT_SURFACE:
                    return "EGL_BAD_CURRENT_SURFACE";
                case EGL_BAD_DISPLAY:
                    return "EGL_BAD_DISPLAY";
                case EGL_BAD_MATCH:
                    return "EGL_BAD_MATCH";
                case EGL_BAD_NATIVE_PIXMAP:
                    return "EGL_BAD_NATIVE_PIXMAP";
                case EGL_BAD_NATIVE_WINDOW:
                    return "EGL_BAD_NATIVE_WINDOW";
                case EGL_BAD_PARAMETER:
                    return "EGL_BAD_PARAMETER";
                case EGL_BAD_SURFACE:
                    return "EGL_BAD_SURFACE";
                case EGL_CONTEXT_LOST:
                    return "EGL_CONTEXT_LOST";
                default:
                    return "Unknown";
            }
        }

        #region EGL Imports and Constants
        static RenderContextEGL()
        {
            if (Noesis.Platform.ID == Noesis.PlatformID.Android)
            {
                eglGetDisplay           = AndroidImports.eglGetDisplay;
                eglInitialize           = AndroidImports.eglInitialize;
                eglChooseConfig         = AndroidImports.eglChooseConfig;
                eglCreateContext        = AndroidImports.eglCreateContext;
                eglDestroySurface       = AndroidImports.eglDestroySurface;
                eglCreateWindowSurface  = AndroidImports.eglCreateWindowSurface;
                eglSurfaceAttrib        = AndroidImports.eglSurfaceAttrib;
                eglMakeCurrent          = AndroidImports.eglMakeCurrent;
                eglSwapInterval         = AndroidImports.eglSwapInterval;
                eglSwapBuffers          = AndroidImports.eglSwapBuffers;
                eglDestroyContext       = AndroidImports.eglDestroyContext;
                eglTerminate            = AndroidImports.eglTerminate;
                eglGetError             = AndroidImports.eglGetError;
                glBindFramebuffer       = AndroidImports.glBindFramebuffer;
                glViewport              = AndroidImports.glViewport;
                glClearStencil          = AndroidImports.glClearStencil;
                glClear                 = AndroidImports.glClear;
                glColorMask             = AndroidImports.glColorMask;
                glClearColor            = AndroidImports.glClearColor;
                glDisable               = AndroidImports.glDisable;
                glGetIntegerv           = AndroidImports.glGetIntegerv;
                glPixelStorei           = AndroidImports.glPixelStorei;
                glReadPixels            = AndroidImports.glReadPixels;
            }
            else
            {
                eglGetDisplay           = LinuxImports.eglGetDisplay;
                eglInitialize           = LinuxImports.eglInitialize;
                eglChooseConfig         = LinuxImports.eglChooseConfig;
                eglCreateContext        = LinuxImports.eglCreateContext;
                eglDestroySurface       = LinuxImports.eglDestroySurface;
                eglCreateWindowSurface  = LinuxImports.eglCreateWindowSurface;
                eglSurfaceAttrib        = LinuxImports.eglSurfaceAttrib;
                eglMakeCurrent          = LinuxImports.eglMakeCurrent;
                eglSwapInterval         = LinuxImports.eglSwapInterval;
                eglSwapBuffers          = LinuxImports.eglSwapBuffers;
                eglDestroyContext       = LinuxImports.eglDestroyContext;
                eglTerminate            = LinuxImports.eglTerminate;
                eglGetError             = LinuxImports.eglGetError;
                glBindFramebuffer       = LinuxImports.glBindFramebuffer;
                glViewport              = LinuxImports.glViewport;
                glClearStencil          = LinuxImports.glClearStencil;
                glClear                 = LinuxImports.glClear;
                glColorMask             = LinuxImports.glColorMask;
                glClearColor            = LinuxImports.glClearColor;
                glDisable               = LinuxImports.glDisable;
                glGetIntegerv           = LinuxImports.glGetIntegerv;
                glPixelStorei           = LinuxImports.glPixelStorei;
                glReadPixels            = LinuxImports.glReadPixels;
            }
        }

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
        private const int EGL_NATIVE_VISUAL_ID = 0x302E;
        private const int EGL_NONE = 0x3038;
        private const int EGL_CONTEXT_CLIENT_VERSION = 0x3098;
        private const int EGL_SWAP_BEHAVIOR = 0x3093;
        private const int EGL_BUFFER_DESTROYED = 0x3095;

        private const uint EGL_FALSE = 0;
        private const uint EGL_TRUE = 1;

        private const int EGL_SUCCESS = 0x3000;
        private const int EGL_NOT_INITIALIZED = 0x3001;
        private const int EGL_BAD_ACCESS = 0x3002;
        private const int EGL_BAD_ALLOC = 0x3003;
        private const int EGL_BAD_ATTRIBUTE = 0x3004;
        private const int EGL_BAD_CONFIG = 0x3005;
        private const int EGL_BAD_CONTEXT = 0x3006;
        private const int EGL_BAD_CURRENT_SURFACE = 0x3007;
        private const int EGL_BAD_DISPLAY = 0x3008;
        private const int EGL_BAD_MATCH = 0x3009;
        private const int EGL_BAD_NATIVE_PIXMAP = 0x300A;
        private const int EGL_BAD_NATIVE_WINDOW = 0x300B;
        private const int EGL_BAD_PARAMETER = 0x300C;
        private const int EGL_BAD_SURFACE = 0x300D;
        private const int EGL_CONTEXT_LOST = 0x300E;
        private const uint EGL_PLATFORM_GBM_KHR = 0x31D7;

        private const uint GL_FRAMEBUFFER = 0x8D40;
        private const uint GL_STENCIL_BUFFER_BIT = 0x00000400;
        private const uint GL_COLOR_BUFFER_BIT = 0x00004000;
        private const uint GL_SCISSOR_TEST = 0x00000C11;
        private const uint GL_VIEWPORT = 0x00000BA2;
        private const uint GL_PACK_ALIGNMENT = 0x00000D05;
        private const uint GL_UNSIGNED_BYTE = 0x00001401;
        private const uint GL_RGBA = 0x00001908;

        private delegate IntPtr EGLGetDisplayFn(IntPtr display_id);
        private delegate uint EGLInitializeFn(IntPtr dpy, ref int major, ref int minor);
        private delegate uint EGLChooseConfigFn(IntPtr dpy, int[] attrib_list, IntPtr[] configs, int config_size, ref int num_config);
        private delegate IntPtr EGLCreateContextFn(IntPtr dpy, IntPtr config, IntPtr share_context, int[] attrib_list);
        private delegate uint EGLDestroySurfaceFn(IntPtr dpy, IntPtr surface);
        private delegate IntPtr EGLCreateWindowSurfaceFn(IntPtr dpy, IntPtr config, IntPtr win, int[] attrib_list);
        private delegate uint EGLSurfaceAttribFn(IntPtr dpy, IntPtr surface, int attribute, int value);
        private delegate uint EGLMakeCurrentFn(IntPtr dpy, IntPtr draw, IntPtr read, IntPtr ctx);
        private delegate uint EGLSwapIntervalFn(IntPtr dpy, int interval);
        private delegate uint EGLSwapBuffersFn(IntPtr dpy, IntPtr surface);
        private delegate uint EGLDestroyContextFn(IntPtr dpy, IntPtr ctx);
        private delegate uint EGLTerminateFn(IntPtr dpy);
        private delegate int EGLGetErrorFn();
        private delegate void GLBindFramebufferFn(uint target, uint framebuffer);
        private delegate void GLViewportFn(int x, int y, int width, int height);
        private delegate void GLClearStencilFn(int s);
        private delegate void GLClearFn(uint s);
        private delegate void GLColorMaskFn(bool r, bool g, bool b, bool a);
        private delegate void GLClearColorFn(float r, float g, float b, float a);
        private delegate void GLDisableFn(uint cap);
        private delegate void GLGetIntegervFn(uint pname, int[] param);
        private delegate void GLPixelStoreiFn(uint pname, int param);
        private delegate void GLReadPixelsFn(int x, int y, int width, int height, uint format, uint type, byte[] pixels);

        private static EGLGetDisplayFn eglGetDisplay;
        private static EGLInitializeFn eglInitialize;
        private static EGLChooseConfigFn eglChooseConfig;
        private static EGLCreateContextFn eglCreateContext;
        private static EGLDestroySurfaceFn eglDestroySurface;
        private static EGLCreateWindowSurfaceFn eglCreateWindowSurface;
        private static EGLSurfaceAttribFn eglSurfaceAttrib;
        private static EGLMakeCurrentFn eglMakeCurrent;
        private static EGLSwapIntervalFn eglSwapInterval;
        private static EGLSwapBuffersFn eglSwapBuffers;
        private static EGLDestroyContextFn eglDestroyContext;
        private static EGLTerminateFn eglTerminate;
        private static EGLGetErrorFn eglGetError;
        private static GLBindFramebufferFn glBindFramebuffer;
        private static GLViewportFn glViewport;
        private static GLClearStencilFn glClearStencil;
        private static GLClearFn glClear;
        private static GLColorMaskFn glColorMask;
        private static GLClearColorFn glClearColor;
        private static GLDisableFn glDisable;
        private static GLGetIntegervFn glGetIntegerv;
        private static GLPixelStoreiFn glPixelStorei;
        private static GLReadPixelsFn glReadPixels;

        private static class AndroidImports
        {
            [DllImport("libEGL.so")]
            public static extern IntPtr eglGetDisplay(IntPtr display_id);

            [DllImport("libEGL.so")]
            public static extern uint eglInitialize(IntPtr dpy, ref int major, ref int minor);

            [DllImport("libEGL.so")]
            public static extern uint eglChooseConfig(IntPtr dpy, int[] attrib_list, IntPtr[] configs, int config_size, ref int num_config);

            [DllImport("libEGL.so")]
            public static extern IntPtr eglCreateContext(IntPtr dpy, IntPtr config, IntPtr share_context, int[] attrib_list);

            [DllImport("libEGL.so")]
            public static extern uint eglDestroySurface(IntPtr dpy, IntPtr surface);

            [DllImport("libEGL.so")]
            public static extern IntPtr eglCreateWindowSurface(IntPtr dpy, IntPtr config, IntPtr win, int[] attrib_list);

            [DllImport("libEGL.so")]
            public static extern uint eglSurfaceAttrib(IntPtr dpy, IntPtr surface, int attribute, int value);

            [DllImport("libEGL.so")]
            public static extern uint eglMakeCurrent(IntPtr dpy, IntPtr draw, IntPtr read, IntPtr ctx);

            [DllImport("libEGL.so")]
            public static extern uint eglSwapInterval(IntPtr dpy, int interval);

            [DllImport("libEGL.so")]
            public static extern uint eglSwapBuffers(IntPtr dpy, IntPtr surface);

            [DllImport("libEGL.so")]
            public static extern uint eglDestroyContext(IntPtr dpy, IntPtr ctx);

            [DllImport("libEGL.so")]
            public static extern uint eglTerminate(IntPtr dpy);

            [DllImport("libEGL.so")]
            public static extern int eglGetError();

            [DllImport("libGLESv2.so")]
            public static extern void glBindFramebuffer(uint target, uint framebuffer);

            [DllImport("libGLESv2.so")]
            public static extern void glViewport(int x, int y, int width, int height);

            [DllImport("libGLESv2.so")]
            public static extern void glClearStencil(int s);

            [DllImport("libGLESv2.so")]
            public static extern void glClear(uint s);

            [DllImport("libGLESv2.so")]
            public static extern void glColorMask(bool r, bool g, bool b, bool a);

            [DllImport("libGLESv2.so")]
            public static extern void glClearColor(float r, float g, float b, float a);

            [DllImport("libGLESv2.so")]
            public static extern void glDisable(uint cap);

            [DllImport("libGLESv2.so")]
            public static extern void glGetIntegerv(uint pname, int[] param);

            [DllImport("libGLESv2.so")]
            public static extern void glPixelStorei(uint pname, int param);

            [DllImport("libGLESv2.so")]
            public static extern void glReadPixels(int x, int y, int width, int height,
                uint format, uint type, byte[] pixels);
        }

        private static class LinuxImports
        {
            [DllImport("libEGL.so.1")]
            public static extern IntPtr eglGetDisplay(IntPtr display_id);

            [DllImport("libEGL.so.1")]
            public static extern uint eglInitialize(IntPtr dpy, ref int major, ref int minor);

            [DllImport("libEGL.so.1")]
            public static extern uint eglChooseConfig(IntPtr dpy, int[] attrib_list, IntPtr[] configs, int config_size, ref int num_config);

            [DllImport("libEGL.so.1")]
            public static extern IntPtr eglCreateContext(IntPtr dpy, IntPtr config, IntPtr share_context, int[] attrib_list);

            [DllImport("libEGL.so.1")]
            public static extern uint eglDestroySurface(IntPtr dpy, IntPtr surface);

            [DllImport("libEGL.so.1")]
            public static extern IntPtr eglCreateWindowSurface(IntPtr dpy, IntPtr config, IntPtr win, int[] attrib_list);

            [DllImport("libEGL.so.1")]
            public static extern uint eglSurfaceAttrib(IntPtr dpy, IntPtr surface, int attribute, int value);

            [DllImport("libEGL.so.1")]
            public static extern uint eglMakeCurrent(IntPtr dpy, IntPtr draw, IntPtr read, IntPtr ctx);

            [DllImport("libEGL.so.1")]
            public static extern uint eglSwapInterval(IntPtr dpy, int interval);

            [DllImport("libEGL.so.1")]
            public static extern uint eglSwapBuffers(IntPtr dpy, IntPtr surface);

            [DllImport("libEGL.so.1")]
            public static extern uint eglDestroyContext(IntPtr dpy, IntPtr ctx);

            [DllImport("libEGL.so.1")]
            public static extern uint eglTerminate(IntPtr dpy);

            [DllImport("libEGL.so.1")]
            public static extern int eglGetError();

            [DllImport("libGLESv2.so.2")]
            public static extern void glBindFramebuffer(uint target, uint framebuffer);

            [DllImport("libGLESv2.so.2")]
            public static extern void glViewport(int x, int y, int width, int height);

            [DllImport("libGLESv2.so.2")]
            public static extern void glClearStencil(int s);

            [DllImport("libGLESv2.so.2")]
            public static extern void glClear(uint s);

            [DllImport("libGLESv2.so.2")]
            public static extern void glColorMask(bool r, bool g, bool b, bool a);

            [DllImport("libGLESv2.so.2")]
            public static extern void glClearColor(float r, float g, float b, float a);

            [DllImport("libGLESv2.so.2")]
            public static extern void glDisable(uint cap);

            [DllImport("libGLESv2.so.2")]
            public static extern void glGetIntegerv(uint pname, int[] param);

            [DllImport("libGLESv2.so.2")]
            public static extern void glPixelStorei(uint pname, int param);

            [DllImport("libGLESv2.so.2")]
            public static extern void glReadPixels(int x, int y, int width, int height,
                uint format, uint type, byte[] pixels);
        }
        #endregion
    }
}
