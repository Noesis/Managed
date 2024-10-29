using System;
using System.Reflection;
using System.Collections.Generic;
using System.Web;
using Xilium.CefGlue;
using Noesis;
using System.Text.Json;

namespace NoesisApp
{
    public class VirtualKeyboardRequestedEventArgs : System.EventArgs
    {
        public Noesis.InputScope InputScope { get; internal set; }
        public string Content { get; internal set; }
        public string Label { get; internal set; }
    }

    public class HttpWebResponse : System.Net.WebResponse
    {
        public override long ContentLength => _contentLength;
        public override string ContentType => _contentType;
        public override System.Net.WebHeaderCollection Headers => _headers;
        public override bool IsFromCache => _isFromCache;
        public override bool IsMutuallyAuthenticated => _isMutuallyAuthenticated;
        public override Uri ResponseUri => _responseUri;
        public override bool SupportsHeaders => _supportsHeaders;

        public System.Net.HttpStatusCode StatusCode => _statusCode;
        public string Server => _server;
        public Version ProtocolVersion => _protocolVersion;
        public string Method => _method;
        public DateTime LastModified => _lastModified;
        public string StatusDescription => _statusDescription;
        public System.Net.CookieCollection Cookies { get => _cookies; set { _cookies = value; } }
        public string ContentEncoding => _contentEncoding;
        public string CharacterSet => _characterSet;

        internal long _contentLength;
        internal string _contentType;
        internal System.Net.WebHeaderCollection _headers;
        internal bool _isFromCache;
        internal bool _isMutuallyAuthenticated;
        internal Uri _responseUri;
        internal bool _supportsHeaders;
        internal System.Net.HttpStatusCode _statusCode;
        internal string _server;
        internal Version _protocolVersion;
        internal string _method;
        internal DateTime _lastModified;
        internal string _statusDescription;
        internal System.Net.CookieCollection _cookies;
        internal string _contentEncoding;
        internal string _characterSet;
    }

    public class NavigationEventArgs : System.EventArgs
    {
        public object Content { get; }
        public object ExtraData { get; }
        public bool IsNavigationInitiator { get; }
        public object Navigator { get; }
        public Uri Uri { get; internal set; }
        public System.Net.WebResponse WebResponse { get; internal set; }
    }

    #region Event Handlers
    public delegate void VirtualKeyboardRequestedEventHandler(object sender, VirtualKeyboardRequestedEventArgs args);

    public delegate void VirtualKeyboardDismissedEventHandler(object sender, System.EventArgs args);

    public delegate void NavigatedEventHandler(object sender, NavigationEventArgs args);

    public delegate void LoadCompletedEventHandler(object sender, NavigationEventArgs args);
    #endregion

    /// <summary>
    /// Represents a control that contains a web page.
    ///
    /// http://msdn.microsoft.com/en-us/library/system.windows.controls.webbrowser.aspx
    /// </summary>
    public class WebBrowser : Noesis.Border
    {
        static WebBrowser()
        {
            // Load CEF. This checks for the correct CEF version.
            CefRuntime.Load();
        }

        private static bool CefIsInitialized = false;

        public static void StaticInitialize()
        {
            if (!CefIsInitialized)
            {
                // Start the secondary CEF process.
                var cefMainArgs = new CefMainArgs(new string[] { "--enable-features=UseOzonePlatform", "--ozone-platform=headless", "--use-gl=egl" });
                var cefApp = new NoesisCefApp();
                var windowsSandboxInfo = IntPtr.Zero;

                // This is where the code path divereges for child processes.
                if (CefRuntime.ExecuteProcess(cefMainArgs, cefApp, windowsSandboxInfo) != -1)
                {
                    Console.Error.WriteLine("CefRuntime could not the secondary process.");
                }

                // Settings for all of CEF (e.g. process management and control).
                var executablePath = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                var cefSettings = new CefSettings
                {
                    BrowserSubprocessPath = System.IO.Path.Combine(executablePath, "cefsimple"),
                    WindowlessRenderingEnabled = true,
                    MultiThreadedMessageLoop = false,
                    NoSandbox = true,
                    PersistSessionCookies = true,
                    CachePath = CachePath
                };

                // Start the browser process (a child process).
                CefRuntime.Initialize(cefMainArgs, cefSettings, cefApp, windowsSandboxInfo);

                // Wait for the cache to be initialized from storage
                var completionCallback = new NoesisCompletionCallback();
                var cookieManager = CefCookieManager.GetGlobal(completionCallback);
                completionCallback.WaitOnUIThread();

                CefIsInitialized = true;
            }
        }

        public WebBrowser()
        {
            StaticInitialize();

            KeyboardNavigation.SetDirectionalNavigation(this, KeyboardNavigationMode.None);
            KeyboardNavigation.SetTabNavigation(this, KeyboardNavigationMode.None);
            KeyboardNavigation.SetControlTabNavigation(this, KeyboardNavigationMode.None);
            KeyboardNavigation.SetAcceptsReturn(this, true);
            this.Background = Noesis.Brushes.White;
            this.Focusable = true;
            _image = new Noesis.Image();
            this.Child = _image;

            // Instruct CEF to not render to a window at all.
            CefWindowInfo cefWindowInfo = CefWindowInfo.Create();
            cefWindowInfo.WindowlessRenderingEnabled = true;
            cefWindowInfo.SetAsWindowless(IntPtr.Zero, false);

            // Settings for the browser window itself (e.g. should JavaScript be enabled?).
            var cefBrowserSettings = new CefBrowserSettings();

            // Initialize some the cust interactions with the browser process.
            _client = new NoesisClient(this);

            // Start up the browser instance.
            string url = _source?.ToString();
            CefBrowserHost.CreateBrowser(cefWindowInfo, _client, cefBrowserSettings, url);

            _client._lifeSpanHandler.BrowserCreatedEvent += OnBrowserCreated;

            this.SizeChanged += OnSizeChanged;
            this.TouchDown += OnTouchDown;
            this.TouchUp += OnTouchUp;
            this.TouchMove += OnTouchMove;
            this.MouseMove += OnMouseMove;
            this.MouseDown += OnMouseDown;
            this.MouseUp += OnMouseUp;
            this.MouseWheel += OnMouseWheel;
            this.KeyDown += OnKeyDown;
            this.KeyUp += OnKeyUp;
            this.TextInput += OnTextInput;
            this.LostKeyboardFocus += OnLostFocus;

            InitKeyMap();

            _virtualKeyboardRequestedArgs = new VirtualKeyboardRequestedEventArgs();

            Loaded += (s, e) => { ((WebBrowser)s).OnLoaded(s, e); };
            Unloaded += (s, e) => { ((WebBrowser)s).OnUnloaded(s, e); };
        }

        private void OnRendering(object sender, Noesis.EventArgs e)
        {
            CefRuntime.DoMessageLoopWork();
        }

        private void OnLoaded(object sender, Noesis.EventArgs e)
        {
            View view = View;
            if (view != null)
            {
                view.Rendering += OnRendering;
            }
        }

        private void OnUnloaded(object sender, Noesis.EventArgs e)
        {
            View view = ((Visual)VisualTreeHelper.GetParent(this))?.View;
            if (view != null)
            {
                view.Rendering -= OnRendering;
            }

            //Browser?.GetHost().CloseBrowser(true);

            //CefRuntime.Shutdown();
        }

        #region Properties

        #region Source
        public Uri Source
        {
            get { return _source; }
            set { Navigate(value); }
        }
        #endregion

        #region Source
        public static string CachePath { get; set; }
        #endregion

        #endregion

        #region Methods

        /// Navigate asynchronously to the document at the specified Uri.
        public void Navigate(Uri source)
        {
            DoNavigate(source);
        }

        public void SetText(string text)
        {
            string code = $@"document.activeElement.value = ""{text}""";
            var frame = Browser?.GetFocusedFrame();
            frame.ExecuteJavaScript(code, "", 0);
        }

        public void InsertText(string text)
        {
            foreach (char c in text)
            {
                OnKey((int)c, CefKeyEventType.Char, CefEventFlags.None);
            }
        }

            public static void DeleteCookies(string url)
        {
            StaticInitialize();

            var cookieManager = CefCookieManager.GetGlobal(null);
            var deleteCookiesCallback = new NoesisDeleteCookiesCallback();
            cookieManager.DeleteCookies(url, "", deleteCookiesCallback);
            deleteCookiesCallback.WaitOnUIThread();
        }

        #endregion

        #region Events
        public event VirtualKeyboardRequestedEventHandler VirtualKeyboardRequested;
        public event VirtualKeyboardDismissedEventHandler VirtualKeyboardDismissed;
        public event NavigatedEventHandler Navigated;
        public event LoadCompletedEventHandler LoadCompleted;
        #endregion

        internal void RequestVirtualKeyboard(string content, string label)
        {
            _virtualKeyboardRequestedArgs.Content = content;
            _virtualKeyboardRequestedArgs.Label = label;
            VirtualKeyboardRequested?.Invoke(this, _virtualKeyboardRequestedArgs);
        }

        internal void DismissVirtualKeyboard()
        {
            VirtualKeyboardDismissed?.Invoke(this, new System.EventArgs());
        }

        internal void RaiseNavigated(NavigationEventArgs args)
        {
            Navigated?.Invoke(this, args);
        }

        internal void RaiseLoadCompleted(NavigationEventArgs args)
        {
            LoadCompleted?.Invoke(this, args);
            var cookieManager = CefCookieManager.GetGlobal(null);
            cookieManager.FlushStore(null);
        }

        #region Key codes
        private void InitKeyMap()
        {
            keyMap = new int[256];

            keyMap[(int)Key.Cancel] = 0x03;
            keyMap[(int)Key.Back] = 0x08;
            keyMap[(int)Key.Tab] = 0x09;
            keyMap[(int)Key.Clear] = 0x0C;
            keyMap[(int)Key.Return] = 0x0D;
            keyMap[(int)Key.Pause] = 0x13;
            keyMap[(int)Key.Capital] = 0x14;
            keyMap[(int)Key.KanaMode] = 0x15;
            keyMap[(int)Key.HangulMode] = 0x15;
            keyMap[(int)Key.JunjaMode] = 0x17;
            keyMap[(int)Key.FinalMode] = 0x18;
            keyMap[(int)Key.HanjaMode] = 0x19;
            keyMap[(int)Key.KanjiMode] = 0x19;
            keyMap[(int)Key.Escape] = 0x1B;
            keyMap[(int)Key.ImeConvert] = 0x1C;
            keyMap[(int)Key.ImeNonConvert] = 0x1D;
            keyMap[(int)Key.ImeAccept] = 0x1E;
            keyMap[(int)Key.ImeModeChange] = 0x1F;
            keyMap[(int)Key.Space] = 0x20;
            keyMap[(int)Key.Prior] = 0x21;
            keyMap[(int)Key.Next] = 0x22;
            keyMap[(int)Key.End] = 0x23;
            keyMap[(int)Key.Home] = 0x24;
            keyMap[(int)Key.Left] = 0x25;
            keyMap[(int)Key.Up] = 0x26;
            keyMap[(int)Key.Right] = 0x27;
            keyMap[(int)Key.Down] = 0x28;
            keyMap[(int)Key.Select] = 0x29;
            keyMap[(int)Key.Print] = 0x2A;
            keyMap[(int)Key.Execute] = 0x2B;
            keyMap[(int)Key.Snapshot] = 0x2C;
            keyMap[(int)Key.Insert] = 0x2D;
            keyMap[(int)Key.Delete] = 0x2E;
            keyMap[(int)Key.Help] = 0x2F;
            keyMap[(int)Key.A] = 0x41;
            keyMap[(int)Key.B] = 0x42;
            keyMap[(int)Key.C] = 0x43;
            keyMap[(int)Key.D] = 0x44;
            keyMap[(int)Key.E] = 0x45;
            keyMap[(int)Key.F] = 0x46;
            keyMap[(int)Key.G] = 0x47;
            keyMap[(int)Key.H] = 0x48;
            keyMap[(int)Key.I] = 0x49;
            keyMap[(int)Key.J] = 0x4a;
            keyMap[(int)Key.K] = 0x4b;
            keyMap[(int)Key.L] = 0x4c;
            keyMap[(int)Key.M] = 0x4d;
            keyMap[(int)Key.N] = 0x4e;
            keyMap[(int)Key.O] = 0x4f;
            keyMap[(int)Key.P] = 0x50;
            keyMap[(int)Key.Q] = 0x51;
            keyMap[(int)Key.R] = 0x52;
            keyMap[(int)Key.S] = 0x53;
            keyMap[(int)Key.T] = 0x54;
            keyMap[(int)Key.U] = 0x55;
            keyMap[(int)Key.V] = 0x56;
            keyMap[(int)Key.W] = 0x57;
            keyMap[(int)Key.X] = 0x58;
            keyMap[(int)Key.Y] = 0x59;
            keyMap[(int)Key.Z] = 0x5a;
            keyMap[(int)Key.D0] = 0x30;
            keyMap[(int)Key.D1] = 0x31;
            keyMap[(int)Key.D2] = 0x32;
            keyMap[(int)Key.D3] = 0x33;
            keyMap[(int)Key.D4] = 0x34;
            keyMap[(int)Key.D5] = 0x35;
            keyMap[(int)Key.D6] = 0x36;
            keyMap[(int)Key.D7] = 0x37;
            keyMap[(int)Key.D8] = 0x38;
            keyMap[(int)Key.D9] = 0x39;
            keyMap[(int)Key.LWin] = 0x5B;
            keyMap[(int)Key.RWin] = 0x5C;
            keyMap[(int)Key.Apps] = 0x5D;
            keyMap[(int)Key.Sleep] = 0x5F;
            keyMap[(int)Key.NumPad0] = 0x60;
            keyMap[(int)Key.NumPad1] = 0x61;
            keyMap[(int)Key.NumPad2] = 0x62;
            keyMap[(int)Key.NumPad3] = 0x63;
            keyMap[(int)Key.NumPad4] = 0x64;
            keyMap[(int)Key.NumPad5] = 0x65;
            keyMap[(int)Key.NumPad6] = 0x66;
            keyMap[(int)Key.NumPad7] = 0x67;
            keyMap[(int)Key.NumPad8] = 0x68;
            keyMap[(int)Key.NumPad9] = 0x69;
            keyMap[(int)Key.Multiply] = 0x6A;
            keyMap[(int)Key.Add] = 0x6B;
            keyMap[(int)Key.Separator] = 0x6C;
            keyMap[(int)Key.Subtract] = 0x6D;
            keyMap[(int)Key.Decimal] = 0x6E;
            keyMap[(int)Key.Divide] = 0x6F;
            keyMap[(int)Key.F1] = 0x70;
            keyMap[(int)Key.F2] = 0x71;
            keyMap[(int)Key.F3] = 0x72;
            keyMap[(int)Key.F4] = 0x73;
            keyMap[(int)Key.F5] = 0x74;
            keyMap[(int)Key.F6] = 0x75;
            keyMap[(int)Key.F7] = 0x76;
            keyMap[(int)Key.F8] = 0x77;
            keyMap[(int)Key.F9] = 0x78;
            keyMap[(int)Key.F10] = 0x79;
            keyMap[(int)Key.F11] = 0x7A;
            keyMap[(int)Key.F12] = 0x7B;
            keyMap[(int)Key.F13] = 0x7C;
            keyMap[(int)Key.F14] = 0x7D;
            keyMap[(int)Key.F15] = 0x7E;
            keyMap[(int)Key.F16] = 0x7F;
            keyMap[(int)Key.F17] = 0x80;
            keyMap[(int)Key.F18] = 0x81;
            keyMap[(int)Key.F19] = 0x82;
            keyMap[(int)Key.F20] = 0x83;
            keyMap[(int)Key.F21] = 0x84;
            keyMap[(int)Key.F22] = 0x85;
            keyMap[(int)Key.F23] = 0x86;
            keyMap[(int)Key.F24] = 0x87;
            keyMap[(int)Key.NumLock] = 0x90;
            keyMap[(int)Key.Scroll] = 0x91;
            keyMap[(int)Key.LeftShift] = 0xA0;
            keyMap[(int)Key.RightShift] = 0xA1;
            keyMap[(int)Key.LeftCtrl] = 0xA2;
            keyMap[(int)Key.RightCtrl] = 0xA3;
            keyMap[(int)Key.BrowserBack] = 0xA6;
            keyMap[(int)Key.BrowserForward] = 0xA7;
            keyMap[(int)Key.BrowserRefresh] = 0xA8;
            keyMap[(int)Key.BrowserStop] = 0xA9;
            keyMap[(int)Key.BrowserSearch] = 0xAA;
            keyMap[(int)Key.BrowserFavorites] = 0xAB;
            keyMap[(int)Key.BrowserHome] = 0xAC;
            keyMap[(int)Key.VolumeMute] = 0xAD;
            keyMap[(int)Key.VolumeDown] = 0xAE;
            keyMap[(int)Key.VolumeUp] = 0xAF;
            keyMap[(int)Key.MediaNextTrack] = 0xB0;
            keyMap[(int)Key.MediaPreviousTrack] = 0xB1;
            keyMap[(int)Key.MediaStop] = 0xB2;
            keyMap[(int)Key.MediaPlayPause] = 0xB3;
            keyMap[(int)Key.LaunchMail] = 0xB4;
            keyMap[(int)Key.LaunchApplication1] = 0xB6;
            keyMap[(int)Key.LaunchApplication2] = 0xB7;
            keyMap[(int)Key.Oem1] = 0xBA; // ';:' for US,
            keyMap[(int)Key.OemPlus] = 0xBB; // '+' any country,
            keyMap[(int)Key.OemComma] = 0xBC; // ',' any country,
            keyMap[(int)Key.OemMinus] = 0xBD; // '-' any country,
            keyMap[(int)Key.OemPeriod] = 0xBE; // '.' any country,
            keyMap[(int)Key.Oem2] = 0xBF; // '/?' for US,
            keyMap[(int)Key.Oem3] = 0xC0; // '`~' for US,
            keyMap[(int)Key.Oem4] = 0xDB; //  '[{' for US,
            keyMap[(int)Key.Oem5] = 0xDC; //  '\|' for US,
            keyMap[(int)Key.Oem6] = 0xDD; //  ']}' for US,
            keyMap[(int)Key.Oem7] = 0xDE; //  ''"' for US,
            keyMap[(int)Key.Oem8] = 0xDF;
            keyMap[(int)Key.Oem102] = 0xE2; //  "<>" or "\|" on RT 102-key kbd.,
            keyMap[(int)Key.ImeProcessed] = 0xE5;
            keyMap[(int)Key.OemAttn] = 0xF0;
            keyMap[(int)Key.OemFinish] = 0xF1;
            keyMap[(int)Key.OemCopy] = 0xF2;
            keyMap[(int)Key.OemAuto] = 0xF3;
            keyMap[(int)Key.OemEnlw] = 0xF4;
            keyMap[(int)Key.OemBackTab] = 0xF5;
            keyMap[(int)Key.Attn] = 0xF6;
            keyMap[(int)Key.CrSel] = 0xF7;
            keyMap[(int)Key.ExSel] = 0xF8;
            keyMap[(int)Key.EraseEof] = 0xF9;
            keyMap[(int)Key.Play] = 0xFA;
            keyMap[(int)Key.Zoom] = 0xFB;
            keyMap[(int)Key.NoName] = 0xFC;
            keyMap[(int)Key.Pa1] = 0xFD;
            keyMap[(int)Key.OemClear] = 0xFE;            
        }
        #endregion

        #region Private members

        private void DoNavigate(Uri source)
        {
            _source = source;
            Browser?.GetMainFrame().LoadUrl(source.ToString());
        }

        private void OnBrowserCreated()
        {
            if (_source != null)
            {
                Browser.GetMainFrame().LoadUrl(_source.ToString());
            }
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs args)
        {
            _client._renderHandler.Resize(args.NewSize);
            _image.Source = _client._renderHandler.TextureSource;
            InvalidateVisual();

            Browser?.GetHost().WasResized();
        }

        private void OnTouchDown(object sender, TouchEventArgs args)
        {
            Focus();
            CaptureTouch(args.TouchDevice);
            var position = args.GetTouchPoint(this);
            CefTouchEvent cefEvent = new CefTouchEvent();
            cefEvent.X = position.X;
            cefEvent.Y = position.Y;
            cefEvent.Type = CefTouchEventType.Pressed;
            cefEvent.Id = (int)args.TouchDevice.Id;
            Browser?.GetHost().SetFocus(true);
            Browser?.GetHost().SendTouchEvent(cefEvent);
            args.Handled = true;
        }

        private void OnTouchUp(object sender, TouchEventArgs args)
        {
            ReleaseTouchCapture(args.TouchDevice);
            var position = args.GetTouchPoint(this);
            CefTouchEvent cefEvent = new CefTouchEvent();
            cefEvent.X = position.X;
            cefEvent.Y = position.Y;
            cefEvent.Type = CefTouchEventType.Released;
            cefEvent.Id = (int)args.TouchDevice.Id;
            Browser?.GetHost().SendTouchEvent(cefEvent);
            args.Handled = true;
        }

        private void OnTouchMove(object sender, TouchEventArgs args)
        {
            var position = args.GetTouchPoint(this);
            CefTouchEvent cefEvent = new CefTouchEvent();
            cefEvent.X = position.X;
            cefEvent.Y = position.Y;
            cefEvent.Type = CefTouchEventType.Moved;
            cefEvent.Id = (int)args.TouchDevice.Id;
            Browser?.GetHost().SendTouchEvent(cefEvent);
            args.Handled = true;
        }

        private void OnMouseWheel(object sender, MouseWheelEventArgs args)
        {
            var position = args.GetPosition(this);
            CefMouseEvent cefEvent = new CefMouseEvent((int)position.X, (int)position.Y, GetModifiers());
            Browser?.GetHost().SendMouseWheelEvent(cefEvent, 0, args.Delta);
            args.Handled = true;
        }

        private void OnMouseMove(object sender, MouseEventArgs args)
        {
            var position = args.GetPosition(this);
            bool mouseLeave = position.X < 0 || position.X > ActualWidth || position.Y < 0 || position.Y > ActualHeight;
            CefMouseEvent cefEvent = new CefMouseEvent((int)position.X, (int)position.Y, GetModifiers());
            Browser?.GetHost().SendMouseMoveEvent(cefEvent, mouseLeave && !IsMouseCaptured);
            args.Handled = true;
        }

        private void OnMouseButton(object sender, MouseButtonEventArgs args, bool mouseUp)
        {
            var position = args.GetPosition(this);
            CefMouseEvent cefEvent = new CefMouseEvent((int)position.X, (int)position.Y, GetModifiers());
            CefMouseButtonType button = CefMouseButtonType.Left;
            if (args.ChangedButton == MouseButton.Middle)
            {
                button = CefMouseButtonType.Middle;
            }
            else if (args.ChangedButton == MouseButton.Right)
            {
                button = CefMouseButtonType.Right;
            }
            Browser?.GetHost().SetFocus(true);
            Browser?.GetHost().SendMouseClickEvent(cefEvent, button, mouseUp, args.ClickCount);
            args.Handled = true;
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs args)
        {
            Focus();
            CaptureMouse();
            OnMouseButton(sender, args, false);
            args.Handled = true;
        }

        private void OnMouseUp(object sender, MouseButtonEventArgs args)
        {
            OnMouseButton(sender, args, true);
            ReleaseMouseCapture();
            args.Handled = true;
        }

        private void OnKey(int key, CefKeyEventType type, CefEventFlags modifiers)
        {
            CefKeyEvent cefEvent = new CefKeyEvent();
            cefEvent.Character = (char)key;
            cefEvent.UnmodifiedCharacter = (char)key;
            cefEvent.NativeKeyCode = key;
            cefEvent.WindowsKeyCode = key;
            cefEvent.EventType = type;
            cefEvent.Modifiers = modifiers;
            Browser?.GetHost().SendKeyEvent(cefEvent);
        }

        private CefEventFlags GetModifiers()
        {
            CefEventFlags flags =
                ((Keyboard.Modifiers & ModifierKeys.Alt) != 0 ? CefEventFlags.AltDown : CefEventFlags.None) |
                ((Keyboard.Modifiers & ModifierKeys.Control) != 0 ? CefEventFlags.ControlDown : CefEventFlags.None) |
                ((Keyboard.Modifiers & ModifierKeys.Shift) != 0 ? CefEventFlags.ShiftDown : CefEventFlags.None) |
                (Keyboard.IsKeyDown(Key.CapsLock) ? CefEventFlags.CapsLockOn : CefEventFlags.None) |
                (Mouse.GetButtonState(MouseButton.Left) == MouseButtonState.Pressed ? CefEventFlags.LeftMouseButton : CefEventFlags.None) |
                (Mouse.GetButtonState(MouseButton.Middle) == MouseButtonState.Pressed ? CefEventFlags.MiddleMouseButton : CefEventFlags.None) |
                (Mouse.GetButtonState(MouseButton.Right) == MouseButtonState.Pressed ? CefEventFlags.RightMouseButton : CefEventFlags.None);

            return flags;
        }

        private void OnKeyDown(object sender, KeyEventArgs args)
        {
            int keyCode = keyMap[(int)args.Key];
            if (keyCode != 0)
            {
                OnKey(keyCode, CefKeyEventType.RawKeyDown, GetModifiers());
            }

            // If args.Handled is true, we won't get OnTextInput events.
            // If args.Handled is false and we press Tab or arrow keys, the control loses focus.
            if (args.Key == Key.Tab || args.Key == Key.Left || args.Key == Key.Right || args.Key == Key.Up || args.Key == Key.Down)
            {
                args.Handled = true;
            }
        }

        private void OnKeyUp(object sender, KeyEventArgs args)
        {
            int keyCode = keyMap[(int)args.Key];
            if (keyCode != 0)
            {
                OnKey(keyCode, CefKeyEventType.KeyUp, GetModifiers());
            }
            args.Handled = true;
        }

        private void OnTextInput(object sender, TextCompositionEventArgs args)
        {
            InsertText(args.Text);

            args.Handled = true;
        }

        private void OnLostFocus(object sender, KeyboardFocusChangedEventArgs args)
        {
            Browser?.GetHost().SetFocus(false);
        }

        private CefBrowser Browser { get => _client._lifeSpanHandler._browser; }

        internal Uri _source;
        private NoesisClient _client;
        private int[] keyMap;
        private Noesis.Image _image;
        internal VirtualKeyboardRequestedEventArgs _virtualKeyboardRequestedArgs;

        internal static Dictionary<string, WebBrowser> _virtualKeyboardRequests = new Dictionary<string, WebBrowser>();

        #endregion
    }

    internal class NoesisRenderHandler : CefRenderHandler
    {
        private WebBrowser _browser;
        private uint _width;
        private uint _height;
        private bool _shuttingDown = false;
        private byte[] _pixels;
        private int _pixelsWidth;
        private int _pixelsHeight;

        public DynamicTextureSource TextureSource { get; private set; }
        protected Texture _texture;

        public NoesisRenderHandler(WebBrowser browser)
        {
            _browser = browser;
            _pixels = new byte[1];
        }

        public void Resize(Noesis.Size size)
        {
            _width = (uint)size.Width;
            _height = (uint)size.Height;

            if (TextureSource == null)
            {
                TextureSource = new DynamicTextureSource(_width, _height, TextureRender, null);
            }
            else
            {
                TextureSource.Resize(_width, _height);
            }
        }

        public Texture TextureRender(RenderDevice device, object user)
        {
            lock (_pixels)
            {
                if (_pixelsWidth != 0 && _pixelsHeight != 0)
                {
                    if (_texture == null || _texture.Width != (uint)_pixelsWidth || (uint)_texture.Height != _pixelsHeight)
                    {
                        _texture = device.CreateTexture("", (uint)_pixelsWidth, (uint)_pixelsHeight, 1, TextureFormat.RGBA8, IntPtr.Zero);
                    }

                    IntPtr ptr = System.Runtime.InteropServices.Marshal.AllocHGlobal(_pixels.Length);
                    System.Runtime.InteropServices.Marshal.Copy(_pixels, 0, ptr, _pixels.Length);
                    device.UpdateTexture(_texture, 0, 0, 0, (uint)_pixelsWidth, (uint)_pixelsHeight, ptr);
                    System.Runtime.InteropServices.Marshal.FreeHGlobal(ptr);
                }
                return _texture;
            }
        }

        public void Dispose()
        {
        }

        protected override CefAccessibilityHandler GetAccessibilityHandler()
        {
            return null;
        }

        protected override void OnScrollOffsetChanged(CefBrowser browser, double x, double y)
        {
        }

        protected override bool GetRootScreenRect(CefBrowser browser, ref CefRectangle rect)
        {
            rect.X = 0;
            rect.Y = 0;
            rect.Width = Math.Max((int)_width, 1);
            rect.Height = Math.Max((int)_height, 1);

            return true;
        }

        protected override bool GetScreenInfo(CefBrowser browser, CefScreenInfo screenInfo)
        {
            screenInfo.DeviceScaleFactor = 1.0F;

            return true;
        }

        protected override void GetViewRect(CefBrowser browser, out CefRectangle rect)
        {
            rect = new CefRectangle();
            rect.X = 0;
            rect.Y = 0;
            rect.Width = Math.Max((int)_width, 1);
            rect.Height = Math.Max((int)_height, 1);
        }

        protected override bool GetScreenPoint(CefBrowser browser, int viewX, int viewY, ref int screenX, ref int screenY)
        {
            screenX = viewX;
            screenY = viewY;

            return false;
        }

        protected override void OnAcceleratedPaint(CefBrowser browser, CefPaintElementType type, CefRectangle[] dirtyRects, IntPtr sharedHandle)
        {
            //NOT USED
        }

        protected override void OnPaint(CefBrowser browser, CefPaintElementType type, CefRectangle[] dirtyRects, IntPtr buffer, int width, int height)
        {
            if (!_shuttingDown)
            {
                lock (_pixels)
                {
                    int size = width * height * 4;
                    if (_pixelsWidth != width || _pixelsHeight != height)
                    {
                        _pixels = new byte[size];
                        _pixelsWidth = width;
                        _pixelsHeight = height;
                    }
                    for (int i = 0; i < width * height; ++i)
                    {
                        _pixels[i * 4 + 2] = System.Runtime.InteropServices.Marshal.ReadByte(buffer, i * 4 + 0);
                        _pixels[i * 4 + 1] = System.Runtime.InteropServices.Marshal.ReadByte(buffer, i * 4 + 1);
                        _pixels[i * 4 + 0] = System.Runtime.InteropServices.Marshal.ReadByte(buffer, i * 4 + 2);
                        _pixels[i * 4 + 3] = System.Runtime.InteropServices.Marshal.ReadByte(buffer, i * 4 + 3);
                    }
                }
            }
        }

        protected override bool StartDragging(CefBrowser browser, CefDragData dragData, CefDragOperationsMask mask, int x, int y)
        {
            return false;
        }

        protected override void UpdateDragCursor(CefBrowser browser, CefDragOperationsMask operation)
        {
        }

        protected override void OnPopupShow(CefBrowser browser, bool show)
        {
        }

        protected override void OnPopupSize(CefBrowser browser, CefRectangle rect)
        {
        }

        protected override void OnImeCompositionRangeChanged(CefBrowser browser, CefRange selectedRange, CefRectangle[] characterBounds)
        {
        }

        static private Noesis.InputScope GetInputScope(CefTextInputMode inputMode)
        {
            switch (inputMode)
            {
                case CefTextInputMode.Default:
                    return Noesis.InputScope.Default;
                case CefTextInputMode.None:
                    return Noesis.InputScope.Default;
                case CefTextInputMode.Text:
                    return Noesis.InputScope.Default;
                case CefTextInputMode.Tel:
                    return Noesis.InputScope.TelephoneNumber;
                case CefTextInputMode.Url:
                    return Noesis.InputScope.Url;
                case CefTextInputMode.Email:
                    return Noesis.InputScope.EmailUserName;
                case CefTextInputMode.Numeric:
                    return Noesis.InputScope.Number;
                case CefTextInputMode.Decimal:
                    return Noesis.InputScope.Number;
                case CefTextInputMode.Search:
                    return Noesis.InputScope.Default;
            }
            return Noesis.InputScope.Default;
        }

        protected override void OnVirtualKeyboardRequested(CefBrowser browser, CefTextInputMode inputMode)
        {
            if (inputMode == CefTextInputMode.None)
            {
                CefRuntime.PostTask(CefThreadId.UI, new NoesisDelegateTask(() => { _browser.DismissVirtualKeyboard(); }));
            }
            else
            {
                var guid = System.Guid.NewGuid().ToString();
                string code = $@"var labels = document.getElementsByTagName('label');
                                 var label = """";
                                 for( var i = 0; i < labels.length; i++ )
                                 {{
                                   if (labels[i].htmlFor == document.activeElement.id)
                                   {{
                                        label = labels[i].innerHTML;
                                    }}
                                 }}
                                 console.log(""Label: "" + label);
                                 var request = {{
                                    ""id"": ""{guid}"",
                                    ""content"": document.activeElement.value,
                                    ""type"": document.activeElement.type,
                                    ""label"": unescape(encodeURIComponent(label)),
                                  }};
                                 window.cefQuery({{
                                    request: JSON.stringify(request),
                                    persistent: false,
                                    onSuccess: function(response) {{}},
                                    inFailure: function(error, message) {{}}
                                 }});
                                 console.log(""XHR sent"");";
                var frame = browser.GetMainFrame();
                frame.ExecuteJavaScript(code, "", 0);
                _browser._virtualKeyboardRequestedArgs.InputScope = GetInputScope(inputMode);
                lock (WebBrowser._virtualKeyboardRequests)
                {
                    WebBrowser._virtualKeyboardRequests.Add(guid, _browser);
                }
            }
        }

        protected override void OnCursorChange(CefBrowser browser, IntPtr cursorHandle, CefCursorType type, CefCursorInfo customCursorInfo)
        {

        }
    }

    internal sealed class NoesisCefApp : CefApp
    {
        protected override void OnBeforeCommandLineProcessing(string processType, CefCommandLine commandLine)
        {
        }
    }

    internal sealed class NoesisLifeSpanHandler : CefLifeSpanHandler
    {
        public CefBrowser _browser;

        protected override void OnAfterCreated(CefBrowser browser)
        {
            _browser = browser;
            BrowserCreatedEvent?.Invoke();
        }

        protected override void OnBeforeClose(CefBrowser browser)
        {
            _browser = null;
        }

        protected override bool OnBeforePopup(CefBrowser browser, CefFrame frame, string targetUrl, string targetFrameName,
            CefWindowOpenDisposition targetDisposition, bool userGesture, CefPopupFeatures popupFeatures, CefWindowInfo windowInfo,
            ref CefClient client, CefBrowserSettings settings, ref CefDictionaryValue extraInfo, ref bool noJavascriptAccess)
        {
            frame.LoadUrl(targetUrl);
            return true;
        }

        public delegate void BrowserCreatedEventHandler();
        public event BrowserCreatedEventHandler BrowserCreatedEvent;
    }

    internal sealed class NoesisDisplayHandler : CefDisplayHandler
    {
        WebBrowser _browser;

        internal NoesisDisplayHandler(WebBrowser browser)
        {
            _browser = browser;
        }

        protected override void OnAddressChange(CefBrowser browser, CefFrame frame, string url)
        {
            _browser._source = new Uri(url);
        }
    }

    internal sealed class NoesisResourceRequestHandler : CefResourceRequestHandler
    {
        WebBrowser _browser;

        internal NoesisResourceRequestHandler(WebBrowser browser)
        {
            _browser = browser;
        }

        protected override CefCookieAccessFilter GetCookieAccessFilter(CefBrowser browser, CefFrame frame, CefRequest request)
        {
            return null;
        }

        private static string GetHeaderName(System.Net.HttpResponseHeader h)
        {
            switch (h)
            {
                case System.Net.HttpResponseHeader.CacheControl: return "Cache-Control";
                case System.Net.HttpResponseHeader.Connection: return "Connection";
                case System.Net.HttpResponseHeader.Date: return "Date";
                case System.Net.HttpResponseHeader.KeepAlive: return "Keep-Alive";
                case System.Net.HttpResponseHeader.Pragma: return "Pragma";
                case System.Net.HttpResponseHeader.Trailer: return "Trailer";
                case System.Net.HttpResponseHeader.TransferEncoding: return "Transfer-Encoding";
                case System.Net.HttpResponseHeader.Upgrade: return "Upgrade";
                case System.Net.HttpResponseHeader.Via: return "Via";
                case System.Net.HttpResponseHeader.Warning: return "Warning";
                case System.Net.HttpResponseHeader.Allow: return "Allow";
                case System.Net.HttpResponseHeader.ContentLength: return "Content-Length";
                case System.Net.HttpResponseHeader.ContentType: return "Content-Type";
                case System.Net.HttpResponseHeader.ContentEncoding: return "Content-Encoding";
                case System.Net.HttpResponseHeader.ContentLanguage: return "Content-Langauge";
                case System.Net.HttpResponseHeader.ContentLocation: return "Content-Location";
                case System.Net.HttpResponseHeader.ContentMd5: return "Content-MD5";
                case System.Net.HttpResponseHeader.ContentRange: return "Range";
                case System.Net.HttpResponseHeader.Expires: return "Expires";
                case System.Net.HttpResponseHeader.LastModified: return "Last-Modified";
                case System.Net.HttpResponseHeader.AcceptRanges: return "Accept-Ranges";
                case System.Net.HttpResponseHeader.Age: return "Age";
                case System.Net.HttpResponseHeader.ETag: return "Etag";
                case System.Net.HttpResponseHeader.Location: return "Location";
                case System.Net.HttpResponseHeader.ProxyAuthenticate: return "Proxy-Authenticate";
                case System.Net.HttpResponseHeader.RetryAfter: return "Retry-After";
                case System.Net.HttpResponseHeader.Server: return "Server";
                case System.Net.HttpResponseHeader.SetCookie: return "Set-Cookie";
                case System.Net.HttpResponseHeader.Vary: return "Vary";
                case System.Net.HttpResponseHeader.WwwAuthenticate: return "WWW-Authenticate";
            }
            return null;
        }

        private static readonly char[] HttpTrimCharacters =
            new char[] { (char)0x09, (char)0xA, (char)0xB, (char)0xC, (char)0xD, (char)0x20 };

        private static string ValidateValue(string value)
        {
            string validatedValue = "";

            // Trim spaces from both ends
            value = value.Trim(HttpTrimCharacters);

            // First, check for correctly formed multi-line value
            // Second, check for absenece of CTL characters
            int crlf = 0;
            for (int i = 0; i < value.Length; ++i)
            {
                char c = (char)(0x000000ff & (uint)value[i]);

                switch (crlf)
                {
                    case 0:
                    {
                        if (c == '\r')
                        {
                            crlf = 1;
                            validatedValue += c;
                        }
                        else if (c == '\n')
                        {
                            crlf = 2;
                            validatedValue += c;
                        }
                        else if (c == 127 || (c < ' ' && c != '\t'))
                        {
                            // invalid character, ignore it
                        }
                        else
                        {
                            validatedValue += c;
                        }
                        break;
                    }
                    case 1:
                    {
                        if (c == '\n')
                        {
                            validatedValue += c;
                        }
                        else
                        {
                            // we always want a \n after a \r
                            validatedValue += '\n';
                            validatedValue += c;
                        }
                        crlf = 2;
                        break;
                    }
                    case 2:
                    {
                        if (c == ' ' || c == '\t')
                        {
                            validatedValue += c;
                        }
                        else
                        {
                            // we always want a space starting a new line
                            validatedValue += ' ';
                            validatedValue += c;
                        }
                        crlf = 0;
                        break;
                    }
                }
            }
            if (crlf == 1)
            {
                validatedValue += '\n';
            }

            return validatedValue;
        }

        private static System.Net.WebHeaderCollection GetHeaders(System.Collections.Specialized.NameValueCollection responseHeaders)
        {
            var headers = new System.Net.WebHeaderCollection();
            foreach (System.Net.HttpResponseHeader h in Enum.GetValues(typeof(System.Net.HttpResponseHeader)))
            {
                var hName = GetHeaderName(h);
                var value = responseHeaders.Get(hName);
                if (value != null)
                {
                    value = ValidateValue(value);

                    try
                    {
                        headers.Add(h, value);
                    }
                    catch (ArgumentException) { }
                }
            }
            return headers;
        }

        private class NoesisCookieVisitor : CefCookieVisitor
        {
            public delegate void NavigationEventHandler(NavigationEventArgs args);
            internal NoesisCookieVisitor(NavigationEventArgs args, HttpWebResponse webResponse, NavigationEventHandler navigationHandler)
            {
                _args = args;
                _webResponse = webResponse;
                _webResponse._cookies = new System.Net.CookieCollection();
                _navigationHandler = navigationHandler;
            }

            protected override bool Visit(CefCookie cookie, int count, int total, out bool delete)
            {
                var c = new System.Net.Cookie(cookie.Name, cookie.Value, cookie.Path, cookie.Domain);
                c.Secure = cookie.Secure;
                c.HttpOnly = cookie.HttpOnly;
                c.Expires = cookie.Expires.HasValue ? cookie.Expires.Value : DateTime.MaxValue;
                c.Expired = c.Expires < DateTime.Now;
                _webResponse._cookies.Add(c);
                delete = false;
                return true;
            }

            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing);

                if (disposing)
                {
                    _navigationHandler(_args);
                }
            }

            private NavigationEventArgs _args;
            private HttpWebResponse _webResponse;
            NavigationEventHandler _navigationHandler;
        }

        private static void FillCookiesRaiseNavigationEvent(NavigationEventArgs args, HttpWebResponse webResponse, NoesisCookieVisitor.NavigationEventHandler navigationHandler, string url)
        {
            var cookieManager = CefCookieManager.GetGlobal(null);
            if (!cookieManager.VisitUrlCookies(url, true, new NoesisCookieVisitor(args, webResponse, navigationHandler)))
            {
                navigationHandler(args);
            }
        }

        private static HttpWebResponse GetWebResponse(CefRequest request, CefResponse response)
        {
            var responseHeaders = response.GetHeaderMap();
            var webResponse = new HttpWebResponse();
            string contentLength = responseHeaders.Get("Content-Length");
            webResponse._contentLength = contentLength != null ? (long)Convert.ToInt64(contentLength) : -1;
            webResponse._contentType = responseHeaders.Get("Content-Type");
            webResponse._headers = GetHeaders(responseHeaders);
            webResponse._isFromCache = false;
            webResponse._isMutuallyAuthenticated = false;
            webResponse._responseUri = response.Url != null ? new Uri(response.Url) : new Uri(request.Url);
            webResponse._supportsHeaders = false;
            webResponse._statusCode = (System.Net.HttpStatusCode)response.Status;
            webResponse._server = responseHeaders.Get("Server");
            webResponse._protocolVersion = new Version();
            webResponse._method = request.Method;
            var lastModified = responseHeaders.Get("Last-Modified");
            webResponse._lastModified = lastModified != null ? Convert.ToDateTime(lastModified) : DateTime.MinValue;
            webResponse._statusDescription = response.StatusText;
            webResponse._contentEncoding = response.MimeType;
            webResponse._characterSet = response.Charset;
            return webResponse;
        }

        private static void FillArgsAndRaiseNavigationEvent(NoesisCookieVisitor.NavigationEventHandler navigationHandler, CefRequest request, CefResponse response)
        {
            var navigationArgs = new NavigationEventArgs();
            var webResponse = GetWebResponse(request, response);
            navigationArgs.Uri = new Uri(request.Url);
            navigationArgs.WebResponse = webResponse;
            FillCookiesRaiseNavigationEvent(navigationArgs, webResponse, navigationHandler, request.Url);
        }

        protected override bool OnResourceResponse(CefBrowser browser, CefFrame frame, CefRequest request, CefResponse response)
        {
            if (request.ResourceType == CefResourceType.MainFrame)
            {
                FillArgsAndRaiseNavigationEvent(_browser.RaiseNavigated, request, response);
                
            }
            return false;
        }

        protected override void OnResourceLoadComplete(CefBrowser browser, CefFrame frame, CefRequest request, CefResponse response, CefUrlRequestStatus status, long receivedContentLength)
        {
            if (request.ResourceType == CefResourceType.MainFrame)
            {
                FillArgsAndRaiseNavigationEvent(_browser.RaiseLoadCompleted, request, response);
            }
        }
    }

    internal sealed class NoesisRequestHandler : CefRequestHandler
    {
        WebBrowser _browser;

        internal NoesisRequestHandler(WebBrowser browser)
        {
            _browser = browser;
        }

        protected override CefResourceRequestHandler GetResourceRequestHandler(CefBrowser browser, CefFrame frame, CefRequest request, bool isNavigation, bool isDownload, string requestInitiator, ref bool disableDefaultHandling)
        {
            return new NoesisResourceRequestHandler(_browser);
        }
    }

    internal sealed class NoesisClient : CefClient
    {
        public NoesisRenderHandler _renderHandler;
        public NoesisLifeSpanHandler _lifeSpanHandler;
        public NoesisDisplayHandler _displayHandler;
        public NoesisRequestHandler _requestHandler;

        public NoesisClient(WebBrowser browser)
        {
            _renderHandler = new NoesisRenderHandler(browser);
            _lifeSpanHandler = new NoesisLifeSpanHandler();
            _displayHandler = new NoesisDisplayHandler(browser);
            _requestHandler = new NoesisRequestHandler(browser);
        }

        protected override CefRenderHandler GetRenderHandler()
        {
            return _renderHandler;
        }

        protected override CefLifeSpanHandler GetLifeSpanHandler()
        {
            return _lifeSpanHandler;
        }

        protected override CefDisplayHandler GetDisplayHandler()
        {
            return _displayHandler;
        }

        protected override CefRequestHandler GetRequestHandler()
        {
            return _requestHandler;
        }

        internal class NoesisRequest
        {
            public string id {  get; set; }
            public string content { get; set; }
            public string type { get; set; }
            public string label { get; set; }
        }

        protected override bool OnProcessMessageReceived(CefBrowser browser, CefFrame frame, CefProcessId sourceProcess, CefProcessMessage message)
        {
            if (message.Name == "cefQueryMsg")
            {
                var arguments = message.Arguments;

                const int paramsIndex = 2;
                var type = arguments.GetValueType(paramsIndex);

                if (type == CefValueType.String)
                {
                    var @string = arguments.GetString(paramsIndex);
                    NoesisRequest req = JsonSerializer.Deserialize<NoesisRequest>(@string);

                    WebBrowser b = null;
                    lock (WebBrowser._virtualKeyboardRequests)
                    {
                        if (WebBrowser._virtualKeyboardRequests.TryGetValue(req.id, out b))
                        {
                            WebBrowser._virtualKeyboardRequests.Remove(req.id);
                        }
                    }

                    if (b != null)
                    {
                        if (req.type == "password")
                        {
                            b._virtualKeyboardRequestedArgs.InputScope = InputScope.Password;
                        }

                        var label = HttpUtility.HtmlDecode(req.label);
                        CefRuntime.PostTask(CefThreadId.UI, new NoesisDelegateTask(() => { b.RequestVirtualKeyboard(req.content, label); }));
                    }
                }
            }

            return base.OnProcessMessageReceived(browser, frame, sourceProcess, message);
        }
    }

    internal class NoesisDelegateTask : CefTask
    {
        internal delegate void DelegateTask();

        private DelegateTask _delegate;

        internal NoesisDelegateTask(DelegateTask d)
        {
            _delegate = d;
        }

        protected override void Execute()
        {
            _delegate();
        }
    }

    internal class NoesisCompletionCallback : CefCompletionCallback
    {
        private bool _complete = false;

        protected override void OnComplete()
        {
            _complete = true;
        }

        public void WaitOnUIThread()
        {
            while (!_complete)
            {
                CefRuntime.DoMessageLoopWork();
            }
        }
    }

    internal class NoesisDeleteCookiesCallback : CefDeleteCookiesCallback
    {
        private bool _complete = false;

        protected override void OnComplete(int numDeleted)
        {
            _complete = true;
        }

        public void WaitOnUIThread()
        {
            while (!_complete)
            {
                CefRuntime.DoMessageLoopWork();
            }
        }
    }
}
