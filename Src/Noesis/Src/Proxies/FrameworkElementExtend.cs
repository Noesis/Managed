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