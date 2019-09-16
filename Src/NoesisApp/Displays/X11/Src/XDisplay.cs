using System;
using System.Runtime.InteropServices;
using System.Text;
using Noesis;

namespace NoesisApp
{
    public class XDisplay : Display
    {
        public XDisplay(): this(false)
        {
        }

        public XDisplay(bool fullScreen)
        {
            _display = XOpenDisplay("");

            int screenNumber = XDefaultScreen(_display);
            int depth = XDefaultDepth(_display, screenNumber);
            IntPtr visual = XDefaultVisual(_display, screenNumber);
            IntPtr screenPtr = XScreenOfDisplay(_display, screenNumber);
            Screen screen = Noesis.Marshal.PtrToStructure<Screen>(screenPtr);

            IntPtr root = XRootWindow(_display, screenNumber);

            XSetWindowAttributes attr = new XSetWindowAttributes();
            attr.event_mask = EventMask.FocusChangeMask | EventMask.StructureNotifyMask | EventMask.SubstructureNotifyMask |
                EventMask.PointerMotionMask | EventMask.ButtonPressMask | EventMask.ButtonReleaseMask | EventMask.KeyPressMask | EventMask.KeyReleaseMask;
            _window = XCreateWindow(_display, root, 0, 0, (uint)screen.width, (uint)screen.height, 0, depth, (uint)WindowClass.InputOutput, visual, (IntPtr)AttributeMask.CWEventMask, ref attr);

            if (fullScreen)
            {
                uint[] data = { XInternAtom(_display, "_NET_WM_STATE_FULLSCREEN", true) };
                uint property = XInternAtom(_display, "_NET_WM_STATE", false);

                XChangeProperty(_display, (IntPtr)_window, property, 4 /*XA_ATOM*/, 32, 0 /*PropModeReplace*/, data, 1);
            }

            _xim = XOpenIM(_display, IntPtr.Zero, null, null);
            if (_xim == IntPtr.Zero)
            {
                XSetLocaleModifiers("@im=none");
                _xim = XOpenIM(_display, IntPtr.Zero, null, null);
            }

            if (_xim != IntPtr.Zero)
            {
                _xic = XCreateIC(_xim, "inputStyle", (IntPtr)0x0408L, "clientWindow", (IntPtr)_window, "focusWindow", (IntPtr)_window, IntPtr.Zero);
                if (_xic != IntPtr.Zero)
                {
                    XSetICFocus(_xic);
                    _utf8 = new UTF8Encoding();
                }
            }
        }

        ~XDisplay()
        {
        }

        #region Display overrides
        public override IntPtr NativeHandle
        {
            get { return _display; }
        }

        public override IntPtr NativeWindow
        {
            get { return _window; }
        }

        public override int ClientWidth
        {
            get
            {
                XWindowAttributes attr = new XWindowAttributes();
                XGetWindowAttributes(_display, _window, out attr);
                return (int)attr.width;
            }
        }

        public override int ClientHeight
        {
            get
            {
                XWindowAttributes attr = new XWindowAttributes();
                XGetWindowAttributes(_display, _window, out attr);
                return (int)attr.height;
            }
        }

        public override void EnterMessageLoop(bool runInBackground)
        {
            IntPtr e = System.Runtime.InteropServices.Marshal.AllocHGlobal(24 * sizeof(long));
            bool running = true;
            while (running)
            {
                while (XPending(_display) != 0)
                {
                    XNextEvent(_display, e);

                    if (XFilterEvent(e, IntPtr.Zero))
                    {
                        continue;
                    }

                    XAnyEvent anyEvent = (XAnyEvent)System.Runtime.InteropServices.Marshal.PtrToStructure(e, typeof(XAnyEvent));

                    switch ((Event)anyEvent.type)
                    {
                        case Event.ClientMessage:
                        {
                            running = false;
                            break;
                        }
                        case Event.ConfigureNotify:
                        {
                            XConfigureEvent configureEvent = (XConfigureEvent)System.Runtime.InteropServices.Marshal.PtrToStructure(e, typeof(XConfigureEvent));

                            LocationChanged?.Invoke(this, configureEvent.x, configureEvent.y);
                            SizeChanged?.Invoke(this, configureEvent.width, configureEvent.height);
                            break;
                        }

                        case Event.FocusIn:
                        {
                            Activated?.Invoke(this);
                            break;
                        }

                        case Event.FocusOut:
                        {

                            Deactivated?.Invoke(this);
                            break;
                        }

                        case Event.MotionNotify:
                        {
                            XMotionEvent motionEvent = (XMotionEvent)System.Runtime.InteropServices.Marshal.PtrToStructure(e, typeof(XMotionEvent));
                            MouseMove?.Invoke(this, motionEvent.x, motionEvent.y);
                            break;
                        }

                        case Event.ButtonPress:
                        {
                            XButtonEvent buttonEvent = (XButtonEvent)System.Runtime.InteropServices.Marshal.PtrToStructure(e, typeof(XButtonEvent));

                            switch ((Button)buttonEvent.button)
                            {
                                case Button.Button1:
                                {
                                    MouseButtonDown?.Invoke(this, buttonEvent.x, buttonEvent.y, MouseButton.Left);
                                    break;
                                }

                                case Button.Button2:
                                {
                                    MouseButtonDown?.Invoke(this, buttonEvent.x, buttonEvent.y, MouseButton.Middle);
                                    break;
                                }

                                case Button.Button3:
                                {
                                    MouseButtonDown?.Invoke(this, buttonEvent.x, buttonEvent.y, MouseButton.Right);
                                    break;
                                }

                                case Button.Button4:
                                {
                                    MouseWheel?.Invoke(this, buttonEvent.x, buttonEvent.y, 120);
                                    break;
                                }

                                case Button.Button5:
                                {
                                    MouseWheel?.Invoke(this, buttonEvent.x, buttonEvent.y, -120);
                                    break;
                                }
                            }
                            break;
                        }

                        case Event.ButtonRelease:
                        {
                            XButtonEvent buttonEvent = (XButtonEvent)System.Runtime.InteropServices.Marshal.PtrToStructure(e, typeof(XButtonEvent));

                            switch ((Button)buttonEvent.button)
                            {
                                case Button.Button1:
                                {
                                    MouseButtonUp?.Invoke(this, buttonEvent.x, buttonEvent.y, MouseButton.Left);
                                    break;
                                }

                                case Button.Button2:
                                {
                                    MouseButtonUp?.Invoke(this, buttonEvent.x, buttonEvent.y, MouseButton.Middle);
                                    break;
                                }

                                case Button.Button3:
                                {
                                    MouseButtonUp?.Invoke(this, buttonEvent.x, buttonEvent.y, MouseButton.Right);
                                    break;
                                }
                            }
                            break;
                        }

                        case Event.KeyPress:
                        {
                            XKeyEvent keyEvent = (XKeyEvent)System.Runtime.InteropServices.Marshal.PtrToStructure(e, typeof(XKeyEvent));
                            KeySym ks = XLookupKeysym(ref keyEvent, 0);
                            Key key = NoesisKey(ks);
                            if (key != Key.None)
                            {
                                KeyDown?.Invoke(this, key);
                            }

                           if (_xic != IntPtr.Zero)
                            {
                                Status status = 0;
                                byte[] buffer = new byte[256];
                                KeySym ret_ks = (KeySym)0;
                                int size = Xutf8LookupString(_xic, e, buffer, 255, ref ret_ks, ref status);
                                if (size > 0 && ((int)status == XLookupChars || (int)   status == XLookupBoth))
                                {
                                    buffer[size] = 0;
                                    Decoder decoder = _utf8.GetDecoder();
                                    char[] text = new char[256];
                                    int bytesUsed = 0;
                                    int charsUsed = 0;
                                    bool completed = false;
                                    decoder.Convert(buffer, 0, size, text, 0, 255, true, out bytesUsed, out charsUsed, out completed);
                                    for (int i = 0; i < charsUsed; ++i)
                                    {
                                        Char?.Invoke(this, text[i]);
                                    }
                                }
                            }
                            break;
                        }

                        case Event.KeyRelease:
                        {
                            XKeyEvent keyEvent = (XKeyEvent)System.Runtime.InteropServices.Marshal.PtrToStructure(e, typeof(XKeyEvent));
                            KeySym ks = XLookupKeysym(ref keyEvent, 0);
                            Key key = NoesisKey(ks);
                            if (key != Key.None)
                            {
                                KeyUp?.Invoke(this, key);
                            }
                            break;
                        }
                    }
                }

                Render?.Invoke(this);
            }
            System.Runtime.InteropServices.Marshal.FreeHGlobal(e);
        }

        public override void SetSize(int width, int height)
        {
            XWindowAttributes attr = new XWindowAttributes();
            XGetWindowAttributes(_display, _window, out attr);
            XResizeWindow(_display, _window, (uint)(width + attr.border_width), (uint)(height + attr.border_width));
        }

        public override void Show()
        {
            XMapWindow(_display, _window);
            XRaiseWindow(_display, _window);
        }
        #endregion

        #region Constants
        public enum WindowClass : uint
        {
            InputOutput = 1,
            InputOnly = 2
        }

        public enum AttributeMask : uint
        {
            CWBackPixmap = (1 << 0),
            CWBackPixel = (1 << 1),
            CWBorderPixmap = (1 << 2),
            CWBorderPixel = (1 << 3),
            CWBitGravity = (1 << 4),
            CWWinGravity = (1 << 5),
            CWBackingStore = (1 << 6),
            CWBackingPlanes = (1 << 7),
            CWBackingPixel = (1 << 8),
            CWOverrideRedirect = (1 << 9),
            CWSaveUnder = (1 << 10),
            CWEventMask = (1 << 11),
            CWDontPropagate = (1 << 12),
            CWColormap = (1 << 13),
            CWCursor = (1 << 14),
        }

        public enum Status : int
        {
            Failure = 0,
        }

        public enum Event : int
        {
            KeyPress = 2,
            KeyRelease = 3,
            ButtonPress = 4,
            ButtonRelease = 5,
            MotionNotify = 6,
            EnterNotify = 7,
            LeaveNotify = 8,
            FocusIn = 9,
            FocusOut = 10,
            KeymapNotify = 11,
            Expose = 12,
            GraphicsExpose = 13,
            NoExpose = 14,
            VisibilityNotify = 15,
            CreateNotify = 16,
            DestroyNotify = 17,
            UnmapNotify = 18,
            MapNotify = 19,
            MapRequest = 20,
            ReparentNotify = 21,
            ConfigureNotify = 22,
            ConfigureRequest = 23,
            GravityNotify = 24,
            ResizeRequest = 25,
            CirculateNotify = 26,
            CirculateRequest = 27,
            PropertyNotify = 28,
            SelectionClear = 29,
            SelectionRequest = 30,
            SelectionNotify = 31,
            ColormapNotify = 32,
            ClientMessage = 33,
            MappingNotify = 34,
            GenericEvent = 35,
            LASTEvent = 36
        }

        public enum EventMask : long
        {
            NoEventMask = 0L,
            KeyPressMask = (1L << 0),
            KeyReleaseMask = (1L << 1),
            ButtonPressMask = (1L << 2),
            ButtonReleaseMask = (1L << 3),
            EnterWindowMask = (1L << 4),
            LeaveWindowMask = (1L << 5),
            PointerMotionMask = (1L << 6),
            PointerMotionHintMask = (1L << 7),
            Button1MotionMask = (1L << 8),
            Button2MotionMask = (1L << 9),
            Button3MotionMask = (1L << 10),
            Button4MotionMask = (1L << 11),
            Button5MotionMask = (1L << 12),
            ButtonMotionMask = (1L << 13),
            KeymapStateMask = (1L << 14),
            ExposureMask = (1L << 15),
            VisibilityChangeMask = (1L << 16),
            StructureNotifyMask = (1L << 17),
            ResizeRedirectMask = (1L << 18),
            SubstructureNotifyMask = (1L << 19),
            SubstructureRedirectMask = (1L << 20),
            FocusChangeMask = (1L << 21),
            PropertyChangeMask = (1L << 22),
            ColormapChangeMask = (1L << 23),
            OwnerGrabButtonMask = (1L << 24),
        }

        public enum Button : uint
        {
            Button1 = 1,
            Button2 = 2,
            Button3 = 3,
            Button4 = 4,
            Button5 = 5
        }

        public enum KeySym : uint
        {
            XK_Cancel = 0xff69,
            XK_BackSpace = 0xff08,
            XK_Tab = 0xff09,
            XK_Linefeed = 0xff0a,
            XK_Clear = 0xff0b,
            XK_Return = 0xff0d,
            XK_Pause = 0xff13,
            XK_Caps_Lock = 0xffe5,
            XK_Escape = 0xff1b,
            XK_space = 0x0020,
            XK_Page_Up = 0xff55,
            XK_Page_Down = 0xff56,
            XK_End = 0xff57,
            XK_Home = 0xff50,
            XK_Left = 0xff51,
            XK_Up = 0xff52,
            XK_Right = 0xff53,
            XK_Down = 0xff54,
            XK_Select = 0xff60,
            XK_Execute = 0xff62,
            XK_Print = 0xff61,
            XK_Insert = 0xff63,
            XK_Delete = 0xffff,
            XK_Help = 0xff6a,
            XK_0 = 0x0030,
            XK_1 = 0x0031,
            XK_2 = 0x0032,
            XK_3 = 0x0033,
            XK_4 = 0x0034,
            XK_5 = 0x0035,
            XK_6 = 0x0036,
            XK_7 = 0x0037,
            XK_8 = 0x0038,
            XK_9 = 0x0039,
            XK_a = 0x0061,
            XK_b = 0x0062,
            XK_c = 0x0063,
            XK_d = 0x0064,
            XK_e = 0x0065,
            XK_f = 0x0066,
            XK_g = 0x0067,
            XK_h = 0x0068,
            XK_i = 0x0069,
            XK_j = 0x006a,
            XK_k = 0x006b,
            XK_l = 0x006c,
            XK_m = 0x006d,
            XK_n = 0x006e,
            XK_o = 0x006f,
            XK_p = 0x0070,
            XK_q = 0x0071,
            XK_r = 0x0072,
            XK_s = 0x0073,
            XK_t = 0x0074,
            XK_u = 0x0075,
            XK_v = 0x0076,
            XK_w = 0x0077,
            XK_x = 0x0078,
            XK_y = 0x0079,
            XK_z = 0x007a,
            XK_KP_0 = 0xffb0,
            XK_KP_1 = 0xffb1,
            XK_KP_2 = 0xffb2,
            XK_KP_3 = 0xffb3,
            XK_KP_4 = 0xffb4,
            XK_KP_5 = 0xffb5,
            XK_KP_6 = 0xffb6,
            XK_KP_7 = 0xffb7,
            XK_KP_8 = 0xffb8,
            XK_KP_9 = 0xffb9,
            XK_KP_Multiply = 0xffaa,
            XK_KP_Add = 0xffab,
            XK_KP_Separator = 0xffac,
            XK_KP_Subtract = 0xffad,
            XK_KP_Decimal = 0xffae,
            XK_KP_Divide = 0xffaf,
            XK_F1 = 0xffbe,
            XK_F2 = 0xffbf,
            XK_F3 = 0xffc0,
            XK_F4 = 0xffc1,
            XK_F5 = 0xffc2,
            XK_F6 = 0xffc3,
            XK_F7 = 0xffc4,
            XK_F8 = 0xffc5,
            XK_F9 = 0xffc6,
            XK_F10 = 0xffc7,
            XK_F11 = 0xffc8,
            XK_F12 = 0xffc9,
            XK_F13 = 0xffca,
            XK_F14 = 0xffcb,
            XK_F15 = 0xffcc,
            XK_F16 = 0xffcd,
            XK_F17 = 0xffce,
            XK_F18 = 0xffcf,
            XK_F19 = 0xffd0,
            XK_F20 = 0xffd1,
            XK_F21 = 0xffd2,
            XK_F22 = 0xffd3,
            XK_F23 = 0xffd4,
            XK_F24 = 0xffd5,
            XK_Num_Lock = 0xff7f,
            XK_Scroll_Lock = 0xff14,
            XK_Shift_L = 0xffe1,
            XK_Shift_R = 0xffe2,
            XK_Control_L = 0xffe3,
            XK_Control_R = 0xffe4,
            XK_Alt_L = 0xffe9,
            XK_Alt_R = 0xffea,
        }
        #endregion

        #region Types
        [StructLayout(LayoutKind.Sequential)]
        public struct Screen
        {
            public IntPtr ext_data;
            public IntPtr display;
            public IntPtr root;
            public int width, height;
            public int mwidth, mheight;
            public int ndepths;
            public IntPtr depths;
            public int root_depth;
            public IntPtr root_visual;
            public IntPtr default_gc;
            public IntPtr cmap;
            public IntPtr white_pixel;
            public IntPtr black_pixel;
            public int max_maps, min_maps;
            public int backing_store;
            public bool save_unders;
            public long root_input_mask;
        }

        [StructLayout(LayoutKind.Sequential, Size = (24 * sizeof(long)))]
        public struct XAnyEvent
        {
            public int type;
            public IntPtr serial;
            public bool send_event;
            public IntPtr display;
            public IntPtr window;
        }

        [StructLayout(LayoutKind.Sequential, Size = (24 * sizeof(long)))]
        public struct XConfigureEvent
        {
            public int type;
            public IntPtr serial;
            public bool send_event;
            public IntPtr display;
            public IntPtr parent;
            public IntPtr window;
            public int x;
            public int y;
            public int width;
            public int height;
            public int border_width;
            public IntPtr above;
            public bool override_redirect;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct XMotionEvent
        {
            public int type;
            public IntPtr serial;
            public bool send_event;
            public IntPtr display;
            public IntPtr window;
            public IntPtr root;
            public IntPtr subwindow;
            public IntPtr time;
            public int x, y;
            public int x_root, y_root;
            public uint state;
            public byte is_hint;
            public bool same_screen;
        }

        [StructLayout(LayoutKind.Sequential, Size = (24 * sizeof(long)))]
        public struct XButtonEvent
        {
            public int type;
            public IntPtr serial;
            public bool send_event;
            public IntPtr display;
            public IntPtr window;
            public IntPtr root;
            public IntPtr subwindow;
            public IntPtr time;
            public int x, y;
            public int x_root, y_root;
            public uint state;
            public uint button;
            public bool same_screen;
        }

        [StructLayout(LayoutKind.Sequential, Size = (24 * sizeof(long)))]
        public struct XKeyEvent
        {
            public int type;
            public IntPtr serial;
            public bool send_event;
            public IntPtr display;
            public IntPtr window;
            public IntPtr root;
            public IntPtr subwindow;
            public IntPtr time;
            public int x, y;
            public int x_root, y_root;
            public uint state;
            public uint keycode;
            public bool same_screen;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct XSetWindowAttributes
        {
            public IntPtr background_pixmap;
            public IntPtr background_pixel;
            public IntPtr border_pixmap;
            public IntPtr border_pixel;
            public int bit_gravity;
            public int win_gravity;
            public int backing_store;
            public IntPtr backing_planes;
            public IntPtr backing_pixel;
            public bool save_under;
            public EventMask event_mask;
            public EventMask do_not_propagate_mask;
            public bool override_redirect;
            public IntPtr colormap;
            public Cursor cursor;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct XWindowAttributes
        {
            public int x, y;
            public uint width, height;
            public int border_width;
            public int depth;
            public IntPtr visual;
            public IntPtr root;
            public int @class;
            public int bit_gravity;
            public int win_gravity;
            public int backing_store;
            public IntPtr backing_planes;
            public IntPtr backing_pixel;
            public bool save_under;
            public IntPtr colormap;
            public bool map_installed;
            public int map_state;
            public long all_event_masks;
            public long your_event_masks;
            public long do_not_propagate_mask;
            public bool override_redirect;
            public IntPtr screen;
        }
        #endregion

        #region Key mapping
        private static Key NoesisKey(KeySym k)
        {
            switch (k)
            {
                case KeySym.XK_Cancel: return Key.Cancel;
                case KeySym.XK_BackSpace: return Key.Back;
                case KeySym.XK_Tab: return Key.Tab;
                case KeySym.XK_Linefeed: return Key.LineFeed;
                case KeySym.XK_Clear: return Key.Clear;
                case KeySym.XK_Return: return Key.Return;
                case KeySym.XK_Pause: return Key.Pause;
                case KeySym.XK_Caps_Lock: return Key.CapsLock;
                case KeySym.XK_Escape: return Key.Escape;
                case KeySym.XK_space: return Key.Space;
                case KeySym.XK_Page_Up: return Key.PageUp;
                case KeySym.XK_Page_Down: return Key.PageDown;
                case KeySym.XK_End: return Key.End;
                case KeySym.XK_Home: return Key.Home;
                case KeySym.XK_Left: return Key.Left;
                case KeySym.XK_Up: return Key.Up;
                case KeySym.XK_Right: return Key.Right;
                case KeySym.XK_Down: return Key.Down;
                case KeySym.XK_Select: return Key.Select;
                case KeySym.XK_Execute: return Key.Execute;
                case KeySym.XK_Print: return Key.PrintScreen;
                case KeySym.XK_Insert: return Key.Insert;
                case KeySym.XK_Delete: return Key.Delete;
                case KeySym.XK_Help: return Key.Help;
                case KeySym.XK_0: return Key.D0;
                case KeySym.XK_1: return Key.D1;
                case KeySym.XK_2: return Key.D2;
                case KeySym.XK_3: return Key.D3;
                case KeySym.XK_4: return Key.D4;
                case KeySym.XK_5: return Key.D5;
                case KeySym.XK_6: return Key.D6;
                case KeySym.XK_7: return Key.D7;
                case KeySym.XK_8: return Key.D8;
                case KeySym.XK_9: return Key.D9;
                case KeySym.XK_a: return Key.A;
                case KeySym.XK_b: return Key.B;
                case KeySym.XK_c: return Key.C;
                case KeySym.XK_d: return Key.D;
                case KeySym.XK_e: return Key.E;
                case KeySym.XK_f: return Key.F;
                case KeySym.XK_g: return Key.G;
                case KeySym.XK_h: return Key.H;
                case KeySym.XK_i: return Key.I;
                case KeySym.XK_j: return Key.J;
                case KeySym.XK_k: return Key.K;
                case KeySym.XK_l: return Key.L;
                case KeySym.XK_m: return Key.M;
                case KeySym.XK_n: return Key.N;
                case KeySym.XK_o: return Key.O;
                case KeySym.XK_p: return Key.P;
                case KeySym.XK_q: return Key.Q;
                case KeySym.XK_r: return Key.R;
                case KeySym.XK_s: return Key.S;
                case KeySym.XK_t: return Key.T;
                case KeySym.XK_u: return Key.U;
                case KeySym.XK_v: return Key.V;
                case KeySym.XK_w: return Key.W;
                case KeySym.XK_x: return Key.X;
                case KeySym.XK_y: return Key.Y;
                case KeySym.XK_z: return Key.Z;
                case KeySym.XK_KP_0: return Key.NumPad0;
                case KeySym.XK_KP_1: return Key.NumPad1;
                case KeySym.XK_KP_2: return Key.NumPad2;
                case KeySym.XK_KP_3: return Key.NumPad3;
                case KeySym.XK_KP_4: return Key.NumPad4;
                case KeySym.XK_KP_5: return Key.NumPad5;
                case KeySym.XK_KP_6: return Key.NumPad6;
                case KeySym.XK_KP_7: return Key.NumPad7;
                case KeySym.XK_KP_8: return Key.NumPad8;
                case KeySym.XK_KP_9: return Key.NumPad9;
                case KeySym.XK_KP_Multiply: return Key.Multiply;
                case KeySym.XK_KP_Add: return Key.Add;
                case KeySym.XK_KP_Separator: return Key.Separator;
                case KeySym.XK_KP_Subtract: return Key.Subtract;
                case KeySym.XK_KP_Decimal: return Key.Decimal;
                case KeySym.XK_KP_Divide: return Key.Divide;
                case KeySym.XK_F1: return Key.F1;
                case KeySym.XK_F2: return Key.F2;
                case KeySym.XK_F3: return Key.F3;
                case KeySym.XK_F4: return Key.F4;
                case KeySym.XK_F5: return Key.F5;
                case KeySym.XK_F6: return Key.F6;
                case KeySym.XK_F7: return Key.F7;
                case KeySym.XK_F8: return Key.F8;
                case KeySym.XK_F9: return Key.F9;
                case KeySym.XK_F10: return Key.F10;
                case KeySym.XK_F11: return Key.F11;
                case KeySym.XK_F12: return Key.F12;
                case KeySym.XK_F13: return Key.F13;
                case KeySym.XK_F14: return Key.F14;
                case KeySym.XK_F15: return Key.F15;
                case KeySym.XK_F16: return Key.F16;
                case KeySym.XK_F17: return Key.F17;
                case KeySym.XK_F18: return Key.F18;
                case KeySym.XK_F19: return Key.F19;
                case KeySym.XK_F20: return Key.F20;
                case KeySym.XK_F21: return Key.F21;
                case KeySym.XK_F22: return Key.F22;
                case KeySym.XK_F23: return Key.F23;
                case KeySym.XK_F24: return Key.F24;
                case KeySym.XK_Num_Lock: return Key.NumLock;
                case KeySym.XK_Scroll_Lock: return Key.Scroll;
                case KeySym.XK_Shift_L: return Key.LeftShift;
                case KeySym.XK_Shift_R: return Key.RightShift;
                case KeySym.XK_Control_L: return Key.LeftCtrl;
                case KeySym.XK_Control_R: return Key.RightCtrl;
                case KeySym.XK_Alt_L: return Key.LeftAlt;
                case KeySym.XK_Alt_R: return Key.RightAlt;
            }

            return Key.None;
        }

        const int XBufferOverflow = -1;
        const int XLookupNone = 1;
        const int XLookupChars = 2;
        const int XLookupKeySym = 3;
        const int XLookupBoth = 4;
        #endregion

        #region Imports
        [DllImport("libX11.so.6")]
        public static extern IntPtr XOpenDisplay(string display);

        [DllImport("libX11.so.6")]
        public static extern int XDefaultScreen(IntPtr display);

        [DllImport("libX11.so.6")]
        public static extern int XDefaultDepth(IntPtr display, int screen_number);

        [DllImport("libX11.so.6")]
        public static extern IntPtr XDefaultVisual(IntPtr display, int screen_number);

        [DllImport("libX11.so.6")]
        public static extern IntPtr XScreenOfDisplay(IntPtr display, int screen_number);

        [DllImport("libX11.so.6")]
        public static extern IntPtr XRootWindow(IntPtr display, int screen_number);

        [DllImport("libX11.so.6")]
        public static extern IntPtr XCreateWindow(IntPtr display, IntPtr parent, int x, int y, uint width, uint height, uint border_width, int depth, uint @class, IntPtr visual, IntPtr valuemask, ref XSetWindowAttributes attributes);

        [DllImport("libX11.so.6")]
        public static extern int XMapWindow(IntPtr display, IntPtr window);

        [DllImport("libX11.so.6")]
        public static extern Status XRaiseWindow(IntPtr display, IntPtr window);

        [DllImport("libX11.so.6")]
        public static extern int XPending(IntPtr display);

        [DllImport("libX11.so.6")]
        public static extern Status XNextEvent(IntPtr display, IntPtr event_return);

        [DllImport("libX11.so.6")]
        public static extern bool XFilterEvent(IntPtr @event, IntPtr window);

        [DllImport("libX11.so.6")]
        public static extern Status XGetWindowAttributes(IntPtr display, IntPtr window, out XWindowAttributes attributes);

        [DllImport("libX11.so.6")]
        public static extern Status XResizeWindow(IntPtr display, IntPtr window, uint width, uint height);

        [DllImport("libX11.so.6")]
        public static extern KeySym XLookupKeysym(ref XKeyEvent key_event, int index);

        [DllImport("libX11.so.6")]
        public static extern uint XInternAtom(IntPtr display, string atom_name, bool only_if_exists);

        [DllImport("libX11.so.6")]
        public static extern int XChangeProperty(IntPtr display, IntPtr window, uint property, uint type, int format, int mode, uint[] data, int nelements);

        [DllImport("libX11.so.6")]
        public static extern IntPtr XOpenIM(IntPtr display, IntPtr db, string res_name, string res_class);

        [DllImport("libX11.so.6")]
        public static extern IntPtr XSetLocaleModifiers(string modifier_list);

        [DllImport("libX11.so.6", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr XCreateIC(IntPtr im, string arg0, IntPtr val0, string arg1, IntPtr val1, string arg2, IntPtr val2, IntPtr terminator);

        [DllImport("libX11.so.6")]
        public static extern void XSetICFocus(IntPtr ic);

        [DllImport("libX11.so.6")]
        public static extern int Xutf8LookupString(IntPtr ic, IntPtr @event, byte[] buffer_return, int bytes_buffer, ref KeySym keysym_return, ref Status status_return);
        #endregion

        private IntPtr _display;
        private IntPtr _window;
        private IntPtr _xim;
        private IntPtr _xic;
        private UTF8Encoding _utf8;
    }
}
