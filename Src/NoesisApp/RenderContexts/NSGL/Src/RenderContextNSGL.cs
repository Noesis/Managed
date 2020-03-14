using System;
using System.Runtime.InteropServices;
using Noesis;
using CoreAnimation;
using AppKit;

namespace NoesisApp
{
    public class RenderContextNSGL : RenderContext
    {
        static RenderContextNSGL()
        {
        }

        ~RenderContextNSGL()
        {
            NSOpenGLContext.ClearCurrentContext();
        }

        public override RenderDevice Device
        {
            get { return _device; }
        }

        private IntPtr _display;
        private IntPtr _window;
        private RenderDeviceGL _device;
        private NSOpenGLContext _context;

        public override void Init(IntPtr display, IntPtr window, uint samples, bool vsync, bool sRGB)
        {
            _display = display;
            _window = window;

            NSOpenGLPixelFormatAttribute[] attrs =
            {
                NSOpenGLPixelFormatAttribute.OpenGLProfile, (NSOpenGLPixelFormatAttribute)NSOpenGLProfile.Version3_2Core,
                NSOpenGLPixelFormatAttribute.DoubleBuffer,
                NSOpenGLPixelFormatAttribute.Accelerated,
                NSOpenGLPixelFormatAttribute.ColorSize, (NSOpenGLPixelFormatAttribute)24,
                NSOpenGLPixelFormatAttribute.Multisample,
                NSOpenGLPixelFormatAttribute.SampleBuffers, (NSOpenGLPixelFormatAttribute)(samples > 1 ? 1 : 0),
                NSOpenGLPixelFormatAttribute.Samples, (NSOpenGLPixelFormatAttribute)samples,
                NSOpenGLPixelFormatAttribute.AlphaSize, (NSOpenGLPixelFormatAttribute)0,
                NSOpenGLPixelFormatAttribute.DepthSize, (NSOpenGLPixelFormatAttribute)0,
                NSOpenGLPixelFormatAttribute.StencilSize, (NSOpenGLPixelFormatAttribute)0,
                (NSOpenGLPixelFormatAttribute)0
            };

            NSOpenGLPixelFormat pixelFormat = new NSOpenGLPixelFormat(attrs);
            if (pixelFormat == null)
            {
                attrs[1] = (NSOpenGLPixelFormatAttribute)NSOpenGLProfile.VersionLegacy;
                pixelFormat = new NSOpenGLPixelFormat(attrs);
            }

            _context = new NSOpenGLContext(pixelFormat, null);
            _context.SwapInterval = vsync ? true : false;
            NSView view = (NSView)ObjCRuntime.Runtime.GetNSObject(window);
            CALayer layer = new CALayer();
            view.Layer = layer;

            _context.View = view;
            _context.MakeCurrentContext();

            _device = new RenderDeviceGL();
        }

        public override void SetWindow(IntPtr window) { }

        public override void BeginRender()
        {
        }

        public override void EndRender()
        {
        }

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
            throw new NotImplementedException();
        }

        public override void Swap()
        {
            _context.FlushBuffer();
        }

        public override void Resize()
        {
            _context.Update();
        }

        private const uint GL_FRAMEBUFFER           = 0x8D40;
        private const uint GL_STENCIL_BUFFER_BIT    = 0x00000400;
        private const uint GL_COLOR_BUFFER_BIT      = 0x00004000;

        private const string LibraryName = "/System/Library/Frameworks/OpenGL.framework/OpenGL";

        [DllImport(LibraryName)]
        static extern void glBindFramebuffer(uint target, uint framebuffer);

        [DllImport(LibraryName)]
        static extern void glViewport(int x, int y, int width, int height);

        [DllImport(LibraryName)]
        static extern void glClearStencil(int s);

        [DllImport(LibraryName)]
        static extern void glClear(uint s);

        [DllImport(LibraryName)]
        static extern void glColorMask(bool r, bool g, bool b, bool a);

        [DllImport(LibraryName)]
        static extern void glClearColor(float r, float g, float b, float a);
    }
}
