using System;
using System.Runtime.InteropServices;
using Android.Views;
using Android.Runtime;
using Android.Graphics;

namespace NoesisApp
{
    public class AndroidView : SurfaceView, ISurfaceHolderCallback
    {
        public AndroidView(AndroidActivity activity) : base(activity)
        {
            Holder.AddCallback(this);
        }

        public IntPtr NativeHandle { get { return _nativeHandle; } }

        #region SurfaceView overrides
        protected override void OnSizeChanged(int w, int h, int oldw, int oldh)
        {
            AndroidActivity.Current.EventQueue.Enqueue(new WindowSizeChangedActivityEvent());
            base.OnSizeChanged(w, h, oldw, oldh);
        }

        public override bool OnTouchEvent(MotionEvent e)
        {
            AndroidActivity.Current.EventQueue.Enqueue(new TouchActivityEvent(e));
            return true;
        }
        #endregion

        #region ISurfaceHolderCallback implementation
        void ISurfaceHolderCallback.SurfaceCreated(ISurfaceHolder holder)
        {
            // Get native window handler used to initialize RenderContext
            _nativeHandle = ANativeWindow_fromSurface(JNIEnv.Handle, holder.Surface.Handle);

            AndroidActivity.Current.EventQueue.Enqueue(new WindowCreatedActivityEvent());
        }

        void ISurfaceHolderCallback.SurfaceChanged(ISurfaceHolder holder,
            [GeneratedEnum] Format format, int width, int height)
        {
        }

        void ISurfaceHolderCallback.SurfaceDestroyed(ISurfaceHolder holder)
        {
            AndroidActivity.Current.EventQueue.Enqueue(new WindowDestroyedActivityEvent());

            // Wait until AndroidDisplay has processed the window destroyed event
            while (!AndroidActivity.Current.EventQueue.IsEmpty)
            {
            }

            // Release native window handle
            if (_nativeHandle != IntPtr.Zero)
            {
                ANativeWindow_release(_nativeHandle);
                _nativeHandle = IntPtr.Zero;
            }
        }
        #endregion

        #region Private members
        private IntPtr _nativeHandle = IntPtr.Zero;
        #endregion

        #region Imports

        [DllImport("android")]
        private static extern IntPtr ANativeWindow_fromSurface(IntPtr jni, IntPtr surface);

        [DllImport("android")]
        private static extern void ANativeWindow_release(IntPtr surface);
        #endregion
    }
}
