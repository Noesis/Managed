using System.Runtime.InteropServices;
using NoesisApp;
using System;
using System.Text;
using Noesis;
using System.Collections.Generic;

namespace NoesisApp
{
    public class FbdevDisplay : Display
    {
        public FbdevDisplay()
        {
            _close = false;
            _display = IntPtr.Zero;

            _width = (int)Fbdev.GetWidth();
            _height = (int)Fbdev.GetHeight();

            Window w = new Window();
            w.width = (ushort)_width;
            w.height = (ushort)_height;
            _window = System.Runtime.InteropServices.Marshal.AllocHGlobal(2 * sizeof(ushort));
            System.Runtime.InteropServices.Marshal.StructureToPtr(w, _window, false);

            LibEvdev.KeyDown += OnKeyDown;
            LibEvdev.KeyUp += OnKeyUp;
            LibEvdev.Char += OnChar;

            LibEvdev.TouchDown += OnTouchDown;
            LibEvdev.TouchUp += OnTouchUp;
            LibEvdev.TouchMove += OnTouchMove;
        }

        ~FbdevDisplay()
        {
            System.Runtime.InteropServices.Marshal.FreeHGlobal(_window);
        }

        #region Display overrides
        public override IntPtr NativeHandle { get { return _display; } }
        public override IntPtr NativeWindow { get { return _window; } }

        public override int ClientWidth { get { return _width; } }
        public override int ClientHeight { get { return _height; } }

        public override void Close()
        {
            _close = true;
        }

        public override void EnterMessageLoop(bool runInBackground)
        {
            bool running = true;
            SizeChanged?.Invoke(this, _width, _height);
            Activated?.Invoke(this);
            while (running)
            {
                LibEvdev.HandleEvents();
                Render?.Invoke(this);

                if (_close)
                {
                    bool cancel = false;
                    Closing?.Invoke(this, ref cancel);
                    if (!cancel)
                    {
                        Closed?.Invoke(this);
                        running = false;
                    }
                    _close = false;
                }
            }
        }
        #endregion

        #region Types
        [StructLayout(LayoutKind.Sequential)]
        public struct Window
        {
            public ushort width;
            public ushort height;
        }
        #endregion

        #region Input events
        private void OnKeyDown(Key key)
        {
            KeyDown?.Invoke(this, key);
        }
        
        private void OnKeyUp(Key key)
        {
            KeyUp?.Invoke(this, key);
        }

        private void OnChar(uint c)
        {
            Char?.Invoke(this, c);
        }

        private void OnTouchDown(float x, float y, ulong id)
        {
            TouchDown?.Invoke(this, (int)(x * _width), (int)(y * _height), id);
        }

        private void OnTouchUp(float x, float y, ulong id)
        {
            TouchUp?.Invoke(this, (int)(x * _width), (int)(y * _height), id);
        }

        private void OnTouchMove(float x, float y, ulong id)
        {
            TouchMove?.Invoke(this, (int)(x * _width), (int)(y * _height), id);
        }
        #endregion

        #region Private members
        private IntPtr _display;
        private IntPtr _window;

        private int _width;
        private int _height;
        private bool _close;
        #endregion
    }
}
