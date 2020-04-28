using System;
using Android.Views;
using Android.Views.InputMethods;
using Android.Content;
using Android.App;

namespace NoesisApp
{
    public class AndroidDisplay : Display
    {
        public AndroidDisplay()
        {
            FillKeyTable();

            while (AndroidActivity.Current.View.NativeHandle == IntPtr.Zero)
            {
                ProcessMessages();
            }

            Activity activity = (Activity)AndroidActivity.Current.View.Context;
            View = activity.Window.DecorView;
        }

        public override IntPtr NativeHandle
        {
            get { return IntPtr.Zero; }
        }

        public override IntPtr NativeWindow
        {
            get { return AndroidActivity.Current.View.NativeHandle; }
        }

        public override int ClientWidth
        {
            get { return AndroidActivity.Current.View.Width; }
        }

        public override int ClientHeight
        {
            get { return AndroidActivity.Current.View.Height; }
        }

        public override void OpenSoftwareKeyboard(Noesis.UIElement focused)
        {
            InputManager?.ShowSoftInput(View, ShowFlags.Implicit);
        }

        public override void CloseSoftwareKeyboard()
        {
            InputManager?.HideSoftInputFromWindow(View.WindowToken, HideSoftInputFlags.None);
        }

        public override void Show()
        {
            LocationChanged?.Invoke(this, 0, 0);
        }

        public override void OpenUrl(string url)
        {
            var uri = Android.Net.Uri.Parse(url);
            var intent = new Intent(Intent.ActionView, uri);
            Activity activity = (Activity)AndroidActivity.Current.View.Context;
            activity.StartActivity(intent);
        }

        public override void EnterMessageLoop(bool runInBackground)
        {
            while (ProcessMessages())
            {
                if (NativeWindow != IntPtr.Zero)
                {
                    Render?.Invoke(this);
                }
            }
        }

        public override void Close()
        {
            AndroidActivity.Current.Finish();
        }

        private bool ProcessMessages()
        {
            int numEvents = AndroidActivity.Current.EventQueue.Count;
            for (int i = 0; i < numEvents; ++i)
            {
                ActivityEvent ev;
                if (AndroidActivity.Current.EventQueue.TryDequeue(out ev))
                {
                    switch (ev.Type)
                    {
                        case ActivityEventType.WindowCreated:
                        {
                            NativeHandleChanged?.Invoke(this, NativeWindow);
                            break;
                        }
                        case ActivityEventType.WindowDestroyed:
                        {
                            break;
                        }
                        case ActivityEventType.WindowSizeChanged:
                        {
                            SizeChanged?.Invoke(this, ClientWidth, ClientHeight);
                            break;
                        }
                        case ActivityEventType.KeyEvent:
                        {
                            OnKeyEvent((KeyActivityEvent)ev);
                            break;
                        }
                        case ActivityEventType.TouchEvent:
                        {
                            OnTouchEvent((TouchActivityEvent)ev);
                            break;
                        }
                    }
                }
            }

            return true;
        }

        private void OnKeyEvent(KeyActivityEvent ev)
        {
            switch (ev.Action)
            {
                case KeyEventActions.Down:
                {
                    Noesis.Key key = _keyTable[(int)ev.KeyCode];
                    if (key != Noesis.Key.None)
                    {
                        KeyDown?.Invoke(this, key);
                    }

                    if (ev.Char != 0)
                    {
                        Char?.Invoke(this, ev.Char);
                    }

                    break;
                }
                case KeyEventActions.Up:
                {
                    Noesis.Key key = _keyTable[(int)ev.KeyCode];
                    if (key != Noesis.Key.None)
                    {
                        KeyUp?.Invoke(this, key);
                    }

                    break;
                }
            }
        }

        private void OnTouchEvent(TouchActivityEvent ev)
        {
            for (int i = 0; i < ev.Pointers.Count; ++i)
            {
                TouchActivityEvent.PointerInfo pointer = ev.Pointers[i];

                switch (ev.Action)
                {
                    case MotionEventActions.Down:
                    case MotionEventActions.PointerDown:
                    {
                        TouchDown?.Invoke(this, pointer.X, pointer.Y, pointer.Id);
                        break;
                    }
                    case MotionEventActions.Up:
                    case MotionEventActions.PointerUp:
                    {
                        TouchUp?.Invoke(this, pointer.X, pointer.Y, pointer.Id);
                        break;
                    }
                    case MotionEventActions.Move:
                    {
                        TouchMove?.Invoke(this, pointer.X, pointer.Y, pointer.Id);
                        break;
                    }
                }
            }
        }

        private void FillKeyTable()
        {
            _keyTable = new Noesis.Key[]
            {
                /*_keyTable[Keycode.Unknown] = */ Noesis.Key.None,
                /*_keyTable[Keycode.SoftLeft] = */ Noesis.Key.None,
                /*_keyTable[Keycode.SoftRight] = */ Noesis.Key.None,
                /*_keyTable[Keycode.Home] = */ Noesis.Key.Home,
                /*_keyTable[Keycode.Back] = */ Noesis.Key.None,
                /*_keyTable[Keycode.Call] = */ Noesis.Key.None,
                /*_keyTable[Keycode.Endcall] = */ Noesis.Key.None,
                /*_keyTable[Keycode.Num0] = */ Noesis.Key.D0,
                /*_keyTable[Keycode.Num1] = */ Noesis.Key.D1,
                /*_keyTable[Keycode.Num2] = */ Noesis.Key.D2,
                /*_keyTable[Keycode.Num3] = */ Noesis.Key.D3,
                /*_keyTable[Keycode.Num4] = */ Noesis.Key.D4,
                /*_keyTable[Keycode.Num5] = */ Noesis.Key.D5,
                /*_keyTable[Keycode.Num6] = */ Noesis.Key.D6,
                /*_keyTable[Keycode.Num7] = */ Noesis.Key.D7,
                /*_keyTable[Keycode.Num8] = */ Noesis.Key.D8,
                /*_keyTable[Keycode.Num9] = */ Noesis.Key.D9,
                /*_keyTable[Keycode.Star] = */ Noesis.Key.Multiply,
                /*_keyTable[Keycode.Pound] = */ Noesis.Key.None,
                /*_keyTable[Keycode.DpadUp] = */ Noesis.Key.Up,
                /*_keyTable[Keycode.DpadDown] = */ Noesis.Key.Down,
                /*_keyTable[Keycode.DpadLeft] = */ Noesis.Key.Left,
                /*_keyTable[Keycode.DpadRight] = */ Noesis.Key.Right,
                /*_keyTable[Keycode.DpadCenter] = */ Noesis.Key.Return,
                /*_keyTable[Keycode.VolumeUp] = */ Noesis.Key.None,
                /*_keyTable[Keycode.VolumeDown] = */ Noesis.Key.None,
                /*_keyTable[Keycode.Power] = */ Noesis.Key.None,
                /*_keyTable[Keycode.Camera] = */ Noesis.Key.None,
                /*_keyTable[Keycode.Clear] = */ Noesis.Key.None,
                /*_keyTable[Keycode.A] = */ Noesis.Key.A,
                /*_keyTable[Keycode.B] = */ Noesis.Key.B,
                /*_keyTable[Keycode.C] = */ Noesis.Key.C,
                /*_keyTable[Keycode.D] = */ Noesis.Key.D,
                /*_keyTable[Keycode.E] = */ Noesis.Key.E,
                /*_keyTable[Keycode.F] = */ Noesis.Key.F,
                /*_keyTable[Keycode.G] = */ Noesis.Key.G,
                /*_keyTable[Keycode.H] = */ Noesis.Key.H,
                /*_keyTable[Keycode.I] = */ Noesis.Key.I,
                /*_keyTable[Keycode.J] = */ Noesis.Key.J,
                /*_keyTable[Keycode.K] = */ Noesis.Key.K,
                /*_keyTable[Keycode.L] = */ Noesis.Key.L,
                /*_keyTable[Keycode.M] = */ Noesis.Key.M,
                /*_keyTable[Keycode.N] = */ Noesis.Key.N,
                /*_keyTable[Keycode.O] = */ Noesis.Key.O,
                /*_keyTable[Keycode.P] = */ Noesis.Key.P,
                /*_keyTable[Keycode.Q] = */ Noesis.Key.Q,
                /*_keyTable[Keycode.R] = */ Noesis.Key.R,
                /*_keyTable[Keycode.S] = */ Noesis.Key.S,
                /*_keyTable[Keycode.T] = */ Noesis.Key.T,
                /*_keyTable[Keycode.U] = */ Noesis.Key.U,
                /*_keyTable[Keycode.V] = */ Noesis.Key.V,
                /*_keyTable[Keycode.W] = */ Noesis.Key.W,
                /*_keyTable[Keycode.X] = */ Noesis.Key.X,
                /*_keyTable[Keycode.Y] = */ Noesis.Key.Y,
                /*_keyTable[Keycode.Z] = */ Noesis.Key.Z,
                /*_keyTable[Keycode.Comma] = */ Noesis.Key.OemComma,
                /*_keyTable[Keycode.Period] = */ Noesis.Key.OemPeriod,
                /*_keyTable[Keycode.AltLeft] = */ Noesis.Key.LeftAlt,
                /*_keyTable[Keycode.AltRight] = */ Noesis.Key.RightAlt,
                /*_keyTable[Keycode.ShiftLeft] = */ Noesis.Key.LeftShift,
                /*_keyTable[Keycode.ShiftRight] = */ Noesis.Key.RightShift,
                /*_keyTable[Keycode.Tab] = */ Noesis.Key.Tab,
                /*_keyTable[Keycode.Space] = */ Noesis.Key.Space,
                /*_keyTable[Keycode.Sym] = */ Noesis.Key.RightAlt,
                /*_keyTable[Keycode.Explorer] = */ Noesis.Key.None,
                /*_keyTable[Keycode.Envelope] = */ Noesis.Key.None,
                /*_keyTable[Keycode.Enter] = */ Noesis.Key.Enter,
                /*_keyTable[Keycode.Del] = */ Noesis.Key.Back,
                /*_keyTable[Keycode.Grave] = */ Noesis.Key.Oem3,
                /*_keyTable[Keycode.Minus] = */ Noesis.Key.Subtract,
                /*_keyTable[Keycode.Equals] = */ Noesis.Key.OemPlus,
                /*_keyTable[Keycode.LeftBracket] = */ Noesis.Key.Oem4,
                /*_keyTable[Keycode.RightBracket] = */ Noesis.Key.Oem6,
                /*_keyTable[Keycode.Backslash] = */ Noesis.Key.Oem5,
                /*_keyTable[Keycode.Semicolon] = */ Noesis.Key.Oem1,
                /*_keyTable[Keycode.Apostrophe] = */ Noesis.Key.Oem7,
                /*_keyTable[Keycode.Slash] = */ Noesis.Key.Oem2,
                /*_keyTable[Keycode.At] = */ Noesis.Key.None,
                /*_keyTable[Keycode.Num] = */ Noesis.Key.LeftAlt,
                /*_keyTable[Keycode.Headsethook] = */ Noesis.Key.None,
                /*_keyTable[Keycode.Focus] = */ Noesis.Key.None,
                /*_keyTable[Keycode.Plus] = */ Noesis.Key.OemPlus,
                /*_keyTable[Keycode.Menu] = */ Noesis.Key.LeftAlt,
                /*_keyTable[Keycode.Notification] = */ Noesis.Key.None,
                /*_keyTable[Keycode.Search] = */ Noesis.Key.None,
                /*_keyTable[Keycode.MediaPlayPause] = */ Noesis.Key.None,
                /*_keyTable[Keycode.MediaStop] = */ Noesis.Key.None,
                /*_keyTable[Keycode.MediaNext] = */ Noesis.Key.None,
                /*_keyTable[Keycode.MediaPrevious] = */ Noesis.Key.None,
                /*_keyTable[Keycode.MediaRewind] = */ Noesis.Key.None,
                /*_keyTable[Keycode.MediaFastForward] = */ Noesis.Key.None,
                /*_keyTable[Keycode.Mute] = */ Noesis.Key.None,
                /*_keyTable[Keycode.PageUp] = */ Noesis.Key.Prior,
                /*_keyTable[Keycode.PageDown] = */ Noesis.Key.Next,
                /*_keyTable[Keycode.Pictsymbols] = */ Noesis.Key.None,
                /*_keyTable[Keycode.SwitchCharset] = */ Noesis.Key.None,
                /*_keyTable[Keycode.ButtonA] = */ Noesis.Key.None,
                /*_keyTable[Keycode.ButtonB] = */ Noesis.Key.None,
                /*_keyTable[Keycode.ButtonC] = */ Noesis.Key.None,
                /*_keyTable[Keycode.ButtonX] = */ Noesis.Key.None,
                /*_keyTable[Keycode.ButtonY] = */ Noesis.Key.None,
                /*_keyTable[Keycode.ButtonZ] = */ Noesis.Key.None,
                /*_keyTable[Keycode.ButtonL1] = */ Noesis.Key.None,
                /*_keyTable[Keycode.ButtonR1] = */ Noesis.Key.None,
                /*_keyTable[Keycode.ButtonL2] = */ Noesis.Key.None,
                /*_keyTable[Keycode.ButtonR2] = */ Noesis.Key.None,
                /*_keyTable[Keycode.ButtonThumbl] = */ Noesis.Key.None,
                /*_keyTable[Keycode.ButtonThumbr] = */ Noesis.Key.None,
                /*_keyTable[Keycode.ButtonStart] = */ Noesis.Key.None,
                /*_keyTable[Keycode.ButtonSelect] = */ Noesis.Key.None,
                /*_keyTable[Keycode.ButtonMode] = */ Noesis.Key.None,
                /*_keyTable[Keycode.Escape] = */ Noesis.Key.Escape,
                /*_keyTable[Keycode.ForwardDel] = */ Noesis.Key.Delete,
                /*_keyTable[Keycode.CtrlLeft] = */ Noesis.Key.LeftCtrl,
                /*_keyTable[Keycode.CtrlRight] = */ Noesis.Key.RightCtrl,
                /*_keyTable[Keycode.CapsLock] = */ Noesis.Key.CapsLock,
                /*_keyTable[Keycode.ScrollLock] = */ Noesis.Key.Scroll,
                /*_keyTable[Keycode.MetaLeft] = */ Noesis.Key.None,
                /*_keyTable[Keycode.MetaRight] = */ Noesis.Key.None,
                /*_keyTable[Keycode.Function] = */ Noesis.Key.None,
                /*_keyTable[Keycode.Sysrq] = */ Noesis.Key.None,
                /*_keyTable[Keycode.Break] = */ Noesis.Key.Pause,
                /*_keyTable[Keycode.MoveHome] = */ Noesis.Key.Home,
                /*_keyTable[Keycode.MoveEnd] = */ Noesis.Key.End,
                /*_keyTable[Keycode.Insert] = */ Noesis.Key.Insert,
                /*_keyTable[Keycode.Forward] = */ Noesis.Key.None,
                /*_keyTable[Keycode.MediaPlay] = */ Noesis.Key.None,
                /*_keyTable[Keycode.MediaPause] = */ Noesis.Key.None,
                /*_keyTable[Keycode.MediaClose] = */ Noesis.Key.None,
                /*_keyTable[Keycode.MediaEject] = */ Noesis.Key.None,
                /*_keyTable[Keycode.MediaRecord] = */ Noesis.Key.None,
                /*_keyTable[Keycode.F1] = */ Noesis.Key.F1,
                /*_keyTable[Keycode.F2] = */ Noesis.Key.F2,
                /*_keyTable[Keycode.F3] = */ Noesis.Key.F3,
                /*_keyTable[Keycode.F4] = */ Noesis.Key.F4,
                /*_keyTable[Keycode.F5] = */ Noesis.Key.F5,
                /*_keyTable[Keycode.F6] = */ Noesis.Key.F6,
                /*_keyTable[Keycode.F7] = */ Noesis.Key.F7,
                /*_keyTable[Keycode.F8] = */ Noesis.Key.F8,
                /*_keyTable[Keycode.F9] = */ Noesis.Key.F9,
                /*_keyTable[Keycode.F10] = */ Noesis.Key.F10,
                /*_keyTable[Keycode.F11] = */ Noesis.Key.F11,
                /*_keyTable[Keycode.F12] = */ Noesis.Key.F12,
                /*_keyTable[Keycode.NumLock] = */ Noesis.Key.NumLock,
                /*_keyTable[Keycode.Numpad0] = */ Noesis.Key.NumPad0,
                /*_keyTable[Keycode.Numpad1] = */ Noesis.Key.NumPad1,
                /*_keyTable[Keycode.Numpad2] = */ Noesis.Key.NumPad2,
                /*_keyTable[Keycode.Numpad3] = */ Noesis.Key.NumPad3,
                /*_keyTable[Keycode.Numpad4] = */ Noesis.Key.NumPad4,
                /*_keyTable[Keycode.Numpad5] = */ Noesis.Key.NumPad5,
                /*_keyTable[Keycode.Numpad6] = */ Noesis.Key.NumPad6,
                /*_keyTable[Keycode.Numpad7] = */ Noesis.Key.NumPad7,
                /*_keyTable[Keycode.Numpad8] = */ Noesis.Key.NumPad8,
                /*_keyTable[Keycode.Numpad9] = */ Noesis.Key.NumPad9,
                /*_keyTable[Keycode.NumpadDivide] = */ Noesis.Key.Divide,
                /*_keyTable[Keycode.NumpadMultiply] = */ Noesis.Key.Multiply,
                /*_keyTable[Keycode.NumpadSubtract] = */ Noesis.Key.Subtract,
                /*_keyTable[Keycode.NumpadAdd] = */ Noesis.Key.Add,
                /*_keyTable[Keycode.NumpadDot] = */ Noesis.Key.Decimal,
                /*_keyTable[Keycode.NumpadComma] = */ Noesis.Key.None,
                /*_keyTable[Keycode.NumpadEnter] = */ Noesis.Key.Enter,
                /*_keyTable[Keycode.NumpadEquals] = */ Noesis.Key.None,
                /*_keyTable[Keycode.NumpadLeftParen] = */ Noesis.Key.None,
                /*_keyTable[Keycode.NumpadRightParen] = */ Noesis.Key.None,
                /*_keyTable[Keycode.VolumeMute] = */ Noesis.Key.None,
                /*_keyTable[Keycode.Info] = */ Noesis.Key.None,
                /*_keyTable[Keycode.ChannelUp] = */ Noesis.Key.None,
                /*_keyTable[Keycode.ChannelDown] = */ Noesis.Key.None,
                /*_keyTable[Keycode.ZoomIn] = */ Noesis.Key.None,
                /*_keyTable[Keycode.ZoomOut] = */ Noesis.Key.None,
                /*_keyTable[Keycode.Tv] = */ Noesis.Key.None,
                /*_keyTable[Keycode.Window] = */ Noesis.Key.None,
                /*_keyTable[Keycode.Guide] = */ Noesis.Key.None,
                /*_keyTable[Keycode.Dvr] = */ Noesis.Key.None,
                /*_keyTable[Keycode.Bookmark] = */ Noesis.Key.None,
                /*_keyTable[Keycode.Captions] = */ Noesis.Key.None,
                /*_keyTable[Keycode.Settings] = */ Noesis.Key.None,
                /*_keyTable[Keycode.TvPower] = */ Noesis.Key.None,
                /*_keyTable[Keycode.TvInput] = */ Noesis.Key.None,
                /*_keyTable[Keycode.StbPower] = */ Noesis.Key.None,
                /*_keyTable[Keycode.StbInput] = */ Noesis.Key.None,
                /*_keyTable[Keycode.AvrPower] = */ Noesis.Key.None,
                /*_keyTable[Keycode.AvrInput] = */ Noesis.Key.None,
                /*_keyTable[Keycode.ProgRed] = */ Noesis.Key.None,
                /*_keyTable[Keycode.ProgGreen] = */ Noesis.Key.None,
                /*_keyTable[Keycode.ProgYellow] = */ Noesis.Key.None,
                /*_keyTable[Keycode.ProgBlue] = */ Noesis.Key.None,
                /*_keyTable[Keycode.AppSwitch] = */ Noesis.Key.None,
                /*_keyTable[Keycode.Button1] = */ Noesis.Key.None,
                /*_keyTable[Keycode.Button2] = */ Noesis.Key.None,
                /*_keyTable[Keycode.Button3] = */ Noesis.Key.None,
                /*_keyTable[Keycode.Button4] = */ Noesis.Key.None,
                /*_keyTable[Keycode.Button5] = */ Noesis.Key.None,
                /*_keyTable[Keycode.Button6] = */ Noesis.Key.None,
                /*_keyTable[Keycode.Button7] = */ Noesis.Key.None,
                /*_keyTable[Keycode.Button8] = */ Noesis.Key.None,
                /*_keyTable[Keycode.Button9] = */ Noesis.Key.None,
                /*_keyTable[Keycode.Button10] = */ Noesis.Key.None,
                /*_keyTable[Keycode.Button11] = */ Noesis.Key.None,
                /*_keyTable[Keycode.Button12] = */ Noesis.Key.None,
                /*_keyTable[Keycode.Button13] = */ Noesis.Key.None,
                /*_keyTable[Keycode.Button14] = */ Noesis.Key.None,
                /*_keyTable[Keycode.Button15] = */ Noesis.Key.None,
                /*_keyTable[Keycode.Button16] = */ Noesis.Key.None,
                /*_keyTable[Keycode.LanguageSwitch] = */ Noesis.Key.None,
                /*_keyTable[Keycode.MannerMode] = */ Noesis.Key.None,
                /*_keyTable[Keycode.ThreeDMode] = */ Noesis.Key.None,
                /*_keyTable[Keycode.Contacts] = */ Noesis.Key.None,
                /*_keyTable[Keycode.Calendar] = */ Noesis.Key.None,
                /*_keyTable[Keycode.Music] = */ Noesis.Key.None,
                /*_keyTable[Keycode.Calculator] = */ Noesis.Key.None,
                /*_keyTable[Keycode.ZenkakuHankaku] = */ Noesis.Key.None,
                /*_keyTable[Keycode.Eisu] = */ Noesis.Key.None,
                /*_keyTable[Keycode.Muhenkan] = */ Noesis.Key.None,
                /*_keyTable[Keycode.Henkan] = */ Noesis.Key.None,
                /*_keyTable[Keycode.KatakanaHiragana] = */ Noesis.Key.None,
                /*_keyTable[Keycode.Yen] = */ Noesis.Key.None,
                /*_keyTable[Keycode.Ro] = */ Noesis.Key.None,
                /*_keyTable[Keycode.Kana] = */ Noesis.Key.None,
                /*_keyTable[Keycode.Assist] = */ Noesis.Key.None,
                /*_keyTable[Keycode.BrightnessDown] = */ Noesis.Key.None,
                /*_keyTable[Keycode.BrightnessUp] = */ Noesis.Key.None,
                /*_keyTable[Keycode.MediaAudioTrack] = */ Noesis.Key.None,
                /*_keyTable[Keycode.Sleep] = */ Noesis.Key.None,
                /*_keyTable[Keycode.Wakeup] = */ Noesis.Key.None,
                /*_keyTable[Keycode.Pairing] = */ Noesis.Key.None,
                /*_keyTable[Keycode.MediaTopMenu] = */ Noesis.Key.None,
                /*_keyTable[Keycode.K11] = */ Noesis.Key.None,
                /*_keyTable[Keycode.K12] = */ Noesis.Key.None,
                /*_keyTable[Keycode.LastChannel] = */ Noesis.Key.None,
                /*_keyTable[Keycode.TvDataService] = */ Noesis.Key.None,
                /*_keyTable[Keycode.VoiceAssist] = */ Noesis.Key.None,
                /*_keyTable[Keycode.TvRadioService] = */ Noesis.Key.None,
                /*_keyTable[Keycode.TvTeletext] = */ Noesis.Key.None,
                /*_keyTable[Keycode.TvNumberEntry] = */ Noesis.Key.None,
                /*_keyTable[Keycode.TvTerrestrialAnalog] = */ Noesis.Key.None,
                /*_keyTable[Keycode.TvTerrestrialDigital] = */ Noesis.Key.None,
                /*_keyTable[Keycode.TvSatellite] = */ Noesis.Key.None,
                /*_keyTable[Keycode.TvSatelliteBs] = */ Noesis.Key.None,
                /*_keyTable[Keycode.TvSatelliteCs] = */ Noesis.Key.None,
                /*_keyTable[Keycode.TvSatelliteService] = */ Noesis.Key.None,
                /*_keyTable[Keycode.TvNetwork] = */ Noesis.Key.None,
                /*_keyTable[Keycode.TvAntennaCable] = */ Noesis.Key.None,
                /*_keyTable[Keycode.TvInputHdmi1] = */ Noesis.Key.None,
                /*_keyTable[Keycode.TvInputHdmi2] = */ Noesis.Key.None,
                /*_keyTable[Keycode.TvInputHdmi3] = */ Noesis.Key.None,
                /*_keyTable[Keycode.TvInputHdmi4] = */ Noesis.Key.None,
                /*_keyTable[Keycode.TvInputComposite1] = */ Noesis.Key.None,
                /*_keyTable[Keycode.TvInputComposite2] = */ Noesis.Key.None,
                /*_keyTable[Keycode.TvInputComponent1] = */ Noesis.Key.None,
                /*_keyTable[Keycode.TvInputComponent2] = */ Noesis.Key.None,
                /*_keyTable[Keycode.TvInputVga1] = */ Noesis.Key.None,
                /*_keyTable[Keycode.TvAudioDescription] = */ Noesis.Key.None,
                /*_keyTable[Keycode.TvAudioDescriptionMixUp] = */ Noesis.Key.None,
                /*_keyTable[Keycode.TvAudioDescriptionMixDown] = */ Noesis.Key.None,
                /*_keyTable[Keycode.TvZoomMode] = */ Noesis.Key.None,
                /*_keyTable[Keycode.TvContentsMenu] = */ Noesis.Key.None,
                /*_keyTable[Keycode.TvMediaContextMenu] = */ Noesis.Key.None,
                /*_keyTable[Keycode.TvTimerProgramming] = */ Noesis.Key.None,
                /*_keyTable[Keycode.Help] = */ Noesis.Key.None,
                /*_keyTable[Keycode.NavigatePrevious] = */ Noesis.Key.None,
                /*_keyTable[Keycode.NavigateNext] = */ Noesis.Key.None,
                /*_keyTable[Keycode.NavigateIn] = */ Noesis.Key.None,
                /*_keyTable[Keycode.NavigateOut] = */ Noesis.Key.None,
                /*_keyTable[Keycode.StemPrimary] = */ Noesis.Key.None,
                /*_keyTable[Keycode.Stem1] = */ Noesis.Key.None,
                /*_keyTable[Keycode.Stem2] = */ Noesis.Key.None,
                /*_keyTable[Keycode.Stem3] = */ Noesis.Key.None,
                /*_keyTable[Keycode.DpadUpLeft] = */ Noesis.Key.None,
                /*_keyTable[Keycode.DpadDownLeft] = */ Noesis.Key.None,
                /*_keyTable[Keycode.DpadUpRight] = */ Noesis.Key.None,
                /*_keyTable[Keycode.DpadDownRight] = */ Noesis.Key.None,
                /*_keyTable[Keycode.MediaSkipForward] = */ Noesis.Key.None,
                /*_keyTable[Keycode.MediaSkipBackward] = */ Noesis.Key.None,
                /*_keyTable[Keycode.MediaStepForward] = */ Noesis.Key.None,
                /*_keyTable[Keycode.MediaStepBackward] = */ Noesis.Key.None,
                /*_keyTable[Keycode.SoftSleep] = */ Noesis.Key.None,
                /*_keyTable[Keycode.Cut] = */ Noesis.Key.None,
                /*_keyTable[Keycode.Copy] = */ Noesis.Key.None,
                /*_keyTable[Keycode.Paste] = */ Noesis.Key.None,
                /*_keyTable[Keycode.SystemNavigationUp] = */ Noesis.Key.None,
                /*_keyTable[Keycode.SystemNavigationDown] = */ Noesis.Key.None,
                /*_keyTable[Keycode.SystemNavigationLeft] = */ Noesis.Key.None,
                /*_keyTable[Keycode.SystemNavigationRight] = */ Noesis.Key.None
            };
        }

        private View View { get; set; }

        private InputMethodManager InputManager
        {
            get { return (InputMethodManager)View.Context.GetSystemService(Context.InputMethodService); }
        }

        private Noesis.Key[] _keyTable;
    }
}
