using System;
using Noesis;

namespace NoesisApp
{
    public enum WindowStyle
    {
        /// Only the client area is visible - the title bar and border are not shown
        None,
        /// A window with a single border. This is the default value
        SingleBorderWindow,
        /// A window with a 3-D border
        ThreeDBorderWindow,
        /// A fixed tool window
        ToolWindow
    }

    public enum WindowState
    {
        /// The window is maximized
        Maximized,
        /// The window is minimized
        Minimized,
        /// The window is restored
        Normal
    }

    public enum ResizeMode
    {
        /// A window can only be minimized and restored. The Minimize and Maximize buttons are both
        /// shown, but only the Minimize button is enabled
        CanMinimize,
        /// A window can be resized. The Minimize and Maximize buttons are both shown and enabled
        CanResize,
        /// A window can be resized. The Minimize and Maximize buttons are both shown and enabled.
        /// A resize grip appears in the bottom-right corner of the window
        CanResizeWithGrip,
        /// A window cannot be resized. The Minimize and Maximize buttons are not displayed in the
        /// title bar
        NoResize
    };

    public enum WindowStartupLocation
    {
        /// The startup location of a Window is set from code, or defers to the default location
        Manual,
        /// The startup location of a Window is the center of the screen that contains the mouse cursor
        CenterScreen,
        /// The startup location of a Window is the center of the Window that owns it, as specified by
        /// the Window Owner property
        CenterOwner
    };

    public delegate void LocationChangedEventHandler(Display display, int x, int y);
    public delegate void SizeChangedEventHandler(Display display, int width, int height);
    public delegate void StateChangedEventHandler(Display display, WindowState state);
    public delegate void FileDroppedEventHandler(Display display, string filename);
    public delegate void ActivatedEventHandler(Display display);
    public delegate void DeactivatedEventHandler(Display display);
    public delegate void RenderEventHandler(Display display);
    public delegate void MouseMoveEventHandler(Display display, int x, int y);
    public delegate void MouseButtonDownEventHandler(Display display, int x, int y, MouseButton b);
    public delegate void MouseButtonUpEventHandler(Display display, int x, int y, MouseButton b);
    public delegate void MouseDoubleClickEventHandler(Display display, int x, int y, MouseButton b);
    public delegate void MouseWheelEventHandler(Display display, int x, int y, int delta);
    public delegate void KeyDownEventHandler(Display display, Key key);
    public delegate void KeyUpEventHandler(Display display, Key key);
    public delegate void CharEventHandler(Display display, uint c);
    public delegate void TouchMoveEventHandler(Display display, int x, int y, ulong id);
    public delegate void TouchDownEventHandler(Display display, int x, int y, ulong id);
    public delegate void TouchUpEventHandler(Display display, int x, int y, ulong id);
    public delegate void NativeHandleChangedEventHandler(Display display, IntPtr window);

    public abstract class Display
    {
        /// <summary>
        /// Returns system display handle
        /// </summary>
        public abstract IntPtr NativeHandle { get; }

        /// <summary>
        /// Returns system window handle
        /// </summary>
        public abstract IntPtr NativeWindow { get; }

        /// <summary>
        /// Returns the width of the window client area
        /// </summary>
        public abstract int ClientWidth { get; }

        /// <summary>
        /// Returns the height of the window client area
        /// </summary>
        public abstract int ClientHeight { get; }

        /// <summary>
        /// Changes the text of the title bar (if it has one)
        /// </summary>
        public virtual void SetTitle(string title) { }

        /// <summary>
        /// Changes position
        /// </summary>
        public virtual void SetLocation(int x, int y) { }

        /// <summary>
        /// Changes size
        /// </summary>
        public virtual void SetSize(int width, int height) { }

        /// <summary>
        /// Sets window border style
        /// </summary>
        public virtual void SetWindowStyle(WindowStyle windowStyle) { }

        /// <summary>
        /// Sets window state
        /// </summary>
        public virtual void SetWindowState(WindowState windowState) { }

        /// <summary>
        /// Sets window resize mode
        /// </summary>
        public virtual void SetResizeMode(ResizeMode resizeMode) { }

        /// <summary>
        /// Sets if the window has a task bar button 
        /// </summary>
        public virtual void SetShowInTaskbar(bool showInTaskbar) { }

        /// <summary>
        /// Sets if a window appears in the topmost z-order
        /// </summary>
        public virtual void SetTopmost(bool topmost) { }

        /// <summary>
        /// Sets if this element can be used as the target of a drag-and-drop operation
        /// </summary>
        public virtual void SetAllowFileDrop(bool allowFileDrop) { }

        /// <summary>
        /// Sets the position of the window when first shown
        /// </summary>
        public virtual void SetWindowStartupLocation(WindowStartupLocation location) { }

        /// <summary>
        /// Activates the window and displays it in its current size and position
        /// </summary>
        public virtual void Show() { }

        /// <summary>
        /// Destroys window
        /// </summary>
        public virtual void Close() { }

        /// <summary>
        /// Opens on-screen keyboard
        /// </summary>
        public virtual void OpenSoftwareKeyboard(UIElement focused) { }

        /// <summary>
        /// Closes on-screen keyboard
        /// </summary>
        public virtual void CloseSoftwareKeyboard() { }

        /// <summary>
        /// Updates mouse cursor icon
        /// </summary>
        public virtual void SetCursor(Cursor cursor) { }

        /// <summary>
        /// Opens URL in a browser
        /// </summary>
        public virtual void OpenUrl(string url) { }

        /// <summary>
        /// Plays a sound specified by the given filename
        /// </summary>
        public virtual void PlaySound(string filename, float volume) { }


        /// <summary>
        /// Retrieves pending messages (if allowed this function may sleep the thread)
        /// Returns false if application should terminate, true otherwise
        /// </summary>
        public virtual void EnterMessageLoop(bool runInBackground) { }


        /// Occurs when the window's location changes
        public LocationChangedEventHandler LocationChanged;

        /// Occurs when the window's size changes
        public SizeChangedEventHandler SizeChanged;

        /// Occurs when the window's state changes
        public StateChangedEventHandler StateChanged;

        /// Occurs when files are dragged and dropped over the window
        public FileDroppedEventHandler FileDropped;

        /// Occurs when a window becomes the foreground window
        public ActivatedEventHandler Activated;

        /// Occurs when a window becomes a background window
        public DeactivatedEventHandler Deactivated;

        /// Occurs when window needs to render a frame
        public RenderEventHandler Render;

        /// Occurs when mouse is moved over the window
        public MouseMoveEventHandler MouseMove;

        /// Occurs when a mouse button is pressed over the window
        public MouseButtonDownEventHandler MouseButtonDown;

        /// Occurs when a mouse button is released over the window
        public MouseButtonUpEventHandler MouseButtonUp;

        /// Occurs when a mouse button is double clicked over the window
        public MouseDoubleClickEventHandler MouseDoubleClick;

        /// Occurs when mouse wheel is rotated over the window
        public MouseWheelEventHandler MouseWheel;

        /// Occurs when mouse horizontal wheel is rotated over the window
        public MouseWheelEventHandler MouseHWheel;

        /// Occurs when a key is pressed over the window
        public KeyDownEventHandler KeyDown;

        /// Occurs when a key is released over the window
        public KeyUpEventHandler KeyUp;

        /// Occurs when a key is translated to the corresponding character over the window
        public CharEventHandler Char;

        /// Occurs when a finger touches the window
        public TouchDownEventHandler TouchDown;

        /// Occurs when a finger moves on the window
        public TouchMoveEventHandler TouchMove;

        /// Occurs when a finger raises off of the window
        public TouchUpEventHandler TouchUp;

        /// Occurs when the native window handle changes
        public NativeHandleChangedEventHandler NativeHandleChanged;
    }
}
