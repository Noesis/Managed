using System;
using System.Runtime.InteropServices;

namespace Noesis
{
    /// <summary>
    /// 2D texture that can be used as render target.
    /// </summary>
    public abstract class RenderTarget : BaseComponent
    {
        /// <summary>
        /// Returns the resolve texture.
        /// </summary>
        public abstract Texture Texture { get; }

        #region Private members
        static RenderTarget()
        {
            Noesis_RenderTarget_SetCallbacks(_getTexture);
        }

        internal new static IntPtr Extend(string typeName)
        {
            return Noesis_RenderTarget_Extend(Marshal.StringToHGlobalAnsi(typeName));
        }

        protected RenderTarget() { }
        internal RenderTarget(IntPtr cPtr, bool cMemoryOwn) : base(cPtr, cMemoryOwn) { }
        #endregion

        #region Callbacks
        private delegate IntPtr Callback_GetTexture(IntPtr cPtr);
        private static Callback_GetTexture _getTexture = GetTexture;

        [MonoPInvokeCallback(typeof(Callback_GetTexture))]
        private static IntPtr GetTexture(IntPtr cPtr)
        {
            try
            {
                RenderTarget rt = (RenderTarget)Noesis.Extend.GetExtendInstance(cPtr);
                if (rt != null)
                {
                    Texture tex = rt.Texture;
                    HandleRef texPtr = Noesis.Extend.GetInstanceHandle(tex);
                    return texPtr.Handle;
                }
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }

            return IntPtr.Zero;
        }
        #endregion

        #region Imports
        [DllImport(Library.Name)]
        static extern void Noesis_RenderTarget_SetCallbacks(Callback_GetTexture getTexture);

        [DllImport(Library.Name)]
        static extern IntPtr Noesis_RenderTarget_Extend(IntPtr typeName);
        #endregion
    }

    public class NativeRenderTarget : RenderTarget
    {
        public override Texture Texture
        {
            get
            {
                IntPtr cPtr = Noesis_RenderTarget_GetTexture(swigCPtr);
                return (Texture)Noesis.Extend.GetProxy(cPtr, false);
            }
        }

        #region Private members
        internal new static NativeRenderTarget CreateProxy(IntPtr cPtr, bool cMemoryOwn)
        {
            return new NativeRenderTarget(cPtr, cMemoryOwn);
        }

        internal NativeRenderTarget(IntPtr cPtr, bool cMemoryOwn) : base(cPtr, cMemoryOwn)
        {
        }
        #endregion

        #region Imports
        [DllImport(Library.Name)]
        static extern IntPtr Noesis_RenderTarget_GetTexture(HandleRef rt);
        #endregion
    }
}
