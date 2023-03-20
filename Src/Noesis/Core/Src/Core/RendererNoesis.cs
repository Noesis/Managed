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

            Noesis_Renderer_Init(CPtr, BaseComponent.getCPtr(device));
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
        /// Generates offscreen textures. This function fills internal textures and must be invoked
        /// before binding the main render target. This is especially critical in tiled
        /// architectures. Returns 'false' when no commands are generated and restoring the GPU state
        /// is not needed.
        /// </summary>
        public bool RenderOffscreen()
        {
            return Noesis_Renderer_RenderOffscreen(CPtr);
        }

        /// <summary>
        /// Renders UI in the active render target and viewport dimensions
        /// </summary>
        public void Render(bool flipY = false, bool clear = false)
        {
            Noesis_Renderer_Render(CPtr, flipY, clear);
        }

        #region Private members
        internal Renderer(IntPtr cPtr, bool ownMemory): base(cPtr, ownMemory)
        {
        }

        internal new static Renderer CreateProxy(IntPtr cPtr, bool cMemoryOwn)
        {
            return new Renderer(cPtr, cMemoryOwn);
        }

        internal HandleRef CPtr { get { return BaseComponent.getCPtr(this); } }
        #endregion

        #region Imports
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
        static extern bool Noesis_Renderer_RenderOffscreen(HandleRef renderer);

        [DllImport(Library.Name)]
        static extern void Noesis_Renderer_Render(HandleRef renderer, bool flipY, bool clear);
        #endregion
    }
}
