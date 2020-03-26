using System;
using System.Runtime.InteropServices;

namespace Noesis
{

    public partial class FrameworkElement
    {
        public object FindResource(object key)
        {
            object resource = TryFindResource(key);
            if (resource != null)
            {
                return resource;
            }

            throw new InvalidOperationException("Resource not found '" + key.ToString() +  "'");
        }

        public object TryFindResource(object key)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }

            if (key is string)
            {
                return FindResourceHelper((string)key);
            }

            if (key is Type)
            {
                return FindResourceHelper(((Type)key).FullName);
            }

            throw new Exception("Only String or Type resource keys supported.");
        }

        protected virtual Size MeasureOverride(Size availableSize)
        {
            Size desiredSize = new Size(0.0f, 0.0f);
            if (_measureBaseCallback != null)
            {
                _measureBaseCallback(swigCPtr, ref availableSize, ref desiredSize);
            }
            return desiredSize;
        }

        protected virtual Size ArrangeOverride(Size finalSize)
        {
            Size renderSize = new Size(0.0f, 0.0f);
            if (_measureBaseCallback != null)
            {
                _measureBaseCallback(swigCPtr, ref finalSize, ref renderSize);
            }
            return renderSize;
        }

        protected virtual bool ConnectEvent(object source, string eventName, string handlerName)
        {
            return false;
        }

        #region FindResource implementation

        private object FindResourceHelper(string key)
        {
            IntPtr cPtr = FrameworkElement_FindResourceHelper(swigCPtr, key);
            return Noesis.Extend.GetProxy(cPtr, false);
        }

        [DllImport(Library.Name)]
        private static extern IntPtr FrameworkElement_FindResourceHelper(HandleRef element,
            string key);

        #endregion

        #region Extend overrides implementation

        internal delegate void MeasureBaseCallback(HandleRef cPtr, ref Size availableSize,
            ref Size desiredSize);
        MeasureBaseCallback _measureBaseCallback = null;

        internal Size CallMeasureOverride(Size availableSize, MeasureBaseCallback callback)
        {
            _measureBaseCallback = callback;
            Size desiredSize = MeasureOverride(availableSize);
            _measureBaseCallback = null;

            return desiredSize;
        }

        internal Size CallArrangeOverride(Size finalSize, MeasureBaseCallback callback)
        {
            _measureBaseCallback = callback;
            Size renderSize = ArrangeOverride(finalSize);
            _measureBaseCallback = null;

            return renderSize;
        }

        internal bool CallConnectEvent(object source, string eventName, string handlerName)
        {
            return ConnectEvent(source, eventName, handlerName);
        }

        #endregion
    }

}