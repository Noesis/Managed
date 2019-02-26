using System;

namespace Noesis
{

    public partial class FrameworkElement
    {
        public object FindResource(object key)
        {
            if (key is string)
            {
                return FindStringResource(key as string);
            }

            if (key is Type)
            {
                return FindTypeResource(key as Type);
            }

            throw new Exception("Only String or Type resource keys supported.");
        }

        public object TryFindResource(object key)
        {
            if (key is string)
            {
                return TryFindStringResource(key as string);
            }

            if (key is Type)
            {
                return TryFindTypeResource(key as Type);
            }

            throw new Exception("Only String or Type resource keys supported.");
        }

        protected virtual Size MeasureOverride(Size availableSize)
        {
            // Negative desired size is not allowed but we use it here to indicate we have to call
            // base class implementation in native code
            return Size.Empty;
        }

        protected virtual Size ArrangeOverride(Size finalSize)
        {
            // Negative render size is not allowed but we use it here to indicate we have to call
            // base class implementation in native code
            return Size.Empty;
        }

        protected virtual bool ConnectEvent(object source, string eventName, string handlerName)
        {
            return false;
        }

        #region FindResource implementation

        private object FindStringResource(string key)
        {
            IntPtr cPtr = NoesisGUI_PINVOKE.FrameworkElement_FindStringResource(swigCPtr, key);
            return Noesis.Extend.GetProxy(cPtr, false);
        }

        private object FindTypeResource(Type key)
        {
            IntPtr nativeType = Noesis.Extend.GetNativeType(key);
            IntPtr cPtr = NoesisGUI_PINVOKE.FrameworkElement_FindTypeResource(swigCPtr, nativeType);
            return Noesis.Extend.GetProxy(cPtr, false);
        }
        private object TryFindStringResource(string key)
        {
            IntPtr cPtr = NoesisGUI_PINVOKE.FrameworkElement_TryFindStringResource(swigCPtr, key);
            return Noesis.Extend.GetProxy(cPtr, false);
        }

        private object TryFindTypeResource(Type key)
        {
            IntPtr nativeType = Noesis.Extend.GetNativeType(key);
            IntPtr cPtr = NoesisGUI_PINVOKE.FrameworkElement_TryFindTypeResource(swigCPtr, nativeType);
            return Noesis.Extend.GetProxy(cPtr, false);
        }

        #endregion

        #region Extend overrides implementation

        internal Size CallMeasureOverride(Size availableSize)
        {
            return MeasureOverride(availableSize);
        }

        internal Size CallArrangeOverride(Size finalSize)
        {
            return ArrangeOverride(finalSize);
        }

        internal bool CallConnectEvent(object source, string eventName, string handlerName)
        {
            return ConnectEvent(source, eventName, handlerName);
        }

        #endregion
    }

}