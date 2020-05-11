using System;
using System.Runtime.InteropServices;

namespace Noesis
{
    public enum TextureFormat
    {
        RGBA8,
        R8
    }

    [StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct Tile
    {
        [MarshalAs(UnmanagedType.U4)]
        public uint X;
        [MarshalAs(UnmanagedType.U4)]
        public uint Y;
        [MarshalAs(UnmanagedType.U4)]
        public uint Width;
        [MarshalAs(UnmanagedType.U4)]
        public uint Height;
    }

    /// <summary>
    /// Abstraction of a graphics rendering device.
    /// </summary>
    public class RenderDevice
    {
        /// <summary>
        /// Width of offscreen textures (0 = automatic). Default is automatic.
        /// </summary>
        public uint OffscreenWidth
        {
            get { return Noesis_RenderDevice_GetOffscreenWidth(CPtr); }
            set { Noesis_RenderDevice_SetOffscreenWidth(CPtr, value); }
        }

        /// <summary>
        /// Height of offscreen textures (0 = automatic). Default is automatic.
        /// </summary>
        public uint OffscreenHeight
        {
            get { return Noesis_RenderDevice_GetOffscreenHeight(CPtr); }
            set { Noesis_RenderDevice_SetOffscreenHeight(CPtr, value); }
        }

        /// <summary>
        /// Multisampling of offscreen textures. Default is 1x.
        /// </summary>
        public uint OffscreenSampleCount
        {
            get { return Noesis_RenderDevice_GetOffscreenSampleCount(CPtr); }
            set { Noesis_RenderDevice_SetOffscreenSampleCount(CPtr, value); }
        }

        /// <summary>
        /// Number of offscreen textures created at startup. Default is 0.
        /// </summary>
        public uint OffscreenDefaultNumSurfaces
        {
            get { return Noesis_RenderDevice_GetOffscreenDefaultNumSurfaces(CPtr); }
            set { Noesis_RenderDevice_SetOffscreenDefaultNumSurfaces(CPtr, value); }
        }

        /// <summary>
        /// Maximum number of offscreen textures (0 = unlimited). Default is unlimited.
        /// </summary>
        public uint OffscreenMaxNumSurfaces
        {
            get { return Noesis_RenderDevice_GetOffscreenMaxNumSurfaces(CPtr); }
            set { Noesis_RenderDevice_SetOffscreenMaxNumSurfaces(CPtr, value); }
        }

        /// <summary>
        /// Width of texture used to cache glyphs. Default is 1024.
        /// </summary>
        public uint GlyphCacheWidth
        {
            get { return Noesis_RenderDevice_GetGlyphCacheWidth(CPtr); }
            set { Noesis_RenderDevice_SetGlyphCacheWidth(CPtr, value); }
        }

        /// <summary>
        /// Height of texture used to cache glyphs. Default is 1024.
        /// </summary>
        public uint GlyphCacheHeight
        {
            get { return Noesis_RenderDevice_GetGlyphCacheHeight(CPtr); }
            set { Noesis_RenderDevice_SetGlyphCacheHeight(CPtr, value); }
        }

        /// <summary>
        /// Creates a dynamic texture with given dimensions and format. The content will be updated
        /// using UpdateTexture().
        /// </summary>
        public Texture CreateTexture(string label, uint width, uint height, uint numLevels,
            TextureFormat format)
        {
            IntPtr texture = Noesis_RenderDevice_CreateTexture(CPtr, label,
                width, height, numLevels, (int)format);

            return new Texture(texture, true);
        }

        /// <summary>
        /// Updates texture mipmap copying the given data to desired position. The passed data is
        /// tightly packed (no extra pitch). Origin is located at the left of the first scanline
        /// </summary>
        public void UpdateTexture(Texture texture, uint level, uint x, uint y,
            uint width, uint height, byte[] data)
        {
            if (texture == null)
            {
                throw new ArgumentNullException("texture");
            }
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            Noesis_RenderDevice_UpdateTexture(CPtr, BaseComponent.getCPtr(texture),
                level, x, y, width, height, data);
        }

        /// <summary>
        /// Clears the given region to transparent (#000000) and sets the scissor rectangle to fit it.
        /// Until next call to EndTile() all rendering commands will only update the extents of the tile.
        /// </summary>
        public void BeginTile(Tile tile, uint surfaceWidth, uint surfaceHeight)
        {
            Noesis_RenderDevice_BeginTile(CPtr, ref tile, surfaceWidth, surfaceHeight);
        }

        /// <summary>
        /// Completes rendering to the tile specified by BeginTile.
        /// </summary>
        public void EndTile()
        {
            Noesis_RenderDevice_EndTile(CPtr);
        }

        /// <summary>
        /// Creates render target surface with given dimensions and number of samples
        /// </summary>
        public RenderTarget CreateRenderTarget(string label, uint width, uint height, uint sampleCount)
        {
            IntPtr renderTarget = Noesis_RenderDevice_CreateRenderTarget(CPtr, label,
                width, height, sampleCount);

            return new RenderTarget(renderTarget, true);
        }

        /// <summary>
        /// Binds render target and sets viewport to cover the entire surface.
        /// </summary>
        public void SetRenderTarget(RenderTarget surface)
        {
            if (surface == null)
            {
                throw new ArgumentNullException("surface");
            }

            Noesis_RenderDevice_SetRenderTarget(CPtr, surface.CPtr);
        }

        /// <summary>
        /// Resolves multisample render target.
        /// </summary>
        public void ResolveRenderTarget(RenderTarget surface, Tile[] tiles)
        {
            if (surface == null)
            {
                throw new ArgumentNullException("surface");
            }

            Noesis_RenderDevice_ResolveRenderTarget(CPtr, surface.CPtr, tiles, tiles.Length);
        }

        #region Private members
        public RenderDevice(IntPtr cPtr, bool memoryOwn)
        {
            _renderDevice = new BaseComponent(cPtr, memoryOwn);
        }

        internal HandleRef CPtr { get { return BaseComponent.getCPtr(_renderDevice); } }

        private BaseComponent _renderDevice;
        #endregion

        #region Imports
        [DllImport(Library.Name)]
        static extern uint Noesis_RenderDevice_GetOffscreenWidth(HandleRef device);

        [DllImport(Library.Name)]
        static extern void Noesis_RenderDevice_SetOffscreenWidth(HandleRef device, uint w);

        [DllImport(Library.Name)]
        static extern uint Noesis_RenderDevice_GetOffscreenHeight(HandleRef device);

        [DllImport(Library.Name)]
        static extern void Noesis_RenderDevice_SetOffscreenHeight(HandleRef device, uint h);

        [DllImport(Library.Name)]
        static extern uint Noesis_RenderDevice_GetOffscreenSampleCount(HandleRef device);

        [DllImport(Library.Name)]
        static extern void Noesis_RenderDevice_SetOffscreenSampleCount(HandleRef device, uint c);

        [DllImport(Library.Name)]
        static extern uint Noesis_RenderDevice_GetOffscreenDefaultNumSurfaces(HandleRef device);

        [DllImport(Library.Name)]
        static extern void Noesis_RenderDevice_SetOffscreenDefaultNumSurfaces(HandleRef device,
            uint n);

        [DllImport(Library.Name)]
        static extern uint Noesis_RenderDevice_GetOffscreenMaxNumSurfaces(HandleRef device);

        [DllImport(Library.Name)]
        static extern void Noesis_RenderDevice_SetOffscreenMaxNumSurfaces(HandleRef device, uint n);

        [DllImport(Library.Name)]
        static extern uint Noesis_RenderDevice_GetGlyphCacheWidth(HandleRef device);

        [DllImport(Library.Name)]
        static extern void Noesis_RenderDevice_SetGlyphCacheWidth(HandleRef device, uint w);

        [DllImport(Library.Name)]
        static extern uint Noesis_RenderDevice_GetGlyphCacheHeight(HandleRef device);

        [DllImport(Library.Name)]
        static extern void Noesis_RenderDevice_SetGlyphCacheHeight(HandleRef device, uint w);

        [DllImport(Library.Name)]
        static extern IntPtr Noesis_RenderDevice_CreateTexture(HandleRef device,
            [MarshalAs(UnmanagedType.LPWStr)]string label, uint width, uint height, uint numLevels,
            int format);

        [DllImport(Library.Name)]
        public static extern void Noesis_RenderDevice_UpdateTexture(HandleRef device, HandleRef texture,
            uint level, uint x, uint y, uint width, uint height, byte[] data);

        [DllImport(Library.Name)]
        static extern void Noesis_RenderDevice_BeginTile(HandleRef device, ref Tile tile,
            uint width, uint height);

        [DllImport(Library.Name)]
        static extern void Noesis_RenderDevice_EndTile(HandleRef device);

        [DllImport(Library.Name)]
        static extern IntPtr Noesis_RenderDevice_CreateRenderTarget(HandleRef device,
            [MarshalAs(UnmanagedType.LPWStr)]string label, uint width, uint height, uint sampleCount);

        [DllImport(Library.Name)]
        static extern void Noesis_RenderDevice_SetRenderTarget(HandleRef device, HandleRef surface);

        [DllImport(Library.Name)]
        static extern void Noesis_RenderDevice_ResolveRenderTarget(HandleRef device, HandleRef surface,
            [In, Out] Tile[] tiles, int numTiles);
        #endregion
    }

    /// <summary>
    ///  Creates an OpenGL RenderDevice.
    /// </summary>
    public class RenderDeviceGL : RenderDevice
    {
        public RenderDeviceGL() :
            base(Noesis_RenderDevice_CreateGL(), true)
        {
        }

        #region Imports
        [DllImport(Library.Name)]
        static extern IntPtr Noesis_RenderDevice_CreateGL();
        #endregion
    }

    /// <summary>
    ///  Creates a D3D11 RenderDevice.
    /// </summary>
    public class RenderDeviceD3D11 : RenderDevice
    {
        public RenderDeviceD3D11(IntPtr deviceContext) : this(deviceContext, false)
        {
        }

        public RenderDeviceD3D11(IntPtr deviceContext, bool sRGB) :
            base(Noesis_RenderDevice_CreateD3D11(deviceContext, sRGB), true)
        {
        }

        #region Imports
        [DllImport(Library.Name)]
        static extern IntPtr Noesis_RenderDevice_CreateD3D11(IntPtr deviceContext, bool sRGB);
        #endregion
    }

    /// <summary>
    ///  Creates a Metal RenderDevice.
    /// </summary>
    public class RenderDeviceMTL : RenderDevice
    {
        public RenderDeviceMTL(IntPtr device, bool sRGB) :
            base(Noesis_RenderDevice_CreateMTL(device, sRGB), true)
        {
        }

        public void SetOffscreenCommandBuffer(IntPtr commandBuffer)
        {
            Noesis_RenderDeviceMTL_SetOffScreenCommandBuffer(CPtr, commandBuffer);
        }

        public void SetOnScreenEncoder(IntPtr encoder, uint colorFormat, uint stencilFormat, uint sampleCount)
        {
            Noesis_RenderDeviceMTL_SetOnScreenEncoder(CPtr, encoder, colorFormat, stencilFormat, sampleCount);
        }

        #region Imports
        [DllImport(Library.Name)]
        static extern IntPtr Noesis_RenderDevice_CreateMTL(IntPtr device, bool sRGB);

        [DllImport(Library.Name)]
        static extern IntPtr Noesis_RenderDeviceMTL_SetOffScreenCommandBuffer(HandleRef renderDevice, IntPtr commandBuffer);

        [DllImport(Library.Name)]
        static extern IntPtr Noesis_RenderDeviceMTL_SetOnScreenEncoder(HandleRef renderDevice, IntPtr encoder, uint colorFormat, uint stencilFormat, uint sampleCount);
        #endregion
    }
}
