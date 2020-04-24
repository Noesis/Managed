using System;
using System.Runtime.InteropServices;
using Windows.UI.Core;
using Windows.System;
using Windows.UI.Input;
using Windows.Devices.Input;
using Windows.ApplicationModel.Core;

namespace NoesisApp
{
    public class WinRTDisplay : Display
    {
        public WinRTDisplay()
        {
            CoreWindow window = CoreWindow.GetForCurrentThread();

            window.Activated += OnActivated;
            window.SizeChanged += OnSizeChanged;

            window.KeyDown += OnKeyDown;
            window.KeyUp += OnKeyUp;
            window.CharacterReceived += OnCharacterReceived;

            window.PointerMoved += OnPointerMoved;
            window.PointerPressed += OnPointerPressed;
            window.PointerReleased += OnPointerReleased;
            window.PointerWheelChanged += OnPointerWheel;

            _window = GCHandle.Alloc(window);

            _lastClickTime = 0;
            _lastClickButton = Noesis.MouseButton.Left;
        }

        ~WinRTDisplay()
        {
            _window.Free();
        }

        #region Display overrides
        public override IntPtr NativeHandle
        {
            get { return IntPtr.Zero; }
        }

        public override IntPtr NativeWindow
        {
            get
            {
                return (IntPtr)_window;
            }
        }

        public override int ClientWidth
        {
            get { return (int)CoreWindow.GetForCurrentThread().Bounds.Width; }
        }

        public override int ClientHeight
        {
            get { return (int)CoreWindow.GetForCurrentThread().Bounds.Height; }
        }

        public override void Show()
        {
            LocationChanged(this, 0, 0);
            SizeChanged(this, ClientWidth, ClientHeight);
            CoreWindow.GetForCurrentThread().Activate();
        }

        public override void Close()
        {
            CoreApplication.Exit();
        }

        public override void EnterMessageLoop(bool runInBackground)
        {
            for (;;)
            {
                CoreDispatcher dispatcher = CoreWindow.GetForCurrentThread().Dispatcher;
                dispatcher.ProcessEvents(CoreProcessEventsOption.ProcessAllIfPresent);

                Render(this);
            }
        }
        #endregion

        #region Event handlers
        private void OnActivated(CoreWindow window, WindowActivatedEventArgs args)
        {
            switch (args.WindowActivationState)
            {
                case CoreWindowActivationState.CodeActivated:
                case CoreWindowActivationState.PointerActivated:
                {
                    Activated(this);
                    break;
                }
                case CoreWindowActivationState.Deactivated:
                {
                    Deactivated(this);
                    break;
                }
            }
        }

        private void OnSizeChanged(CoreWindow window, WindowSizeChangedEventArgs args)
        {
            SizeChanged(this, (int)args.Size.Width, (int)args.Size.Height);
        }

        private void OnKeyDown(CoreWindow window, KeyEventArgs args)
        {
            Noesis.Key key = _keys[(int)args.VirtualKey];
            if (key != Noesis.Key.None)
            {
                KeyDown(this, key);
            }
        }

        private void OnKeyUp(CoreWindow window, KeyEventArgs args)
        {
            Noesis.Key key = _keys[(int)args.VirtualKey];
            if (key != Noesis.Key.None)
            {
                KeyUp(this, key);
            }
        }

        private void OnCharacterReceived(CoreWindow window, CharacterReceivedEventArgs args)
        {
            Char(this, args.KeyCode);
        }

        private void OnPointerMoved(CoreWindow window, PointerEventArgs args)
        {
            PointerPoint p = args.CurrentPoint;
            PointerDevice device = p.PointerDevice;

            switch (device.PointerDeviceType)
            {
                case PointerDeviceType.Touch:
                case PointerDeviceType.Pen:
                {
                    TouchMove(this, (int)p.Position.X, (int)p.Position.Y, p.PointerId);
                    break;
                }
                case PointerDeviceType.Mouse:
                {
                    MouseMove(this, (int)p.Position.X, (int)p.Position.Y);
                    break;
                }
            }
        }

        private Noesis.MouseButton MouseButton(PointerPointProperties props)
        {
            if (props.PointerUpdateKind == PointerUpdateKind.LeftButtonPressed ||
                props.PointerUpdateKind == PointerUpdateKind.LeftButtonReleased)
            {
                return Noesis.MouseButton.Left;
            }
            if (props.PointerUpdateKind == PointerUpdateKind.MiddleButtonPressed ||
                props.PointerUpdateKind == PointerUpdateKind.MiddleButtonReleased)
            {
                return Noesis.MouseButton.Middle;
            }
            if (props.PointerUpdateKind == PointerUpdateKind.RightButtonPressed ||
                props.PointerUpdateKind == PointerUpdateKind.RightButtonReleased)
            {
                return Noesis.MouseButton.Right;
            }
            if (props.PointerUpdateKind == PointerUpdateKind.XButton1Pressed ||
                props.PointerUpdateKind == PointerUpdateKind.XButton1Released)
            {
                return Noesis.MouseButton.XButton1;
            }
            if (props.PointerUpdateKind == PointerUpdateKind.XButton2Pressed ||
                props.PointerUpdateKind == PointerUpdateKind.XButton2Released)
            {
                return Noesis.MouseButton.XButton2;
            }
            return Noesis.MouseButton.Left;
        }

        private void OnPointerPressed(CoreWindow window, PointerEventArgs args)
        {
            PointerPoint p = args.CurrentPoint;
            PointerDevice device = p.PointerDevice;

            switch (device.PointerDeviceType)
            {
                case PointerDeviceType.Touch:
                case PointerDeviceType.Pen:
                {
                    TouchDown(this, (int)p.Position.X, (int)p.Position.Y, p.PointerId);
                    break;
                }
                case PointerDeviceType.Mouse:
                {
                    ulong time = p.Timestamp;
                    ulong elapsedTime = (time - _lastClickTime) / 1000;
                    Noesis.MouseButton button = MouseButton(p.Properties);
                    if (elapsedTime <= (ulong)GetDoubleClickTime() && button == _lastClickButton)
                    {
                        MouseDoubleClick(this, (int)p.Position.X, (int)p.Position.Y, button);
                        _lastClickTime = 0;
                    }
                    else
                    {
                        MouseButtonDown(this, (int)p.Position.X, (int)p.Position.Y, button);
                        _lastClickTime = time;
                    }
                    _lastClickButton = button;
                    break;
                }
            }
        }

        private void OnPointerReleased(CoreWindow window, PointerEventArgs args)
        {
            PointerPoint p = args.CurrentPoint;
            PointerDevice device = p.PointerDevice;

            switch (device.PointerDeviceType)
            {
                case PointerDeviceType.Touch:
                case PointerDeviceType.Pen:
                {
                    TouchUp(this, (int)p.Position.X, (int)p.Position.Y, p.PointerId);
                    break;
                }
                case PointerDeviceType.Mouse:
                {
                    MouseButtonUp(this, (int)p.Position.X, (int)p.Position.Y, MouseButton(p.Properties));
                    break;
                }
            }
        }

        private void OnPointerWheel(CoreWindow window, PointerEventArgs args)
        {
            PointerPoint p = args.CurrentPoint;
            PointerDevice device = p.PointerDevice;

            if (p.Properties.IsHorizontalMouseWheel)
            {
                MouseHWheel(this, (int)p.Position.X, (int)p.Position.Y, p.Properties.MouseWheelDelta);
            }
            else
            {
                MouseWheel(this, (int)p.Position.X, (int)p.Position.Y, p.Properties.MouseWheelDelta);
            }
        }
        #endregion

        #region Key table
        private static Noesis.Key[] _keys = CreateKeysTable();

        private static Noesis.Key[] CreateKeysTable()
        {
            Noesis.Key[] keys = new Noesis.Key[256];

            keys[(int)VirtualKey.LeftButton] = Noesis.Key.None;
            keys[(int)VirtualKey.RightButton] = Noesis.Key.None;
            keys[(int)VirtualKey.Cancel] = Noesis.Key.Cancel;
            keys[(int)VirtualKey.MiddleButton] = Noesis.Key.None;
            keys[(int)VirtualKey.XButton1] = Noesis.Key.None;
            keys[(int)VirtualKey.XButton2] = Noesis.Key.None;
            keys[(int)VirtualKey.Back] = Noesis.Key.Back;
            keys[(int)VirtualKey.Tab] = Noesis.Key.Tab;
            keys[(int)VirtualKey.Clear] = Noesis.Key.Clear;
            keys[(int)VirtualKey.Enter] = Noesis.Key.Return;
            keys[(int)VirtualKey.Shift] = Noesis.Key.LeftShift;
            keys[(int)VirtualKey.Control] = Noesis.Key.LeftCtrl;
            keys[(int)VirtualKey.Menu] = Noesis.Key.LeftAlt;
            keys[(int)VirtualKey.Pause] = Noesis.Key.Pause;
            keys[(int)VirtualKey.CapitalLock] = Noesis.Key.Capital;
            keys[(int)VirtualKey.Kana] = Noesis.Key.KanaMode;
            keys[(int)VirtualKey.Hangul] = Noesis.Key.HangulMode;
            keys[(int)VirtualKey.Junja] = Noesis.Key.JunjaMode;
            keys[(int)VirtualKey.Final] = Noesis.Key.None;
            keys[(int)VirtualKey.Kanji] = Noesis.Key.KanjiMode;
            keys[(int)VirtualKey.Escape] = Noesis.Key.Escape;
            keys[(int)VirtualKey.Convert] = Noesis.Key.ImeConvert;
            keys[(int)VirtualKey.NonConvert] = Noesis.Key.ImeNonConvert;
            keys[(int)VirtualKey.Accept] = Noesis.Key.ImeAccept;
            keys[(int)VirtualKey.ModeChange] = Noesis.Key.ImeModeChange;
            keys[(int)VirtualKey.Space] = Noesis.Key.Space;
            keys[(int)VirtualKey.PageUp] = Noesis.Key.Prior;
            keys[(int)VirtualKey.PageDown] = Noesis.Key.Next;
            keys[(int)VirtualKey.End] = Noesis.Key.End;
            keys[(int)VirtualKey.Home] = Noesis.Key.Home;
            keys[(int)VirtualKey.Left] = Noesis.Key.Left;
            keys[(int)VirtualKey.Up] = Noesis.Key.Up;
            keys[(int)VirtualKey.Right] = Noesis.Key.Right;
            keys[(int)VirtualKey.Down] = Noesis.Key.Down;
            keys[(int)VirtualKey.Select] = Noesis.Key.Select;
            keys[(int)VirtualKey.Print] = Noesis.Key.Print;
            keys[(int)VirtualKey.Execute] = Noesis.Key.Execute;
            keys[(int)VirtualKey.Snapshot] = Noesis.Key.Snapshot;
            keys[(int)VirtualKey.Insert] = Noesis.Key.Insert;
            keys[(int)VirtualKey.Delete] = Noesis.Key.Delete;
            keys[(int)VirtualKey.Help] = Noesis.Key.Help;
            keys[(int)VirtualKey.Number0] = Noesis.Key.D0;
            keys[(int)VirtualKey.Number1] = Noesis.Key.D1;
            keys[(int)VirtualKey.Number2] = Noesis.Key.D2;
            keys[(int)VirtualKey.Number3] = Noesis.Key.D3;
            keys[(int)VirtualKey.Number4] = Noesis.Key.D4;
            keys[(int)VirtualKey.Number5] = Noesis.Key.D5;
            keys[(int)VirtualKey.Number6] = Noesis.Key.D6;
            keys[(int)VirtualKey.Number7] = Noesis.Key.D7;
            keys[(int)VirtualKey.Number8] = Noesis.Key.D8;
            keys[(int)VirtualKey.Number9] = Noesis.Key.D9;
            keys[(int)VirtualKey.A] = Noesis.Key.A;
            keys[(int)VirtualKey.B] = Noesis.Key.B;
            keys[(int)VirtualKey.C] = Noesis.Key.C;
            keys[(int)VirtualKey.D] = Noesis.Key.D;
            keys[(int)VirtualKey.E] = Noesis.Key.E;
            keys[(int)VirtualKey.F] = Noesis.Key.F;
            keys[(int)VirtualKey.G] = Noesis.Key.G;
            keys[(int)VirtualKey.H] = Noesis.Key.H;
            keys[(int)VirtualKey.I] = Noesis.Key.I;
            keys[(int)VirtualKey.J] = Noesis.Key.J;
            keys[(int)VirtualKey.K] = Noesis.Key.K;
            keys[(int)VirtualKey.L] = Noesis.Key.L;
            keys[(int)VirtualKey.M] = Noesis.Key.M;
            keys[(int)VirtualKey.N] = Noesis.Key.N;
            keys[(int)VirtualKey.O] = Noesis.Key.O;
            keys[(int)VirtualKey.P] = Noesis.Key.P;
            keys[(int)VirtualKey.Q] = Noesis.Key.Q;
            keys[(int)VirtualKey.R] = Noesis.Key.R;
            keys[(int)VirtualKey.S] = Noesis.Key.S;
            keys[(int)VirtualKey.T] = Noesis.Key.T;
            keys[(int)VirtualKey.U] = Noesis.Key.U;
            keys[(int)VirtualKey.V] = Noesis.Key.V;
            keys[(int)VirtualKey.W] = Noesis.Key.W;
            keys[(int)VirtualKey.X] = Noesis.Key.X;
            keys[(int)VirtualKey.Y] = Noesis.Key.Y;
            keys[(int)VirtualKey.Z] = Noesis.Key.Z;
            keys[(int)VirtualKey.LeftWindows] = Noesis.Key.LWin;
            keys[(int)VirtualKey.RightWindows] = Noesis.Key.RWin;
            keys[(int)VirtualKey.Application] = Noesis.Key.Apps;
            keys[(int)VirtualKey.Sleep] = Noesis.Key.Sleep;
            keys[(int)VirtualKey.NumberPad0] = Noesis.Key.NumPad0;
            keys[(int)VirtualKey.NumberPad1] = Noesis.Key.NumPad1;
            keys[(int)VirtualKey.NumberPad2] = Noesis.Key.NumPad2;
            keys[(int)VirtualKey.NumberPad3] = Noesis.Key.NumPad3;
            keys[(int)VirtualKey.NumberPad4] = Noesis.Key.NumPad4;
            keys[(int)VirtualKey.NumberPad5] = Noesis.Key.NumPad5;
            keys[(int)VirtualKey.NumberPad6] = Noesis.Key.NumPad6;
            keys[(int)VirtualKey.NumberPad7] = Noesis.Key.NumPad7;
            keys[(int)VirtualKey.NumberPad8] = Noesis.Key.NumPad8;
            keys[(int)VirtualKey.NumberPad9] = Noesis.Key.NumPad9;
            keys[(int)VirtualKey.Multiply] = Noesis.Key.Multiply;
            keys[(int)VirtualKey.Add] = Noesis.Key.Add;
            keys[(int)VirtualKey.Separator] = Noesis.Key.Separator;
            keys[(int)VirtualKey.Subtract] = Noesis.Key.Subtract;
            keys[(int)VirtualKey.Decimal] = Noesis.Key.Decimal;
            keys[(int)VirtualKey.Divide] = Noesis.Key.Divide;
            keys[(int)VirtualKey.F1] = Noesis.Key.F1;
            keys[(int)VirtualKey.F2] = Noesis.Key.F2;
            keys[(int)VirtualKey.F3] = Noesis.Key.F3;
            keys[(int)VirtualKey.F4] = Noesis.Key.F4;
            keys[(int)VirtualKey.F5] = Noesis.Key.F5;
            keys[(int)VirtualKey.F6] = Noesis.Key.F6;
            keys[(int)VirtualKey.F7] = Noesis.Key.F7;
            keys[(int)VirtualKey.F8] = Noesis.Key.F8;
            keys[(int)VirtualKey.F9] = Noesis.Key.F9;
            keys[(int)VirtualKey.F10] = Noesis.Key.F10;
            keys[(int)VirtualKey.F11] = Noesis.Key.F11;
            keys[(int)VirtualKey.F12] = Noesis.Key.F12;
            keys[(int)VirtualKey.F13] = Noesis.Key.F13;
            keys[(int)VirtualKey.F14] = Noesis.Key.F14;
            keys[(int)VirtualKey.F15] = Noesis.Key.F15;
            keys[(int)VirtualKey.F16] = Noesis.Key.F16;
            keys[(int)VirtualKey.F17] = Noesis.Key.F17;
            keys[(int)VirtualKey.F18] = Noesis.Key.F18;
            keys[(int)VirtualKey.F19] = Noesis.Key.F19;
            keys[(int)VirtualKey.F20] = Noesis.Key.F20;
            keys[(int)VirtualKey.F21] = Noesis.Key.F21;
            keys[(int)VirtualKey.F22] = Noesis.Key.F22;
            keys[(int)VirtualKey.F23] = Noesis.Key.F23;
            keys[(int)VirtualKey.F24] = Noesis.Key.F24;
            keys[(int)VirtualKey.NavigationView] = Noesis.Key.None;
            keys[(int)VirtualKey.NavigationMenu] = Noesis.Key.None;
            keys[(int)VirtualKey.NavigationUp] = Noesis.Key.None;
            keys[(int)VirtualKey.NavigationDown] = Noesis.Key.None;
            keys[(int)VirtualKey.NavigationLeft] = Noesis.Key.None;
            keys[(int)VirtualKey.NavigationRight] = Noesis.Key.None;
            keys[(int)VirtualKey.NavigationAccept] = Noesis.Key.None;
            keys[(int)VirtualKey.NavigationCancel] = Noesis.Key.None;
            keys[(int)VirtualKey.NumberKeyLock] = Noesis.Key.NumLock;
            keys[(int)VirtualKey.Scroll] = Noesis.Key.Scroll;
            keys[(int)VirtualKey.LeftShift] = Noesis.Key.LeftShift;
            keys[(int)VirtualKey.RightShift] = Noesis.Key.RightShift;
            keys[(int)VirtualKey.LeftControl] = Noesis.Key.LeftCtrl;
            keys[(int)VirtualKey.RightControl] = Noesis.Key.RightCtrl;
            keys[(int)VirtualKey.LeftMenu] = Noesis.Key.LeftAlt;
            keys[(int)VirtualKey.RightMenu] = Noesis.Key.RightAlt;
            keys[(int)VirtualKey.GoBack] = Noesis.Key.BrowserBack;
            keys[(int)VirtualKey.GoForward] = Noesis.Key.BrowserForward;
            keys[(int)VirtualKey.Refresh] = Noesis.Key.BrowserRefresh;
            keys[(int)VirtualKey.Stop] = Noesis.Key.BrowserStop;
            keys[(int)VirtualKey.Search] = Noesis.Key.BrowserSearch;
            keys[(int)VirtualKey.Favorites] = Noesis.Key.BrowserFavorites;
            keys[(int)VirtualKey.GoHome] = Noesis.Key.BrowserHome;
            keys[(int)VirtualKey.GamepadA] = Noesis.Key.GamepadAccept;
            keys[(int)VirtualKey.GamepadB] = Noesis.Key.GamepadCancel;
            keys[(int)VirtualKey.GamepadX] = Noesis.Key.GamepadContext1;
            keys[(int)VirtualKey.GamepadY] = Noesis.Key.GamepadContext2;
            keys[(int)VirtualKey.GamepadRightShoulder] = Noesis.Key.GamepadPageRight;
            keys[(int)VirtualKey.GamepadLeftShoulder] = Noesis.Key.GamepadPageLeft;
            keys[(int)VirtualKey.GamepadLeftTrigger] = Noesis.Key.GamepadPageUp;
            keys[(int)VirtualKey.GamepadRightTrigger] = Noesis.Key.GamepadPageDown;
            keys[(int)VirtualKey.GamepadDPadUp] = Noesis.Key.GamepadUp;
            keys[(int)VirtualKey.GamepadDPadDown] = Noesis.Key.GamepadDown;
            keys[(int)VirtualKey.GamepadDPadLeft] = Noesis.Key.GamepadLeft;
            keys[(int)VirtualKey.GamepadDPadRight] = Noesis.Key.GamepadRight;
            keys[(int)VirtualKey.GamepadMenu] = Noesis.Key.GamepadMenu;
            keys[(int)VirtualKey.GamepadView] = Noesis.Key.GamepadView;
            keys[(int)VirtualKey.GamepadLeftThumbstickButton] = Noesis.Key.GamepadContext3;
            keys[(int)VirtualKey.GamepadRightThumbstickButton] = Noesis.Key.GamepadContext4;
            keys[(int)VirtualKey.GamepadLeftThumbstickUp] = Noesis.Key.GamepadUp;
            keys[(int)VirtualKey.GamepadLeftThumbstickDown] = Noesis.Key.GamepadDown;
            keys[(int)VirtualKey.GamepadLeftThumbstickRight] = Noesis.Key.GamepadRight;
            keys[(int)VirtualKey.GamepadLeftThumbstickLeft] = Noesis.Key.GamepadLeft;
            keys[(int)VirtualKey.GamepadRightThumbstickUp] = Noesis.Key.GamepadPageUp;
            keys[(int)VirtualKey.GamepadRightThumbstickDown] = Noesis.Key.GamepadPageDown;
            keys[(int)VirtualKey.GamepadRightThumbstickRight] = Noesis.Key.GamepadPageRight;
            keys[(int)VirtualKey.GamepadRightThumbstickLeft] = Noesis.Key.GamepadPageLeft;

            return keys;
        }
        #endregion

        #region Imports
        [DllImport("user32", CharSet = CharSet.Auto, ExactSpelling = true)]
        private static extern int GetDoubleClickTime();
        #endregion

        GCHandle _window;
        ulong _lastClickTime;
        Noesis.MouseButton _lastClickButton;
    }
}
