using System;
using System.Runtime.InteropServices;
using Noesis;

namespace NoesisApp
{
    /// <summary>
    /// RenderContext implementation is in charge of the initialization of a rendering device.
    /// </summary>
    public abstract class RenderContext
    {
        /// <summary>
        /// Returns the rendering device maintained by this context.
        /// </summary>
        public abstract RenderDevice Device { get; }

        /// <summary>
        /// Initializes the rendering context with the given window and multisampling samples.
        /// </summary>
        public abstract void Init(IntPtr display, IntPtr window, uint samples, bool vsync, bool sRGB);

        /// <summary>
        /// Changes the window associated to this context.
        /// </summary>
        public abstract void SetWindow(IntPtr window);

        /// <summary>
        /// Called prior to rendering.
        /// </summary>
        public abstract void BeginRender();

        /// <summary>
        /// Called after the rendering.
        /// </summary>
        public abstract void EndRender();

        /// <summary>
        /// Binds the render targets associated with the window swap chain and clears it.
        /// </summary>
        public abstract void SetDefaultRenderTarget(int width, int height, bool doClearColor);

        /// <summary>
        /// Grabs an image with the content of current render target
        /// </summary>
        public abstract ImageCapture CaptureRenderTarget(RenderTarget surface);

        /// <summary>
        /// Copy the content of the back-buffer to the front buffer.
        /// </summary>
        public abstract void Swap();

        /// <summary>
        /// Should be called when the window is resized.
        /// </summary>
        public abstract void Resize();
    }

    /// <summary>
    /// Pixel data for A8R8G8B8 graphics image.
    /// </summary>
    public class ImageCapture
    {
        /// <summary>
        /// Gets image width.
        /// </summary>
        public uint Width { get; private set; }

        /// <summary>
        /// Gets image height.
        /// </summary>
        public uint Height { get; private set; }

        /// <summary>
        /// The number of bytes from one row of pixels to the next row of pixels.
        /// </summary>
        public uint Stride { get { return Width * 4; } }

        /// <summary>
        /// Returns array of internal bits of the image.
        /// </summary>
        public byte[] Pixels { get; private set; }

        public ImageCapture(uint w, uint h)
        {
            Width = w;
            Height = h;
            Pixels = new byte[w * h * 4];
        }
    }
}
