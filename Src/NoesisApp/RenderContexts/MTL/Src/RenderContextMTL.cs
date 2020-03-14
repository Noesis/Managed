using System;
using Noesis;
using CoreAnimation;
using CoreGraphics;
using Metal;
#if __IOS__
using UIKit;
#else
using AppKit;
#endif

namespace NoesisApp
{
    public class RenderContextMTL : RenderContext
    {
        public RenderContextMTL()
        {
            _dev = MTLDevice.SystemDefault;
        }

        public override RenderDevice Device
        {
            get { return _device; }
        }

        private IntPtr _display;
        private IntPtr _window;
        private RenderDeviceMTL _device;
        private IMTLDevice _dev;
        private CAMetalLayer _layer;
        private ICAMetalDrawable _drawable;
        private IMTLCommandQueue _cmdQueue;
        private IMTLCommandBuffer _cmdBuffer;
        private IMTLCommandEncoder _cmdEncoder;
        private MTLRenderPassDescriptor _descriptor;
        private uint _samples;

        public override void Init(IntPtr display, IntPtr window, uint samples, bool vsync, bool sRGB)
        {
            _display = display;
            _window = window;

            while (!_dev.SupportsTextureSampleCount(samples))
            {
                samples >>= 1;
            }
            _samples = samples;

            _layer = new CAMetalLayer();
            _layer.Device = _dev;
            _layer.PixelFormat = sRGB ? MTLPixelFormat.BGRA8Unorm_sRGB : MTLPixelFormat.BGRA8Unorm;
            _layer.FramebufferOnly = true;

#if __IOS__
            UIView view = (UIView)ObjCRuntime.Runtime.GetNSObject(window);
            view.Layer.AddSublayer(_layer);
#else
            // Mac
            NSView view = (NSView)ObjCRuntime.Runtime.GetNSObject(window);
            view.Layer.AddSublayer(_layer);
#endif
            _cmdQueue = _dev.CreateCommandQueue();

            _descriptor = new MTLRenderPassDescriptor();
            _descriptor.ColorAttachments[0].ClearColor = new MTLClearColor(0.0, 1.0, 0.0, 0.0);

            if (samples > 1)
            {
                _descriptor.ColorAttachments[0].StoreAction = MTLStoreAction.MultisampleResolve;
            }
            else
            {
                _descriptor.ColorAttachments[0].StoreAction = MTLStoreAction.Store;
            }

            _descriptor.StencilAttachment.ClearStencil = 0;
            _descriptor.StencilAttachment.LoadAction = MTLLoadAction.Clear;
            _descriptor.StencilAttachment.StoreAction = MTLStoreAction.DontCare;

            _device = new Noesis.RenderDeviceMTL(_dev.Handle, sRGB);
        }

        public override void SetWindow(IntPtr window) { }

        public override void BeginRender()
        {
            _cmdBuffer = _cmdQueue.CommandBuffer();

            _device.SetOffscreenCommandBuffer(_cmdBuffer.Handle);
        }

        public override void EndRender()
        {
            _cmdEncoder.EndEncoding();
        }

        public override void SetDefaultRenderTarget(int width, int height, bool doClearColor)
        {
            _drawable = _layer.NextDrawable();

            MTLRenderPassColorAttachmentDescriptor attachment = _descriptor.ColorAttachments[0];
            attachment.LoadAction = doClearColor ? MTLLoadAction.Clear : MTLLoadAction.DontCare;

            if (attachment.StoreAction == MTLStoreAction.MultisampleResolve)
            {
                attachment.ResolveTexture = _drawable.Texture;
            }
            else
            {
                attachment.Texture = _drawable.Texture;
            }

            _cmdEncoder = _cmdBuffer.CreateRenderCommandEncoder(_descriptor);

            _device.SetOnScreenEncoder(_cmdEncoder.Handle, (uint)_layer.PixelFormat, (uint)MTLPixelFormat.Stencil8, _samples);
        }

        public override ImageCapture CaptureRenderTarget(RenderTarget surface)
        {
            throw new NotImplementedException();
        }

        public override void Swap()
        {
            _cmdBuffer.PresentDrawable(_drawable);
            _cmdBuffer.Commit();
        }

        public override void Resize()
        {
#if __IOS__
            UIView view = (UIView)ObjCRuntime.Runtime.GetNSObject(_window);
            nfloat scale = UIScreen.MainScreen.Scale;
            nfloat width = view.Bounds.Size.Width * scale;
            nfloat height = view.Bounds.Size.Height * scale;
            _layer.Frame = view.Layer.Frame;
#else
            NSView view = (NSView)ObjCRuntime.Runtime.GetNSObject(_window);
            nfloat width = view.Bounds.Size.Width;
            nfloat height = view.Bounds.Size.Height;
#endif
            _layer.DrawableSize = new CGSize(width, height);

            MTLTextureDescriptor descriptor = new MTLTextureDescriptor();
            if (_samples > 1)
            {
                descriptor.TextureType = MTLTextureType.k2DMultisample;
                descriptor.PixelFormat = _layer.PixelFormat;
                descriptor.Width = (uint)width;
                descriptor.Height = (uint)height;
                descriptor.SampleCount = _samples;
                IMTLTexture colorAA = _dev.CreateTexture(descriptor);
                _descriptor.ColorAttachments[0].Texture = colorAA;
            }

            descriptor.TextureType = _samples > 1 ? MTLTextureType.k2DMultisample : MTLTextureType.k2D;
            descriptor.PixelFormat = MTLPixelFormat.Stencil8;
            descriptor.Width = (uint)width;
            descriptor.Height = (uint)height;
            descriptor.SampleCount = _samples;

            IMTLTexture stencil = _dev.CreateTexture(descriptor);
            _descriptor.StencilAttachment.Texture = stencil;
        }

    }
}
