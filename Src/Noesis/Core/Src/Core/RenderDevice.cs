using System;
using System.Runtime.InteropServices;

namespace Noesis
{
    public enum TextureFormat
    {
        /// <summary>A four-component format that supports 8 bits per channel including alpha</summary>
        RGBA8,
        /// <summary>A four-component format that supports 8 bits for each color channel and 8 bits unused</summary>
        RGBX8,
        /// <summary>A single-component format that supports 8 bits for the red channel</summary>
        R8
    }

    /// <summary>
    // Render device capabilities.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct DeviceCaps
    {
        [MarshalAs(UnmanagedType.R4)]
        public float CenterPixelOffset;
        [MarshalAs(UnmanagedType.U1)]
        public bool LinearRendering;
        [MarshalAs(UnmanagedType.U1)]
        public bool SubpixelRendering;
    }

    /// <summary>
    /// A tile is a region of a render target. The origin is located at the lower left corner.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
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
    /// Shader effect.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct Shader
    {
        public int Index { get { return this.v; } }
        public string Name { get { return ((Enum)this.v).ToString(); } }

        /// <summary>
        /// This is the high-level shader description that gets sent in each draw call.
        /// Implementations must map this to corresponding low-level shaders and layouts
        /// </summary>
        public enum Enum
        {
            /// <summary>Shader for debug modes</summary>
            RGBA,

            /// <summary>Stencil only with null pixel shader</summary>
            Mask,

            /// <summary>Shader used for clearing render target</summary>
            Clear,

            /// <summary>Shaders for rendering geometry</summary>
            Path_Solid,
            Path_Linear,
            Path_Radial,
            Path_Pattern,
            Path_Pattern_Clamp,
            Path_Pattern_Repeat,
            Path_Pattern_MirrorU,
            Path_Pattern_MirrorV,
            Path_Pattern_Mirror,

            /// <summary>Shaders for rendering geometry with PPAA</summary>
            Path_AA_Solid,
            Path_AA_Linear,
            Path_AA_Radial,
            Path_AA_Pattern,
            Path_AA_Pattern_Clamp,
            Path_AA_Pattern_Repeat,
            Path_AA_Pattern_MirrorU,
            Path_AA_Pattern_MirrorV,
            Path_AA_Pattern_Mirror,

            /// <summary>Shaders for rendering distance fields</summary>
            SDF_Solid,
            SDF_Linear,
            SDF_Radial,
            SDF_Pattern,
            SDF_Pattern_Clamp,
            SDF_Pattern_Repeat,
            SDF_Pattern_MirrorU,
            SDF_Pattern_MirrorV,
            SDF_Pattern_Mirror,

            /// <summary>
            /// Shaders for rendering distance fields with subpixel rendering.
            /// These shaders are only used when the device reports support for
            /// subpixel rendering in DeviceCaps. Otherwise SDF_* shaders are used
            /// </summary>
            SDF_LCD_Solid,
            SDF_LCD_Linear,
            SDF_LCD_Radial,
            SDF_LCD_Pattern,
            SDF_LCD_Pattern_Clamp,
            SDF_LCD_Pattern_Repeat,
            SDF_LCD_Pattern_MirrorU,
            SDF_LCD_Pattern_MirrorV,
            SDF_LCD_Pattern_Mirror,

            /// <summary>Shaders for offscreen rendering</summary>
            Opacity_Solid,
            Opacity_Linear,
            Opacity_Radial,
            Opacity_Pattern,
            Opacity_Pattern_Clamp,
            Opacity_Pattern_Repeat,
            Opacity_Pattern_MirrorU,
            Opacity_Pattern_MirrorV,
            Opacity_Pattern_Mirror,

            /// <summary>Gaussian upsampling/downsampling step</summary>
            Upsample,
            Downsample,

            /// <summary>Effects rendering</summary>
            Shadow,
            Blur,
            Custom_Effect,

            Count
        }

        private readonly byte v;

        /// <summary>
        /// Vertex shaders for each Noesis.Shader.Enum (see table VertexForShader[])
        /// </summary>
        public struct Vertex
        {
            public enum Enum
            {
                Pos,
                PosColor,
                PosTex0,
                PosTex0Rect,
                PosTex0RectTile,
                PosColorCoverage,
                PosTex0Coverage,
                PosTex0CoverageRect,
                PosTex0CoverageRectTile,
                PosColorTex1_SDF,
                PosTex0Tex1_SDF,
                PosTex0Tex1Rect_SDF,
                PosTex0Tex1RectTile_SDF,
                PosColorTex1,
                PosTex0Tex1,
                PosTex0Tex1Rect,
                PosTex0Tex1RectTile,
                PosColorTex0Tex1,
                PosTex0Tex1_Downsample,
                PosColorTex1Rect,
                PosColorTex0RectImagePos,

                Count
            }

            /// <summary>
            /// Vertex Formats for each vertex shader (see table FormatForVertex[])
            /// </summary>
            public struct Format
            {
                public enum Enum
                {
                    Pos,
                    PosColor,
                    PosTex0,
                    PosTex0Rect,
                    PosTex0RectTile,
                    PosColorCoverage,
                    PosTex0Coverage,
                    PosTex0CoverageRect,
                    PosTex0CoverageRectTile,
                    PosColorTex1,
                    PosTex0Tex1,
                    PosTex0Tex1Rect,
                    PosTex0Tex1RectTile,
                    PosColorTex0Tex1,
                    PosColorTex1Rect,
                    PosColorTex0RectImagePos,

                    Count
                }

                /// <summary>
                /// Attributes for each vertex format (see tables AttributesForFormat[] and SizeForFormat[])
                /// </summary>
                public struct Attr
                {
                    [Flags]
                    public enum Enum
                    {
                    //  --------------------------------------------------------------------------------
                    //  Attr                    Interpolation       Semantic
                    //  --------------------------------------------------------------------------------
                        Pos         = 1,    //  linear              Position (xy)
                        Color       = 2,    //  nointerpolation     sRGB Color (rgba)
                        Tex0        = 4,    //  linear              TexCoord0 (uv)
                        Tex1        = 8,    //  linear              TexCoord1 (uv)
                        Coverage    = 16,   //  linear              Coverage (alpha)
                        Rect        = 32,   //  nointerpolation     Rect (x0, y0, x1, y1)
                        Tile        = 64,   //  nointerpolation     Rect (x, y, width, height)
                        ImagePos    = 128,  //  linear              Position (xy) - Scale(zw)
                    }

                    /// <summary>
                    /// Types for each vertex format attribute (see tables TypeForAttr[] and SizeForType[])
                    /// </summary>
                    public struct Type
                    {
                        public enum Enum
                        {
                            /// <summary>One 32-bit floating-point component</summary>
                            Float,
                            /// <summary>Two 32-bit floating-point components</summary>
                            Float2,
                            /// <summary>Four 32-bit floating-point components</summary>
                            Float4,
                            /// <summary>Four 8-bit normalized unsigned integer components</summary>
                            UByte4Norm,
                            /// <summary>Four 16-bit normalized unsigned integer components</summary>
                            UShort4Norm,

                            Count
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Table for getting the vertex shader corresponding to each Noesis.Shader.Enum
        /// Shader.Enum => Shader.Vertex.Enum</summary>
        public static Shader.Vertex.Enum VertexForShader(Shader.Enum shader)
        {
            int[] map = new int[(int)Shader.Enum.Count]
            {
                0, 0, 0, 1, 2, 2, 2, 3, 4, 4, 4, 4, 5, 6, 6, 6, 7, 8, 8, 8, 8, 9, 10, 10, 10, 11, 12, 12, 12,
                12, 9, 10, 10, 10, 11, 12, 12, 12, 12, 13, 14, 14, 14, 15, 16, 16, 16, 16, 17, 18, 19, 13, 20
            };

            return (Shader.Vertex.Enum)map[(int)shader];
        }

        /// <summary>
        /// Table for getting the vertex format corresponding to each vertex shader
        /// Shader.Vertex.Enum => Shader.Vertex.Format.Enum
        /// </summary>
        public static Shader.Vertex.Format.Enum FormatForVertex(Shader.Vertex.Enum vertex)
        {
            int[] map = new int[(int)Shader.Vertex.Enum.Count]
            {
                0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 9, 10, 11, 12, 13, 10, 14, 15
            };

            return (Shader.Vertex.Format.Enum)map[(int)vertex];
        }

        /// <summary>Size in bytes for each vertex format</summary>
        public static int SizeForFormat(Shader.Vertex.Format.Enum format)
        {
            int[] map = new int[(int)Shader.Vertex.Format.Enum.Count]
            {
                8, 12, 16, 24, 40, 16, 20, 28, 44, 20, 24, 32, 48, 28, 28, 44
            };

            return map[(int)format];
        }

        /// <summary>Table for getting the attribute bitmask corresponding to each vertex format</summary>
        public static Shader.Vertex.Format.Attr.Enum AttributesForFormat(Shader.Vertex.Format.Enum format)
        {
            int[] map = new int[(int)Shader.Vertex.Format.Enum.Count]
            {
                1, 3, 5, 37, 101, 19, 21, 53, 117, 11, 13, 45, 109, 15, 43, 167
            };

            return (Shader.Vertex.Format.Attr.Enum)map[(int)format];
        }

        /// <summary>
        /// Table for getting the type corresponding to each vertex format attribute
        /// Shader.Vertex.Format.Attr.Enum => Shader.Vertex.Format.Attr.Type.Enum
        /// </summary>
        public static Shader.Vertex.Format.Attr.Type.Enum TypeForAttr(Shader.Vertex.Format.Attr.Enum attr)
        {
            int[] map = new int[]
            {
                1, 3, 1, 1, 0, 4, 2, 2
            };

            return (Shader.Vertex.Format.Attr.Type.Enum)map[(int)attr];
        }

        /// <summary>
        /// Table for getting the size in bytes corresponding to each vertex format attribute type
        /// </summary>
        public static int SizeForType(Shader.Vertex.Format.Attr.Type.Enum type)
        {
            int[] map = new int[(int)Shader.Vertex.Format.Attr.Type.Enum.Count]
            {
                4, 8, 16, 4, 8
            };

            return map[(int)type];
        }
    }

    /// <summary>
    /// Alpha blending mode.
    /// </summary>
    public enum BlendMode
    {
                                //  ----------------------------------------------------
                                //  COLOR                       ALPHA
                                //  ----------------------------------------------------
        Src,                    //  cs                          as
        SrcOver,                //  cs + cd * (1 - as)          as + ad * (1 - as)
        SrcOver_Multiply,       //  cs * cd + cd * (1 - as)     as + ad * (1 - as)
        SrcOver_Screen,         //  cs + cd * (1 - cs)          as + ad * (1 - as)
        SrcOver_Additive,       //  cs + cs                     as + ad * (1 - as)
        SrcOver_Dual            //  cs + cd * (1 - as) [RGB]    as + ad * (1 - as) [RGB]
                                //  ----------------------------------------------------
    }

    /// <summary>
    /// Stencil buffer mode.
    /// </summary>
    public enum StencilMode
    {
        /// <summary>Stencil testing disabled</summary>
        Disabled,
        /// <summary>Stencil testing enabled</summary>
        Equal_Keep,
        /// <summary>Stencil enabled and incremented</summary>
        Equal_Incr,
        /// <summary>Stencil enabled and decremented</summary>
        Equal_Decr,
        /// <summary>Set the stencil data to 0</summary>
        Clear,
    }

    /// <summary>
    /// Render state.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct RenderState
    {
        public bool ColorEnable { get { return (this.v & 1 << 0) != 0; } }
        public BlendMode BlendMode { get { return (BlendMode)((this.v >> 1) & 7); } }
        public StencilMode StencilMode { get { return (StencilMode)((this.v >> 4) & 7); } }
        public bool Wireframe { get { return (this.v & 1 << 7) != 0; } }

        public override bool Equals(object obj) { return obj is RenderState && v == ((RenderState)obj).v; }
        public override int GetHashCode() { return 238427441 + v.GetHashCode(); }

        private readonly byte v;
    }

    /// <summary>
    /// Texture wrapping mode.
    /// </summary>
    public enum WrapMode
    {
        /// <summary>Clamp between 0.0 and 1.0</summary>
        ClampToEdge,
        /// <summary>Out of range texture coordinates return transparent zero (0,0,0,0)</summary>
        ClampToZero,
        /// <summary>Wrap to the other side of the texture</summary>
        Repeat,
        /// <summary>The same as repeat but flipping horizontally</summary>
        MirrorU,
        /// <summary>The same as repeat but flipping vertically</summary>
        MirrorV,
        /// <summary>The combination of MirrorU and MirrorV</summary>
        Mirror
    }

    /// <summary>
    /// Texture minification and magnification filter
    /// </summary>
    public enum MinMagFilter
    {
        /// <summary>Select the single pixel nearest to the sample point</summary>
        Nearest,
        /// <summary>Select two pixels in each dimension and interpolate linearly between them</summary>
        Linear
    }

    /// <summary>
    /// Texture Mipmap filter
    /// </summary>
    public enum MipFilter
    {
        /// <summary>Texture sampled from mipmap level 0</summary>
        Disabled,
        /// <summary>The nearest mipmap level is selected</summary>
        Nearest,
        /// <summary>Both nearest levels are sampled and linearly interpolated</summary>
        Linear
    }

    /// <summary>
    /// Texture sampler state.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct SamplerState
    {
        public WrapMode WrapMode { get { return (WrapMode)((this.v >> 0) & 7); } }
        public MinMagFilter MinMagFilter { get { return (MinMagFilter)((this.v >> 3) & 1); } }
        public MipFilter MipFilter { get { return (MipFilter)((this.v >> 4) & 3); } }

        public override bool Equals(object obj) { return obj is SamplerState && v == ((SamplerState)obj).v; }
        public override int GetHashCode() { return 238427441 + v.GetHashCode(); }

        private readonly byte v;
    }

    /// <summary>
    /// Uniform shader values. For unused buffers, 'Values' is set to IntPtr.Zero and 'NumDwords' is zero.
    /// A hash for the content is given to avoid unnecessary constant buffer updates.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct UniformData
    {
        public IntPtr Values;
        public uint NumWords;
        public uint Hash;
    }

    /// <summary>
    /// Render batch information.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct Batch
    {
        /// <summary>Render state</summary>
        public readonly Shader Shader;
        public readonly RenderState RenderState;
        public readonly byte StencilRef;

        /// <summary>Draw parameters</summary>
        public readonly uint VertexOffset;
        public readonly uint NumVertices;
        public readonly uint StartIndex;
        public readonly uint NumIndices;

        /// <summary>Textures (unused textures are set to null)</summary>
        private readonly IntPtr pattern;
        private readonly IntPtr ramps;
        private readonly IntPtr image;
        private readonly IntPtr glyphs;
        private readonly IntPtr shadow;

        public Texture Pattern { get { return (Texture)Extend.GetProxy(pattern, false); } }
        public Texture Ramps { get { return (Texture)Extend.GetProxy(ramps, false); } }
        public Texture Image { get { return (Texture)Extend.GetProxy(image, false); } }
        public Texture Glyphs { get { return (Texture)Extend.GetProxy(glyphs, false); } }
        public Texture Shadow { get { return (Texture)Extend.GetProxy(shadow, false); } }

        /// <summary>Sampler states corresponding to each texture</summary>
        public readonly SamplerState PatternSampler;
        public readonly SamplerState RampsSampler;
        public readonly SamplerState ImageSampler;
        public readonly SamplerState GlyphsSampler;
        public readonly SamplerState ShadowSampler;

        /// <summary>Vertex and pixel shader uniform buffers</summary>
        public readonly UniformData VertexUniform0;
        public readonly UniformData VertexUniform1;
        public readonly UniformData PixelUniform0;
        public readonly UniformData PixelUniform1;

        /// <summary>Custom data set in ShaderEffect.SetPixelShader() or BrushShader.SetPixelShader()</summary>
        public readonly IntPtr PixelShader;
    };

    /// <summary>
    /// Abstraction of a graphics rendering device.
    /// </summary>
    public abstract class RenderDevice : BaseComponent
    {
        #region Configuration properties
        /// <summary>
        /// Width of offscreen textures (0 = automatic). Default is automatic.
        /// </summary>
        public uint OffscreenWidth
        {
            get { return Noesis_RenderDevice_GetOffscreenWidth(swigCPtr); }
            set { Noesis_RenderDevice_SetOffscreenWidth(swigCPtr, value); }
        }

        /// <summary>
        /// Height of offscreen textures (0 = automatic). Default is automatic.
        /// </summary>
        public uint OffscreenHeight
        {
            get { return Noesis_RenderDevice_GetOffscreenHeight(swigCPtr); }
            set { Noesis_RenderDevice_SetOffscreenHeight(swigCPtr, value); }
        }

        /// <summary>
        /// Multisampling of offscreen textures. Default is 1x.
        /// </summary>
        public uint OffscreenSampleCount
        {
            get { return Noesis_RenderDevice_GetOffscreenSampleCount(swigCPtr); }
            set { Noesis_RenderDevice_SetOffscreenSampleCount(swigCPtr, value); }
        }

        /// <summary>
        /// Number of offscreen textures created at startup. Default is 0.
        /// </summary>
        public uint OffscreenDefaultNumSurfaces
        {
            get { return Noesis_RenderDevice_GetOffscreenDefaultNumSurfaces(swigCPtr); }
            set { Noesis_RenderDevice_SetOffscreenDefaultNumSurfaces(swigCPtr, value); }
        }

        /// <summary>
        /// Maximum number of offscreen textures (0 = unlimited). Default is unlimited.
        /// </summary>
        public uint OffscreenMaxNumSurfaces
        {
            get { return Noesis_RenderDevice_GetOffscreenMaxNumSurfaces(swigCPtr); }
            set { Noesis_RenderDevice_SetOffscreenMaxNumSurfaces(swigCPtr, value); }
        }

        /// <summary>
        /// Width of texture used to cache glyphs. Default is 1024.
        /// </summary>
        public uint GlyphCacheWidth
        {
            get { return Noesis_RenderDevice_GetGlyphCacheWidth(swigCPtr); }
            set { Noesis_RenderDevice_SetGlyphCacheWidth(swigCPtr, value); }
        }

        /// <summary>
        /// Height of texture used to cache glyphs. Default is 1024.
        /// </summary>
        public uint GlyphCacheHeight
        {
            get { return Noesis_RenderDevice_GetGlyphCacheHeight(swigCPtr); }
            set { Noesis_RenderDevice_SetGlyphCacheHeight(swigCPtr, value); }
        }
        #endregion

        /// <summary>
        /// Retrieves device render capabilities.
        /// </summary>
        public abstract DeviceCaps Caps { get; }

        /// <summary>
        /// Creates render target surface with given dimensions and number of samples
        /// </summary>
        public abstract RenderTarget CreateRenderTarget(string label, uint width, uint height,
            uint sampleCount, bool needsStencil);

        /// <summary>
        /// Creates render target surface with given dimensions and number of samples
        /// </summary>
        public abstract RenderTarget CloneRenderTarget(string label, RenderTarget surface);

        /// <summary>
        /// Binds render target and sets viewport to cover the entire surface.
        /// </summary>
        public abstract void SetRenderTarget(RenderTarget surface);

        /// <summary>
        /// Indicates that until the next call to EndTile(), all drawing commands will only update
        /// the contents of the render target defined by the extension of the given tile. This is a
        /// good place to enable scissoring and apply optimizations for tile-based GPU architectures.
        /// </summary>
        public abstract void BeginTile(RenderTarget surface, Tile tile);

        /// <summary>
        /// Completes rendering to the tile specified by BeginTile().
        /// </summary>
        public abstract void EndTile(RenderTarget surface);

        /// <summary>
        /// Resolves multisample render target.
        /// </summary>
        public abstract void ResolveRenderTarget(RenderTarget surface, Tile[] tiles);

        /// <summary>
        /// Creates a texture with given dimensions and format. For immutable textures, the content of
        /// each mipmap is given in 'data'. The passed data is tightly packed (no extra pitch).
        /// When 'data' is null the texture is considered dynamic and will be updated using UpdateTexture().
        /// </summary>
        public abstract Texture CreateTexture(string label, uint width, uint height, uint numLevels,
            TextureFormat format, IntPtr data);

        /// <summary>
        /// Updates texture mipmap copying the given data to the desired position. The passed data is
        /// tightly packed (no extra pitch). Origin is located at the left of the first scanline.
        /// </summary>
        public abstract void UpdateTexture(Texture texture, uint level, uint x, uint y,
            uint width, uint height, IntPtr data);

        /// <summary>
        /// Begins rendering offscreen commands.
        /// </summary>
        public abstract void BeginOffscreenRender();

        /// <summary>
        /// Ends rendering offscreen commands.
        /// </summary>
        public abstract void EndOffscreenRender();

        /// <summary>
        /// Begins rendering onscreen commands.
        /// </summary>
        public abstract void BeginOnscreenRender();

        /// <summary>
        /// Ends rendering onscreen commands.
        /// </summary>
        public abstract void EndOnscreenRender();

        /// <summary>
        /// Gets a pointer to stream vertices.
        /// </summary>
        public abstract IntPtr MapVertices(uint bytes);

        /// <summary>
        /// Invalidates the pointer previously mapped.
        /// </summary>
        public abstract void UnmapVertices();

        /// <summary>
        /// Gets a pointer to stream 16-bit indices.
        /// </summary>
        public abstract IntPtr MapIndices(uint bytes);

        /// <summary>
        /// Invalidates the pointer previously mapped.
        /// </summary>
        public abstract void UnmapIndices();

        /// <summary>
        /// Draws primitives for the given batch.
        /// </summary>
        public abstract void DrawBatch(ref Batch batch);

        #region Private members
        static RenderDevice()
        {
            Noesis_RenderDevice_SetCallbacks(
                _getCaps,
                _createRenderTarget,
                _cloneRenderTarget,
                _setRenderTarget,
                _beginTile,
                _endTile,
                _resolveRenderTarget,
                _createTexture,
                _updateTexture,
                _beginOffscreenRender,
                _endOffscreenRender,
                _beginOnscreenRender,
                _endOnscreenRender,
                _mapVertices,
                _unmapVertices,
                _mapIndices,
                _unmapIndices,
                _drawBatch);
        }

        internal new static IntPtr Extend(string typeName)
        {
            return Noesis_RenderDevice_Extend(Marshal.StringToHGlobalAnsi(typeName));
        }

        protected RenderDevice() { }
        internal RenderDevice(IntPtr cPtr, bool cMemoryOwn) : base(cPtr, cMemoryOwn) { }
        #endregion

        #region Callbacks
        private delegate void Callback_GetCaps(IntPtr cPtr, ref DeviceCaps caps);
        private static Callback_GetCaps _getCaps = GetCaps;

        [MonoPInvokeCallback(typeof(Callback_GetCaps))]
        private static void GetCaps(IntPtr cPtr, ref DeviceCaps caps)
        {
            try
            {
                RenderDevice device = (RenderDevice)Noesis.Extend.GetExtendInstance(cPtr);
                if (device != null)
                {
                    caps = device.Caps;
                }
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }
        }

        private delegate IntPtr Callback_CreateRenderTarget(IntPtr cPtr, string label,
            uint width, uint height, uint sampleCount, bool needsStencil);
        private static Callback_CreateRenderTarget _createRenderTarget = CreateRenderTarget;

        [MonoPInvokeCallback(typeof(Callback_CreateRenderTarget))]
        private static IntPtr CreateRenderTarget(IntPtr cPtr, string label,
            uint width, uint height, uint sampleCount, bool needsStencil)
        {
            try
            {
                RenderDevice device = (RenderDevice)Noesis.Extend.GetExtendInstance(cPtr);
                if (device != null)
                {
                    RenderTarget rt = device.CreateRenderTarget(label, width, height, sampleCount, needsStencil);
                    HandleRef rtPtr = Noesis.Extend.GetInstanceHandle(rt);
                    BaseComponent.AddReference(rtPtr.Handle); // released by native bindings
                    return rtPtr.Handle;
                }
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }

            return IntPtr.Zero;
        }

        private delegate IntPtr Callback_CloneRenderTarget(IntPtr cPtr, string label, IntPtr surface);
        private static Callback_CloneRenderTarget _cloneRenderTarget = CloneRenderTarget;

        [MonoPInvokeCallback(typeof(Callback_CloneRenderTarget))]
        private static IntPtr CloneRenderTarget(IntPtr cPtr, string label, IntPtr surfacePtr)
        {
            try
            {
                RenderDevice device = (RenderDevice)Noesis.Extend.GetExtendInstance(cPtr);
                if (device != null)
                {
                    RenderTarget surface = (RenderTarget)Noesis.Extend.GetExtendInstance(surfacePtr);
                    RenderTarget rt = device.CloneRenderTarget(label, surface);
                    HandleRef rtPtr = Noesis.Extend.GetInstanceHandle(rt);
                    BaseComponent.AddReference(rtPtr.Handle); // released by native bindings
                    return rtPtr.Handle;
                }
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }

            return IntPtr.Zero;
        }

        private delegate void Callback_SetRenderTarget(IntPtr cPtr, IntPtr surface);
        private static Callback_SetRenderTarget _setRenderTarget = SetRenderTarget;

        [MonoPInvokeCallback(typeof(Callback_SetRenderTarget))]
        private static void SetRenderTarget(IntPtr cPtr, IntPtr surfacePtr)
        {
            try
            {
                RenderDevice device = (RenderDevice)Noesis.Extend.GetExtendInstance(cPtr);
                if (device != null)
                {
                    RenderTarget surface = (RenderTarget)Noesis.Extend.GetExtendInstance(surfacePtr);
                    device.SetRenderTarget(surface);
                }
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }
        }

        private delegate void Callback_BeginTile(IntPtr cPtr, IntPtr surface, ref Tile tile);
        private static Callback_BeginTile _beginTile = BeginTile;

        [MonoPInvokeCallback(typeof(Callback_BeginTile))]
        private static void BeginTile(IntPtr cPtr, IntPtr surfacePtr, ref Tile tile)
        {
            try
            {
                RenderDevice device = (RenderDevice)Noesis.Extend.GetExtendInstance(cPtr);
                if (device != null)
                {
                    RenderTarget surface = (RenderTarget)Noesis.Extend.GetExtendInstance(surfacePtr);
                    device.BeginTile(surface, tile);
                }
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }
        }

        private delegate void Callback_EndTile(IntPtr cPtr, IntPtr surface);
        private static Callback_EndTile _endTile = EndTile;

        [MonoPInvokeCallback(typeof(Callback_EndTile))]
        private static void EndTile(IntPtr cPtr, IntPtr surfacePtr)
        {
            try
            {
                RenderDevice device = (RenderDevice)Noesis.Extend.GetExtendInstance(cPtr);
                if (device != null)
                {
                    RenderTarget surface = (RenderTarget)Noesis.Extend.GetExtendInstance(surfacePtr);
                    device.EndTile(surface);
                }
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }
        }

        private delegate void Callback_ResolveRenderTarget(IntPtr cPtr, IntPtr surface,
            [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] Tile[] tiles, int numTiles);
        private static Callback_ResolveRenderTarget _resolveRenderTarget = ResolveRenderTarget;

        [MonoPInvokeCallback(typeof(Callback_ResolveRenderTarget))]
        private static void ResolveRenderTarget(IntPtr cPtr, IntPtr surfacePtr, Tile[] tiles, int numTiles)
        {
            try
            {
                RenderDevice device = (RenderDevice)Noesis.Extend.GetExtendInstance(cPtr);
                if (device != null)
                {
                    RenderTarget surface = (RenderTarget)Noesis.Extend.GetExtendInstance(surfacePtr);
                    device.ResolveRenderTarget(surface, tiles);
                }
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }
        }

        private delegate IntPtr Callback_CreateTexture(IntPtr cPtr, string label,
            uint width, uint height, uint numLevels, int format, IntPtr data);
        private static Callback_CreateTexture _createTexture = CreateTexture;

        [MonoPInvokeCallback(typeof(Callback_CreateTexture))]
        private static IntPtr CreateTexture(IntPtr cPtr, string label, uint width, uint height,
            uint numLevels, int format, IntPtr data)
        {
            try
            {
                RenderDevice device = (RenderDevice)Noesis.Extend.GetExtendInstance(cPtr);
                if (device != null)
                {
                    Texture tex = device.CreateTexture(label, width, height, numLevels,
                        (TextureFormat)format, data);
                    HandleRef texPtr = Noesis.Extend.GetInstanceHandle(tex);
                    BaseComponent.AddReference(texPtr.Handle); // released by native bindings
                    return texPtr.Handle;
                }
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }

            return IntPtr.Zero;
        }

        private delegate void Callback_UpdateTexture(IntPtr cPtr, IntPtr texPtr,
            uint level, uint x, uint y, uint width, uint height, IntPtr data);
        private static Callback_UpdateTexture _updateTexture = UpdateTexture;

        [MonoPInvokeCallback(typeof(Callback_UpdateTexture))]
        private static void UpdateTexture(IntPtr cPtr, IntPtr texPtr, uint level, uint x, uint y,
            uint width, uint height, IntPtr data)
        {
            try
            {
                RenderDevice device = (RenderDevice)Noesis.Extend.GetExtendInstance(cPtr);
                if (device != null)
                {
                    Texture tex = (Texture)Noesis.Extend.GetExtendInstance(texPtr);
                    device.UpdateTexture(tex, level, x, y, width, height, data);
                }
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }
        }

        private delegate void Callback_BeginOffscreenRender(IntPtr cPtr);
        private static Callback_BeginOffscreenRender _beginOffscreenRender = BeginOffscreenRender;

        [MonoPInvokeCallback(typeof(Callback_BeginOffscreenRender))]
        private static void BeginOffscreenRender(IntPtr cPtr)
        {
            try
            {
                RenderDevice device = (RenderDevice)Noesis.Extend.GetExtendInstance(cPtr);
                if (device != null)
                {
                    device.BeginOffscreenRender();
                }
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }
        }

        private delegate void Callback_EndOffscreenRender(IntPtr cPtr);
        private static Callback_EndOffscreenRender _endOffscreenRender = EndOffscreenRender;

        [MonoPInvokeCallback(typeof(Callback_EndOffscreenRender))]
        private static void EndOffscreenRender(IntPtr cPtr)
        {
            try
            {
                RenderDevice device = (RenderDevice)Noesis.Extend.GetExtendInstance(cPtr);
                if (device != null)
                {
                    device.EndOffscreenRender();
                }
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }
        }

        private delegate void Callback_BeginOnscreenRender(IntPtr cPtr);
        private static Callback_BeginOnscreenRender _beginOnscreenRender = BeginOnscreenRender;

        [MonoPInvokeCallback(typeof(Callback_BeginOnscreenRender))]
        private static void BeginOnscreenRender(IntPtr cPtr)
        {
            try
            {
                RenderDevice device = (RenderDevice)Noesis.Extend.GetExtendInstance(cPtr);
                if (device != null)
                {
                    device.BeginOnscreenRender();
                }
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }
        }

        private delegate void Callback_EndOnscreenRender(IntPtr cPtr);
        private static Callback_EndOnscreenRender _endOnscreenRender = EndOnscreenRender;

        [MonoPInvokeCallback(typeof(Callback_EndOnscreenRender))]
        private static void EndOnscreenRender(IntPtr cPtr)
        {
            try
            {
                RenderDevice device = (RenderDevice)Noesis.Extend.GetExtendInstance(cPtr);
                if (device != null)
                {
                    device.EndOnscreenRender();
                }
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }
        }

        private delegate IntPtr Callback_MapVertices(IntPtr cPtr, uint bytes);
        private static Callback_MapVertices _mapVertices = MapVertices;

        [MonoPInvokeCallback(typeof(Callback_MapVertices))]
        private static IntPtr MapVertices(IntPtr cPtr, uint bytes)
        {
            try
            {
                RenderDevice device = (RenderDevice)Noesis.Extend.GetExtendInstance(cPtr);
                if (device != null)
                {
                    return device.MapVertices(bytes);
                }
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }

            return IntPtr.Zero;
        }

        private delegate void Callback_UnmapVertices(IntPtr cPtr);
        private static Callback_UnmapVertices _unmapVertices = UnmapVertices;

        [MonoPInvokeCallback(typeof(Callback_UnmapVertices))]
        private static void UnmapVertices(IntPtr cPtr)
        {
            try
            {
                RenderDevice device = (RenderDevice)Noesis.Extend.GetExtendInstance(cPtr);
                if (device != null)
                {
                    device.UnmapVertices();
                }
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }
        }

        private delegate IntPtr Callback_MapIndices(IntPtr cPtr, uint bytes);
        private static Callback_MapIndices _mapIndices = MapIndices;

        [MonoPInvokeCallback(typeof(Callback_MapIndices))]
        private static IntPtr MapIndices(IntPtr cPtr, uint bytes)
        {
            try
            {
                RenderDevice device = (RenderDevice)Noesis.Extend.GetExtendInstance(cPtr);
                if (device != null)
                {
                    return device.MapIndices(bytes);
                }
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }

            return IntPtr.Zero;
        }

        private delegate void Callback_UnmapIndices(IntPtr cPtr);
        private static Callback_UnmapIndices _unmapIndices = UnmapIndices;

        [MonoPInvokeCallback(typeof(Callback_UnmapIndices))]
        private static void UnmapIndices(IntPtr cPtr)
        {
            try
            {
                RenderDevice device = (RenderDevice)Noesis.Extend.GetExtendInstance(cPtr);
                if (device != null)
                {
                    device.UnmapIndices();
                }
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }
        }

        private delegate void Callback_DrawBatch(IntPtr cPtr, [In] ref Batch batch);
        private static Callback_DrawBatch _drawBatch = DrawBatch;

        [MonoPInvokeCallback(typeof(Callback_DrawBatch))]
        private static void DrawBatch(IntPtr cPtr, ref Batch batch)
        {
            try
            {
                RenderDevice device = (RenderDevice)Noesis.Extend.GetExtendInstance(cPtr);
                if (device != null)
                {
                    device.DrawBatch(ref batch);
                }
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }
        }
        #endregion

        #region Imports
        [DllImport(Library.Name)]
        static extern uint Noesis_RenderDevice_SetCallbacks(
            Callback_GetCaps getCaps,
            Callback_CreateRenderTarget createRenderTarget,
            Callback_CloneRenderTarget cloneRenderTarget,
            Callback_SetRenderTarget setRenderTarget,
            Callback_BeginTile beginTile,
            Callback_EndTile endTile,
            Callback_ResolveRenderTarget resolveRenderTarget,
            Callback_CreateTexture createTexture,
            Callback_UpdateTexture updateTexture,
            Callback_BeginOffscreenRender beginOffscreenRender,
            Callback_EndOffscreenRender endOffscreenRender,
            Callback_BeginOnscreenRender beginOnscreenRender,
            Callback_EndOnscreenRender endOnscreenRender,
            Callback_MapVertices mapVertices,
            Callback_UnmapVertices unmapVertices,
            Callback_MapIndices mapIndices,
            Callback_UnmapIndices unmapIndices,
            Callback_DrawBatch drawBatch);

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
        static extern void Noesis_RenderDevice_SetOffscreenDefaultNumSurfaces(HandleRef device, uint n);

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
        static extern IntPtr Noesis_RenderDevice_Extend(IntPtr typeName);
        #endregion
    }

    public class NativeRenderDevice : RenderDevice
    {
        public override DeviceCaps Caps
        {
            get
            {
                DeviceCaps caps = new DeviceCaps();
                Noesis_RenderDevice_GetCaps(swigCPtr, ref caps);
                return caps;
            }
        }

        public override RenderTarget CreateRenderTarget(string label, uint width, uint height,
            uint sampleCount, bool needsStencil)
        {
            IntPtr cPtr = Noesis_RenderDevice_CreateRenderTarget(swigCPtr, label,
                width, height, sampleCount, needsStencil);

            return new NativeRenderTarget(cPtr, true);
        }

        public override RenderTarget CloneRenderTarget(string label, RenderTarget surface)
        {
            if (surface == null)
            {
                throw new ArgumentNullException("surface");
            }

            IntPtr cPtr = Noesis_RenderDevice_CloneRenderTarget(swigCPtr, BaseComponent.getCPtr(surface));

            return new NativeRenderTarget(cPtr, true);
        }

        public override void SetRenderTarget(RenderTarget surface)
        {
            if (surface == null)
            {
                throw new ArgumentNullException("surface");
            }

            Noesis_RenderDevice_SetRenderTarget(swigCPtr, BaseComponent.getCPtr(surface));
        }

        public override void BeginTile(RenderTarget surface, Tile tile)
        {
            if (surface == null)
            {
                throw new ArgumentNullException("surface");
            }

            Noesis_RenderDevice_BeginTile(swigCPtr, BaseComponent.getCPtr(surface), ref tile);
        }

        public override void EndTile(RenderTarget surface)
        {
            if (surface == null)
            {
                throw new ArgumentNullException("surface");
            }

            Noesis_RenderDevice_EndTile(swigCPtr, BaseComponent.getCPtr(surface));
        }

        public override void ResolveRenderTarget(RenderTarget surface, Tile[] tiles)
        {
            if (surface == null)
            {
                throw new ArgumentNullException("surface");
            }

            Noesis_RenderDevice_ResolveRenderTarget(swigCPtr, BaseComponent.getCPtr(surface),
                tiles, tiles.Length);
        }

        public override Texture CreateTexture(string label, uint width, uint height, uint numLevels,
            TextureFormat format, IntPtr data)
        {
            IntPtr texture = Noesis_RenderDevice_CreateTexture(swigCPtr, label,
                width, height, numLevels, (int)format, data);

            return new NativeTexture(texture, true);
        }

        public override void UpdateTexture(Texture texture, uint level, uint x, uint y,
            uint width, uint height, IntPtr data)
        {
            if (texture == null)
            {
                throw new ArgumentNullException("texture");
            }
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            Noesis_RenderDevice_UpdateTexture(swigCPtr, BaseComponent.getCPtr(texture),
                level, x, y, width, height, data);
        }

        public override void BeginOffscreenRender()
        {
            Noesis_RenderDevice_BeginOffscreenRender(swigCPtr);
        }

        public override void EndOffscreenRender()
        {
            Noesis_RenderDevice_EndOffscreenRender(swigCPtr);
        }

        public override void BeginOnscreenRender()
        {
            Noesis_RenderDevice_BeginOnscreenRender(swigCPtr);
        }

        public override void EndOnscreenRender()
        {
            Noesis_RenderDevice_EndOnscreenRender(swigCPtr);
        }

        public override IntPtr MapVertices(uint bytes)
        {
            return Noesis_RenderDevice_MapVertices(swigCPtr, bytes);
        }

        public override void UnmapVertices()
        {
            Noesis_RenderDevice_UnmapVertices(swigCPtr);
        }

        public override IntPtr MapIndices(uint bytes)
        {
            return Noesis_RenderDevice_MapIndices(swigCPtr, bytes);
        }

        public override void UnmapIndices()
        {
            Noesis_RenderDevice_UnmapIndices(swigCPtr);
        }

        public override void DrawBatch(ref Batch batch)
        {
            Noesis_RenderDevice_DrawBatch(swigCPtr, ref batch);
        }

        #region Private members
        internal new static NativeRenderDevice CreateProxy(IntPtr cPtr, bool cMemoryOwn)
        {
            return new NativeRenderDevice(cPtr, cMemoryOwn);
        }

        internal NativeRenderDevice(IntPtr cPtr, bool cMemoryOwn) : base(cPtr, cMemoryOwn)
        {
        }
        protected class ManagedTexture
        {
            internal object Texture;
        }

        protected static Texture WrapTexture(object texture, IntPtr texPtr)
        {
            Texture tex = new NativeTexture(texPtr, true);

            if (texture != null)
            {
                tex.SetPrivateData(new ManagedTexture { Texture = texture });
            }

            return tex;
        }
        #endregion

        #region Imports
        [DllImport(Library.Name)]
        static extern void Noesis_RenderDevice_GetCaps(HandleRef device, ref DeviceCaps caps);

        [DllImport(Library.Name)]
        static extern IntPtr Noesis_RenderDevice_CreateRenderTarget(HandleRef device,
            [MarshalAs(UnmanagedType.LPWStr)]string label, uint width, uint height, uint sampleCount, bool needsStencil);

        [DllImport(Library.Name)]
        static extern IntPtr Noesis_RenderDevice_CloneRenderTarget(HandleRef device, HandleRef surface);

        [DllImport(Library.Name)]
        static extern void Noesis_RenderDevice_SetRenderTarget(HandleRef device, HandleRef surface);

        [DllImport(Library.Name)]
        static extern void Noesis_RenderDevice_BeginTile(HandleRef device, HandleRef surface,
            [In] ref Tile tile);

        [DllImport(Library.Name)]
        static extern void Noesis_RenderDevice_EndTile(HandleRef device, HandleRef surface);

        [DllImport(Library.Name)]
        static extern void Noesis_RenderDevice_ResolveRenderTarget(HandleRef device, HandleRef surface,
            [In, Out] Tile[] tiles, int numTiles);

        [DllImport(Library.Name)]
        static extern IntPtr Noesis_RenderDevice_CreateTexture(HandleRef device,
            [MarshalAs(UnmanagedType.LPWStr)]string label, uint width, uint height, uint numLevels,
            int format, IntPtr data);

        [DllImport(Library.Name)]
        public static extern void Noesis_RenderDevice_UpdateTexture(HandleRef device, HandleRef texture,
            uint level, uint x, uint y, uint width, uint height, IntPtr data);

        [DllImport(Library.Name)]
        static extern void Noesis_RenderDevice_BeginOffscreenRender(HandleRef device);

        [DllImport(Library.Name)]
        static extern void Noesis_RenderDevice_EndOffscreenRender(HandleRef device);

        [DllImport(Library.Name)]
        static extern void Noesis_RenderDevice_BeginOnscreenRender(HandleRef device);

        [DllImport(Library.Name)]
        static extern void Noesis_RenderDevice_EndOnscreenRender(HandleRef device);

        [DllImport(Library.Name)]
        static extern IntPtr Noesis_RenderDevice_MapVertices(HandleRef device, uint bytes);

        [DllImport(Library.Name)]
        static extern void Noesis_RenderDevice_UnmapVertices(HandleRef device);

        [DllImport(Library.Name)]
        static extern IntPtr Noesis_RenderDevice_MapIndices(HandleRef device, uint bytes);

        [DllImport(Library.Name)]
        static extern void Noesis_RenderDevice_UnmapIndices(HandleRef device);

        [DllImport(Library.Name)]
        static extern void Noesis_RenderDevice_DrawBatch(HandleRef device, ref Batch batch);
        #endregion
    }

    /// <summary>
    ///  Creates an OpenGL RenderDevice.
    /// </summary>
    public class RenderDeviceGL : NativeRenderDevice
    {
        public RenderDeviceGL() : this(false)
        {
        }

        public RenderDeviceGL(bool sRGB) :
            base(Noesis_RenderDeviceGL_Create(sRGB), true)
        {
        }

        /// <summary>
        /// Creates a Texture wrapper for the specified GL texture native handle
        /// <param name="nativePointer">GLuint texture name</param>
        /// </summary>
        public static Texture WrapTexture(object texture, IntPtr nativePointer,
            int width, int height, int numMipMaps, bool isInverted, bool hasAlpha)
        {
            return WrapTexture(texture, Noesis_RenderDeviceGL_WrapTexture(nativePointer,
                width, height, numMipMaps, isInverted, hasAlpha));
        }

        #region Imports
        [DllImport(Library.Name)]
        static extern IntPtr Noesis_RenderDeviceGL_Create(bool sRGB);

        [DllImport(Library.Name)]
        static extern IntPtr Noesis_RenderDeviceGL_WrapTexture(IntPtr nativePointer,
            int width, int height, int numMipMaps, bool isInverted, bool hasAlpha);
        #endregion
    }

    /// <summary>
    ///  Creates a D3D11 RenderDevice.
    /// </summary>
    public class RenderDeviceD3D11 : NativeRenderDevice
    {
        public RenderDeviceD3D11(IntPtr deviceContext) : this(deviceContext, false)
        {
        }

        public RenderDeviceD3D11(IntPtr deviceContext, bool sRGB) :
            base(Noesis_RenderDeviceD3D11_Create(deviceContext, sRGB), true)
        {
        }

        /// <summary>
        /// Creates a Texture wrapper for the specified D3D11 texture native handle
        /// <param name="nativePointer">ID3D11Texture2D native pointer</param>
        /// </summary>
        public static Texture WrapTexture(object texture, IntPtr nativePointer,
            int width, int height, int numMipMaps, bool isInverted, bool hasAlpha)
        {
            return WrapTexture(texture, Noesis_RenderDeviceD3D11_WrapTexture(nativePointer,
                width, height, numMipMaps, isInverted, hasAlpha));
        }

        /// <summary>
        /// Gets D3D11 texture native handle
        /// </summary>
        public static IntPtr GetTextureNativePointer(Texture texture)
        {
            return Noesis_RenderDeviceD3D11_GetTextureNativePointer(BaseComponent.getCPtr(texture));
        }

        /// <summary>
        /// Enables the hardware scissor rectangle
        /// </summary>
        public void EnableScissorRect()
        {
            Noesis_RenderDeviceD3D11_EnableScissorRect(swigCPtr);
        }

        /// <summary>
        /// Disables the hardware scissor rectangle
        /// </summary>
        public void DisableScissorRect()
        {
            Noesis_RenderDeviceD3D11_DisableScissorRect(swigCPtr);
        }

        #region Imports
        [DllImport(Library.Name)]
        private static extern IntPtr Noesis_RenderDeviceD3D11_Create(IntPtr deviceContext, bool sRGB);

        [DllImport(Library.Name)]
        private static extern IntPtr Noesis_RenderDeviceD3D11_WrapTexture(IntPtr nativePointer,
            int width, int height, int numMipMaps, bool isInverted, bool hasAlpha);

        [DllImport(Library.Name)]
        private static extern IntPtr Noesis_RenderDeviceD3D11_GetTextureNativePointer(HandleRef texture);

        [DllImport(Library.Name)]
        private static extern void Noesis_RenderDeviceD3D11_EnableScissorRect(HandleRef device);

        [DllImport(Library.Name)]
        private static extern void Noesis_RenderDeviceD3D11_DisableScissorRect(HandleRef device);
        #endregion
    }

    /// <summary>
    ///  Creates a D3D12 RenderDevice.
    /// </summary>
    public class RenderDeviceD3D12 : NativeRenderDevice
    {
        public RenderDeviceD3D12(IntPtr device, IntPtr frameFence, int colorFormat, int stencilFormat, int samples) :
            this(device, frameFence, colorFormat, stencilFormat, samples, false)
        {
        }

        public RenderDeviceD3D12(IntPtr device, IntPtr frameFence, int colorFormat, int stencilFormat, int samples, bool sRGB) :
            base(Noesis_RenderDeviceD3D12_Create(device, frameFence, colorFormat, stencilFormat, samples, sRGB), true)
        {
        }

        /// <summary>
        /// Creates a Texture wrapper for the specified D3D12 texture native handle
        /// <param name="nativePointer">ID3D12Resource native pointer</param>
        /// </summary>
        public static Texture WrapTexture(object texture, IntPtr nativePointer,
            int width, int height, int numMipMaps, bool isInverted, bool hasAlpha)
        {
            return WrapTexture(texture, Noesis_RenderDeviceD3D12_WrapTexture(nativePointer,
                width, height, numMipMaps, isInverted, hasAlpha));
        }

        /// <summary>
        /// Gets D3D12 texture native handle
        /// </summary>
        public static IntPtr GetTextureNativePointer(Texture texture)
        {
            return Noesis_RenderDeviceD3D12_GetTextureNativePointer(BaseComponent.getCPtr(texture));
        }

        /// <summary>
        /// Sets active command list
        /// </summary>
        public void SetCommandList(IntPtr commands, long fenceValue)
        {
            Noesis_RenderDeviceD3D12_SetCommandList(swigCPtr, commands, fenceValue);
        }

        #region Imports
        [DllImport(Library.Name)]
        static extern IntPtr Noesis_RenderDeviceD3D12_Create(IntPtr device, IntPtr frameFence,
            int colorFormat, int stencilFormat, int samples, bool sRGB);

        [DllImport(Library.Name)]
        static extern IntPtr Noesis_RenderDeviceD3D12_WrapTexture(IntPtr nativePointer,
            int width, int height, int numMipMaps, bool isInverted, bool hasAlpha);

        [DllImport(Library.Name)]
        private static extern IntPtr Noesis_RenderDeviceD3D12_GetTextureNativePointer(HandleRef texture);
        #endregion

        [DllImport(Library.Name)]
        static extern void Noesis_RenderDeviceD3D12_SetCommandList(HandleRef device, IntPtr commands,
            long fenceValue);
    }

    /// <summary>
    ///  Creates a Metal RenderDevice.
    /// </summary>
    public class RenderDeviceMTL : NativeRenderDevice
    {
        public RenderDeviceMTL(IntPtr device, bool sRGB) :
            base(Noesis_RenderDeviceMTL_Create(device, sRGB), true)
        {
        }

        /// <summary>
        /// Creates a Texture wrapper for the specified Metal texture id
        /// <param name="textureId">MTLTexture id</param>
        /// </summary>
        public static Texture WrapTexture(object texture, IntPtr textureId,
            int width, int height, int numMipMaps, bool isInverted, bool hasAlpha)
        {
            return WrapTexture(texture, Noesis_RenderDeviceMTL_WrapTexture(textureId,
                width, height, numMipMaps, isInverted, hasAlpha));
        }

        public void SetOffscreenCommandBuffer(IntPtr commandBuffer)
        {
            Noesis_RenderDeviceMTL_SetOffScreenCommandBuffer(swigCPtr, commandBuffer);
        }

        public void SetOnScreenEncoder(IntPtr encoder, uint colorFormat, uint stencilFormat, uint sampleCount)
        {
            Noesis_RenderDeviceMTL_SetOnScreenEncoder(swigCPtr, encoder, colorFormat, stencilFormat, sampleCount);
        }

        #region Imports
        [DllImport(Library.Name)]
        static extern IntPtr Noesis_RenderDeviceMTL_Create(IntPtr device, bool sRGB);

        [DllImport(Library.Name)]
        static extern IntPtr Noesis_RenderDeviceMTL_WrapTexture(IntPtr textureId,
            int width, int height, int numMipMaps, bool isInverted, bool hasAlpha);

        [DllImport(Library.Name)]
        static extern void Noesis_RenderDeviceMTL_SetOffScreenCommandBuffer(HandleRef renderDevice,
            IntPtr commandBuffer);

        [DllImport(Library.Name)]
        static extern void Noesis_RenderDeviceMTL_SetOnScreenEncoder(HandleRef renderDevice,
            IntPtr encoder, uint colorFormat, uint stencilFormat, uint sampleCount);
        #endregion
    }

    /// <summary>
    /// Creates a GNM RenderDevice (PlayStation 4).
    /// </summary>
    public class RenderDeviceGNM : NativeRenderDevice
    {
        public struct GPUAllocator
        {
            // IntPtr AllocateGarlic(uint size, uint alignment)
            public Func<uint, uint, IntPtr> AllocateGarlic;
            // void ReleaseGarlic(IntPtr ptr)
            public Action<IntPtr> ReleaseGarlic;
            // IntPtr AllocateOnion(uint size, uint alignment)
            public Func<uint, uint, IntPtr> AllocateOnion;
            // void ReleaseOnion(IntPtr ptr)
            public Action<IntPtr> ReleaseOnion;
        }

        public RenderDeviceGNM(bool sRGB, GPUAllocator allocator) :
            base(CreateDevice(sRGB, allocator), true)
        {
        }

        private static IntPtr CreateDevice(bool sRGB, GPUAllocator allocator)
        {
            _allocator = allocator;
            return Noesis_RenderDeviceGNM_Create(sRGB, _garlicAlloc, _garlicFree, _onionAlloc, _onionFree);
        }

        /// <summary>
        /// Creates a Texture wrapper for the specified GNM texture native handle
        /// </summary>
        public static Texture WrapTexture(object texture, IntPtr nativePointer,
            int width, int height, int numMipMaps, bool isInverted, bool hasAlpha)
        {
            return WrapTexture(texture, Noesis_RenderDeviceGNM_WrapTexture(nativePointer,
                width, height, numMipMaps, isInverted, hasAlpha));
        }

        public void SetContext(IntPtr context)
        {
            Noesis_RenderDeviceGNM_SetContext(swigCPtr, context);
        }

        #region Allocator callbacks
        [MonoPInvokeCallback(typeof(Callback_Alloc))]
        private static IntPtr GarlicAlloc(IntPtr user, uint size, uint alignment)
        {
            try
            {
                return _allocator.AllocateGarlic(size, alignment);
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }

            return IntPtr.Zero;
        }

        [MonoPInvokeCallback(typeof(Callback_Free))]
        private static void GarlicFree(IntPtr user, IntPtr address)
        {
            try
            {
                _allocator.ReleaseGarlic(address);
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }
        }

        [MonoPInvokeCallback(typeof(Callback_Alloc))]
        private static IntPtr OnionAlloc(IntPtr user, uint size, uint alignment)
        {
            try
            {
                return _allocator.AllocateOnion(size, alignment);
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }

            return IntPtr.Zero;
        }

        [MonoPInvokeCallback(typeof(Callback_Free))]
        private static void OnionFree(IntPtr user, IntPtr address)
        {
            try
            {
                _allocator.ReleaseOnion(address);
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }
        }

        private delegate IntPtr Callback_Alloc(IntPtr user, uint size, uint alignment);
        private delegate void Callback_Free(IntPtr user, IntPtr address);

        private static Callback_Alloc _garlicAlloc = GarlicAlloc;
        private static Callback_Free _garlicFree = GarlicFree;
        private static Callback_Alloc _onionAlloc = OnionAlloc;
        private static Callback_Free _onionFree = OnionFree;

        private static GPUAllocator _allocator;
        #endregion

        #region Imports
        [DllImport(Library.Name)]
        static extern IntPtr Noesis_RenderDeviceGNM_Create(bool sRGB,
            Callback_Alloc garlicAlloc, Callback_Free garlicFree,
            Callback_Alloc onionAlloc, Callback_Free onionFree);

        [DllImport(Library.Name)]
        static extern IntPtr Noesis_RenderDeviceGNM_WrapTexture(IntPtr nativePointer,
            int width, int height, int numMipMaps, bool isInverted, bool hasAlpha);

        [DllImport(Library.Name)]
        static extern IntPtr Noesis_RenderDeviceGNM_SetContext(HandleRef device, IntPtr context);
        #endregion
    }

    /// <summary>
    /// Creates an AGC RenderDevice (PlayStation 5).
    /// </summary>
    public class RenderDeviceAGC : NativeRenderDevice
    {
        public struct GPUAllocator
        {
            // IntPtr Alloc(uint size, uint alignment)
            public Func<uint, uint, IntPtr> Alloc;
            // void Free(IntPtr ptr)
            public Action<IntPtr> Free;
            // void RegisterResource(IntPtr ptr, uint size, uint type, string label)
            public Action<IntPtr, uint, uint, string> RegisterResource;
        }

        public RenderDeviceAGC(bool sRGB, GPUAllocator allocator) :
            base(CreateDevice(sRGB, allocator), true)
        {
        }

        private static IntPtr CreateDevice(bool sRGB, GPUAllocator allocator)
        {
            _allocator = allocator;
            return Noesis_RenderDeviceAGC_Create(sRGB, _alloc, _free, _registerResource);
        }

        /// <summary>
        /// Creates a Texture wrapper for the specified AGC texture native handle
        /// </summary>
        public static Texture WrapTexture(object texture, IntPtr nativePointer,
            int width, int height, int numMipMaps, bool isInverted, bool hasAlpha)
        {
            return WrapTexture(texture, Noesis_RenderDeviceAGC_WrapTexture(nativePointer,
                width, height, numMipMaps, isInverted, hasAlpha));
        }

        public void SetCommandBuffer(IntPtr drawCommandBuffer)
        {
            Noesis_RenderDeviceAGC_SetCommandBuffer(swigCPtr, drawCommandBuffer);
        }

        #region Allocator callbacks
        [MonoPInvokeCallback(typeof(Callback_Alloc))]
        private static IntPtr Alloc(IntPtr user, uint size, uint alignment)
        {
            try
            {
                return _allocator.Alloc(size, alignment);
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }

            return IntPtr.Zero;
        }

        [MonoPInvokeCallback(typeof(Callback_Free))]
        private static void Free(IntPtr user, IntPtr address)
        {
            try
            {
                _allocator.Free(address);
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }
        }

        [MonoPInvokeCallback(typeof(Callback_RegisterResource))]
        private static void RegisterResource(IntPtr user, IntPtr ptr, uint size, uint type,
            IntPtr labelPtr)
        {
            try
            {
                string label = Noesis.Extend.StringFromNativeUtf8(labelPtr);
                _allocator.RegisterResource(ptr, size, type, label);
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }
        }

        private delegate IntPtr Callback_Alloc(IntPtr user, uint size, uint alignment);
        private delegate void Callback_Free(IntPtr user, IntPtr address);
        private delegate void Callback_RegisterResource(IntPtr user, IntPtr ptr, uint size,
            uint type, IntPtr label);

        private static Callback_Alloc _alloc = Alloc;
        private static Callback_Free _free = Free;
        private static Callback_RegisterResource _registerResource = RegisterResource;

        private static GPUAllocator _allocator;
        #endregion

        #region Imports
        [DllImport(Library.Name)]
        static extern IntPtr Noesis_RenderDeviceAGC_Create(bool sRGB,
            Callback_Alloc alloc, Callback_Free free, Callback_RegisterResource registerResource);

        [DllImport(Library.Name)]
        static extern IntPtr Noesis_RenderDeviceAGC_WrapTexture(IntPtr nativePointer,
            int width, int height, int numMipMaps, bool isInverted, bool hasAlpha);

        [DllImport(Library.Name)]
        static extern IntPtr Noesis_RenderDeviceAGC_SetCommandBuffer(HandleRef device,
            IntPtr drawCommandBuffer);
        #endregion
    }
}
