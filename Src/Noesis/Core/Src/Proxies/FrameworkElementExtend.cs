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
            _callBaseMeasure = true;
            return new Size(0f, 0f);
        }

        protected virtual Size ArrangeOverride(Size finalSize)
        {
            _callBaseArrange = true;
            return new Size(0f, 0f);
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

        private bool _callBaseMeasure;
        internal Size CallMeasureOverride(Size availableSize, out bool callBase)
        {
            _callBaseMeasure = false;
            Size desiredSize = MeasureOverride(availableSize);
            callBase = _callBaseMeasure;
            return desiredSize;
        }

        private bool _callBaseArrange;
        internal Size CallArrangeOverride(Size finalSize, out bool callBase)
        {
            _callBaseArrange = false;
            Size renderSize = ArrangeOverride(finalSize);
            callBase = _callBaseArrange;
            return renderSize;
        }

        internal bool CallConnectEvent(object source, string eventName, string handlerName)
        {
            return ConnectEvent(source, eventName, handlerName);
        }

        #endregion
    }

}