using System;
using System.Runtime.InteropServices;

namespace Noesis
{
    public class Renderer : BaseComponent
    {
        /// <summary>
        /// Initializes the Renderer with the specified render device.
        /// </summary>
        /// <param name="vgOptions">Vector graphics options.</param>
        public void Init(RenderDevice device)
        {
            if (device == null)
            {
                throw new ArgumentNullException("device");
            }

            Noesis_Renderer_Init(CPtr, device.CPtr);
        }

        /// <summary>
        /// Free allocated render resources and render tree
        /// </summary>
        public void Shutdown()
        {
            Noesis_Renderer_Shutdown(CPtr);
        }

        /// <summary>
        /// Determines the visible region. By default it is set to cover the view dimensions.
        /// </summary>
        /// <param name="x">Horizontal start of visible region.</param>
        /// <param name="y">Vertical start of visible region.</param>
        /// <param name="width">Horizontal size of visible region.</param>
        /// <param name="height">Vertical size of visible region.</param>
        public void SetRenderRegion(float x, float y, float width, float height)
        {
            Noesis_Renderer_SetRenderRegion(CPtr, x, y, width, height);
        }

        /// <summary>
        /// Applies last changes happened in the view. This function does not interacts with the
        /// render device. Returns whether the render tree really changed.
        /// </summary>
        public bool UpdateRenderTree()
        {
            return Noesis_Renderer_UpdateRenderTree(CPtr);
        }

        /// <summary>
        /// Indicates if offscreen textures are needed at the current render tree state. When this
        /// function returns true, it is mandatory to call RenderOffscreen() before Render()
        /// </summary>
        public bool NeedsOffscreen()
        {
            return Noesis_Renderer_NeedsOffscreen(CPtr);
        }

        /// <summary>
        /// Generates offscreen textures. This function fills internal textures and must be invoked
        /// before binding the main render target. This is especially critical in tiled
        /// architectures.
        /// </summary>
        public void RenderOffscreen()
        {
            Noesis_Renderer_RenderOffscreen(CPtr);
        }

        /// <summary>
        /// Renders UI in the active render target and viewport dimensions
        /// </summary>
        public void Render()
        {
            Noesis_Renderer_Render(CPtr);
        }

        #region Private members
        internal Renderer(IntPtr cPtr, bool ownMemory): base(cPtr, ownMemory)
        {
        }

        internal new static Renderer CreateProxy(IntPtr cPtr, bool cMemoryOwn)
        {
            return new Renderer(cPtr, cMemoryOwn);
        }

        new internal static IntPtr GetStaticType()
        {
            return Noesis_Renderer_GetStaticType();
        }

        private HandleRef CPtr { get { return BaseComponent.getCPtr(this); } }
        #endregion

        #region Imports
        [DllImport(Library.Name)]
        static extern IntPtr Noesis_Renderer_GetStaticType();

        [DllImport(Library.Name)]
        static extern void Noesis_Renderer_Init(HandleRef renderer, HandleRef device);

        [DllImport(Library.Name)]
        static extern void Noesis_Renderer_Shutdown(HandleRef renderer);

        [DllImport(Library.Name)]
        static extern void Noesis_Renderer_SetRenderRegion(HandleRef renderer,
            float x, float y, float width, float height);

        [DllImport(Library.Name)]
        [return: MarshalAs(UnmanagedType.U1)]
        static extern bool Noesis_Renderer_UpdateRenderTree(HandleRef renderer);

        [DllImport(Library.Name)]
        [return: MarshalAs(UnmanagedType.U1)]
        static extern bool Noesis_Renderer_NeedsOffscreen(HandleRef renderer);

        [DllImport(Library.Name)]
        static extern void Noesis_Renderer_RenderOffscreen(HandleRef renderer);

        [DllImport(Library.Name)]
        static extern void Noesis_Renderer_Render(HandleRef renderer);
        #endregion
    }
}
