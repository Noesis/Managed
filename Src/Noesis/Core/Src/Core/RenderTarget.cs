using System;
using System.Runtime.InteropServices;

namespace Noesis
{
    /// <summary>
    /// 2D texture that can be used as render target.
    /// </summary>
    public class RenderTarget
    {
        /// <summary>
        /// Returns the resolve texture.
        /// </summary>
        public Texture Texture
        {
            get
            {
                IntPtr cPtr = Noesis_RenderTarget_GetTexture(CPtr);
                Texture tex = (cPtr == IntPtr.Zero) ? null : new Texture(cPtr, false);
                return tex;
            }
        }

        #region Private members
        internal RenderTarget(IntPtr cPtr, bool memoryOwn)
        {
            _renderTarget = new BaseComponent(cPtr, memoryOwn);
        }

        internal HandleRef CPtr { get { return BaseComponent.getCPtr(_renderTarget); } }

        private BaseComponent _renderTarget;
        #endregion

        #region Imports
        [DllImport(Library.Name)]
        static extern IntPtr Noesis_RenderTarget_GetTexture(HandleRef rt);
        #endregion
    }
}
