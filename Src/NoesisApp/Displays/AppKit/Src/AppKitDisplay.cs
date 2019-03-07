using System;
using AppKit;
using Foundation;
using CoreGraphics;
using System.Drawing;
using Noesis;

namespace NoesisApp
{
    public class AppKitDisplay : Display
    {
        public bool Closed { get; set; }

        private Window _window;

        public AppKitDisplay()
        {
            NSApplication.Init();

            CGRect screenFrame = NSScreen.MainScreen.Frame;
            RectangleF frame = new RectangleF((float)screenFrame.X, (float)screenFrame.Y,
                (float)screenFrame.Width * 0.74f, (float)screenFrame.Height * 0.74f);
            NSWindowStyle style = NSWindowStyle.Titled | NSWindowStyle.Closable |
                NSWindowStyle.Miniaturizable | NSWindowStyle.Resizable;

            _window = new Window(frame, style, NSBackingStore.Buffered, false);
            _window.AppKitDisplay = this;
            _window.ReleasedWhenClosed = false;
            _window.BackgroundColor = NSColor.Black;
            _window.IsOpaque = true;
            _window.AcceptsMouseMovedEvents = true;
            //_window.RegisterForDraggedTypes(new NSArray())
            _window.Delegate = new WindowDelegate(this, _window);

            FillKeyTable();
        }

        public override IntPtr NativeHandle
        {
            get { return IntPtr.Zero; }
        }

        public override IntPtr NativeWindow
        {
            get { return _window.ContentView.Handle; }
        }

        public override int ClientWidth
        {
            get { return (int)(_window.ContentView.Bounds.Size.Width); }
        }

        public override int ClientHeight
        {
            get { return (int)(_window.ContentView.Bounds.Size.Height); }
        }

        public override void Show()
        {
            SizeChanged?.Invoke(this, ClientWidth, ClientHeight);
            LocationChanged?.Invoke(this, 0, 0);
            _window.Center();
            _window.MakeKeyAndOrderFront(_window);
        }

        public override void EnterMessageLoop(bool runInBackground)
        {
            NSApplication app = NSApplication.SharedApplication;
            do
            {
                NSEvent evt = null;
                do
                {
                    bool wait = !_window.IsKeyWindow && !runInBackground;
                    NSDate expiration = wait ? NSDate.DistantFuture : NSDate.DistantPast;
                    evt = app.NextEvent(NSEventMask.AnyEvent, expiration, NSRunLoopMode.Default, true);

                    if (evt != null)
                    {
                        app.SendEvent(evt);
                    }
                } while (evt != null);

                if (!Closed)
                {
                    OnRender();
                }
            } while (!Closed);
        }

        public override void OpenSoftwareKeyboard(Noesis.UIElement focused)
        {
        }

        public override void CloseSoftwareKeyboard()
        {
        }

        public void OnActivated()
        {
            Activated?.Invoke(this);
        }

        public void OnDeactivated()
        {
            Deactivated?.Invoke(this);
        }

        public void OnRender()
        {
            Render?.Invoke(this);
        }

        public void OnSizeChanged(int Width, int Height)
        {
            SizeChanged?.Invoke(this, Width, Height);
        }

        public void OnLocationChanged(int X, int Y)
        {
            LocationChanged?.Invoke(this, X, Y);
        }

        public void OnMouseButtonDown(int x, int y, MouseButton button)
        {
            MouseButtonDown?.Invoke(this, x, y, button);
        }

        public void OnMouseMove(int x, int y)
        {
            MouseMove?.Invoke(this, x, y);
        }

        public void OnMouseButtonUp(int x, int y, MouseButton button)
        {
            MouseButtonUp?.Invoke(this, x, y, button);
        }

        public void OnMouseDoubleClick(int x, int y, MouseButton button)
        {
            MouseDoubleClick?.Invoke(this, x, y, button);
        }

        public void OnMouseWheel(int x, int y, int delta)
        {
            MouseWheel?.Invoke(this, x, y, delta);
        }

        public void OnChar(char c)
        {
            Char?.Invoke(this, c);
        }

        public void OnDeleteBackward()
        {
            KeyDown?.Invoke(this, Noesis.Key.Back);
            Char?.Invoke(this, '\b');
            KeyUp?.Invoke(this, Noesis.Key.Back);
        }

        public void OnKeyDown(UInt16 key)
        {
            KeyDown?.Invoke(this, _keyTable[key]);
        }

        public void OnKeyUp(UInt16 key)
        {
            KeyUp?.Invoke(this, _keyTable[key]);
        }

        #region Types
        public class WindowDelegate : NSWindowDelegate
        {
            private AppKitDisplay _display;
            private Window _window;

            public WindowDelegate(AppKitDisplay display, Window window)
            {
                _display = display;
                _window = window;
            }

            public override void WillClose(NSNotification notification)
            {
                _display.Closed = true;
            }

            public override void DidBecomeKey(NSNotification notification)
            {
                _display.OnActivated();
            }

            public override void DidResignKey(NSNotification notification)
            {
                _display.OnDeactivated();
            }

            public override void DidResize(NSNotification notification)
            {
                _display.OnSizeChanged((int)_window.Frame.Size.Width, (int)_window.Frame.Size.Height);
            }

            public override void DidMove(NSNotification notification)
            {
                _display.OnLocationChanged((int)_window.Frame.Location.X, (int)_window.Frame.Location.Y);
            }
        }

        public class Window : NSWindow
        {
            public AppKitDisplay AppKitDisplay { get; set; }

            private KeyModifier _modifiers;

            [System.Flags]
            private enum KeyModifier
            {
                None = 0,
                Shift = 1,
                Alt = 2,
                Control = 4
            }

            public Window(RectangleF rect, NSWindowStyle style, NSBackingStore backingStore, bool defer) :
                base(rect, style, backingStore, defer)
            {
            }

            private void UpdateModifiers(NSEvent evt)
            {
                KeyModifier modifiers = 0;

                if (evt.ModifierFlags.HasFlag(NSEventModifierMask.ShiftKeyMask)) modifiers |= KeyModifier.Shift;
                if (evt.ModifierFlags.HasFlag(NSEventModifierMask.AlternateKeyMask)) modifiers |= KeyModifier.Alt;
                if (evt.ModifierFlags.HasFlag(NSEventModifierMask.CommandKeyMask)) modifiers |= KeyModifier.Control;
                if (evt.ModifierFlags.HasFlag(NSEventModifierMask.ControlKeyMask)) modifiers |= KeyModifier.Control;

                KeyModifier delta = modifiers ^ _modifiers;

                if (delta.HasFlag(KeyModifier.Shift))
                {
                    if (modifiers.HasFlag(KeyModifier.Shift))
                    {
                        AppKitDisplay.OnKeyDown((UInt16)NSKey.Shift);
                    }
                    else
                    {
                        AppKitDisplay.OnKeyUp((UInt16)NSKey.Shift);
                    }
                }

                if (delta.HasFlag(KeyModifier.Alt))
                {
                    if (modifiers.HasFlag(KeyModifier.Alt))
                    {
                        AppKitDisplay.OnKeyDown((UInt16)NSKey.Option);
                    }
                    else
                    {
                        AppKitDisplay.OnKeyUp((UInt16)NSKey.Option);
                    }
                }

                if (delta.HasFlag(KeyModifier.Control))
                {
                    if (modifiers.HasFlag(KeyModifier.Control))
                    {
                        AppKitDisplay.OnKeyDown((UInt16)NSKey.Control);
                    }
                    else
                    {
                        AppKitDisplay.OnKeyUp((UInt16)NSKey.Control);
                    }
                }

                _modifiers = modifiers;
            }

            private PointF MousePos(NSEvent evt)
            {
                return new PointF((float)evt.LocationInWindow.X, (float)(ContentView.Bounds.Size.Height - evt.LocationInWindow.Y));
            }

            public override void MouseDown(NSEvent evt)
            {
                UpdateModifiers(evt);
                PointF position = MousePos(evt);

                if (evt.ClickCount == 2)
                {
                    AppKitDisplay.OnMouseDoubleClick((int)position.X, (int)position.Y, MouseButton.Left);
                }
                else
                {
                    AppKitDisplay.OnMouseButtonDown((int)position.X, (int)position.Y, MouseButton.Left);
                }
            }

            public override void MouseUp(NSEvent evt)
            {
                UpdateModifiers(evt);
                PointF position = MousePos(evt);

                AppKitDisplay.OnMouseButtonUp((int)position.X, (int)position.Y, MouseButton.Left);
            }

            public override void RightMouseDown(NSEvent evt)
            {
                UpdateModifiers(evt);
                PointF position = MousePos(evt);

                if (evt.ClickCount == 2)
                {
                    AppKitDisplay.OnMouseDoubleClick((int)position.X, (int)position.Y, MouseButton.Right);
                }
                else
                {
                    AppKitDisplay.OnMouseButtonDown((int)position.X, (int)position.Y, MouseButton.Right);
                }
            }

            public override void RightMouseUp(NSEvent evt)
            {
                UpdateModifiers(evt);
                PointF position = MousePos(evt);

                AppKitDisplay.OnMouseButtonUp((int)position.X, (int)position.Y, MouseButton.Right);
            }

            public override void OtherMouseDown(NSEvent evt)
            {
                UpdateModifiers(evt);
                PointF position = MousePos(evt);

                if (evt.ClickCount == 2)
                {
                    AppKitDisplay.OnMouseDoubleClick((int)position.X, (int)position.Y, MouseButton.Middle);
                }
                else
                {
                    AppKitDisplay.OnMouseButtonDown((int)position.X, (int)position.Y, MouseButton.Middle);
                }
            }

            public override void OtherMouseUp(NSEvent evt)
            {
                UpdateModifiers(evt);
                PointF position = MousePos(evt);

                AppKitDisplay.OnMouseButtonUp((int)position.X, (int)position.Y, MouseButton.Middle);
            }

            public override void MouseMoved(NSEvent evt)
            {
                UpdateModifiers(evt);
                PointF position = MousePos(evt);

                AppKitDisplay.OnMouseMove((int)position.X, (int)position.Y);
            }

            public override void MouseDragged(NSEvent evt)
            {
                UpdateModifiers(evt);
                PointF position = MousePos(evt);

                AppKitDisplay.OnMouseMove((int)position.X, (int)position.Y);
            }

            public override void RightMouseDragged(NSEvent evt)
            {
                UpdateModifiers(evt);
                PointF position = MousePos(evt);

                AppKitDisplay.OnMouseMove((int)position.X, (int)position.Y);
            }

            public override void OtherMouseDragged(NSEvent evt)
            {
                UpdateModifiers(evt);
                PointF position = MousePos(evt);

                AppKitDisplay.OnMouseMove((int)position.X, (int)position.Y);
            }

            public override void ScrollWheel(NSEvent evt)
            {
                UpdateModifiers(evt);
                PointF position = MousePos(evt);

                AppKitDisplay.OnMouseWheel((int)position.X, (int)position.Y, (int)evt.DeltaY);
            }

            public override void KeyDown(NSEvent evt)
            {
                UpdateModifiers(evt);
                AppKitDisplay.OnKeyDown(evt.KeyCode);
                //InterpretKeyEvents(new NSArray(evt));
            }

            public override void KeyUp(NSEvent evt)
            {
                UpdateModifiers(evt);
                AppKitDisplay.OnKeyUp(evt.KeyCode);
            }

            public void InsertText(string text)
            {
                foreach (char c in text)
                {
                    AppKitDisplay.OnChar(c);
                }
            }
        }
        #endregion

        #region Keys table
        private Key[] _keyTable;

        private void FillKeyTable()
        {
            _keyTable = new Key[256];

            _keyTable[(UInt16)NSKey.Q] = Key.Q;
            _keyTable[(UInt16)NSKey.W] = Key.W;
            _keyTable[(UInt16)NSKey.E] = Key.E;
            _keyTable[(UInt16)NSKey.R] = Key.R;
            _keyTable[(UInt16)NSKey.T] = Key.T;
            _keyTable[(UInt16)NSKey.Y] = Key.Y;
            _keyTable[(UInt16)NSKey.U] = Key.U;
            _keyTable[(UInt16)NSKey.I] = Key.I;
            _keyTable[(UInt16)NSKey.O] = Key.O;
            _keyTable[(UInt16)NSKey.P] = Key.P;
            _keyTable[(UInt16)NSKey.A] = Key.A;
            _keyTable[(UInt16)NSKey.S] = Key.S;
            _keyTable[(UInt16)NSKey.D] = Key.D;
            _keyTable[(UInt16)NSKey.F] = Key.F;
            _keyTable[(UInt16)NSKey.G] = Key.G;
            _keyTable[(UInt16)NSKey.H] = Key.H;
            _keyTable[(UInt16)NSKey.J] = Key.J;
            _keyTable[(UInt16)NSKey.K] = Key.K;
            _keyTable[(UInt16)NSKey.L] = Key.L;
            _keyTable[(UInt16)NSKey.Z] = Key.Z;
            _keyTable[(UInt16)NSKey.X] = Key.X;
            _keyTable[(UInt16)NSKey.C] = Key.C;
            _keyTable[(UInt16)NSKey.V] = Key.V;
            _keyTable[(UInt16)NSKey.B] = Key.B;
            _keyTable[(UInt16)NSKey.N] = Key.N;
            _keyTable[(UInt16)NSKey.M] = Key.M;
            _keyTable[(UInt16)NSKey.D1] = Key.D1;
            _keyTable[(UInt16)NSKey.D2] = Key.D2;
            _keyTable[(UInt16)NSKey.D3] = Key.D3;
            _keyTable[(UInt16)NSKey.D4] = Key.D4;
            _keyTable[(UInt16)NSKey.D5] = Key.D5;
            _keyTable[(UInt16)NSKey.D6] = Key.D6;
            _keyTable[(UInt16)NSKey.D7] = Key.D7;
            _keyTable[(UInt16)NSKey.D8] = Key.D8;
            _keyTable[(UInt16)NSKey.D9] = Key.D9;
            _keyTable[(UInt16)NSKey.D0] = Key.D0;

            _keyTable[(UInt16)NSKey.Keypad0] = Key.NumPad0;
            _keyTable[(UInt16)NSKey.Keypad1] = Key.NumPad1;
            _keyTable[(UInt16)NSKey.Keypad2] = Key.NumPad2;
            _keyTable[(UInt16)NSKey.Keypad3] = Key.NumPad3;
            _keyTable[(UInt16)NSKey.Keypad4] = Key.NumPad4;
            _keyTable[(UInt16)NSKey.Keypad5] = Key.NumPad5;
            _keyTable[(UInt16)NSKey.Keypad6] = Key.NumPad6;
            _keyTable[(UInt16)NSKey.Keypad7] = Key.NumPad7;
            _keyTable[(UInt16)NSKey.Keypad8] = Key.NumPad8;
            _keyTable[(UInt16)NSKey.Keypad9] = Key.NumPad9;

            _keyTable[(UInt16)NSKey.Return] = Key.Return;
            _keyTable[(UInt16)NSKey.Tab] = Key.Tab;
            _keyTable[(UInt16)NSKey.Space] = Key.Space;
            _keyTable[(UInt16)NSKey.Delete] = Key.Back;
            _keyTable[(UInt16)NSKey.ForwardDelete] = Key.Delete;
            _keyTable[(UInt16)NSKey.Escape] = Key.Escape;
            _keyTable[(UInt16)NSKey.Shift] = Key.LeftShift;
            _keyTable[(UInt16)NSKey.Control] = Key.LeftCtrl;
            _keyTable[(UInt16)NSKey.Option] = Key.LeftAlt;
            _keyTable[(UInt16)NSKey.Home] = Key.Home;
            _keyTable[(UInt16)NSKey.Help] = Key.Help;
            _keyTable[(UInt16)NSKey.End] = Key.End;
            _keyTable[(UInt16)NSKey.LeftArrow] = Key.Left;
            _keyTable[(UInt16)NSKey.RightArrow] = Key.Right;
            _keyTable[(UInt16)NSKey.DownArrow] = Key.Down;
            _keyTable[(UInt16)NSKey.UpArrow] = Key.Up;

            _keyTable[(UInt16)NSKey.F1] = Key.F1;
            _keyTable[(UInt16)NSKey.F2] = Key.F2;
            _keyTable[(UInt16)NSKey.F3] = Key.F3;
            _keyTable[(UInt16)NSKey.F4] = Key.F4;
            _keyTable[(UInt16)NSKey.F5] = Key.F5;
            _keyTable[(UInt16)NSKey.F6] = Key.F6;
            _keyTable[(UInt16)NSKey.F7] = Key.F7;
            _keyTable[(UInt16)NSKey.F8] = Key.F8;
            _keyTable[(UInt16)NSKey.F9] = Key.F9;
            _keyTable[(UInt16)NSKey.F10] = Key.F10;
            _keyTable[(UInt16)NSKey.F11] = Key.F11;
            _keyTable[(UInt16)NSKey.F12] = Key.F12;
            _keyTable[(UInt16)NSKey.F13] = Key.F13;
            _keyTable[(UInt16)NSKey.F14] = Key.F14;
            _keyTable[(UInt16)NSKey.F15] = Key.F15;
            _keyTable[(UInt16)NSKey.F16] = Key.F16;
            //_keyTable[(UInt16)NSKey.F17] = Key.F17;
            _keyTable[(UInt16)NSKey.F18] = Key.F18;
            _keyTable[(UInt16)NSKey.F19] = Key.F19;
            _keyTable[(UInt16)NSKey.F20] = Key.F20;
        }
        #endregion
    }
}
