using System;
using System.Runtime.InteropServices;

namespace Noesis
{

    public partial class FrameworkElement
    {
        protected override int VisualChildrenCount
        {
            get { return (int)GetVisualChildrenCountHelper(); }
        }

        protected override Visual GetVisualChild(int index)
        {
            return GetVisualChildHelper((uint)index);
        }

        public void SetResourceReference(DependencyProperty dp, object key)
        {
            string validKey = ResourceDictionary.GetValidKey(key);
            SetResourceReferenceHelper(dp, validKey);
        }

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
            string validKey = ResourceDictionary.GetValidKey(key);
            return FindResourceHelper(validKey);
        }

        protected virtual Size MeasureOverride(Size availableSize)
        {
            Size desiredSize = Size.Empty;
            MeasureOverrideHelper(availableSize, ref desiredSize);
            return desiredSize;
        }
        internal Size InternalMeasureOverride(Size availableSize)
        {
            return MeasureOverride(availableSize);
        }

        protected virtual Size ArrangeOverride(Size finalSize)
        {
            Size renderSize = Size.Empty;
            ArrangeOverrideHelper(finalSize, ref renderSize);
            return renderSize;
        }
        internal Size InternalArrangeOverride(Size finalSize)
        {
            return ArrangeOverride(finalSize);
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
    }

}