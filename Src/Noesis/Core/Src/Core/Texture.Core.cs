using System;
using System.Runtime.InteropServices;

namespace Noesis
{
    /// <summary>
    /// 2D texture that can be used as render target.
    /// </summary>
    public abstract partial class Texture : BaseComponent
    {
        /// <summary>
        /// Returns the width of the texture
        /// </summary>
        public abstract uint Width { get; }

        /// <summary>
        /// Returns the height of the texture
        /// </summary>
        public abstract uint Height { get; }

        /// <summary>
        /// True if the texture has mipmaps
        /// </summary>
        public abstract bool HasMipMaps { get; }

        /// <summary>
        /// True is the texture must be vertically inverted when mapped. This is true for render targets
        /// on platforms (OpenGL) where texture V coordinate is zero at the "bottom of the texture"
        /// </summary>
        public abstract bool IsInverted { get; }

        /// <summary>
        /// Returns true if the texture has an alpha channel that is not completely white. Just a hint
        /// to optimize rendering as alpha blending can be disabled when there is not transparency.
        /// </summary>
        public abstract bool HasAlpha { get; }
        /// <summary>
        /// Stores custom private data
        /// </summary>
        public void SetPrivateData(object data)
        {
            Noesis_Texture_SetPrivateData(swigCPtr, Noesis.Extend.GetInstanceHandle(data));
        }

        #region Private members
        static Texture()
        {
            Noesis_Texture_SetCallbacks(
                _getWidth,
                _getHeight,
                _hasMipMaps,
                _isInverted,
                _hasAlpha);
        }

        internal new static IntPtr Extend(string typeName)
        {
            return Noesis_Texture_Extend(Marshal.StringToHGlobalAnsi(typeName));
        }

        protected Texture() { }
        #endregion

        #region Callbacks
        private delegate uint Callback_GetWidth(IntPtr cPtr);
        private static Callback_GetWidth _getWidth = GetWidth;

        [MonoPInvokeCallback(typeof(Callback_GetWidth))]
        private static uint GetWidth(IntPtr cPtr)
        {
            try
            {
                Texture tex = (Texture)Noesis.Extend.GetExtendInstance(cPtr);
                if (tex != null)
                {
                    return tex.Width;
                }
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }

            return 0;
        }

        private delegate uint Callback_GetHeight(IntPtr cPtr);
        private static Callback_GetHeight _getHeight = GetHeight;

        [MonoPInvokeCallback(typeof(Callback_GetHeight))]
        private static uint GetHeight(IntPtr cPtr)
        {
            try
            {
                Texture tex = (Texture)Noesis.Extend.GetExtendInstance(cPtr);
                if (tex != null)
                {
                    return tex.Height;
                }
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }

            return 0;
        }

        private delegate bool Callback_HasMipMaps(IntPtr cPtr);
        private static Callback_HasMipMaps _hasMipMaps = GetHasMipMaps;

        [MonoPInvokeCallback(typeof(Callback_HasMipMaps))]
        private static bool GetHasMipMaps(IntPtr cPtr)
        {
            try
            {
                Texture tex = (Texture)Noesis.Extend.GetExtendInstance(cPtr);
                if (tex != null)
                {
                    return tex.HasMipMaps;
                }
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }

            return false;
        }

        private delegate bool Callback_IsInverted(IntPtr cPtr);
        private static Callback_IsInverted _isInverted = GetIsInverted;

        [MonoPInvokeCallback(typeof(Callback_IsInverted))]
        private static bool GetIsInverted(IntPtr cPtr)
        {
            try
            {
                Texture tex = (Texture)Noesis.Extend.GetExtendInstance(cPtr);
                if (tex != null)
                {
                    return tex.IsInverted;
                }
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }

            return false;
        }

        private delegate bool Callback_HasAlpha(IntPtr cPtr);
        private static Callback_HasAlpha _hasAlpha = GetHasAlpha;

        [MonoPInvokeCallback(typeof(Callback_HasAlpha))]
        private static bool GetHasAlpha(IntPtr cPtr)
        {
            try
            {
                Texture tex = (Texture)Noesis.Extend.GetExtendInstance(cPtr);
                if (tex != null)
                {
                    return tex.HasAlpha;
                }
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }

            return false;
        }
        #endregion

        #region Imports
        [DllImport(Library.Name)]
        static extern void Noesis_Texture_SetCallbacks(
            Callback_GetWidth getWidth,
            Callback_GetHeight getHeight,
            Callback_HasMipMaps hasMipMaps,
            Callback_IsInverted isInverted,
            Callback_HasAlpha hasAlpha);

        [DllImport(Library.Name)]
        static extern void Noesis_Texture_SetPrivateData(HandleRef tex, HandleRef data);

        [DllImport(Library.Name)]
        static extern IntPtr Noesis_Texture_Extend(IntPtr typeName);
        #endregion
    }

    public class NativeTexture : Texture
    {
        public override uint Width { get { return Noesis_Texture_GetWidth(swigCPtr); } }
        public override uint Height { get { return Noesis_Texture_GetHeight(swigCPtr); } }
        public override bool HasMipMaps { get { return Noesis_Texture_HasMipMaps(swigCPtr); } }
        public override bool IsInverted { get { return Noesis_Texture_IsInverted(swigCPtr); } }
        public override bool HasAlpha { get { return Noesis_Texture_HasAlpha(swigCPtr); } }

        #region Private members
        internal new static NativeTexture CreateProxy(IntPtr cPtr, bool cMemoryOwn)
        {
            return new NativeTexture(cPtr, cMemoryOwn);
        }

        internal NativeTexture(IntPtr cPtr, bool cMemoryOwn) : base(cPtr, cMemoryOwn)
        {
        }
        #endregion

        #region Imports
        [DllImport(Library.Name)]
        static extern uint Noesis_Texture_GetWidth(HandleRef tex);

        [DllImport(Library.Name)]
        static extern uint Noesis_Texture_GetHeight(HandleRef tex);

        [DllImport(Library.Name)]
        static extern bool Noesis_Texture_HasMipMaps(HandleRef tex);

        [DllImport(Library.Name)]
        static extern bool Noesis_Texture_IsInverted(HandleRef tex);

        [DllImport(Library.Name)]
        static extern bool Noesis_Texture_HasAlpha(HandleRef tex);
        #endregion
    }
}
