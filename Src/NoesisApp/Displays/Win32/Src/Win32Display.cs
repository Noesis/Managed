using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace NoesisApp
{
    public class Win32Display : Display
    {
        private const string ClassName = "NoesisWindow";

        public Win32Display()
        {
            _shcore = WinApi.LoadLibrary("shcore.dll");
            if (_shcore != IntPtr.Zero)
            {
                IntPtr setProcessDpiAwarePtr = WinApi.GetProcAddress(_shcore, "SetProcessDpiAwareness");
                if (setProcessDpiAwarePtr != IntPtr.Zero)
                {
                    WinApi.SetProcessDpiAwareness setProcessDpiAwareness =
                        Marshal.GetDelegateForFunctionPointer<WinApi.SetProcessDpiAwareness>(setProcessDpiAwarePtr);

                    setProcessDpiAwareness(WinApi.ProcessDpiAwareness.PROCESS_PER_MONITOR_DPI_AWARE);
                }

                IntPtr getDpiForMonitorPtr = WinApi.GetProcAddress(_shcore, "GetDpiForMonitor");
                if (getDpiForMonitorPtr != IntPtr.Zero)
                {
                    _getDpiForMonitor = Marshal.GetDelegateForFunctionPointer<WinApi.GetDpiForMonitor>(getDpiForMonitorPtr);
                }
            }


            IntPtr hInstance = WinApi.GetModuleHandle(null);
            WinApi.WindowClassEx windowClass;
            if (!WinApi.GetClassInfoEx(hInstance, ClassName, out windowClass))
            {
                IntPtr ClassNameHandle = System.Runtime.InteropServices.Marshal.StringToHGlobalUni(ClassName);
                windowClass = new WinApi.WindowClassEx
                {
                    Size = (uint)Noesis.Marshal.SizeOf<WinApi.WindowClassEx>(),
                    Styles = WinApi.WindowClassStyles.CS_DBLCLKS,
                    WindowProc = _wndProc,
                    ClassExtraBytes = 0,
                    WindowExtraBytes = 0,
                    InstanceHandle = hInstance,
                    IconHandle = WinApi.LoadIcon(hInstance, new IntPtr(101)),
                    SmallIconHandle = WinApi.LoadIcon(hInstance, new IntPtr(101)),
                    CursorHandle = IntPtr.Zero,
                    BackgroundBrushHandle = IntPtr.Zero,
                    MenuName = IntPtr.Zero,
                    ClassName = ClassNameHandle
                };

                WinApi.RegisterClassEx(ref windowClass);

                System.Runtime.InteropServices.Marshal.FreeHGlobal(ClassNameHandle);
            }

            WinApi.WindowStyles flags;
            WinApi.WindowExStyles flagsEx;
            BuildStyleFlags(WindowStyle.SingleBorderWindow, WindowState.Normal, ResizeMode.CanResize,
                true, false, false, out flags, out flagsEx);

            GCHandle display = GCHandle.Alloc(this);
            try
            {
                IntPtr wnd = WinApi.CreateWindowEx(flagsEx, ClassName, "", flags,
                    (int)WinApi.CreateWindowFlags.CW_USEDEFAULT,
                    (int)WinApi.CreateWindowFlags.CW_USEDEFAULT,
                    (int)WinApi.CreateWindowFlags.CW_USEDEFAULT,
                    (int)WinApi.CreateWindowFlags.CW_USEDEFAULT,
                    IntPtr.Zero, IntPtr.Zero, hInstance, GCHandle.ToIntPtr(display));
            }
            finally
            {
                display.Free();
            }
        }

        ~Win32Display()
        {
            WinApi.ReleaseDC(_hWnd, _hDC);
            _hDC = IntPtr.Zero;
            RemoveDisplay(NativeWindow);

            if (_shcore != IntPtr.Zero)
            {
                WinApi.FreeLibrary(_shcore);
            }
        }

        #region Display overrides
        public override IntPtr NativeHandle
        {
            get { return _hDC; }
        }

        public override IntPtr NativeWindow
        {
            get { return _hWnd; }
        }

        public override int ClientWidth
        {
            get
            {
                WinApi.Rectangle r;
                WinApi.GetClientRect(NativeWindow, out r);
                return r.Right - r.Left;
            }
        }

        public override int ClientHeight
        {
            get
            {
                WinApi.Rectangle r;
                WinApi.GetClientRect(NativeWindow, out r);
                return r.Bottom - r.Top;
            }
        }

        public override float Scale
        {
            get
            {
                uint dpiX = 96, dpiY = 96;
                if (_getDpiForMonitor != null)
                {
                    IntPtr hMonitor = WinApi.MonitorFromWindow(NativeWindow, WinApi.MonitorFlag.MONITOR_DEFAULTTONEAREST);
                    _getDpiForMonitor(hMonitor, WinApi.MonitorDpiType.MDT_EFFECTIVE_DPI, out dpiX, out dpiY);
                }
                return dpiX / 96.0f;
            }
        }

        public override void AdjustWindowSize(ref int width, ref int height)
        {
            WinApi.Rectangle r = new WinApi.Rectangle
            {
                Left = 0, Top = 0, Right = width, Bottom = height
            };

            WinApi.WindowStyles flags = (WinApi.WindowStyles)WinApi.GetWindowLong(NativeWindow,
                (int)WinApi.WindowLongFlags.GWL_STYLE);
            WinApi.WindowExStyles flagsEx = (WinApi.WindowExStyles)WinApi.GetWindowLong(NativeWindow,
                (int)WinApi.WindowLongFlags.GWL_EXSTYLE);

            WinApi.AdjustWindowRectEx(ref r, (uint)flags, false, (uint)flagsEx);

            width = r.Right - r.Left;
            height = r.Bottom - r.Top;
        }

        public override void SetTitle(string title)
        {
            WinApi.SetWindowText(NativeWindow, title);
        }

        public override void SetLocation(int x, int y)
        {
            WinApi.SetWindowPos(NativeWindow, IntPtr.Zero, x, y, 0, 0,
                WinApi.WindowPositionFlags.SWP_NOACTIVATE |
                WinApi.WindowPositionFlags.SWP_NOZORDER |
                WinApi.WindowPositionFlags.SWP_NOSIZE);
        }

        public override void SetSize(int width, int height)
        {
            WinApi.SetWindowPos(NativeWindow, IntPtr.Zero, 0, 0, width, height,
                WinApi.WindowPositionFlags.SWP_NOACTIVATE |
                WinApi.WindowPositionFlags.SWP_NOZORDER |
                WinApi.WindowPositionFlags.SWP_NOMOVE);
        }

        public override void SetWindowStyle(WindowStyle windowStyle)
        {
            WinApi.WindowStyles flags = (WinApi.WindowStyles)WinApi.GetWindowLong(NativeWindow,
                (int)WinApi.WindowLongFlags.GWL_STYLE);
            WinApi.WindowExStyles flagsEx = (WinApi.WindowExStyles)WinApi.GetWindowLong(NativeWindow,
                (int)WinApi.WindowLongFlags.GWL_EXSTYLE);

            BuildStyleFlags(windowStyle, GetState(flags), GetResizeMode(flags),
                GetShowInTaskbar(flagsEx), GetTopmost(flagsEx), GetAllowFileDrop(flagsEx),
                out flags, out flagsEx);

            SetStyleFlags(flags, flagsEx);
        }

        public override void SetWindowState(WindowState windowState)
        {
            WinApi.WindowStyles flags = (WinApi.WindowStyles)WinApi.GetWindowLong(NativeWindow,
                (int)WinApi.WindowLongFlags.GWL_STYLE);
            WinApi.WindowExStyles flagsEx = (WinApi.WindowExStyles)WinApi.GetWindowLong(NativeWindow,
                (int)WinApi.WindowLongFlags.GWL_EXSTYLE);

            BuildStyleFlags(GetStyle(flags, flagsEx), windowState, GetResizeMode(flags),
                GetShowInTaskbar(flagsEx), GetTopmost(flagsEx), GetAllowFileDrop(flagsEx),
                out flags, out flagsEx);

            SetStyleFlags(flags, flagsEx);
        }

        public override void SetResizeMode(ResizeMode resizeMode)
        {
            WinApi.WindowStyles flags = (WinApi.WindowStyles)WinApi.GetWindowLong(NativeWindow,
                (int)WinApi.WindowLongFlags.GWL_STYLE);
            WinApi.WindowExStyles flagsEx = (WinApi.WindowExStyles)WinApi.GetWindowLong(NativeWindow,
                (int)WinApi.WindowLongFlags.GWL_EXSTYLE);

            BuildStyleFlags(GetStyle(flags, flagsEx), GetState(flags), resizeMode,
                GetShowInTaskbar(flagsEx), GetTopmost(flagsEx), GetAllowFileDrop(flagsEx),
                out flags, out flagsEx);

            SetStyleFlags(flags, flagsEx);
        }

        public override void SetShowInTaskbar(bool showInTaskbar)
        {
            WinApi.WindowStyles flags = (WinApi.WindowStyles)WinApi.GetWindowLong(NativeWindow,
                (int)WinApi.WindowLongFlags.GWL_STYLE);
            WinApi.WindowExStyles flagsEx = (WinApi.WindowExStyles)WinApi.GetWindowLong(NativeWindow,
                (int)WinApi.WindowLongFlags.GWL_EXSTYLE);

            BuildStyleFlags(GetStyle(flags, flagsEx), GetState(flags), GetResizeMode(flags),
                showInTaskbar, GetTopmost(flagsEx), GetAllowFileDrop(flagsEx),
                out flags, out flagsEx);

            SetStyleFlags(flags, flagsEx);
        }

        public override void SetTopmost(bool topmost)
        {
            WinApi.WindowStyles flags = (WinApi.WindowStyles)WinApi.GetWindowLong(NativeWindow,
                (int)WinApi.WindowLongFlags.GWL_STYLE);
            WinApi.WindowExStyles flagsEx = (WinApi.WindowExStyles)WinApi.GetWindowLong(NativeWindow,
                (int)WinApi.WindowLongFlags.GWL_EXSTYLE);

            BuildStyleFlags(GetStyle(flags, flagsEx), GetState(flags), GetResizeMode(flags),
                GetShowInTaskbar(flagsEx), topmost, GetAllowFileDrop(flagsEx),
                out flags, out flagsEx);

            SetStyleFlags(flags, flagsEx);
        }

        public override void SetAllowFileDrop(bool allowFileDrop)
        {
            WinApi.WindowStyles flags = (WinApi.WindowStyles)WinApi.GetWindowLong(NativeWindow,
                (int)WinApi.WindowLongFlags.GWL_STYLE);
            WinApi.WindowExStyles flagsEx = (WinApi.WindowExStyles)WinApi.GetWindowLong(NativeWindow,
                (int)WinApi.WindowLongFlags.GWL_EXSTYLE);

            BuildStyleFlags(GetStyle(flags, flagsEx), GetState(flags), GetResizeMode(flags),
                GetShowInTaskbar(flagsEx), GetTopmost(flagsEx), allowFileDrop,
                out flags, out flagsEx);

            SetStyleFlags(flags, flagsEx);
        }

        public override void SetWindowStartupLocation(WindowStartupLocation location)
        {
            _startupLocation = location;
        }

        public override void Show()
        {
            switch (_startupLocation)
            {
                case WindowStartupLocation.Manual:
                {
                    break;
                }
                case WindowStartupLocation.CenterScreen:
                case WindowStartupLocation.CenterOwner:
                {
                    CenterWindow();
                    break;
                }
            }

            WinApi.WindowStyles flags = (WinApi.WindowStyles)WinApi.GetWindowLong(NativeWindow,
                (int)WinApi.WindowLongFlags.GWL_STYLE);

            switch (GetState(flags))
            {
                case WindowState.Maximized:
                {
                    WinApi.ShowWindow(NativeWindow, WinApi.ShowWindowCommands.SW_SHOWMAXIMIZED);
                    break;
                }
                case WindowState.Minimized:
                {
                    WinApi.ShowWindow(NativeWindow, WinApi.ShowWindowCommands.SW_SHOWMINIMIZED);
                    break;
                }
                case WindowState.Normal:
                {
                    WinApi.ShowWindow(NativeWindow, WinApi.ShowWindowCommands.SW_SHOWNORMAL);
                    break;
                }
            }
        }

        public override void Close()
        {
            WinApi.SendMessage(NativeWindow, (uint)WinApi.WM.CLOSE, IntPtr.Zero, IntPtr.Zero);
        }

        public override void OpenSoftwareKeyboard(Noesis.UIElement focused)
        {
        }

        public override void CloseSoftwareKeyboard()
        {
        }

        public override void SetCursor(Noesis.Cursor cursor)
        {
            _cursor = cursor;

            // As explained in MSDN documentation WM_SETCURSOR message is not sent if mouse is
            // captured by a window, so we have to manually send the message here otherwise
            // drag & drop cursor changes won't be visible
            if (WinApi.GetCapture() != IntPtr.Zero)
            {
                WinApi.SendMessage(NativeWindow, (uint)WinApi.WM.SETCURSOR,
                    IntPtr.Zero, new IntPtr((int)WinApi.HitTestResult.HTCLIENT));
            }
        }

        public override void OpenUrl(string url)
        {
            if (WinApi.ShellExecute(IntPtr.Zero, null, url, null, null,
                (int)WinApi.ShowWindowCommands.SW_SHOW).ToInt32() <= 32)
            {
                // If the link does not open it is probably a local URL. Try using the exe path
                StringBuilder exe = new StringBuilder(512);
                WinApi.GetModuleFileName(IntPtr.Zero, exe, exe.Capacity);

                string path = System.IO.Path.GetDirectoryName(exe.ToString()) + "/" + url;

                WinApi.ShellExecute(IntPtr.Zero, null, path, null, null, 
                    (int)WinApi.ShowWindowCommands.SW_SHOW);
            }
        }

        public override void PlayAudio(string filename, float volume)
        {
        }

        public override void EnterMessageLoop(bool runInBackground)
        {
            for (;;)
            {
                WinApi.Message msg;
                bool inspector = Noesis.GUI.IsInspectorConnected;
                bool sleep = !inspector && !runInBackground && NativeWindow != WinApi.GetForegroundWindow();

                while (GetNextMessage(sleep, out msg))
                {
                    if (msg.message == (uint)WinApi.WM.QUIT)
                    {
                        return;
                    }

                    WinApi.TranslateMessage(ref msg);
                    WinApi.DispatchMessage(ref msg);
                    sleep = false;
                }

                Render?.Invoke(this);
            }
        }
        #endregion

        #region WindowProc
        private static WinApi.WindowProc _wndProc = WndProc;

        private static IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
        {
            Win32Display display = null;

            if (msg == (uint)WinApi.WM.NCCREATE)
            {
                WinApi.CreateStruct info = Noesis.Marshal.PtrToStructure<WinApi.CreateStruct>(lParam);
                display = (Win32Display)GCHandle.FromIntPtr(info.CreateParams).Target;
                display._hWnd = hWnd;
                display._hDC = WinApi.GetDC(hWnd);
                AddDisplay(hWnd, display);
            }
            else
            {
                display = GetDisplay(hWnd);
            }

            if (display != null && display.DispatchEvent(msg, wParam, lParam))
            {
                return IntPtr.Zero;
            }

            return WinApi.DefWindowProc(hWnd, msg, wParam, lParam);
        }

        private bool DispatchEvent(uint msg, IntPtr wParam, IntPtr lParam)
        {
            switch ((WinApi.WM)msg)
            {
                case WinApi.WM.SETCURSOR:
                {
                    if (LoWord(lParam) == (int)WinApi.HitTestResult.HTCLIENT)
                    {
                        if (_cursors[(int)_cursor] != IntPtr.Zero)
                        {
                            WinApi.SetCursor(_cursors[(int)_cursor]);
                            return true;
                        }
                    }
                    return false;
                }
                case WinApi.WM.MOVE:
                {
                    WinApi.Rectangle r;
                    WinApi.GetWindowRect(NativeWindow, out r);
                    LocationChanged?.Invoke(this, r.Left, r.Top);
                    return false;
                }
                case WinApi.WM.EXITSIZEMOVE:
                {
                    //TSF::MoveWindow(NativeWindow);
                    return false;
                }
                case WinApi.WM.SIZE:
                {
                    WinApi.Rectangle r;
                    WinApi.GetWindowRect(NativeWindow, out r);
                    int width = r.Right - r.Left;
                    int height = r.Bottom - r.Top;

                    switch ((WinApi.WindowSizeFlag)Word(wParam))
                    {
                        case WinApi.WindowSizeFlag.SIZE_RESTORED:
                        {
                            StateChanged?.Invoke(this, WindowState.Normal);
                            SizeChanged?.Invoke(this, width, height);
                            break;
                        }
                        case WinApi.WindowSizeFlag.SIZE_MAXIMIZED:
                        {
                            StateChanged?.Invoke(this, WindowState.Maximized);
                            SizeChanged?.Invoke(this, width, height);
                            break;
                        }
                        case WinApi.WindowSizeFlag.SIZE_MINIMIZED:
                        {
                            StateChanged?.Invoke(this, WindowState.Minimized);
                            break;
                        }
                    }

                    return false;
                }
                case WinApi.WM.ACTIVATE:
                {
                    switch ((WinApi.WindowActivateFlag)LoWord(wParam))
                    {
                        case WinApi.WindowActivateFlag.WA_INACTIVE:
                        {
                            Deactivated?.Invoke(this);
                            //TSF::DeactivateWindow(NativeWindow);
                            break;
                        }

                        default:
                        {
                            //TSF::ActivateWindow(NativeWindow);
                            Activated?.Invoke(this);
                            break;
                        }
                    }

                    return false;
                }
                case WinApi.WM.CLOSE:
                {
                    bool cancel = false;
                    Closing?.Invoke(this, ref cancel);
                    if (cancel)
                    {
                        return true;
                    }

                    return false;
                }
                case WinApi.WM.DESTROY:
                {
                    Closed?.Invoke(this);

                    WinApi.PostQuitMessage(0);
                    return false;
                }
                case WinApi.WM.DROPFILES:
                {
                    //HDROP hDrop = (HDROP)wParam;
                    //unsigned int numFiles = DragQueryFileW(hDrop, 0xFFFFFFFF, NULL, 0);

                    //for (unsigned int i = 0; i < numFiles; ++i)
                    //{
                    //    WCHAR filenameU32[MAX_PATH];
                    //    if (DragQueryFileW(hDrop, i, filenameU32, MAX_PATH) != 0)
                    //    {
                    //        char filename[MAX_PATH];
                    //        uint32_t numChars = UTF8::UTF16To8((uint16_t*)filenameU32, filename, MAX_PATH);
                    //        NS_ASSERT(numChars <= MAX_PATH);

                    //        mFileDropped(this, filename);
                    //    }
                    //}

                    //DragFinish(hDrop);
                    //SetForegroundWindow(NativeWindow);

                    return false;
                }
                // BEGIN - Mouse Input Notifications
                case WinApi.WM.MOUSEMOVE:
                {
                    MouseMove?.Invoke(this, LoWord(lParam), HiWord(lParam));
                    return false;
                }
                case WinApi.WM.LBUTTONDOWN:
                {
                    WinApi.SetCapture(NativeWindow);
                    WinApi.SetFocus(NativeWindow);
                    MouseButtonDown?.Invoke(this, LoWord(lParam), HiWord(lParam), Noesis.MouseButton.Left);
                    return false;
                }
                case WinApi.WM.MBUTTONDOWN:
                {
                    WinApi.SetCapture(NativeWindow);
                    WinApi.SetFocus(NativeWindow);
                    MouseButtonDown?.Invoke(this, LoWord(lParam), HiWord(lParam), Noesis.MouseButton.Middle);
                    return false;
                }
                case WinApi.WM.RBUTTONDOWN:
                {
                    WinApi.SetCapture(NativeWindow);
                    WinApi.SetFocus(NativeWindow);
                    MouseButtonDown?.Invoke(this, LoWord(lParam), HiWord(lParam), Noesis.MouseButton.Right);
                    return false;
                }
                case WinApi.WM.LBUTTONUP:
                {
                    MouseButtonUp?.Invoke(this, LoWord(lParam), HiWord(lParam), Noesis.MouseButton.Left);
                    WinApi.ReleaseCapture();
                    return false;
                }
                case WinApi.WM.MBUTTONUP:
                {
                    MouseButtonUp?.Invoke(this, LoWord(lParam), HiWord(lParam), Noesis.MouseButton.Middle);
                    WinApi.ReleaseCapture();
                    return false;
                }
                case WinApi.WM.RBUTTONUP:
                {
                    MouseButtonUp?.Invoke(this, LoWord(lParam), HiWord(lParam), Noesis.MouseButton.Right);
                    WinApi.ReleaseCapture();
                    return false;
                }
                case WinApi.WM.LBUTTONDBLCLK:
                {
                    MouseDoubleClick?.Invoke(this, LoWord(lParam), HiWord(lParam), Noesis.MouseButton.Left);
                    return false;
                }
                case WinApi.WM.RBUTTONDBLCLK:
                {
                    MouseDoubleClick?.Invoke(this, LoWord(lParam), HiWord(lParam), Noesis.MouseButton.Right);
                    return false;
                }
                case WinApi.WM.MBUTTONDBLCLK:
                {
                    MouseDoubleClick?.Invoke(this, LoWord(lParam), HiWord(lParam), Noesis.MouseButton.Middle);
                    return false;
                }
                case WinApi.WM.MOUSEWHEEL:
                {
                    // Mouse wheel event receives mouse position in screen coordinates
                    WinApi.Point point;
                    point.X = LoWord(lParam);
                    point.Y = HiWord(lParam);
                    WinApi.ScreenToClient(NativeWindow, ref point);

                    MouseWheel?.Invoke(this, point.X, point.Y, HiWord(wParam));
                    return false;
                }
                case WinApi.WM.MOUSEHWHEEL:
                {
                    // Mouse wheel event receives mouse position in screen coordinates
                    WinApi.Point point;
                    point.X = LoWord(lParam);
                    point.Y = HiWord(lParam);
                    WinApi.ScreenToClient(NativeWindow, ref point);

                    MouseHWheel?.Invoke(this, point.X, point.Y, HiWord(wParam));
                    return false;
                }
                // END - Mouse Input Notifications
                // BEGIN -- Keyboard Input Notifications
                case WinApi.WM.KEYDOWN:
                {
                    int index = LoWord(wParam);
                    if (index <= 0xff && _keys[index] != Noesis.Key.None)
                    {
                        KeyDown?.Invoke(this, _keys[index]);
                    }

                    return false;
                }
                case WinApi.WM.KEYUP:
                {
                    int index = LoWord(wParam);
                    if (index <= 0xff && _keys[index] != Noesis.Key.None)
                    {
                        KeyUp?.Invoke(this, _keys[index]);
                    }

                    return false;
                }
                case WinApi.WM.SYSKEYDOWN:
                {
                    if ((lParam.ToInt64() & 0x0000000020000000) != 0) // ALT key is pressed
                    {
                        int index = LoWord(wParam);
                        if (index <= 0xff && _keys[index] != Noesis.Key.None)
                        {
                            KeyDown?.Invoke(this, _keys[index]);
                        }
                    }
                    return false;
                }
                case WinApi.WM.SYSKEYUP:
                {
                    if ((lParam.ToInt64() & 0x0000000020000000) != 0) // ALT key is pressed
                    {
                        int index = LoWord(wParam);
                        if (index <= 0xff && _keys[index] != Noesis.Key.None)
                        {
                            KeyUp?.Invoke(this, _keys[index]);
                        }
                    }
                    return false;
                }
                case WinApi.WM.CHAR:
                {
                    Char?.Invoke(this, (uint)wParam.ToInt32());
                    return false;
                }
                // END --Keyboard Input Notifications
                // BEGIN -- Touch Input Notifications
                case WinApi.WM.POINTERDOWN:
                {
                    ulong id = (ulong)LoWord(wParam);
                    WinApi.Point point;
                    point.X = LoWord(lParam);
                    point.Y = HiWord(lParam);
                    WinApi.ScreenToClient(NativeWindow, ref point);
                    TouchDown?.Invoke(this, point.X, point.Y, id);
                    return true;
                }
                case WinApi.WM.POINTERUPDATE:
                {
                    ulong id = (ulong)LoWord(wParam);
                    WinApi.Point point;
                    point.X = LoWord(lParam);
                    point.Y = HiWord(lParam);
                    WinApi.ScreenToClient(NativeWindow, ref point);
                    TouchMove?.Invoke(this, point.X, point.Y, id);
                    return true;
                }
                case WinApi.WM.POINTERUP:
                {
                    ulong id = (ulong)LoWord(wParam);
                    WinApi.Point point;
                    point.X = LoWord(lParam);
                    point.Y = HiWord(lParam);
                    WinApi.ScreenToClient(NativeWindow, ref point);
                    TouchUp?.Invoke(this, point.X, point.Y, id);
                    return true;
                }
                // END -- Touch Input Notifications
            }

            return false;
        }
        #endregion

        #region Helpers
        private void BuildStyleFlags(WindowStyle style, WindowState state, ResizeMode resize,
            bool showInTaskbar, bool topmost, bool allowDragFiles,
            out WinApi.WindowStyles flags, out WinApi.WindowExStyles flagsEx)
        {
            flags = 0;
            flagsEx = 0;

            switch (style)
            {
                case WindowStyle.None:
                {
                    flags |= WinApi.WindowStyles.WS_POPUP;
                    break;
                }
                case WindowStyle.SingleBorderWindow:
                {
                    flags |= WinApi.WindowStyles.WS_CAPTION | WinApi.WindowStyles.WS_SYSMENU;
                    flagsEx |= WinApi.WindowExStyles.WS_EX_WINDOWEDGE;
                    break;
                }
                case WindowStyle.ThreeDBorderWindow:
                {
                    flags |= WinApi.WindowStyles.WS_CAPTION | WinApi.WindowStyles.WS_SYSMENU;
                    flagsEx |= WinApi.WindowExStyles.WS_EX_CLIENTEDGE;
                    break;
                }
                case WindowStyle.ToolWindow:
                {
                    flags |= WinApi.WindowStyles.WS_CAPTION;
                    flagsEx |= WinApi.WindowExStyles.WS_EX_TOOLWINDOW;
                    break;
                }
            }

            switch (state)
            {
                case WindowState.Maximized:
                {
                    flags |= WinApi.WindowStyles.WS_MAXIMIZE;
                    break;
                }
                case WindowState.Minimized:
                {
                    flags |= WinApi.WindowStyles.WS_MINIMIZE;
                    break;
                }
                case WindowState.Normal:
                {
                    break;
                }
            }

            switch (resize)
            {
                case ResizeMode.CanMinimize:
                {
                    flags |= WinApi.WindowStyles.WS_MINIMIZEBOX;
                    break;
                }
                case ResizeMode.CanResize:
                case ResizeMode.CanResizeWithGrip:
                {
                    flags |= WinApi.WindowStyles.WS_THICKFRAME |
                        WinApi.WindowStyles.WS_MINIMIZEBOX |
                        WinApi.WindowStyles.WS_MAXIMIZEBOX;
                    break;
                }
                case ResizeMode.NoResize:
                {
                    break;
                }
            }

            if (showInTaskbar)
            {
                flagsEx |= WinApi.WindowExStyles.WS_EX_APPWINDOW;
            }
            else
            {
                flagsEx |= WinApi.WindowExStyles.WS_EX_NOACTIVATE;
            }

            if (topmost)
            {
                flagsEx |= WinApi.WindowExStyles.WS_EX_TOPMOST;
            }
        }

        private void SetStyleFlags(WinApi.WindowStyles flags, WinApi.WindowExStyles flagsEx)
        {
            WinApi.SetWindowLong(NativeWindow, (int)WinApi.WindowLongFlags.GWL_STYLE, (int)flags);
            WinApi.SetWindowLong(NativeWindow, (int)WinApi.WindowLongFlags.GWL_EXSTYLE, (int)flagsEx);
            WinApi.SetWindowPos(NativeWindow, IntPtr.Zero, 0, 0, 0, 0,
                WinApi.WindowPositionFlags.SWP_NOACTIVATE |
                WinApi.WindowPositionFlags.SWP_NOMOVE |
                WinApi.WindowPositionFlags.SWP_NOSIZE |
                WinApi.WindowPositionFlags.SWP_NOZORDER |
                WinApi.WindowPositionFlags.SWP_FRAMECHANGED);
        }

        private WindowStyle GetStyle(WinApi.WindowStyles flags, WinApi.WindowExStyles flagsEx)
        {
            if ((flags & WinApi.WindowStyles.WS_CAPTION) != 0)
            {
                if ((flagsEx & WinApi.WindowExStyles.WS_EX_TOOLWINDOW) != 0)
                {
                    return WindowStyle.ToolWindow;
                }
                else if ((flags & WinApi.WindowStyles.WS_SYSMENU) != 0 &&
                    (flagsEx & WinApi.WindowExStyles.WS_EX_CLIENTEDGE) != 0)
                {
                    return WindowStyle.ThreeDBorderWindow;
                }
            }
            else if ((flags & WinApi.WindowStyles.WS_POPUP) != 0)
            {
                return WindowStyle.None;
            }

            return WindowStyle.SingleBorderWindow;
        }

        private WindowState GetState(WinApi.WindowStyles flags)
        {
            if ((flags & WinApi.WindowStyles.WS_MAXIMIZE) != 0)
            {
                return WindowState.Maximized;
            }
            else if ((flags & WinApi.WindowStyles.WS_MINIMIZE) != 0)
            {
                return WindowState.Minimized;
            }

            return WindowState.Normal;
        }

        private ResizeMode GetResizeMode(WinApi.WindowStyles flags)
        {
            if ((flags & WinApi.WindowStyles.WS_MINIMIZEBOX) != 0)
            {
                if ((flags & WinApi.WindowStyles.WS_THICKFRAME) != 0 &&
                    (flags & WinApi.WindowStyles.WS_MAXIMIZEBOX) != 0)
                {
                    return ResizeMode.CanResize;
                }
                else
                {
                    return ResizeMode.CanMinimize;
                }
            }

            return ResizeMode.NoResize;
        }

        private bool GetShowInTaskbar(WinApi.WindowExStyles flagsEx)
        {
            if ((flagsEx & WinApi.WindowExStyles.WS_EX_APPWINDOW) != 0)
            {
                return true;
            }
            else if ((flagsEx & WinApi.WindowExStyles.WS_EX_NOACTIVATE) != 0)
            {
                return false;
            }

            return false;
        }

        private bool GetTopmost(WinApi.WindowExStyles flagsEx)
        {
            return (flagsEx & WinApi.WindowExStyles.WS_EX_TOPMOST) != 0;
        }

        private bool GetAllowFileDrop(WinApi.WindowExStyles flagsEx)
        {
            return (flagsEx & WinApi.WindowExStyles.WS_EX_ACCEPTFILES) != 0;
        }

        private void CenterWindow()
        {
            IntPtr desktop = WinApi.GetDesktopWindow();
            IntPtr monitor = WinApi.MonitorFromWindow(desktop, WinApi.MonitorFlag.MONITOR_DEFAULTTOPRIMARY);

            WinApi.MonitorInfo info = new WinApi.MonitorInfo();
            info.Size = (uint)Noesis.Marshal.SizeOf<WinApi.MonitorInfo>();
            WinApi.GetMonitorInfo(monitor, ref info);

            int monitorWidth = info.MonitorRect.Right - info.MonitorRect.Left;
            int monitorHeight = info.MonitorRect.Bottom - info.MonitorRect.Top;

            WinApi.Rectangle r;
            WinApi.GetWindowRect(NativeWindow, out r);

            int width = r.Right - r.Left;
            int height = r.Bottom - r.Top;

            int x = (int)(0.5f * (monitorWidth - width));
            int y = (int)(0.5f * (monitorHeight - height));
            SetLocation(x, y);
        }

        bool GetNextMessage(bool wait, out WinApi.Message msg)
        {
            if (wait)
            {
                WinApi.GetMessage(out msg, IntPtr.Zero, 0u, 0u);
                return true;
            }
            else
            {
                return WinApi.PeekMessage(out msg, IntPtr.Zero, 0u, 0u,
                    (uint)WinApi.PeekMessageFlags.PM_REMOVE);
            }
        }

        private int LoWord(IntPtr n)
        {
            return (short)((unchecked((int)(long)n)) & 0xffff);
        }
        private int HiWord(IntPtr n)
        {
            return (short)(((unchecked((int)(long)n)) >> 16) & 0xffff);
        }

        private int Word(IntPtr n)
        {
            return (unchecked((int)(long)n));
        }
        #endregion

        #region Displays table
        private static void AddDisplay(IntPtr id, Win32Display display)
        {
            lock (_displays)
            {
                _displays.Add(id, new WeakReference(display));
            }
        }

        private static void RemoveDisplay(IntPtr id)
        {
            lock (_displays)
            {
                _displays.Remove(id);
            }
        }

        private static Win32Display GetDisplay(IntPtr id)
        {
            WeakReference display = null;

            lock (_displays)
            {
                _displays.TryGetValue(id, out display);
            }

            return (Win32Display)display?.Target;
        }

        private static Dictionary<IntPtr, WeakReference> _displays =
            new Dictionary<IntPtr, WeakReference>();
        #endregion

        #region Cursors table
        private static IntPtr[] _cursors = CreateCursorsTable();

        private static IntPtr[] CreateCursorsTable()
        {
            IntPtr[] cursors = new IntPtr[(int)Noesis.Cursor.Count];

            cursors[(int)Noesis.Cursor.No] = WinApi.LoadCursor(IntPtr.Zero, new IntPtr((int)WinApi.SystemCursor.IDC_NO));
            cursors[(int)Noesis.Cursor.Arrow] = WinApi.LoadCursor(IntPtr.Zero, new IntPtr((int)WinApi.SystemCursor.IDC_ARROW));
            cursors[(int)Noesis.Cursor.AppStarting] = WinApi.LoadCursor(IntPtr.Zero, new IntPtr((int)WinApi.SystemCursor.IDC_APPSTARTING));
            cursors[(int)Noesis.Cursor.Cross] = WinApi.LoadCursor(IntPtr.Zero, new IntPtr((int)WinApi.SystemCursor.IDC_CROSS));
            cursors[(int)Noesis.Cursor.Help] = WinApi.LoadCursor(IntPtr.Zero, new IntPtr((int)WinApi.SystemCursor.IDC_HELP));
            cursors[(int)Noesis.Cursor.IBeam] = WinApi.LoadCursor(IntPtr.Zero, new IntPtr((int)WinApi.SystemCursor.IDC_IBEAM));
            cursors[(int)Noesis.Cursor.SizeAll] = WinApi.LoadCursor(IntPtr.Zero, new IntPtr((int)WinApi.SystemCursor.IDC_SIZEALL));
            cursors[(int)Noesis.Cursor.SizeNESW] = WinApi.LoadCursor(IntPtr.Zero, new IntPtr((int)WinApi.SystemCursor.IDC_SIZENESW));
            cursors[(int)Noesis.Cursor.SizeNS] = WinApi.LoadCursor(IntPtr.Zero, new IntPtr((int)WinApi.SystemCursor.IDC_SIZENS));
            cursors[(int)Noesis.Cursor.SizeNWSE] = WinApi.LoadCursor(IntPtr.Zero, new IntPtr((int)WinApi.SystemCursor.IDC_SIZENWSE));
            cursors[(int)Noesis.Cursor.SizeWE] = WinApi.LoadCursor(IntPtr.Zero, new IntPtr((int)WinApi.SystemCursor.IDC_SIZEWE));
            cursors[(int)Noesis.Cursor.Wait] = WinApi.LoadCursor(IntPtr.Zero, new IntPtr((int)WinApi.SystemCursor.IDC_WAIT));
            cursors[(int)Noesis.Cursor.Hand] = WinApi.LoadCursor(IntPtr.Zero, new IntPtr((int)WinApi.SystemCursor.IDC_HAND));

            return cursors;
        }
        #endregion

        #region Key table
        private static Noesis.Key[] _keys = CreateKeysTable();

        private static Noesis.Key[] CreateKeysTable()
        {
            Noesis.Key[] keys = new Noesis.Key[256];

            keys[(int)WinApi.VirtualKey.LBUTTON] = Noesis.Key.None;
            keys[(int)WinApi.VirtualKey.RBUTTON] = Noesis.Key.None;
            keys[(int)WinApi.VirtualKey.CANCEL] = Noesis.Key.Cancel;
            keys[(int)WinApi.VirtualKey.MBUTTON] = Noesis.Key.None;
            keys[(int)WinApi.VirtualKey.XBUTTON1] = Noesis.Key.None;
            keys[(int)WinApi.VirtualKey.XBUTTON2] = Noesis.Key.None;
            keys[(int)WinApi.VirtualKey.BACK] = Noesis.Key.Back;
            keys[(int)WinApi.VirtualKey.TAB] = Noesis.Key.Tab;
            keys[(int)WinApi.VirtualKey.CLEAR] = Noesis.Key.Clear;
            keys[(int)WinApi.VirtualKey.RETURN] = Noesis.Key.Return;
            keys[(int)WinApi.VirtualKey.SHIFT] = Noesis.Key.LeftShift;
            keys[(int)WinApi.VirtualKey.CONTROL] = Noesis.Key.LeftCtrl;
            keys[(int)WinApi.VirtualKey.MENU] = Noesis.Key.LeftAlt;
            keys[(int)WinApi.VirtualKey.PAUSE] = Noesis.Key.Pause;
            keys[(int)WinApi.VirtualKey.CAPITAL] = Noesis.Key.Capital;
            keys[(int)WinApi.VirtualKey.KANA] = Noesis.Key.KanaMode;
            keys[(int)WinApi.VirtualKey.HANGUL] = Noesis.Key.HangulMode;
            keys[(int)WinApi.VirtualKey.JUNJA] = Noesis.Key.JunjaMode;
            keys[(int)WinApi.VirtualKey.FINAL] = Noesis.Key.None;
            keys[(int)WinApi.VirtualKey.KANJI] = Noesis.Key.KanjiMode;
            keys[(int)WinApi.VirtualKey.ESCAPE] = Noesis.Key.Escape;
            keys[(int)WinApi.VirtualKey.CONVERT] = Noesis.Key.ImeConvert;
            keys[(int)WinApi.VirtualKey.NONCONVERT] = Noesis.Key.ImeNonConvert;
            keys[(int)WinApi.VirtualKey.ACCEPT] = Noesis.Key.ImeAccept;
            keys[(int)WinApi.VirtualKey.MODECHANGE] = Noesis.Key.ImeModeChange;
            keys[(int)WinApi.VirtualKey.SPACE] = Noesis.Key.Space;
            keys[(int)WinApi.VirtualKey.PRIOR] = Noesis.Key.Prior;
            keys[(int)WinApi.VirtualKey.NEXT] = Noesis.Key.Next;
            keys[(int)WinApi.VirtualKey.END] = Noesis.Key.End;
            keys[(int)WinApi.VirtualKey.HOME] = Noesis.Key.Home;
            keys[(int)WinApi.VirtualKey.LEFT] = Noesis.Key.Left;
            keys[(int)WinApi.VirtualKey.UP] = Noesis.Key.Up;
            keys[(int)WinApi.VirtualKey.RIGHT] = Noesis.Key.Right;
            keys[(int)WinApi.VirtualKey.DOWN] = Noesis.Key.Down;
            keys[(int)WinApi.VirtualKey.SELECT] = Noesis.Key.Select;
            keys[(int)WinApi.VirtualKey.PRINT] = Noesis.Key.Print;
            keys[(int)WinApi.VirtualKey.EXECUTE] = Noesis.Key.Execute;
            keys[(int)WinApi.VirtualKey.SNAPSHOT] = Noesis.Key.Snapshot;
            keys[(int)WinApi.VirtualKey.INSERT] = Noesis.Key.Insert;
            keys[(int)WinApi.VirtualKey.DELETE] = Noesis.Key.Delete;
            keys[(int)WinApi.VirtualKey.HELP] = Noesis.Key.Help;
            keys[(int)WinApi.VirtualKey.D0] = Noesis.Key.D0;
            keys[(int)WinApi.VirtualKey.D1] = Noesis.Key.D1;
            keys[(int)WinApi.VirtualKey.D2] = Noesis.Key.D2;
            keys[(int)WinApi.VirtualKey.D3] = Noesis.Key.D3;
            keys[(int)WinApi.VirtualKey.D4] = Noesis.Key.D4;
            keys[(int)WinApi.VirtualKey.D5] = Noesis.Key.D5;
            keys[(int)WinApi.VirtualKey.D6] = Noesis.Key.D6;
            keys[(int)WinApi.VirtualKey.D7] = Noesis.Key.D7;
            keys[(int)WinApi.VirtualKey.D8] = Noesis.Key.D8;
            keys[(int)WinApi.VirtualKey.D9] = Noesis.Key.D9;
            keys[(int)WinApi.VirtualKey.A] = Noesis.Key.A;
            keys[(int)WinApi.VirtualKey.B] = Noesis.Key.B;
            keys[(int)WinApi.VirtualKey.C] = Noesis.Key.C;
            keys[(int)WinApi.VirtualKey.D] = Noesis.Key.D;
            keys[(int)WinApi.VirtualKey.E] = Noesis.Key.E;
            keys[(int)WinApi.VirtualKey.F] = Noesis.Key.F;
            keys[(int)WinApi.VirtualKey.G] = Noesis.Key.G;
            keys[(int)WinApi.VirtualKey.H] = Noesis.Key.H;
            keys[(int)WinApi.VirtualKey.I] = Noesis.Key.I;
            keys[(int)WinApi.VirtualKey.J] = Noesis.Key.J;
            keys[(int)WinApi.VirtualKey.K] = Noesis.Key.K;
            keys[(int)WinApi.VirtualKey.L] = Noesis.Key.L;
            keys[(int)WinApi.VirtualKey.M] = Noesis.Key.M;
            keys[(int)WinApi.VirtualKey.N] = Noesis.Key.N;
            keys[(int)WinApi.VirtualKey.O] = Noesis.Key.O;
            keys[(int)WinApi.VirtualKey.P] = Noesis.Key.P;
            keys[(int)WinApi.VirtualKey.Q] = Noesis.Key.Q;
            keys[(int)WinApi.VirtualKey.R] = Noesis.Key.R;
            keys[(int)WinApi.VirtualKey.S] = Noesis.Key.S;
            keys[(int)WinApi.VirtualKey.T] = Noesis.Key.T;
            keys[(int)WinApi.VirtualKey.U] = Noesis.Key.U;
            keys[(int)WinApi.VirtualKey.V] = Noesis.Key.V;
            keys[(int)WinApi.VirtualKey.W] = Noesis.Key.W;
            keys[(int)WinApi.VirtualKey.X] = Noesis.Key.X;
            keys[(int)WinApi.VirtualKey.Y] = Noesis.Key.Y;
            keys[(int)WinApi.VirtualKey.Z] = Noesis.Key.Z;
            keys[(int)WinApi.VirtualKey.LWIN] = Noesis.Key.LWin;
            keys[(int)WinApi.VirtualKey.RWIN] = Noesis.Key.RWin;
            keys[(int)WinApi.VirtualKey.APPS] = Noesis.Key.Apps;
            keys[(int)WinApi.VirtualKey.SLEEP] = Noesis.Key.Sleep;
            keys[(int)WinApi.VirtualKey.NUMPAD0] = Noesis.Key.NumPad0;
            keys[(int)WinApi.VirtualKey.NUMPAD1] = Noesis.Key.NumPad1;
            keys[(int)WinApi.VirtualKey.NUMPAD2] = Noesis.Key.NumPad2;
            keys[(int)WinApi.VirtualKey.NUMPAD3] = Noesis.Key.NumPad3;
            keys[(int)WinApi.VirtualKey.NUMPAD4] = Noesis.Key.NumPad4;
            keys[(int)WinApi.VirtualKey.NUMPAD5] = Noesis.Key.NumPad5;
            keys[(int)WinApi.VirtualKey.NUMPAD6] = Noesis.Key.NumPad6;
            keys[(int)WinApi.VirtualKey.NUMPAD7] = Noesis.Key.NumPad7;
            keys[(int)WinApi.VirtualKey.NUMPAD8] = Noesis.Key.NumPad8;
            keys[(int)WinApi.VirtualKey.NUMPAD9] = Noesis.Key.NumPad9;
            keys[(int)WinApi.VirtualKey.MULTIPLY] = Noesis.Key.Multiply;
            keys[(int)WinApi.VirtualKey.ADD] = Noesis.Key.Add;
            keys[(int)WinApi.VirtualKey.SEPARATOR] = Noesis.Key.Separator;
            keys[(int)WinApi.VirtualKey.SUBTRACT] = Noesis.Key.Subtract;
            keys[(int)WinApi.VirtualKey.DECIMAL] = Noesis.Key.Decimal;
            keys[(int)WinApi.VirtualKey.DIVIDE] = Noesis.Key.Divide;
            keys[(int)WinApi.VirtualKey.F1] = Noesis.Key.F1;
            keys[(int)WinApi.VirtualKey.F2] = Noesis.Key.F2;
            keys[(int)WinApi.VirtualKey.F3] = Noesis.Key.F3;
            keys[(int)WinApi.VirtualKey.F4] = Noesis.Key.F4;
            keys[(int)WinApi.VirtualKey.F5] = Noesis.Key.F5;
            keys[(int)WinApi.VirtualKey.F6] = Noesis.Key.F6;
            keys[(int)WinApi.VirtualKey.F7] = Noesis.Key.F7;
            keys[(int)WinApi.VirtualKey.F8] = Noesis.Key.F8;
            keys[(int)WinApi.VirtualKey.F9] = Noesis.Key.F9;
            keys[(int)WinApi.VirtualKey.F10] = Noesis.Key.F10;
            keys[(int)WinApi.VirtualKey.F11] = Noesis.Key.F11;
            keys[(int)WinApi.VirtualKey.F12] = Noesis.Key.F12;
            keys[(int)WinApi.VirtualKey.F13] = Noesis.Key.F13;
            keys[(int)WinApi.VirtualKey.F14] = Noesis.Key.F14;
            keys[(int)WinApi.VirtualKey.F15] = Noesis.Key.F15;
            keys[(int)WinApi.VirtualKey.F16] = Noesis.Key.F16;
            keys[(int)WinApi.VirtualKey.F17] = Noesis.Key.F17;
            keys[(int)WinApi.VirtualKey.F18] = Noesis.Key.F18;
            keys[(int)WinApi.VirtualKey.F19] = Noesis.Key.F19;
            keys[(int)WinApi.VirtualKey.F20] = Noesis.Key.F20;
            keys[(int)WinApi.VirtualKey.F21] = Noesis.Key.F21;
            keys[(int)WinApi.VirtualKey.F22] = Noesis.Key.F22;
            keys[(int)WinApi.VirtualKey.F23] = Noesis.Key.F23;
            keys[(int)WinApi.VirtualKey.F24] = Noesis.Key.F24;
            keys[(int)WinApi.VirtualKey.NUMLOCK] = Noesis.Key.NumLock;
            keys[(int)WinApi.VirtualKey.SCROLL] = Noesis.Key.Scroll;
            keys[(int)WinApi.VirtualKey.OEM_NEC_EQUAL] = Noesis.Key.None;
            keys[(int)WinApi.VirtualKey.OEM_FJ_JISHO] = Noesis.Key.None;
            keys[(int)WinApi.VirtualKey.OEM_FJ_MASSHOU] = Noesis.Key.None;
            keys[(int)WinApi.VirtualKey.OEM_FJ_TOUROKU] = Noesis.Key.None;
            keys[(int)WinApi.VirtualKey.OEM_FJ_LOYA] = Noesis.Key.None;
            keys[(int)WinApi.VirtualKey.OEM_FJ_ROYA] = Noesis.Key.None;
            keys[(int)WinApi.VirtualKey.LSHIFT] = Noesis.Key.LeftShift;
            keys[(int)WinApi.VirtualKey.RSHIFT] = Noesis.Key.RightShift;
            keys[(int)WinApi.VirtualKey.LCONTROL] = Noesis.Key.LeftCtrl;
            keys[(int)WinApi.VirtualKey.RCONTROL] = Noesis.Key.RightCtrl;
            keys[(int)WinApi.VirtualKey.LMENU] = Noesis.Key.LeftAlt;
            keys[(int)WinApi.VirtualKey.RMENU] = Noesis.Key.RightAlt;
            keys[(int)WinApi.VirtualKey.BROWSER_BACK] = Noesis.Key.BrowserBack;
            keys[(int)WinApi.VirtualKey.BROWSER_FORWARD] = Noesis.Key.BrowserForward;
            keys[(int)WinApi.VirtualKey.BROWSER_REFRESH] = Noesis.Key.BrowserRefresh;
            keys[(int)WinApi.VirtualKey.BROWSER_STOP] = Noesis.Key.BrowserStop;
            keys[(int)WinApi.VirtualKey.BROWSER_SEARCH] = Noesis.Key.BrowserSearch;
            keys[(int)WinApi.VirtualKey.BROWSER_FAVORITES] = Noesis.Key.BrowserFavorites;
            keys[(int)WinApi.VirtualKey.BROWSER_HOME] = Noesis.Key.BrowserHome;
            keys[(int)WinApi.VirtualKey.VOLUME_MUTE] = Noesis.Key.VolumeMute;
            keys[(int)WinApi.VirtualKey.VOLUME_DOWN] = Noesis.Key.VolumeDown;
            keys[(int)WinApi.VirtualKey.VOLUME_UP] = Noesis.Key.VolumeUp;
            keys[(int)WinApi.VirtualKey.MEDIA_NEXT_TRACK] = Noesis.Key.MediaNextTrack;
            keys[(int)WinApi.VirtualKey.MEDIA_PREV_TRACK] = Noesis.Key.MediaPreviousTrack;
            keys[(int)WinApi.VirtualKey.MEDIA_STOP] = Noesis.Key.MediaStop;
            keys[(int)WinApi.VirtualKey.MEDIA_PLAY_PAUSE] = Noesis.Key.MediaPlayPause;
            keys[(int)WinApi.VirtualKey.LAUNCH_MAIL] = Noesis.Key.LaunchMail;
            keys[(int)WinApi.VirtualKey.LAUNCH_MEDIA_SELECT] = Noesis.Key.None;
            keys[(int)WinApi.VirtualKey.LAUNCH_APP1] = Noesis.Key.LaunchApplication1;
            keys[(int)WinApi.VirtualKey.LAUNCH_APP2] = Noesis.Key.LaunchApplication2;
            keys[(int)WinApi.VirtualKey.OEM_1] = Noesis.Key.Oem1;
            keys[(int)WinApi.VirtualKey.OEM_PLUS] = Noesis.Key.OemPlus;
            keys[(int)WinApi.VirtualKey.OEM_COMMA] = Noesis.Key.OemComma;
            keys[(int)WinApi.VirtualKey.OEM_MINUS] = Noesis.Key.OemMinus;
            keys[(int)WinApi.VirtualKey.OEM_PERIOD] = Noesis.Key.OemPeriod;
            keys[(int)WinApi.VirtualKey.OEM_2] = Noesis.Key.Oem2;
            keys[(int)WinApi.VirtualKey.OEM_3] = Noesis.Key.Oem3;
            keys[(int)WinApi.VirtualKey.OEM_4] = Noesis.Key.Oem4;
            keys[(int)WinApi.VirtualKey.OEM_5] = Noesis.Key.Oem5;
            keys[(int)WinApi.VirtualKey.OEM_6] = Noesis.Key.Oem6;
            keys[(int)WinApi.VirtualKey.OEM_7] = Noesis.Key.Oem7;
            keys[(int)WinApi.VirtualKey.OEM_8] = Noesis.Key.Oem8;
            keys[(int)WinApi.VirtualKey.OEM_AX] = Noesis.Key.None;
            keys[(int)WinApi.VirtualKey.OEM_102] = Noesis.Key.Oem102;
            keys[(int)WinApi.VirtualKey.ICO_HELP] = Noesis.Key.None;
            keys[(int)WinApi.VirtualKey.ICO_00] = Noesis.Key.None;
            keys[(int)WinApi.VirtualKey.PROCESSKEY] = Noesis.Key.None;
            keys[(int)WinApi.VirtualKey.ICO_CLEAR] = Noesis.Key.None;
            keys[(int)WinApi.VirtualKey.PACKET] = Noesis.Key.None;
            keys[(int)WinApi.VirtualKey.OEM_RESET] = Noesis.Key.None;
            keys[(int)WinApi.VirtualKey.OEM_JUMP] = Noesis.Key.None;
            keys[(int)WinApi.VirtualKey.OEM_PA1] = Noesis.Key.None;
            keys[(int)WinApi.VirtualKey.OEM_PA2] = Noesis.Key.None;
            keys[(int)WinApi.VirtualKey.OEM_PA3] = Noesis.Key.None;
            keys[(int)WinApi.VirtualKey.OEM_WSCTRL] = Noesis.Key.None;
            keys[(int)WinApi.VirtualKey.OEM_CUSEL] = Noesis.Key.None;
            keys[(int)WinApi.VirtualKey.OEM_ATTN] = Noesis.Key.OemAttn;
            keys[(int)WinApi.VirtualKey.OEM_FINISH] = Noesis.Key.OemFinish;
            keys[(int)WinApi.VirtualKey.OEM_COPY] = Noesis.Key.OemCopy;
            keys[(int)WinApi.VirtualKey.OEM_AUTO] = Noesis.Key.OemAuto;
            keys[(int)WinApi.VirtualKey.OEM_ENLW] = Noesis.Key.OemEnlw;
            keys[(int)WinApi.VirtualKey.OEM_BACKTAB] = Noesis.Key.OemBackTab;
            keys[(int)WinApi.VirtualKey.ATTN] = Noesis.Key.Attn;
            keys[(int)WinApi.VirtualKey.CRSEL] = Noesis.Key.CrSel;
            keys[(int)WinApi.VirtualKey.EXSEL] = Noesis.Key.ExSel;
            keys[(int)WinApi.VirtualKey.EREOF] = Noesis.Key.EraseEof;
            keys[(int)WinApi.VirtualKey.PLAY] = Noesis.Key.Play;
            keys[(int)WinApi.VirtualKey.ZOOM] = Noesis.Key.Zoom;
            keys[(int)WinApi.VirtualKey.NONAME] = Noesis.Key.NoName;
            keys[(int)WinApi.VirtualKey.PA1] = Noesis.Key.Pa1;
            keys[(int)WinApi.VirtualKey.OEM_CLEAR] = Noesis.Key.OemClear;

            return keys;
        }
        #endregion

        #region Private members
        private IntPtr _hWnd = IntPtr.Zero;
        private IntPtr _hDC = IntPtr.Zero;
        private WindowStartupLocation _startupLocation = WindowStartupLocation.Manual;
        private Noesis.Cursor _cursor = Noesis.Cursor.Arrow;
        private WinApi.GetDpiForMonitor _getDpiForMonitor = null;
        private IntPtr _shcore = IntPtr.Zero;
        #endregion

        private static class WinApi
        {
            #region Constants
            [Flags]
            public enum WindowClassStyles
            {
                CS_BYTEALIGNCLIENT = 0x1000,
                CS_BYTEALIGNWINDOW = 0x2000,
                CS_CLASSDC = 0x0040,
                CS_DBLCLKS = 0x0008,
                CS_DROPSHADOW = 0x00020000,
                CS_GLOBALCLASS = 0x4000,
                CS_HREDRAW = 0x0002,
                CS_NOCLOSE = 0x0200,
                CS_OWNDC = 0x0020,
                CS_PARENTDC = 0x0080,
                CS_SAVEBITS = 0x0800,
                CS_VREDRAW = 0x0001
            }

            [Flags]
            public enum WindowStyles
            {
                WS_BORDER = 0x00800000,
                WS_CAPTION = 0x00C00000,
                WS_CHILD = 0x40000000,
                WS_CHILDWINDOW = 0x40000000,
                WS_CLIPCHILDREN = 0x02000000,
                WS_CLIPSIBLINGS = 0x04000000,
                WS_DISABLED = 0x08000000,
                WS_DLGFRAME = 0x00400000,
                WS_GROUP = 0x00020000,
                WS_HSCROLL = 0x00100000,
                WS_ICONIC = 0x20000000,
                WS_MAXIMIZE = 0x01000000,
                WS_MAXIMIZEBOX = 0x00010000,
                WS_MINIMIZE = 0x20000000,
                WS_MINIMIZEBOX = 0x00020000,
                WS_OVERLAPPED = 0x00000000,
                WS_OVERLAPPEDWINDOW = WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU | WS_THICKFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX,
                WS_POPUP = unchecked((int)0x80000000),
                WS_POPUPWINDOW = WS_POPUP | WS_BORDER | WS_SYSMENU,
                WS_SIZEBOX = 0x00040000,
                WS_SYSMENU = 0x00080000,
                WS_TABSTOP = 0x00010000,
                WS_THICKFRAME = 0x00040000,
                WS_TILED = 0x00000000,
                WS_TILEDWINDOW = WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU | WS_THICKFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX,
                WS_VISIBLE = 0x10000000,
                WS_VSCROLL = 0x00200000
            }

            [Flags]
            public enum WindowExStyles
            {
                WS_EX_ACCEPTFILES = 0x00000010,
                WS_EX_APPWINDOW = 0x00040000,
                WS_EX_CLIENTEDGE = 0x00000200,
                WS_EX_COMPOSITED = 0x02000000,
                WS_EX_CONTEXTHELP = 0x00000400,
                WS_EX_CONTROLPARENT = 0x00010000,
                WS_EX_DLGMODALFRAME = 0x00000001,
                WS_EX_LAYERED = 0x00080000,
                WS_EX_LAYOUTRTL = 0x00400000,
                WS_EX_LEFT = 0x00000000,
                WS_EX_LEFTSCROLLBAR = 0x00004000,
                WS_EX_LTRREADING = 0x00000000,
                WS_EX_MDICHILD = 0x00000040,
                WS_EX_NOACTIVATE = 0x08000000,
                WS_EX_NOINHERITLAYOUT = 0x00100000,
                WS_EX_NOPARENTNOTIFY = 0x00000004,
                WS_EX_NOREDIRECTIONBITMAP = 0x00200000,
                WS_EX_OVERLAPPEDWINDOW = WS_EX_WINDOWEDGE | WS_EX_CLIENTEDGE,
                WS_EX_PALETTEWINDOW = WS_EX_WINDOWEDGE | WS_EX_TOOLWINDOW | WS_EX_TOPMOST,
                WS_EX_RIGHT = 0x00001000,
                WS_EX_RIGHTSCROLLBAR = 0x00000000,
                WS_EX_RTLREADING = 0x00002000,
                WS_EX_STATICEDGE = 0x00020000,
                WS_EX_TOOLWINDOW = 0x00000080,
                WS_EX_TOPMOST = 0x00000008,
                WS_EX_TRANSPARENT = 0x00000020,
                WS_EX_WINDOWEDGE = 0x00000100
            }

            [Flags]
            public enum CreateWindowFlags
            {
                CW_USEDEFAULT = unchecked((int)0x80000000)
            }

            [Flags]
            public enum QueueStatusFlags
            {
                QS_ALLEVENTS = QS_INPUT | QS_POSTMESSAGE | QS_TIMER | QS_PAINT | QS_HOTKEY,
                QS_ALLINPUT = QS_INPUT | QS_POSTMESSAGE | QS_TIMER | QS_PAINT | QS_HOTKEY | QS_SENDMESSAGE,
                QS_ALLPOSTMESSAGE = 0x0100,
                QS_HOTKEY = 0x0080,
                QS_INPUT = QS_MOUSE | QS_KEY | QS_RAWINPUT,
                QS_KEY = 0x0001,
                QS_MOUSE = QS_MOUSEMOVE | QS_MOUSEBUTTON,
                QS_MOUSEBUTTON = 0x0004,
                QS_MOUSEMOVE = 0x0002,
                QS_PAINT = 0x0020,
                QS_POSTMESSAGE = 0x0008,
                QS_RAWINPUT = 0x0400,
                QS_SENDMESSAGE = 0x0040,
                QS_TIMER = 0x0010,
                QS_REFRESH = QS_HOTKEY | QS_KEY | QS_MOUSEBUTTON | QS_PAINT
            }

            [Flags]
            public enum PeekMessageFlags
            {
                PM_NOREMOVE = 0x0000,
                PM_REMOVE = 0x0001,
                PM_NOYIELD = 0x0002,
                PM_QS_INPUT = QueueStatusFlags.QS_INPUT << 16,
                PM_QS_PAINT = QueueStatusFlags.QS_PAINT << 16,
                PM_QS_POSTMESSAGE = (QueueStatusFlags.QS_POSTMESSAGE | QueueStatusFlags.QS_HOTKEY | QueueStatusFlags.QS_TIMER) << 16,
                PM_QS_SENDMESSAGE = QueueStatusFlags.QS_SENDMESSAGE << 16
            }

            [Flags]
            public enum WindowLongFlags
            {
                GWL_EXSTYLE = -20,
                GWLP_HINSTANCE = -6,
                GWLP_HWNDPARENT = -8,
                GWLP_ID = -12,
                GWL_STYLE = -16,
                GWLP_USERDATA = -21,
                GWLP_WNDPROC = -4,
                DWLP_DLGPROC = 0x4,
                DWLP_MSGRESULT = 0,
                DWLP_USER = 0x8
            }

            public enum WindowSizeFlag
            {
                SIZE_MAXHIDE = 4,
                SIZE_MAXIMIZED = 2,
                SIZE_MAXSHOW = 3,
                SIZE_MINIMIZED = 1,
                SIZE_RESTORED = 0
            }

            public enum WindowActivateFlag
            {
                WA_ACTIVE = 1,
                WA_CLICKACTIVE = 2,
                WA_INACTIVE = 0
            }

            [Flags]
            public enum WindowPositionFlags
            {
                SWP_ASYNCWINDOWPOS = 0x4000,
                SWP_DEFERERASE = 0x2000,
                SWP_DRAWFRAME = 0x0020,
                SWP_FRAMECHANGED = 0x0020,
                SWP_HIDEWINDOW = 0x0080,
                SWP_NOACTIVATE = 0x0010,
                SWP_NOCOPYBITS = 0x0100,
                SWP_NOMOVE = 0x0002,
                SWP_NOOWNERZORDER = 0x0200,
                SWP_NOREDRAW = 0x0008,
                SWP_NOREPOSITION = 0x0200,
                SWP_NOSENDCHANGING = 0x0400,
                SWP_NOSIZE = 0x0001,
                SWP_NOZORDER = 0x0004,
                SWP_SHOWWINDOW = 0x0040
            }

            [Flags]
            public enum ShowWindowCommands
            {
                SW_FORCEMINIMIZE = 11,
                SW_HIDE = 0,
                SW_MAXIMIZE = 3,
                SW_MINIMIZE = 6,
                SW_RESTORE = 9,
                SW_SHOW = 5,
                SW_SHOWDEFAULT = 10,
                SW_SHOWMAXIMIZED = 3,
                SW_SHOWMINIMIZED = 2,
                SW_SHOWMINNOACTIVE = 7,
                SW_SHOWNA = 8,
                SW_SHOWNOACTIVATE = 4,
                SW_SHOWNORMAL = 1
            }

            public enum MonitorFlag
            {
                MONITOR_DEFAULTTONULL = 0,
                MONITOR_DEFAULTTOPRIMARY = 1,
                MONITOR_DEFAULTTONEAREST = 2
            }

            public enum MonitorInfoFlag
            {
                MONITORINFOF_NONE = 0,
                MONITORINFOF_PRIMARY = 0x00000001
            }

            public enum MonitorDpiType
            {
                MDT_EFFECTIVE_DPI = 0,
                MDT_ANGULAR_DPI = 1,
                MDT_RAW_DPI = 2,
            }

            public enum ProcessDpiAwareness
            {
                PROCESS_DPI_UNAWARE = 0,
                PROCESS_SYSTEM_DPI_AWARE = 1,
                PROCESS_PER_MONITOR_DPI_AWARE = 2
            }

            public enum WM
            {
                NULL = 0x0000,
                CREATE = 0x0001,
                DESTROY = 0x0002,
                MOVE = 0x0003,
                SIZE = 0x0005,
                ACTIVATE = 0x0006,
                SETFOCUS = 0x0007,
                KILLFOCUS = 0x0008,
                ENABLE = 0x000A,
                SETREDRAW = 0x000B,
                SETTEXT = 0x000C,
                GETTEXT = 0x000D,
                GETTEXTLENGTH = 0x000E,
                PAINT = 0x000F,
                CLOSE = 0x0010,
                QUERYENDSESSION = 0x0011,
                QUERYOPEN = 0x0013,
                ENDSESSION = 0x0016,
                QUIT = 0x0012,
                ERASEBKGND = 0x0014,
                SYSCOLORCHANGE = 0x0015,
                SHOWWINDOW = 0x0018,
                WININICHANGE = 0x001A,
                SETTINGCHANGE = WININICHANGE,
                DEVMODECHANGE = 0x001B,
                ACTIVATEAPP = 0x001C,
                FONTCHANGE = 0x001D,
                TIMECHANGE = 0x001E,
                CANCELMODE = 0x001F,
                SETCURSOR = 0x0020,
                MOUSEACTIVATE = 0x0021,
                CHILDACTIVATE = 0x0022,
                QUEUESYNC = 0x0023,
                GETMINMAXINFO = 0x0024,
                PAINTICON = 0x0026,
                ICONERASEBKGND = 0x0027,
                NEXTDLGCTL = 0x0028,
                SPOOLERSTATUS = 0x002A,
                DRAWITEM = 0x002B,
                MEASUREITEM = 0x002C,
                DELETEITEM = 0x002D,
                VKEYTOITEM = 0x002E,
                CHARTOITEM = 0x002F,
                SETFONT = 0x0030,
                GETFONT = 0x0031,
                SETHOTKEY = 0x0032,
                GETHOTKEY = 0x0033,
                QUERYDRAGICON = 0x0037,
                COMPAREITEM = 0x0039,
                GETOBJECT = 0x003D,
                COMPACTING = 0x0041,
                COMMNOTIFY = 0x0044 /* no longer suported */,
                WINDOWPOSCHANGING = 0x0046,
                WINDOWPOSCHANGED = 0x0047,
                POWER = 0x0048,
                COPYDATA = 0x004A,
                CANCELJOURNAL = 0x004B,
                NOTIFY = 0x004E,
                INPUTLANGCHANGEREQUEST = 0x0050,
                INPUTLANGCHANGE = 0x0051,
                TCARD = 0x0052,
                HELP = 0x0053,
                USERCHANGED = 0x0054,
                NOTIFYFORMAT = 0x0055,
                CONTEXTMENU = 0x007B,
                STYLECHANGING = 0x007C,
                STYLECHANGED = 0x007D,
                DISPLAYCHANGE = 0x007E,
                GETICON = 0x007F,
                SETICON = 0x0080,
                NCCREATE = 0x0081,
                NCDESTROY = 0x0082,
                NCCALCSIZE = 0x0083,
                NCHITTEST = 0x0084,
                NCPAINT = 0x0085,
                NCACTIVATE = 0x0086,
                GETDLGCODE = 0x0087,
                SYNCPAINT = 0x0088,
                NCMOUSEMOVE = 0x00A0,
                NCLBUTTONDOWN = 0x00A1,
                NCLBUTTONUP = 0x00A2,
                NCLBUTTONDBLCLK = 0x00A3,
                NCRBUTTONDOWN = 0x00A4,
                NCRBUTTONUP = 0x00A5,
                NCRBUTTONDBLCLK = 0x00A6,
                NCMBUTTONDOWN = 0x00A7,
                NCMBUTTONUP = 0x00A8,
                NCMBUTTONDBLCLK = 0x00A9,
                NCXBUTTONDOWN = 0x00AB,
                NCXBUTTONUP = 0x00AC,
                NCXBUTTONDBLCLK = 0x00AD,
                INPUT_DEVICE_CHANGE = 0x00FE,
                INPUT = 0x00FF,
                KEYFIRST = 0x0100,
                KEYDOWN = 0x0100,
                KEYUP = 0x0101,
                CHAR = 0x0102,
                DEADCHAR = 0x0103,
                SYSKEYDOWN = 0x0104,
                SYSKEYUP = 0x0105,
                SYSCHAR = 0x0106,
                SYSDEADCHAR = 0x0107,
                UNICHAR = 0x0109,
                KEYLAST = 0x0109,
                IME_STARTCOMPOSITION = 0x010D,
                IME_ENDCOMPOSITION = 0x010E,
                IME_COMPOSITION = 0x010F,
                IME_KEYLAST = 0x010F,
                INITDIALOG = 0x0110,
                COMMAND = 0x0111,
                SYSCOMMAND = 0x0112,
                TIMER = 0x0113,
                HSCROLL = 0x0114,
                VSCROLL = 0x0115,
                INITMENU = 0x0116,
                INITMENUPOPUP = 0x0117,
                GESTURE = 0x0119,
                GESTURENOTIFY = 0x011A,
                MENUSELECT = 0x011F,
                MENUCHAR = 0x0120,
                ENTERIDLE = 0x0121,
                MENURBUTTONUP = 0x0122,
                MENUDRAG = 0x0123,
                MENUGETOBJECT = 0x0124,
                UNINITMENUPOPUP = 0x0125,
                MENUCOMMAND = 0x0126,
                CHANGEUISTATE = 0x0127,
                UPDATEUISTATE = 0x0128,
                QUERYUISTATE = 0x0129,
                CTLCOLORMSGBOX = 0x0132,
                CTLCOLOREDIT = 0x0133,
                CTLCOLORLISTBOX = 0x0134,
                CTLCOLORBTN = 0x0135,
                CTLCOLORDLG = 0x0136,
                CTLCOLORSCROLLBAR = 0x0137,
                CTLCOLORSTATIC = 0x0138,
                MOUSEFIRST = 0x0200,
                MOUSEMOVE = 0x0200,
                LBUTTONDOWN = 0x0201,
                LBUTTONUP = 0x0202,
                LBUTTONDBLCLK = 0x0203,
                RBUTTONDOWN = 0x0204,
                RBUTTONUP = 0x0205,
                RBUTTONDBLCLK = 0x0206,
                MBUTTONDOWN = 0x0207,
                MBUTTONUP = 0x0208,
                MBUTTONDBLCLK = 0x0209,
                MOUSEWHEEL = 0x020A,
                XBUTTONDOWN = 0x020B,
                XBUTTONUP = 0x020C,
                XBUTTONDBLCLK = 0x020D,
                MOUSEHWHEEL = 0x020E,
                MOUSELAST = 0x020E,
                PARENTNOTIFY = 0x0210,
                ENTERMENULOOP = 0x0211,
                EXITMENULOOP = 0x0212,
                NEXTMENU = 0x0213,
                SIZING = 0x0214,
                CAPTURECHANGED = 0x0215,
                MOVING = 0x0216,
                POWERBROADCAST = 0x0218,
                DEVICECHANGE = 0x0219,
                MDICREATE = 0x0220,
                MDIDESTROY = 0x0221,
                MDIACTIVATE = 0x0222,
                MDIRESTORE = 0x0223,
                MDINEXT = 0x0224,
                MDIMAXIMIZE = 0x0225,
                MDITILE = 0x0226,
                MDICASCADE = 0x0227,
                MDIICONARRANGE = 0x0228,
                MDIGETACTIVE = 0x0229,
                MDISETMENU = 0x0230,
                ENTERSIZEMOVE = 0x0231,
                EXITSIZEMOVE = 0x0232,
                DROPFILES = 0x0233,
                MDIREFRESHMENU = 0x0234,
                POINTERDEVICECHANGE = 0x238,
                POINTERDEVICEINRANGE = 0x239,
                POINTERDEVICEOUTOFRANGE = 0x23A,
                TOUCH = 0x0240,
                NCPOINTERUPDATE = 0x0241,
                NCPOINTERDOWN = 0x0242,
                NCPOINTERUP = 0x0243,
                POINTERUPDATE = 0x0245,
                POINTERDOWN = 0x0246,
                POINTERUP = 0x0247,
                POINTERENTER = 0x0249,
                POINTERLEAVE = 0x024A,
                POINTERACTIVATE = 0x024B,
                POINTERCAPTURECHANGED = 0x024C,
                TOUCHHITTESTING = 0x024D,
                POINTERWHEEL = 0x024E,
                POINTERHWHEEL = 0x024F,
                IME_SETCONTEXT = 0x0281,
                IME_NOTIFY = 0x0282,
                IME_CONTROL = 0x0283,
                IME_COMPOSITIONFULL = 0x0284,
                IME_SELECT = 0x0285,
                IME_CHAR = 0x0286,
                IME_REQUEST = 0x0288,
                IME_KEYDOWN = 0x0290,
                IME_KEYUP = 0x0291,
                MOUSEHOVER = 0x02A1,
                MOUSELEAVE = 0x02A3,
                NCMOUSEHOVER = 0x02A0,
                NCMOUSELEAVE = 0x02A2,
                WTSSESSION_CHANGE = 0x02B1,
                TABLET_FIRST = 0x02c0,
                TABLET_LAST = 0x02df,
                DPICHANGED = 0x02E0,
                CUT = 0x0300,
                COPY = 0x0301,
                PASTE = 0x0302,
                CLEAR = 0x0303,
                UNDO = 0x0304,
                RENDERFORMAT = 0x0305,
                RENDERALLFORMATS = 0x0306,
                DESTROYCLIPBOARD = 0x0307,
                DRAWCLIPBOARD = 0x0308,
                PAINTCLIPBOARD = 0x0309,
                VSCROLLCLIPBOARD = 0x030A,
                SIZECLIPBOARD = 0x030B,
                ASKCBFORMATNAME = 0x030C,
                CHANGECBCHAIN = 0x030D,
                HSCROLLCLIPBOARD = 0x030E,
                QUERYNEWPALETTE = 0x030F,
                PALETTEISCHANGING = 0x0310,
                PALETTECHANGED = 0x0311,
                HOTKEY = 0x0312,
                PRINT = 0x0317,
                PRINTCLIENT = 0x0318,
                APPCOMMAND = 0x0319,
                THEMECHANGED = 0x031A,
                CLIPBOARDUPDATE = 0x031D,
                DWMCOMPOSITIONCHANGED = 0x031E,
                DWMNCRENDERINGCHANGED = 0x031F,
                DWMCOLORIZATIONCOLORCHANGED = 0x0320,
                DWMWINDOWMAXIMIZEDCHANGE = 0x0321,
                DWMSENDICONICTHUMBNAIL = 0x0323,
                DWMSENDICONICLIVEPREVIEWBITMAP = 0x0326,
                GETTITLEBARINFOEX = 0x033F,
                HANDHELDFIRST = 0x0358,
                HANDHELDLAST = 0x035F,
                AFXFIRST = 0x0360,
                AFXLAST = 0x037F,
                PENWINFIRST = 0x0380,
                PENWINLAST = 0x038F,
                APP = 0x8000,
                USER = 0x0400
            }

            public enum VirtualKey
            {
                LBUTTON = 0x01,
                RBUTTON = 0x02,
                CANCEL = 0x03,
                MBUTTON = 0x04 /* NOT contiguous with L & RBUTTON */,
                XBUTTON1 = 0x05 /* NOT contiguous with L & RBUTTON */,
                XBUTTON2 = 0x06 /* NOT contiguous with L & RBUTTON */,
                BACK = 0x08,
                TAB = 0x09,
                CLEAR = 0x0C,
                RETURN = 0x0D,
                SHIFT = 0x10,
                CONTROL = 0x11,
                MENU = 0x12,
                PAUSE = 0x13,
                CAPITAL = 0x14,
                KANA = 0x15,
                HANGEUL = 0x15 /* old name - should be here for compatibility */,
                HANGUL = 0x15,
                JUNJA = 0x17,
                FINAL = 0x18,
                HANJA = 0x19,
                KANJI = 0x19,
                ESCAPE = 0x1B,
                CONVERT = 0x1C,
                NONCONVERT = 0x1D,
                ACCEPT = 0x1E,
                MODECHANGE = 0x1F,
                SPACE = 0x20,
                PRIOR = 0x21,
                NEXT = 0x22,
                END = 0x23,
                HOME = 0x24,
                LEFT = 0x25,
                UP = 0x26,
                RIGHT = 0x27,
                DOWN = 0x28,
                SELECT = 0x29,
                PRINT = 0x2A,
                EXECUTE = 0x2B,
                SNAPSHOT = 0x2C,
                INSERT = 0x2D,
                DELETE = 0x2E,
                HELP = 0x2F,
                LWIN = 0x5B,
                RWIN = 0x5C,
                APPS = 0x5D,
                SLEEP = 0x5F,
                NUMPAD0 = 0x60,
                NUMPAD1 = 0x61,
                NUMPAD2 = 0x62,
                NUMPAD3 = 0x63,
                NUMPAD4 = 0x64,
                NUMPAD5 = 0x65,
                NUMPAD6 = 0x66,
                NUMPAD7 = 0x67,
                NUMPAD8 = 0x68,
                NUMPAD9 = 0x69,
                MULTIPLY = 0x6A,
                ADD = 0x6B,
                SEPARATOR = 0x6C,
                SUBTRACT = 0x6D,
                DECIMAL = 0x6E,
                DIVIDE = 0x6F,
                F1 = 0x70,
                F2 = 0x71,
                F3 = 0x72,
                F4 = 0x73,
                F5 = 0x74,
                F6 = 0x75,
                F7 = 0x76,
                F8 = 0x77,
                F9 = 0x78,
                F10 = 0x79,
                F11 = 0x7A,
                F12 = 0x7B,
                F13 = 0x7C,
                F14 = 0x7D,
                F15 = 0x7E,
                F16 = 0x7F,
                F17 = 0x80,
                F18 = 0x81,
                F19 = 0x82,
                F20 = 0x83,
                F21 = 0x84,
                F22 = 0x85,
                F23 = 0x86,
                F24 = 0x87,
                NUMLOCK = 0x90,
                SCROLL = 0x91,
                OEM_NEC_EQUAL = 0x92, // '=' key on numpad,
                OEM_FJ_JISHO = 0x92, // 'Dictionary' key,
                OEM_FJ_MASSHOU = 0x93, // 'Unregister word' key,
                OEM_FJ_TOUROKU = 0x94, // 'Register word' key,
                OEM_FJ_LOYA = 0x95, // 'Left OYAYUBI' key,
                OEM_FJ_ROYA = 0x96, // 'Right OYAYUBI' key,
                LSHIFT = 0xA0,
                RSHIFT = 0xA1,
                LCONTROL = 0xA2,
                RCONTROL = 0xA3,
                LMENU = 0xA4,
                RMENU = 0xA5,
                BROWSER_BACK = 0xA6,
                BROWSER_FORWARD = 0xA7,
                BROWSER_REFRESH = 0xA8,
                BROWSER_STOP = 0xA9,
                BROWSER_SEARCH = 0xAA,
                BROWSER_FAVORITES = 0xAB,
                BROWSER_HOME = 0xAC,
                VOLUME_MUTE = 0xAD,
                VOLUME_DOWN = 0xAE,
                VOLUME_UP = 0xAF,
                MEDIA_NEXT_TRACK = 0xB0,
                MEDIA_PREV_TRACK = 0xB1,
                MEDIA_STOP = 0xB2,
                MEDIA_PLAY_PAUSE = 0xB3,
                LAUNCH_MAIL = 0xB4,
                LAUNCH_MEDIA_SELECT = 0xB5,
                LAUNCH_APP1 = 0xB6,
                LAUNCH_APP2 = 0xB7,
                OEM_1 = 0xBA, // ';:' for US,
                OEM_PLUS = 0xBB, // '+' any country,
                OEM_COMMA = 0xBC, // ',' any country,
                OEM_MINUS = 0xBD, // '-' any country,
                OEM_PERIOD = 0xBE, // '.' any country,
                OEM_2 = 0xBF, // '/?' for US,
                OEM_3 = 0xC0, // '`~' for US,
                OEM_4 = 0xDB, //  '[{' for US,
                OEM_5 = 0xDC, //  '\|' for US,
                OEM_6 = 0xDD, //  ']}' for US,
                OEM_7 = 0xDE, //  ''"' for US,
                OEM_8 = 0xDF,
                OEM_AX = 0xE1, //  'AX' key on Japanese AX kbd,
                OEM_102 = 0xE2, //  "<>" or "\|" on RT 102-key kbd.,
                ICO_HELP = 0xE3, //  Help key on ICO,
                ICO_00 = 0xE4, //  00 key on ICO,
                PROCESSKEY = 0xE5,
                ICO_CLEAR = 0xE6,
                PACKET = 0xE7,
                OEM_RESET = 0xE9,
                OEM_JUMP = 0xEA,
                OEM_PA1 = 0xEB,
                OEM_PA2 = 0xEC,
                OEM_PA3 = 0xED,
                OEM_WSCTRL = 0xEE,
                OEM_CUSEL = 0xEF,
                OEM_ATTN = 0xF0,
                OEM_FINISH = 0xF1,
                OEM_COPY = 0xF2,
                OEM_AUTO = 0xF3,
                OEM_ENLW = 0xF4,
                OEM_BACKTAB = 0xF5,
                ATTN = 0xF6,
                CRSEL = 0xF7,
                EXSEL = 0xF8,
                EREOF = 0xF9,
                PLAY = 0xFA,
                ZOOM = 0xFB,
                NONAME = 0xFC,
                PA1 = 0xFD,
                OEM_CLEAR = 0xFE,
                A = 0x41,
                B = 0x42,
                C = 0x43,
                D = 0x44,
                E = 0x45,
                F = 0x46,
                G = 0x47,
                H = 0x48,
                I = 0x49,
                J = 0x4a,
                K = 0x4b,
                L = 0x4c,
                M = 0x4d,
                N = 0x4e,
                O = 0x4f,
                P = 0x50,
                Q = 0x51,
                R = 0x52,
                S = 0x53,
                T = 0x54,
                U = 0x55,
                V = 0x56,
                W = 0x57,
                X = 0x58,
                Y = 0x59,
                Z = 0x5a,
                D0 = 0x30,
                D1 = 0x31,
                D2 = 0x32,
                D3 = 0x33,
                D4 = 0x34,
                D5 = 0x35,
                D6 = 0x36,
                D7 = 0x37,
                D8 = 0x38,
                D9 = 0x39
            }

            public enum SystemCursor
            {
                IDC_APPSTARTING = 32650,
                IDC_ARROW = 32512,
                IDC_CROSS = 32515,
                IDC_HAND = 32649,
                IDC_HELP = 32651,
                IDC_IBEAM = 32513,
                IDC_ICON = 32641,
                IDC_NO = 32648,
                IDC_SIZE = 32640,
                IDC_SIZEALL = 32646,
                IDC_SIZENESW = 32643,
                IDC_SIZENS = 32645,
                IDC_SIZENWSE = 32642,
                IDC_SIZEWE = 32644,
                IDC_UPARROW = 32516,
                IDC_WAIT = 32514
            }

            public enum HitTestResult
            {
                HTERROR = -2,
                HTTRANSPARENT = -1,
                HTNOWHERE = 0,
                HTCLIENT = 1,
                HTCAPTION = 2,
                HTSYSMENU = 3,
                HTGROWBOX = 4,
                HTSIZE = HTGROWBOX,
                HTMENU = 5,
                HTHSCROLL = 6,
                HTVSCROLL = 7,
                HTMINBUTTON = 8,
                HTREDUCE = HTMINBUTTON,
                HTMAXBUTTON = 9,
                HTZOOM = HTMAXBUTTON,
                HTLEFT = 10,
                HTRIGHT = 11,
                HTTOP = 12,
                HTTOPLEFT = 13,
                HTTOPRIGHT = 14,
                HTBOTTOM = 15,
                HTBOTTOMLEFT = 16,
                HTBOTTOMRIGHT = 17,
                HTBORDER = 18,
                HTOBJECT = 19,
                HTCLOSE = 20,
                HTHELP = 21
            }
            #endregion

            #region Types
            public delegate IntPtr WindowProc(IntPtr hwnd, uint msg, IntPtr wParam, IntPtr lParam);

            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
            public struct WindowClassEx
            {
                public uint Size;
                public WindowClassStyles Styles;
                [MarshalAs(UnmanagedType.FunctionPtr)]
                public WindowProc WindowProc;
                public int ClassExtraBytes;
                public int WindowExtraBytes;
                public IntPtr InstanceHandle;
                public IntPtr IconHandle;
                public IntPtr CursorHandle;
                public IntPtr BackgroundBrushHandle;
                public IntPtr MenuName;
                public IntPtr ClassName;
                public IntPtr SmallIconHandle;
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct CreateStruct
            {
                public IntPtr CreateParams;
                public IntPtr InstanceHandle;
                public IntPtr MenuHandle;
                public IntPtr ParentHwnd;
                public int Height;
                public int Width;
                public int Y;
                public int X;
                public WindowStyles Styles;
                public IntPtr Name;
                public IntPtr ClassName;
                public WindowExStyles ExStyles;
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct Message
            {
                public IntPtr WindowHandle;
                public uint message;
                public IntPtr wParam;
                public IntPtr lParam;
                public int time;
                public Point point;
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct MonitorInfo
            {
                public uint Size;
                public Rectangle MonitorRect;
                public Rectangle WorkRect;
                public MonitorInfoFlag Flags;
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct Rectangle
            {
                public int Left;
                public int Top;
                public int Right;
                public int Bottom;
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct Point
            {
                public int X;
                public int Y;
            }

            public delegate uint SetProcessDpiAwareness(WinApi.ProcessDpiAwareness dpi);
            public delegate uint GetDpiForMonitor(IntPtr hmonitor, WinApi.MonitorDpiType dpiType, out uint dpiX, out uint dpiY);
            #endregion

            #region Imports
            [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Ansi)]
            public static extern IntPtr LoadLibrary([MarshalAs(UnmanagedType.LPStr)]string lpFileName);

            [DllImport("kernel32", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
            public static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

            [DllImport("kernel32", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool FreeLibrary(IntPtr hModule);

            [DllImport("kernel32", CharSet = CharSet.Auto)]
            public static extern IntPtr GetModuleHandle(string lpModuleName);

            [DllImport("user32", CharSet = CharSet.Auto)]
            public static extern bool GetClassInfoEx(IntPtr hInstance, string lpClassName,
                out WindowClassEx lpWndClass);

            [DllImport("user32", CharSet = CharSet.Auto)]
            public static extern ushort RegisterClassEx([In] ref WindowClassEx lpwcx);

            [DllImport("user32", CharSet = CharSet.Auto)]
            public static extern IntPtr CreateWindowEx(WindowExStyles dwExStyle, string lpClassName,
                string lpWindowName, WindowStyles dwStyle, int x, int y, int nWidth, int nHeight,
                IntPtr hwndParent, IntPtr hMenu, IntPtr hInstance, IntPtr lpParam);

            [DllImport("user32", CharSet = CharSet.Auto)]
            public static extern IntPtr GetDC(IntPtr hWnd);

            [DllImport("user32", CharSet = CharSet.Auto)]
            public static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

            [DllImport("user32", CharSet = CharSet.Auto)]
            public static extern IntPtr DefWindowProc(IntPtr hwnd, uint uMsg, IntPtr wParam,
                IntPtr lParam);

            [DllImport("user32", CharSet = CharSet.Auto)]
            public static extern IntPtr SendMessage(IntPtr hwnd, uint msg, IntPtr wParam, IntPtr lParam);

            [DllImport("user32", CharSet = CharSet.Auto)]
            public static extern int GetMessage(out Message lpMsg, IntPtr hwnd, uint wMsgFilterMin,
                uint wMsgFilterMax);

            [DllImport("user32", CharSet = CharSet.Auto)]
            public static extern bool PeekMessage(out Message lpMsg, IntPtr hWnd, uint wMsgFilterMin,
                uint wMsgFilterMax, uint wRemoveMsg);

            [DllImport("user32", CharSet = CharSet.Auto, ExactSpelling = true)]
            public static extern bool TranslateMessage([In] ref Message lpMsg);

            [DllImport("user32", CharSet = CharSet.Auto)]
            public static extern IntPtr DispatchMessage([In] ref Message lpMsg);

            [DllImport("user32", CharSet = CharSet.Auto, ExactSpelling = true)]
            public static extern bool ShowWindow(IntPtr hwnd, ShowWindowCommands nCmdShow);

            [DllImport("user32", CharSet = CharSet.Auto, ExactSpelling = true)]
            public static extern IntPtr GetForegroundWindow();

            [DllImport("user32", CharSet = CharSet.Auto, ExactSpelling = true)]
            public static extern IntPtr GetDesktopWindow();

            [DllImport("user32", CharSet = CharSet.Auto, ExactSpelling = true)]
            public static extern IntPtr MonitorFromWindow(IntPtr hwnd, MonitorFlag dwFlags);

            [DllImport("user32", CharSet = CharSet.Auto)]
            public static extern bool GetMonitorInfo(IntPtr hMonitor, ref MonitorInfo lpmi);

            [DllImport("user32", CharSet = CharSet.Auto)]
            public static extern int GetWindowLong(IntPtr hwnd, int nIndex);

            [DllImport("user32", CharSet = CharSet.Auto)]
            public static extern int SetWindowLong(IntPtr hwnd, int nIndex, int dwNewLong);

            [DllImport("user32", CharSet = CharSet.Auto)]
            public static extern bool SetWindowText(IntPtr hwnd, string lpString);

            [DllImport("user32", CharSet = CharSet.Auto, ExactSpelling = true)]
            public static extern bool SetWindowPos(IntPtr hwnd, IntPtr hWndInsertAfter,
                int x, int y, int cx, int cy, WindowPositionFlags flags);

            [DllImport("user32", CharSet = CharSet.Auto, ExactSpelling = true)]
            public static extern bool GetWindowRect(IntPtr hwnd, out Rectangle lpRect);

            [DllImport("user32", CharSet = CharSet.Auto, ExactSpelling = true)]
            public static extern bool AdjustWindowRectEx(ref Rectangle lpRect, uint dwStyle,
                bool bMenu, uint dwExStyle);

            [DllImport("user32", CharSet = CharSet.Auto, ExactSpelling = true)]
            public static extern bool GetClientRect(IntPtr hwnd, out Rectangle lpRect);

            [DllImport("user32", CharSet = CharSet.Auto, ExactSpelling = true)]
            public static extern bool ScreenToClient(IntPtr hWnd, [In] [Out] ref Point lpPoint);

            [DllImport("user32", CharSet = CharSet.Auto, ExactSpelling = true)]
            public static extern IntPtr GetCapture();

            [DllImport("user32", CharSet = CharSet.Auto, ExactSpelling = true)]
            public static extern IntPtr SetCapture(IntPtr hWnd);

            [DllImport("user32", CharSet = CharSet.Auto, ExactSpelling = true)]
            public static extern bool ReleaseCapture();

            [DllImport("user32", CharSet = CharSet.Auto, ExactSpelling = true)]
            public static extern IntPtr SetFocus(IntPtr hWnd);

            [DllImport("user32", CharSet = CharSet.Auto, ExactSpelling = true)]
            public static extern IntPtr SetCursor(IntPtr hCursor);

            [DllImport("user32", CharSet = CharSet.Auto, ExactSpelling = true)]
            public static extern void PostQuitMessage(int nExitCode);

            [DllImport("user32", CharSet = CharSet.Auto)]
            public static extern IntPtr LoadIcon(IntPtr hInstance, IntPtr lpIconResource);

            [DllImport("user32", CharSet = CharSet.Auto)]
            public static extern IntPtr LoadCursor(IntPtr hInstance, IntPtr lpCursorResource);

            [DllImport("shell32", CharSet = CharSet.Auto, SetLastError = true)]
            public static extern IntPtr ShellExecute(IntPtr hwnd, string lpOperation, string lpFile,
                string lpParameters, string lpDirectory, int nShowCmd);

            [DllImport("kernel32", CharSet = CharSet.Auto, SetLastError = true)]
            [PreserveSig]
            public static extern uint GetModuleFileName([In] IntPtr hModule,
                [Out] StringBuilder lpFilename, [In] [MarshalAs(UnmanagedType.U4)] int nSize);
            #endregion
        }
    }
}
