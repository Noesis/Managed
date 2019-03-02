using System;
using System.Runtime.InteropServices;
using Noesis;
using NoesisApp;

namespace NoesisApp
{
    /// <summary>
    /// Provides the ability to create, configure, show, and manage the lifetime of windows and dialog boxes.
    /// </summary>
    public class Window : ContentControl
    {
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

        static Window()
        {
            WidthProperty.OverrideMetadata(typeof(Window),
                new FrameworkPropertyMetadata(float.NaN, OnWidthChanged));

            HeightProperty.OverrideMetadata(typeof(Window),
                new FrameworkPropertyMetadata(float.NaN, OnHeightChanged));
        }

        public Window()
        {
            Noesis_Window_IgnoreLayout(BaseComponent.getCPtr(this), true);
        }

        public void Init(Display display, RenderContext renderContext, uint samples, bool ppaa)
        {
            _display = display;
            _renderContext = renderContext;
            _samples = samples;
            _ppaa = ppaa;

            // Set display properties
            _display.SetTitle(Title);

            // Set display Location
            float left = Left;
            float top = Top;
            if (!float.IsNaN(left) && !float.IsNaN(top))
            {
                _display.SetLocation((int)left, (int)top);
            }

            // Set display Size
            float width = Width;
            float height = Height;
            if (!float.IsNaN(width) && !float.IsNaN(height))
            {
                _display.SetSize((int)width, (int)height);
            }

            // Create View
            _view = GUI.CreateView(this);
            _view.SetIsPPAAEnabled(ppaa);
            _view.SetTessellationMaxPixelError(TessellationMaxPixelError.HighQuality);
            _view.Renderer.Init(renderContext.Device);

            // Hook to display events
            _display.LocationChanged += OnDisplayLocationChanged;
            _display.SizeChanged += OnDisplaySizeChanged;
            _display.StateChanged += OnDisplayStateChanged;
            _display.FileDropped += OnDisplayFileDropped;
            _display.Activated += OnDisplayActivated;
            _display.Deactivated += OnDisplayDeactivated;
            _display.MouseMove += OnDisplayMouseMove;
            _display.MouseButtonDown += OnDisplayMouseButtonDown;
            _display.MouseButtonUp += OnDisplayMouseButtonUp;
            _display.MouseDoubleClick += OnDisplayMouseDoubleClick;
            _display.MouseWheel += OnDisplayMouseWheel;
            _display.KeyDown += OnDisplayKeyDown;
            _display.KeyUp += OnDisplayKeyUp;
            _display.Char += OnDisplayChar;
            _display.TouchMove += OnDisplayTouchMove;
            _display.TouchDown += OnDisplayTouchDown;
            _display.TouchUp += OnDisplayTouchUp;
        }

        public void Render(double time)
        {
            // Update
            _view.Update(time);

            // Render
            _renderContext.BeginRender();

            Renderer renderer = _view.Renderer;
            renderer.UpdateRenderTree();

            if (renderer.NeedsOffscreen())
            {
                renderer.RenderOffscreen();
            }

            _renderContext.SetDefaultRenderTarget(_display.ClientWidth, _display.ClientHeight, true);
            renderer.Render();

            _renderContext.EndRender();

            _renderContext.Swap();
        }

        public void Shutdown()
        {
            // Unhook to display events
            _display.LocationChanged -= OnDisplayLocationChanged;
            _display.SizeChanged -= OnDisplaySizeChanged;
            _display.StateChanged -= OnDisplayStateChanged;
            _display.FileDropped -= OnDisplayFileDropped;
            _display.Activated -= OnDisplayActivated;
            _display.Deactivated -= OnDisplayDeactivated;
            _display.MouseMove -= OnDisplayMouseMove;
            _display.MouseButtonDown -= OnDisplayMouseButtonDown;
            _display.MouseButtonUp -= OnDisplayMouseButtonUp;
            _display.MouseDoubleClick -= OnDisplayMouseDoubleClick;
            _display.MouseWheel -= OnDisplayMouseWheel;
            _display.KeyDown -= OnDisplayKeyDown;
            _display.KeyUp -= OnDisplayKeyUp;
            _display.Char -= OnDisplayChar;
            _display.TouchMove -= OnDisplayTouchMove;
            _display.TouchDown -= OnDisplayTouchDown;
            _display.TouchUp -= OnDisplayTouchUp;

            _view.Renderer.Shutdown();
            _view = null;
        }

        public void Close()
        {
            _display.Close();
        }

        protected virtual void OnFileDropped(string filename)
        {

        }

        #region Property changed callbacks
        private static void OnTitleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Window window = (Window)d;
            window._display.SetTitle((string)e.NewValue);
        }

        private static void OnTopChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Window window = (Window)d;
            if ((window._flags & WindowFlags.UpdatingWindow) == 0)
            {
                window._flags |= WindowFlags.UpdatingDisplay;
                window._display.SetLocation((int)window.Left, (int)(float)e.NewValue);
                window._flags &= ~WindowFlags.UpdatingDisplay;
            }
        }

        private static void OnLeftChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Window window = (Window)d;
            if ((window._flags & WindowFlags.UpdatingWindow) == 0)
            {
                window._flags |= WindowFlags.UpdatingDisplay;
                window._display.SetLocation((int)(float)e.NewValue, (int)window.Top);
                window._flags &= ~WindowFlags.UpdatingDisplay;
            }
        }

        private static void OnWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Window window = (Window)d;
            if ((window._flags & WindowFlags.UpdatingWindow) == 0)
            {
                window._flags |= WindowFlags.UpdatingDisplay;
                window._display.SetSize((int)(float)e.NewValue, (int)window.Height);
                window._flags &= ~WindowFlags.UpdatingDisplay;
            }
        }

        private static void OnHeightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Window window = (Window)d;
            if ((window._flags & WindowFlags.UpdatingWindow) == 0)
            {
                window._flags |= WindowFlags.UpdatingDisplay;
                window._display.SetSize((int)window.Width, (int)(float)e.NewValue);
                window._flags &= ~WindowFlags.UpdatingDisplay;
            }
        }

        private static void OnWindowStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Window window = (Window)d;
            if ((window._flags & WindowFlags.UpdatingWindow) == 0)
            {
                window._flags |= WindowFlags.UpdatingDisplay;
                window._display.SetWindowState((WindowState)e.NewValue);
                window._flags &= ~WindowFlags.UpdatingDisplay;
            }
        }

        private static void OnWindowStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Window window = (Window)d;
            if ((window._flags & WindowFlags.UpdatingWindow) == 0)
            {
                window._flags |= WindowFlags.UpdatingDisplay;
                window._display.SetWindowStyle((WindowStyle)e.NewValue);
                window._flags &= ~WindowFlags.UpdatingDisplay;
            }
        }

        private static void OnWindowStartupLocationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Window window = (Window)d;
            if ((window._flags & WindowFlags.UpdatingWindow) == 0)
            {
                window._flags |= WindowFlags.UpdatingDisplay;
                window._display.SetWindowStartupLocation((WindowStartupLocation)e.NewValue);
                window._flags &= ~WindowFlags.UpdatingDisplay;
            }
        }

        private static void OnResizeModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Window window = (Window)d;
            if ((window._flags & WindowFlags.UpdatingWindow) == 0)
            {
                window._flags |= WindowFlags.UpdatingDisplay;
                window._display.SetResizeMode((ResizeMode)e.NewValue);
                window._flags &= ~WindowFlags.UpdatingDisplay;
            }
        }

        private static void OnShowInTaskbarChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Window window = (Window)d;
            if ((window._flags & WindowFlags.UpdatingWindow) == 0)
            {
                window._flags |= WindowFlags.UpdatingDisplay;
                window._display.SetShowInTaskbar((bool)e.NewValue);
                window._flags &= ~WindowFlags.UpdatingDisplay;
            }
        }

        private static void OnTopmostChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Window window = (Window)d;
            if ((window._flags & WindowFlags.UpdatingWindow) == 0)
            {
                window._flags |= WindowFlags.UpdatingDisplay;
                window._display.SetTopmost((bool)e.NewValue);
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
                _renderContext.Resize();
                _view.SetSize(_display.ClientWidth, _display.ClientHeight);
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
        }

        void OnDisplayFileDropped(Display display, string filename)
        {
            OnFileDropped(filename);
        }

        void OnDisplayActivated(Display display)
        {
            _view?.Activate();
        }

        void OnDisplayDeactivated(Display display)
        {
            _view?.Deactivate();
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
        private Display _display;
        private RenderContext _renderContext;
        private uint _samples;
        private bool _ppaa;
        private View _view;

        [Flags]
        private enum WindowFlags
        {
            UpdatingDisplay = 1,
            UpdatingWindow = 2
        }
        private WindowFlags _flags;
        #endregion

        #region Imports
        [DllImport(Library.Name)]
        static extern void Noesis_Window_IgnoreLayout(HandleRef window, bool ignore);

        #endregion
    }
}