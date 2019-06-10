using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Noesis
{
    /// <summary>
    /// Manage UI trees: input, update and render.
    /// </summary>
    public class View : DispatcherObject
    {
        /// <summary>
        /// Looks for the view where the node is connected to
        /// </summary>
        /// <returns>A null reference if node is not connected to a View</returns>
        public static View Find(BaseComponent node)
        {
            IntPtr view = Noesis_View_Find(BaseComponent.getCPtr(node));
            return (View)Noesis.Extend.GetProxy(view, false);
        }

        #region Public properties
        /// <summary>
        /// Returns the renderer, to be used in the render thread.
        /// </summary>
        public Renderer Renderer
        {
            get
            {
                IntPtr renderer = Noesis_View_GetRenderer(CPtr);
                return (Renderer)Noesis.Extend.GetProxy(renderer, false);
            }
        }

        /// <summary>
        /// Returns the root element.
        /// </summary>
        public FrameworkElement Content
        {
            get
            {
                IntPtr content = Noesis_View_GetContent(CPtr);
                return (FrameworkElement)Noesis.Extend.GetProxy(content, false);
            }
        }

        #endregion

        #region Configuration
        /// <summary>
        /// Sets the size of the surface where UI elements will layout and render.
        /// </summary>
        /// <param name="width">Surface width in pixels.</param>
        /// <param name="height">Surface height in pixels.</param>
        public void SetSize(int width, int height)
        {
            Noesis_View_SetSize(CPtr, width, height);
        }

        /// <summary>
        /// Per-Primitive Antialiasing (PPAA) implements antialiasing by extruding the contours of
        /// the geometry and smoothing them. Useful when GPU MSAA cannot be used.
        /// </summary>
        /// <param name="mode"></param>
        public void SetIsPPAAEnabled(bool enabled)
        {
            Noesis_View_SetIsPPAAEnabled(CPtr, enabled);
        }

        /// <summary>
        /// Sets the curve tolerance in screen space. MediumQuality is the default value
        /// </summary>
        public void SetTessellationMaxPixelError(TessellationMaxPixelError maxError)
        {
            Noesis_View_SetTessellationMaxPixelError(CPtr, maxError.Error);
        }

        /// <summary>
        /// Enables debugging flags. No debug flags are active by default
        /// </summary>
        public void SetFlags(RenderFlags flags)
        {
            Noesis_View_SetFlags(CPtr, (int)flags);
        }
        #endregion

        #region Input management
        /// <summary>
        /// Activates the view and recovers keyboard focus
        /// </summary>
        public void Activate()
        {
            Noesis_View_Activate(CPtr);
        }

        /// <summary>
        /// Deactivates the view and removes keyboard focus
        /// </summary>
        public void Deactivate()
        {
            Noesis_View_Deactivate(CPtr);
        }

        #region Mouse input events
        /// <summary>
        /// Notifies the View that mouse was moved. The mouse position is specified in renderer
        /// surface pixel coordinates.
        /// </summary>
        /// <param name="x">Mouse x-coordinate.</param>
        /// <param name="y">Mouse y-coordinate.</param>
        /// <returns>True if event was handled</returns>
        public bool MouseMove(int x, int y)
        {
            return Noesis_View_MouseMove(CPtr, x, y);
        }

        /// <summary>
        /// Notifies the View that a mouse button was pressed. The mouse position is specified in
        /// renderer surface pixel coordinates.
        /// </summary>
        /// <param name="x">Mouse x-coordinate.</param>
        /// <param name="y">Mouse y-coordinate.</param>
        /// <param name="button">Indicates which button was pressed.</param>
        public bool MouseButtonDown(int x, int y, Noesis.MouseButton button)
        {
            return Noesis_View_MouseButtonDown(CPtr, x, y, (int)button);
        }

        /// <summary>
        /// Notifies the View that a mouse button was released. The mouse position is specified in
        /// renderer surface pixel coordinates.
        /// </summary>
        /// <param name="x">Mouse x-coordinate.</param>
        /// <param name="y">Mouse y-coordinate.</param>
        /// <param name="button">Indicates which button was released.</param>
        public bool MouseButtonUp(int x, int y, Noesis.MouseButton button)
        {
            return Noesis_View_MouseButtonUp(CPtr, x, y, (int)button);
        }

        /// <summary>
        /// Notifies the View of a mouse button double click. The mouse position is specified in
        /// renderer surface pixel coordinates.
        /// </summary>
        /// <param name="x">Mouse x-coordinate.</param>
        /// <param name="y">Mouse y-coordinate.</param>
        /// <param name="button">Indicates which button was pressed.</param>
        public bool MouseDoubleClick(int x, int y, Noesis.MouseButton button)
        {
            return Noesis_View_MouseDoubleClick(CPtr, x, y, (int)button);
        }

        /// <summary>
        /// Notifies the View that mouse wheel was rotated. The mouse position is specified in
        /// renderer surface pixel coordinates.
        /// </summary>
        /// <param name="x">Mouse x-coordinate.</param>
        /// <param name="y">Mouse y-coordinate.</param>
        /// <param name="wheelRotation">Indicates the amount mouse wheel has changed.</param>
        public bool MouseWheel(int x, int y, int wheelRotation)
        {
            return Noesis_View_MouseWheel(CPtr, x, y, wheelRotation);
        }

        /// <summary>
        /// Notifies the View that mouse wheel was horizontally rotated. The mouse position is
        /// specified in renderer surface pixel coordinates.
        /// </summary>
        /// <param name="x">Mouse x-coordinate.</param>
        /// <param name="y">Mouse y-coordinate.</param>
        /// <param name="wheelRotation">Indicates the amount mouse wheel has changed.</param>
        public bool MouseHWheel(int x, int y, int wheelRotation)
        {
            return Noesis_View_MouseHWheel(CPtr, x, y, wheelRotation);
        }

        /// <summary>
        /// Notifies the View to scroll vertically. Value typically ranges from -1 to 1.
        /// <summary>
        public bool Scroll(float value)
        {
            return Noesis_View_Scroll(CPtr, value);
        }

        /// <summary>
        /// Notifies the View to scroll horizontally. Value typically ranges from -1 to 1.
        /// <summary>
        public bool HScroll(float value)
        {
            return Noesis_View_HScroll(CPtr, value);
        }
        #endregion

        #region Touch input events
        /// <summary>
        /// Notifies the View that a finger is moving on the screen. The finger position is
        /// specified in renderer surface pixel coordinates.
        /// </summary>
        /// <param name="x">Finger x-coordinate.</param>
        /// <param name="y">Finger y-coordinate.</param>
        /// <param name="touchId">Finger identifier.</param>
        public bool TouchMove(int x, int y, ulong touchId)
        {
            return Noesis_View_TouchMove(CPtr, x, y, touchId);
        }

        /// <summary>
        /// Notifies the View that a finger touches the screen. The finger position is
        /// specified in renderer surface pixel coordinates.
        /// </summary>
        /// <param name="x">Finger x-coordinate.</param>
        /// <param name="y">Finger y-coordinate.</param>
        /// <param name="touchId">Finger identifier.</param>
        public bool TouchDown(int x, int y, ulong touchId)
        {
            return Noesis_View_TouchDown(CPtr, x, y, touchId);
        }

        /// <summary>
        /// Notifies the View that a finger is raised off the screen. The finger position is
        /// specified in renderer surface pixel coordinates.
        /// </summary>
        /// <param name="x">Finger x-coordinate.</param>
        /// <param name="y">Finger y-coordinate.</param>
        /// <param name="touchId">Finger identifier.</param>
        public bool TouchUp(int x, int y, ulong touchId)
        {
            return Noesis_View_TouchUp(CPtr, x, y, touchId);
        }
        #endregion

        #region Keyboard input events
        /// <summary>
        /// Notifies the View that a key was pressed.
        /// </summary>
        /// <param name="key">Key identifier.</param>
        public bool KeyDown(Noesis.Key key)
        {
            return Noesis_View_KeyDown(CPtr, (int)key);
        }

        /// <summary>
        /// Notifies the View that a key was released.
        /// </summary>
        /// <param name="key">Key identifier.</param>
        public bool KeyUp(Noesis.Key key)
        {
            return Noesis_View_KeyUp(CPtr, (int)key);
        }

        /// <summary>
        /// Notifies Renderer that a key was translated to the corresponding character.
        /// </summary>
        /// <param name="ch">Unicode character value.</param>
        public bool Char(uint ch)
        {
            return Noesis_View_Char(CPtr, ch);
        }
        #endregion
        #endregion

        #region Render process
        /// <summary>
        /// Performs a layout pass and sends updates to the render tree.
        /// </summary>
        /// <param name="timeInSeconds">Time elapsed since the start of the application.</param>
        public bool Update(double timeInSeconds)
        {
            Noesis.Extend.Update();
            return Noesis_View_Update(CPtr, timeInSeconds);
        }

        /// <summary>
        /// Occurs just before the objects in the composition tree are rendered.
        /// </summary>
        public event RenderingEventHandler Rendering
        {
            add
            {
                if (!_Rendering.ContainsKey(CPtr.Handle))
                {
                    _Rendering.Add(CPtr.Handle, null);
                    Noesis_View_BindRenderingEvent(CPtr, _raiseRendering);
                }

                _Rendering[CPtr.Handle] += value;
            }
            remove
            {
                if (_Rendering.ContainsKey(CPtr.Handle))
                {
                    _Rendering[CPtr.Handle] -= value;

                    if (_Rendering[CPtr.Handle] == null)
                    {
                        Noesis_View_UnbindRenderingEvent(CPtr, _raiseRendering);
                        _Rendering.Remove(CPtr.Handle);
                    }
                }
            }
        }

        #region Rendering event implementation
        internal delegate void RaiseRenderingCallback(IntPtr cPtr, IntPtr sender);
        private static RaiseRenderingCallback _raiseRendering = RaiseRendering;

        [MonoPInvokeCallback(typeof(RaiseRenderingCallback))]
        private static void RaiseRendering(IntPtr cPtr, IntPtr sender)
        {
            try
            {
                if (Noesis.Extend.Initialized)
                {
                    if (sender == IntPtr.Zero)
                    {
                        _Rendering.Remove(cPtr);
                        return;
                    }
                    RenderingEventHandler handler = null;
                    if (!_Rendering.TryGetValue(cPtr, out handler))
                    {
                        throw new InvalidOperationException(
                            "Delegate not registered for Rendering event");
                    }
                    if (handler != null)
                    {
                        handler((View)Noesis.Extend.GetProxy(cPtr, false), EventArgs.Empty);
                    }
                }
            }
            catch (Exception exception)
            {
                Error.UnhandledException(exception);
            }
        }

        internal static Dictionary<IntPtr, RenderingEventHandler> _Rendering =
            new Dictionary<IntPtr, RenderingEventHandler>();
        #endregion

        /// <summary>
        /// Gets stats counters
        /// </summary>
        public ViewStats GetStats()
        {
            ViewStats stats = new ViewStats();
            Noesis_View_GetStats(CPtr, ref stats);
            return stats;
        }
        #endregion

        #region Timer
        /// <summary>
        /// Creates a timer that will be fired at the given milliseconds interval. Returns an ID to
        /// handle restarts and cancellations. The callback returns the next interval or just 0 to
        /// stop the timer
        /// </summary>
        public int CreateTimer(int interval, TimerCallback callback)
        {
            int timerId = Noesis_View_CreateTimer(CPtr, interval, _timerEvent);
            _timers.Add(timerId, callback);
            return timerId;
        }

        /// <summary>
        /// Restarts specified timer with a new interval in milliseconds
        /// </summary>
        public void RestartTimer(int timerId, int interval)
        {
            Noesis_View_RestartTimer(CPtr, timerId, interval);
        }

        /// <summary>
        /// Cancels specified timer
        /// </summary>
        public void CancelTimer(int timerId)
        {
            _timers.Remove(timerId);
            Noesis_View_CancelTimer(CPtr, timerId);
        }

        #region Timer event implementation
        private delegate int TimerEventCallback(IntPtr cPtr, int timerId);
        private static TimerEventCallback _timerEvent = OnTimerEvent;

        [MonoPInvokeCallback(typeof(TimerEventCallback))]
        private static int OnTimerEvent(IntPtr cPtr, int timerId)
        {
            try
            {
                View view = (View)Noesis.Extend.GetProxy(cPtr, false);

                TimerCallback callback;
                if (view._timers.TryGetValue(timerId, out callback))
                {
                    int nextTick = callback();
                    if (nextTick <= 0)
                    {
                        view._timers.Remove(timerId);
                    }

                    return nextTick;
                }
            }
            catch (Exception exception)
            {
                Error.UnhandledException(exception);
            }

            return 0;
        }

        Dictionary<int, TimerCallback> _timers = new Dictionary<int, TimerCallback>();
        #endregion
        #endregion

        #region Private members
        internal View(FrameworkElement content): this(RegisterContent(content))
        {
        }

        internal View(IntPtr cPtr, bool ownMemory): base(cPtr, ownMemory)
        {
        }

        internal new static View CreateProxy(IntPtr cPtr, bool cMemoryOwn)
        {
            // If sCreatingView is not null means this function is being called inside the
            // constructor when the native pointer is being created by Noesis_View_Create(),
            // so we return that same View instead of a new proxy
            if (sCreatingView != null)
            {
                sCreatingView.swigCPtr = new HandleRef(sCreatingView, cPtr);
                return sCreatingView;
            }
            else
            {
                return new View(cPtr, cMemoryOwn);
            }
        }

        new internal static IntPtr GetStaticType()
        {
            return Noesis_View_GetStaticType();
        }

        internal HandleRef CPtr { get { return BaseComponent.getCPtr(this); } }

        #region View creation from C#
        private View(HandleRef content)
        {
            sContentPtr = new HandleRef(null, IntPtr.Zero);
        }

        private static HandleRef RegisterContent(FrameworkElement content)
        {
            sContentPtr = BaseComponent.getCPtr(content);
            return sContentPtr;
        }

        protected override IntPtr CreateCPtr(Type type, out bool registerExtend)
        {
            registerExtend = false;

            sCreatingView = this;
            IntPtr cPtr = Noesis_View_Create(sContentPtr);
            sCreatingView = null;

            return cPtr;
        }

        [ThreadStatic]
        private static HandleRef sContentPtr = new HandleRef(null, IntPtr.Zero);

        [ThreadStatic]
        private static View sCreatingView = null;
        #endregion
        #endregion

        #region Imports
        [DllImport(Library.Name)]
        static extern IntPtr Noesis_View_GetStaticType();

        [DllImport(Library.Name)]
        static extern IntPtr Noesis_View_Find(HandleRef node);

        [DllImport(Library.Name)]
        static extern IntPtr Noesis_View_Create(HandleRef content);

        [DllImport(Library.Name)]
        static extern IntPtr Noesis_View_GetContent(HandleRef view);

        [DllImport(Library.Name)]
        static extern void Noesis_View_SetSize(HandleRef view, int width, int height);

        [DllImport(Library.Name)]
        static extern void Noesis_View_SetIsPPAAEnabled(HandleRef view, bool enabled);

        [DllImport(Library.Name)]
        static extern void Noesis_View_SetTessellationMaxPixelError(HandleRef view, float maxError);

        [DllImport(Library.Name)]
        static extern void Noesis_View_SetFlags(HandleRef view, int flags);

        [DllImport(Library.Name)]
        static extern void Noesis_View_Activate(HandleRef view);

        [DllImport(Library.Name)]
        static extern void Noesis_View_Deactivate(HandleRef view);

        [DllImport(Library.Name)]
        static extern bool Noesis_View_MouseMove(HandleRef view, int x, int y);

        [DllImport(Library.Name)]
        static extern bool Noesis_View_MouseButtonDown(HandleRef view, int x, int y, int button);

        [DllImport(Library.Name)]
        static extern bool Noesis_View_MouseButtonUp(HandleRef view, int x, int y, int button);

        [DllImport(Library.Name)]
        static extern bool Noesis_View_MouseDoubleClick(HandleRef view, int x, int y, int button);

        [DllImport(Library.Name)]
        static extern bool Noesis_View_MouseWheel(HandleRef view, int x, int y, int wheelRotation);

        [DllImport(Library.Name)]
        static extern bool Noesis_View_MouseHWheel(HandleRef view, int x, int y, int wheelRotation);

        [DllImport(Library.Name)]
        static extern bool Noesis_View_Scroll(HandleRef view, float value);

        [DllImport(Library.Name)]
        static extern bool Noesis_View_HScroll(HandleRef view, float value);

        [DllImport(Library.Name)]
        static extern bool Noesis_View_TouchMove(HandleRef view, int x, int y, ulong touchId);

        [DllImport(Library.Name)]
        static extern bool Noesis_View_TouchDown(HandleRef view, int x, int y, ulong touchId);

        [DllImport(Library.Name)]
        static extern bool Noesis_View_TouchUp(HandleRef view, int x, int y, ulong touchId);

        [DllImport(Library.Name)]
        static extern bool Noesis_View_KeyDown(HandleRef view, int key);

        [DllImport(Library.Name)]
        static extern bool Noesis_View_KeyUp(HandleRef view, int key);

        [DllImport(Library.Name)]
        static extern bool Noesis_View_Char(HandleRef view, uint ch);

        [DllImport(Library.Name)]
        static extern bool Noesis_View_Update(HandleRef view, double timeInSeconds);

        [DllImport(Library.Name)]
        static extern IntPtr Noesis_View_GetRenderer(HandleRef view);

        [DllImport(Library.Name)]
        static extern void Noesis_View_BindRenderingEvent(HandleRef view,
            RaiseRenderingCallback callback);

        [DllImport(Library.Name)]
        static extern void Noesis_View_UnbindRenderingEvent(HandleRef view,
            RaiseRenderingCallback callback);

        [DllImport(Library.Name)]
        static extern void Noesis_View_GetStats(HandleRef view, ref ViewStats stats);

        [DllImport(Library.Name)]
        static extern int Noesis_View_CreateTimer(HandleRef view, int interval,
            TimerEventCallback callback);

        [DllImport(Library.Name)]
        static extern void Noesis_View_RestartTimer(HandleRef view, int timerId, int interval);

        [DllImport(Library.Name)]
        static extern void Noesis_View_CancelTimer(HandleRef view, int timerId);
        #endregion
    }

    /// <summary>
    /// Handler for View.Rendering event.
    /// </summary>
    public delegate void RenderingEventHandler(object sender, EventArgs e);

    /// <summary>
    /// Callback for View.Timer event.
    /// </summary>
    public delegate int TimerCallback();

    /// <summary>
    /// Flags to debug UI render.
    /// </summary>
    [Flags]
    public enum RenderFlags
    {
        /// <summary>Toggles wireframe mode when rendering triangles.</summary>
        Wireframe = 1,

        /// <summary>Each batch submitted to the GPU is given a unique solid color.</summary>
        ColorBatches = 2,

        /// <summary>
        /// Display pixel overdraw using blending layers. Different colors are used for each
        /// type of triangle. Green for normal, Red for opacities and Blue for clipping masks.
        /// </summary>
        Overdraw = 4,

        /// <summary>Inverts the render vertically.</summary>
        FlipY = 8,
    };

    /// <summary>
    /// Indicates the tessellation quality applied to vector curves.
    ///
    /// MediumQuality is usually fine for PPAA (non-multisampled) while HighQuality is the 
    /// recommended pixel error if you are rendering to a 8x multisampled surface.
    /// </summary>
    public struct TessellationMaxPixelError
    {
        public static implicit operator TessellationMaxPixelError(float error)
        {
            return new TessellationMaxPixelError { Error = error };
        }

        public float Error { get; private set; }

        public static TessellationMaxPixelError LowQuality { get { return 0.7f; } }
        public static TessellationMaxPixelError MediumQuality { get { return 0.4f; } }
        public static TessellationMaxPixelError HighQuality { get { return 0.2f; } }
    }

    /// <summary>
    /// Provides View statistics.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct ViewStats
    {
        public float FrameTime;
        public float UpdateTime;
        public float RenderTime;

        public uint Triangles;
        public uint Draws;
        public uint Batches;

        public uint Tessellations;
        public uint Flushes;
        public uint GeometrySize;

        public uint Masks;
        public uint Opacities;
        public uint RenderTargetSwitches;

        public uint UploadedRamps;
        public uint RasterizedGlyphs;
        public uint DiscardedGlyphTiles;
    };
}
