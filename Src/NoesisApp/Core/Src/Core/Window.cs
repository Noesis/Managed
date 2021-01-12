using System;
using System.Runtime.InteropServices;
using Noesis;
using NoesisApp;

namespace NoesisApp
{
    public class CancelEventArgs : System.EventArgs
    {
        public bool Cancel { get; set; }

        public CancelEventArgs() : this(false) { }
        public CancelEventArgs(bool cancel) { Cancel = cancel; }
    }

    public delegate void CancelEventHandler(object sender, CancelEventArgs e);

    public class WindowRenderingEventArgs : System.EventArgs
    {
        public double Timestamp { get; internal set; }
    }

    public delegate void WindowRenderingEventHandler(object sender, WindowRenderingEventArgs e);

    public enum SizeToContent
    {
        /// <summary>
        /// Specifies that a window will not automatically set its size to fit the size of its content.
        /// Instead, the size of a window is determined by other properties, including Width, Height,
        /// MaxWidth, MaxHeight, MinWidth, MinHeight.
        /// </summary>
        Manual,

        /// <summary>
        /// Specifies that a window will automatically set its width to fit the width of its content,
        /// but not the height.
        /// </summary>
        Width,

        /// <summary>
        /// Specifies that a window will automatically set its height to fit the height of its content,
        /// but not the width.
        /// </summary>
        Height,

        /// <summary>
        /// Specifies that a window will automatically set both its width and height to fit the width
        /// and height of its content.
        /// </summary>
        WidthAndHeight
    }

    /// <summary>
    /// Provides the ability to create, configure, show, and manage the lifetime of windows and dialog boxes.
    /// </summary>
    public class Window : ContentControl
    {
        #region Window properties
        #region Title
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
            "Title", typeof(string), typeof(Window),
            new PropertyMetadata(string.Empty, OnTitleChanged));

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }
        #endregion

        #region Top
        public static readonly DependencyProperty TopProperty = DependencyProperty.Register(
            "Top", typeof(float), typeof(Window),
            new PropertyMetadata(0.0f, OnTopChanged));

        public float Top
        {
            get { return (float)GetValue(TopProperty); }
            set { SetValue(TopProperty, value); }
        }
        #endregion

        #region Left
        public static readonly DependencyProperty LeftProperty = DependencyProperty.Register(
            "Left", typeof(float), typeof(Window),
            new PropertyMetadata(0.0f, OnLeftChanged));

        public float Left
        {
            get { return (float)GetValue(LeftProperty); }
            set { SetValue(LeftProperty, value); }
        }
        #endregion

        #region WindowState
        public static readonly DependencyProperty WindowStateProperty = DependencyProperty.Register(
            "WindowState", typeof(WindowState), typeof(Window),
            new FrameworkPropertyMetadata(WindowState.Normal, OnWindowStateChanged));

        public WindowState WindowState
        {
            get { return (WindowState)GetValue(WindowStateProperty); }
            set { SetValue(WindowStateProperty, value); }
        }
        #endregion

        #region WindowStyle
        public static readonly DependencyProperty WindowStyleProperty = DependencyProperty.Register(
            "WindowStyle", typeof(WindowStyle), typeof(Window),
            new FrameworkPropertyMetadata(WindowStyle.SingleBorderWindow, OnWindowStyleChanged));

        public WindowStyle WindowStyle
        {
            get { return (WindowStyle)GetValue(WindowStyleProperty); }
            set { SetValue(WindowStyleProperty, value); }
        }
        #endregion

        #region WindowStartupLocation
        public static readonly DependencyProperty WindowStartupLocationProperty = DependencyProperty.Register(
            "WindowStartupLocation", typeof(WindowStartupLocation), typeof(Window),
            new FrameworkPropertyMetadata(WindowStartupLocation.Manual, OnWindowStartupLocationChanged));

        public WindowStartupLocation WindowStartupLocation
        {
            get { return (WindowStartupLocation)GetValue(WindowStartupLocationProperty); }
            set { SetValue(WindowStartupLocationProperty, value); }
        }
        #endregion

        #region ResizeMode
        public static readonly DependencyProperty ResizeModeProperty = DependencyProperty.Register(
            "ResizeMode", typeof(ResizeMode), typeof(Window),
            new FrameworkPropertyMetadata(ResizeMode.CanResize, OnResizeModeChanged));

        public ResizeMode ResizeMode
        {
            get { return (ResizeMode)GetValue(ResizeModeProperty); }
            set { SetValue(ResizeModeProperty, value); }
        }
        #endregion

        #region ShowInTaskbar
        public static readonly DependencyProperty ShowInTaskbarProperty = DependencyProperty.Register(
            "ShowInTaskBar", typeof(bool), typeof(Window),
            new FrameworkPropertyMetadata(true, OnShowInTaskbarChanged));

        public bool ShowInTaskbar
        {
            get { return (bool)GetValue(ShowInTaskbarProperty); }
            set { SetValue(ShowInTaskbarProperty, value); }
        }
        #endregion

        #region SizeToContent
        public static readonly DependencyProperty SizeToContentProperty = DependencyProperty.Register(
            "SizeToContent", typeof(SizeToContent), typeof(Window),
            new FrameworkPropertyMetadata(SizeToContent.Manual));

        public SizeToContent SizeToContent
        {
            get { return (SizeToContent)GetValue(SizeToContentProperty); }
            set { SetValue(SizeToContentProperty, value); }
        }
        #endregion

        #region Topmost
        public static readonly DependencyProperty TopmostProperty = DependencyProperty.Register(
            "Topmost", typeof(bool), typeof(Window),
            new FrameworkPropertyMetadata(false, OnTopmostChanged));

        public bool Topmost
        {
            get { return (bool)GetValue(TopmostProperty); }
            set { SetValue(TopmostProperty, value); }
        }
        #endregion
        #endregion

        #region Window events
        /// <summary>
        /// Occurs when a window becomes the foreground window.
        /// </summary>
        public event System.EventHandler Activated;

        /// <summary>
        /// Occurs when a window becomes a background window.
        /// </summary>
        public event System.EventHandler Deactivated;

        /// <summary>
        /// Occurs directly after Window.Close is called, and can be handled to cancel window closure.
        /// </summary>
        public event CancelEventHandler Closing;

        /// <summary>
        /// Occurs when the window is about to close.
        /// </summary>
        public event System.EventHandler Closed;

        /// <summary>
        /// Occurs when the window's location changes.
        /// </summary>
        public event System.EventHandler LocationChanged;

        /// <summary>
        /// Occurs when the window's WindowState property changes.
        /// </summary>
        public event System.EventHandler StateChanged;
        #endregion

        #region Render properties
        public Display Display { get; private set; }
        public RenderContext RenderContext { get; private set; }
        public uint Samples { get; private set; }
        public bool PPAA { get; private set; }
        public bool LCD { get; private set; }

        /// <summary>
        /// Event raised when window surface is cleared and ready to start rendering
        /// </summary>
        public event WindowRenderingEventHandler Rendering;
        #endregion

        static Window()
        {
            WidthProperty.OverrideMetadata(typeof(Window),
                new FrameworkPropertyMetadata(float.NaN, OnWidthChanged));

            HeightProperty.OverrideMetadata(typeof(Window),
                new FrameworkPropertyMetadata(float.NaN, OnHeightChanged));
        }

        public Window()
        {
            IgnoreLayout(true);
            _renderingArgs = new WindowRenderingEventArgs();
        }

        public void Init(Display display, RenderContext renderContext, uint samples, bool ppaa, bool lcd, bool emulateTouch)
        {
            Display = display;
            RenderContext = renderContext;
            Samples = samples;
            PPAA = ppaa;
            LCD = lcd;
            RenderFlags flags = (ppaa ? RenderFlags.PPAA : 0) | (lcd ? RenderFlags.LCD : 0);

            // Set display properties
            Display.SetTitle(Title);

            // Set display Location
            float left = Left;
            float top = Top;
            if (!float.IsNaN(left) && !float.IsNaN(top))
            {
                Display.SetLocation((int)left, (int)top);
            }

            // Set display Size
            float width = Width;
            float height = Height;

            SizeToContent sizeToContent = SizeToContent;
            if (sizeToContent != SizeToContent.Manual)
            {
                Size contentSize = MeasureContent();
                if (sizeToContent != SizeToContent.Height)
                {
                    width = contentSize.Width;
                }
                if (sizeToContent != SizeToContent.Width)
                {
                    height = contentSize.Height;
                }
            }

            if (!float.IsNaN(width) && !float.IsNaN(height))
            {
                Display.SetSize((int)width, (int)height);
            }

            // Create View
            _view = GUI.CreateView(this);
            _view.SetFlags(flags);
            _view.SetTessellationMaxPixelError(TessellationMaxPixelError.HighQuality);
            _view.SetEmulateTouch(emulateTouch);
            _view.Renderer.Init(renderContext.Device);

            // Hook to display events
            Display.LocationChanged += OnDisplayLocationChanged;
            Display.SizeChanged += OnDisplaySizeChanged;
            Display.StateChanged += OnDisplayStateChanged;
            Display.FileDropped += OnDisplayFileDropped;
            Display.Activated += OnDisplayActivated;
            Display.Deactivated += OnDisplayDeactivated;
            Display.Closing += OnDisplayClosing;
            Display.Closed += OnDisplayClosed;
            Display.MouseMove += OnDisplayMouseMove;
            Display.MouseButtonDown += OnDisplayMouseButtonDown;
            Display.MouseButtonUp += OnDisplayMouseButtonUp;
            Display.MouseDoubleClick += OnDisplayMouseDoubleClick;
            Display.MouseWheel += OnDisplayMouseWheel;
            Display.KeyDown += OnDisplayKeyDown;
            Display.KeyUp += OnDisplayKeyUp;
            Display.Char += OnDisplayChar;
            Display.TouchMove += OnDisplayTouchMove;
            Display.TouchDown += OnDisplayTouchDown;
            Display.TouchUp += OnDisplayTouchUp;
        }

        public void Render(double time)
        {
            // Update
            _view.Update(time);

            // Render
            RenderContext.BeginRender();

            Renderer renderer = _view.Renderer;
            renderer.UpdateRenderTree();
            renderer.RenderOffscreen();

            RenderContext.SetDefaultRenderTarget(Display.ClientWidth, Display.ClientHeight, true);

            _renderingArgs.Timestamp = time;
            Rendering?.Invoke(this, _renderingArgs);

            renderer.Render();

            RenderContext.EndRender();

            RenderContext.Swap();
        }

        public void Shutdown()
        {
            // Unhook to display events
            Display.LocationChanged -= OnDisplayLocationChanged;
            Display.SizeChanged -= OnDisplaySizeChanged;
            Display.StateChanged -= OnDisplayStateChanged;
            Display.FileDropped -= OnDisplayFileDropped;
            Display.Activated -= OnDisplayActivated;
            Display.Deactivated -= OnDisplayDeactivated;
            Display.MouseMove -= OnDisplayMouseMove;
            Display.MouseButtonDown -= OnDisplayMouseButtonDown;
            Display.MouseButtonUp -= OnDisplayMouseButtonUp;
            Display.MouseDoubleClick -= OnDisplayMouseDoubleClick;
            Display.MouseWheel -= OnDisplayMouseWheel;
            Display.KeyDown -= OnDisplayKeyDown;
            Display.KeyUp -= OnDisplayKeyUp;
            Display.Char -= OnDisplayChar;
            Display.TouchMove -= OnDisplayTouchMove;
            Display.TouchDown -= OnDisplayTouchDown;
            Display.TouchUp -= OnDisplayTouchUp;

            _view.Renderer.Shutdown();
            _view = null;
        }

        public void Close()
        {
            Display.Close();
        }

        protected virtual void OnFileDropped(string filename)
        {

        }

        protected override Size MeasureOverride(Size availableSize)
        {
            if (Display != null)
            {
                float clientW = (float)Display.ClientWidth;
                float clientH = (float)Display.ClientHeight;

                SizeToContent sizeToContent = SizeToContent;
                if (sizeToContent == SizeToContent.Manual)
                {
                    base.MeasureOverride(new Size(clientW, clientH));
                    return new Size(Width, Height);
                }
                else
                {
                    // calculate content desired size
                    Size constraint = new Size(
                        sizeToContent == SizeToContent.Height ? clientW : float.PositiveInfinity,
                        sizeToContent == SizeToContent.Width ? clientH : float.PositiveInfinity);

                    Size desiredSize = base.MeasureOverride(constraint);

                    // adjust content size to window size (including borders)
                    int desiredWidth = (int)Math.Ceiling(desiredSize.Width);
                    int desiredHeight = (int)Math.Ceiling(desiredSize.Height);
                    Display.AdjustWindowSize(ref desiredWidth, ref desiredHeight);

                    // update window to the desired size
                    if (sizeToContent != SizeToContent.Height)
                    {
                        Width = desiredWidth;
                    }
                    else
                    {
                        desiredWidth = (int)Width;
                    }

                    if (sizeToContent != SizeToContent.Width)
                    {
                        Height = desiredHeight;
                    }
                    else
                    {
                        desiredHeight = (int)Height;
                    }

                    return new Size(desiredWidth, desiredHeight);
                }
            }

            return base.MeasureOverride(availableSize);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            if (Display != null)
            {
                float clientW = (float)Display.ClientWidth;
                float clientH = (float)Display.ClientHeight;

                base.ArrangeOverride(new Size(clientW, clientH));
                return new Size(Width, Height);
            }

            return base.ArrangeOverride(finalSize);
        }

        private Size MeasureContent()
        {
            // Make sure window is already initialized and template is applied
            InitObject();
            ApplyTemplate();

            // Measure window content
            UIElement child = VisualTreeHelper.GetChild(this, 0) as UIElement;
            if (child != null)
            {
                child.Measure(new Size(float.PositiveInfinity, float.PositiveInfinity));

                Size desiredSize = child.DesiredSize;

                int width = (int)Math.Ceiling(desiredSize.Width);
                int height = (int)Math.Ceiling(desiredSize.Height);
                Display.AdjustWindowSize(ref width, ref height);

                return new Size(width, height);
            }

            return new Size(0.0f, 0.0f);
        }

        #region Property changed callbacks
        private static void OnTitleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Window window = (Window)d;
            window.Display.SetTitle((string)e.NewValue);
        }

        private static void OnTopChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Window window = (Window)d;
            if ((window._flags & WindowFlags.UpdatingWindow) == 0)
            {
                window._flags |= WindowFlags.UpdatingDisplay;
                window.Display.SetLocation((int)window.Left, (int)(float)e.NewValue);
                window._flags &= ~WindowFlags.UpdatingDisplay;
            }
        }

        private static void OnLeftChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Window window = (Window)d;
            if ((window._flags & WindowFlags.UpdatingWindow) == 0)
            {
                window._flags |= WindowFlags.UpdatingDisplay;
                window.Display.SetLocation((int)(float)e.NewValue, (int)window.Top);
                window._flags &= ~WindowFlags.UpdatingDisplay;
            }
        }

        private static void OnWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Window window = (Window)d;
            if ((window._flags & WindowFlags.UpdatingWindow) == 0)
            {
                window._flags |= WindowFlags.UpdatingDisplay;
                window.Display.SetSize((int)(float)e.NewValue, (int)window.Height);
                window._flags &= ~WindowFlags.UpdatingDisplay;
            }
        }

        private static void OnHeightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Window window = (Window)d;
            if ((window._flags & WindowFlags.UpdatingWindow) == 0)
            {
                window._flags |= WindowFlags.UpdatingDisplay;
                window.Display.SetSize((int)window.Width, (int)(float)e.NewValue);
                window._flags &= ~WindowFlags.UpdatingDisplay;
            }
        }

        private static void OnWindowStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Window window = (Window)d;
            if ((window._flags & WindowFlags.UpdatingWindow) == 0)
            {
                window._flags |= WindowFlags.UpdatingDisplay;
                window.Display.SetWindowState((WindowState)e.NewValue);
                window._flags &= ~WindowFlags.UpdatingDisplay;
            }
        }

        private static void OnWindowStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Window window = (Window)d;
            if ((window._flags & WindowFlags.UpdatingWindow) == 0)
            {
                window._flags |= WindowFlags.UpdatingDisplay;
                window.Display.SetWindowStyle((WindowStyle)e.NewValue);
                window._flags &= ~WindowFlags.UpdatingDisplay;
            }
        }

        private static void OnWindowStartupLocationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Window window = (Window)d;
            if ((window._flags & WindowFlags.UpdatingWindow) == 0)
            {
                window._flags |= WindowFlags.UpdatingDisplay;
                window.Display.SetWindowStartupLocation((WindowStartupLocation)e.NewValue);
                window._flags &= ~WindowFlags.UpdatingDisplay;
            }
        }

        private static void OnResizeModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Window window = (Window)d;
            if ((window._flags & WindowFlags.UpdatingWindow) == 0)
            {
                window._flags |= WindowFlags.UpdatingDisplay;
                window.Display.SetResizeMode((ResizeMode)e.NewValue);
                window._flags &= ~WindowFlags.UpdatingDisplay;
            }
        }

        private static void OnShowInTaskbarChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Window window = (Window)d;
            if ((window._flags & WindowFlags.UpdatingWindow) == 0)
            {
                window._flags |= WindowFlags.UpdatingDisplay;
                window.Display.SetShowInTaskbar((bool)e.NewValue);
                window._flags &= ~WindowFlags.UpdatingDisplay;
            }
        }

        private static void OnTopmostChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Window window = (Window)d;
            if ((window._flags & WindowFlags.UpdatingWindow) == 0)
            {
                window._flags |= WindowFlags.UpdatingDisplay;
                window.Display.SetTopmost((bool)e.NewValue);
                window._flags &= ~WindowFlags.UpdatingDisplay;
            }
        }
        #endregion

        #region Display event handlers
        void OnDisplayLocationChanged(Display display, int x, int y)
        {
            if ((_flags & WindowFlags.UpdatingDisplay) == 0)
            {
                _flags |= WindowFlags.UpdatingWindow;
                Left = x;
                Top = y;
                _flags &= ~WindowFlags.UpdatingWindow;
            }

            LocationChanged?.Invoke(this, System.EventArgs.Empty);
        }

        void OnDisplaySizeChanged(Display display, int width, int height)
        {
            if ((_flags & WindowFlags.UpdatingDisplay) == 0)
            {
                _flags |= WindowFlags.UpdatingWindow;
                Width = width;
                Height = height;
                _flags &= ~WindowFlags.UpdatingWindow;
            }

            if (_view != null)
            {
                RenderContext.Resize();
                _view.SetSize(Display.ClientWidth, Display.ClientHeight);
            }
        }

        void OnDisplayStateChanged(Display display, WindowState state)
        {
            if ((_flags & WindowFlags.UpdatingDisplay) == 0)
            {
                _flags |= WindowFlags.UpdatingWindow;
                WindowState = state;
                _flags &= ~WindowFlags.UpdatingWindow;
            }

            StateChanged?.Invoke(this, System.EventArgs.Empty);
        }

        void OnDisplayFileDropped(Display display, string filename)
        {
            OnFileDropped(filename);
        }

        void OnDisplayActivated(Display display)
        {
            _view?.Activate();

            Activated?.Invoke(this, System.EventArgs.Empty);
        }

        void OnDisplayDeactivated(Display display)
        {
            _view?.Deactivate();

            Deactivated?.Invoke(this, System.EventArgs.Empty);
        }

        void OnDisplayClosing(Display display, ref bool cancel)
        {
            CancelEventArgs args = new CancelEventArgs(cancel);
            Closing?.Invoke(this, args);

            cancel = args.Cancel;
        }

        void OnDisplayClosed(Display display)
        {
            Closed?.Invoke(this, System.EventArgs.Empty);
        }

        void OnDisplayMouseMove(Display display, int x, int y)
        {
            _view?.MouseMove(x, y);
        }

        void OnDisplayMouseButtonDown(Display display, int x, int y, MouseButton button)
        {
            _view?.MouseButtonDown(x, y, button);
        }

        void OnDisplayMouseButtonUp(Display display, int x, int y, MouseButton button)
        {
            _view?.MouseButtonUp(x, y, button);
        }

        void OnDisplayMouseDoubleClick(Display display, int x, int y, MouseButton button)
        {
            _view?.MouseDoubleClick(x, y, button);
        }

        void OnDisplayMouseWheel(Display display, int x, int y, int delta)
        {
            _view?.MouseWheel(x, y, delta);
        }

        void OnDisplayKeyDown(Display display, Key key)
        {
            _view?.KeyDown(key);
        }

        void OnDisplayKeyUp(Display display, Key key)
        {
            _view?.KeyUp(key);
        }

        void OnDisplayChar(Display display, uint ch)
        {
            _view?.Char(ch);
        }

        void OnDisplayTouchMove(Display display, int x, int y, ulong id)
        {
            _view?.TouchMove(x, y, id);
        }

        void OnDisplayTouchDown(Display display, int x, int y, ulong id)
        {
            _view?.TouchDown(x, y, id);
        }

        void OnDisplayTouchUp(Display display, int x, int y, ulong id)
        {
            _view?.TouchUp(x, y, id);
        }

        #endregion

        #region Private members
        private View _view;
        private WindowRenderingEventArgs _renderingArgs;

        [Flags]
        private enum WindowFlags
        {
            UpdatingDisplay = 1,
            UpdatingWindow = 2
        }
        private WindowFlags _flags;
        #endregion
    }
}