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

        internal protected virtual Size MeasureOverride(Size availableSize)
        {
            Size desiredSize = new Size(0.0f, 0.0f);
            MeasureBase?.Invoke(swigCPtr, ref availableSize, ref desiredSize);
            return desiredSize;
        }

        internal protected virtual Size ArrangeOverride(Size finalSize)
        {
            Size renderSize = new Size(0.0f, 0.0f);
            ArrangeBase?.Invoke(swigCPtr, ref finalSize, ref renderSize);
            return renderSize;
        }

        internal protected virtual bool ConnectEvent(object source, string eventName, string handlerName)
        {
            return false;
        }

        internal protected virtual void ConnectField(object source, string fieldName)
        {
        }

        public virtual void OnApplyTemplate()
        {
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

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void LayoutBaseCallback(HandleRef cPtr, ref Size size, ref Size outSize);
        internal LayoutBaseCallback MeasureBase = null;
        internal LayoutBaseCallback ArrangeBase = null;

        #endregion
    }

}