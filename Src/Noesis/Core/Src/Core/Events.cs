using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Noesis
{
    #region Event Handlers
    public delegate void EventHandler(object sender, EventArgs args);

    public delegate void DependencyPropertyChangedEventHandler(object sender, DependencyPropertyChangedEventArgs args);

    public delegate void RoutedEventHandler(object sender, RoutedEventArgs args);

    public delegate void MouseEventHandler(object sender, MouseEventArgs args);
    public delegate void MouseButtonEventHandler(object sender, MouseButtonEventArgs args);
    public delegate void MouseWheelEventHandler(object sender, MouseWheelEventArgs args);
    public delegate void QueryCursorEventHandler(object sender, QueryCursorEventArgs args);
    public delegate void KeyboardFocusChangedEventHandler(object sender, KeyboardFocusChangedEventArgs args);
    public delegate void KeyEventHandler(object sender, KeyEventArgs args);
    public delegate void TextCompositionEventHandler(object sender, TextCompositionEventArgs args);
    public delegate void TouchEventHandler(object sender, TouchEventArgs args);
    public delegate void TappedEventHandler(object sender, TappedEventArgs args);
    public delegate void DoubleTappedEventHandler(object sender, DoubleTappedEventArgs args);
    public delegate void HoldingEventHandler(object sender, HoldingEventArgs args);
    public delegate void RightTappedEventHandler(object sender, RightTappedEventArgs args);
    public delegate void ManipulationStartingEventHandler(object sender, ManipulationStartingEventArgs args);
    public delegate void ManipulationStartedEventHandler(object sender, ManipulationStartedEventArgs args);
    public delegate void ManipulationDeltaEventHandler(object sender, ManipulationDeltaEventArgs args);
    public delegate void ManipulationInertiaStartingEventHandler(object sender, ManipulationInertiaStartingEventArgs args);
    public delegate void ManipulationCompletedEventHandler(object sender, ManipulationCompletedEventArgs args);
    public delegate void QueryContinueDragEventHandler(object sender, QueryContinueDragEventArgs args);
    public delegate void GiveFeedbackEventHandler(object sender, GiveFeedbackEventArgs args);
    public delegate void DragEventHandler(object sender, DragEventArgs args);

    public delegate void ScrollEventHandler(object sender, ScrollEventArgs args);
    public delegate void ScrollChangedEventHandler(object sender, ScrollChangedEventArgs args);

    public delegate void SelectionChangedEventHandler(object sender, SelectionChangedEventArgs args);

    public delegate void DragCompletedEventHandler(object sender, DragCompletedEventArgs args);
    public delegate void DragStartedEventHandler(object sender, DragStartedEventArgs args);
    public delegate void DragDeltaEventHandler(object sender, DragDeltaEventArgs args);

    public delegate void SizeChangedEventHandler(object sender, SizeChangedEventArgs args);
    public delegate void RequestBringIntoViewEventHandler(object sender, RequestBringIntoViewEventArgs args);
    public delegate void ContextMenuEventHandler(object sender, ContextMenuEventArgs args);
    public delegate void ToolTipEventHandler(object sender, ToolTipEventArgs args);

    public delegate void RequestNavigateEventHandler(object sender, RequestNavigateEventArgs args);

    public delegate void CanExecuteRoutedEventHandler(object sender, CanExecuteRoutedEventArgs args);
    public delegate void ExecutedRoutedEventHandler(object sender, ExecutedRoutedEventArgs args);

    public delegate void RoutedPropertyChangedEventHandler<T>(object sender, RoutedPropertyChangedEventArgs<T> e);
    #endregion

    #region RoutedPropertyChangedEventArgs<T>
    public class RoutedPropertyChangedEventArgs<T> : RoutedEventArgs
    {
        public T OldValue
        {
            get { return RoutedPropertyChangedEventArgsHelper.GetOldValue<T>(getCPtr(this)); }
        }

        public T NewValue
        {
            get { return RoutedPropertyChangedEventArgsHelper.GetNewValue<T>(getCPtr(this)); }
        }

        internal RoutedPropertyChangedEventArgs(IntPtr cPtr, bool ownMemory) : base(cPtr, false)
        {
        }

        internal static new void InvokeHandler(Delegate handler, IntPtr sender, IntPtr args)
        {
            RoutedPropertyChangedEventHandler<T> handler_ = (RoutedPropertyChangedEventHandler<T>)handler;
            if (handler_ != null)
            {
                handler_(Extend.GetProxy(sender, false), new RoutedPropertyChangedEventArgs<T>(args, false));
            }
        }
    }

    internal static class RoutedPropertyChangedEventArgsHelper
    {
        public static T GetOldValue<T>(HandleRef cPtr)
        {
            int type = Extend.GetNativePropertyType(typeof(T));
            IntPtr value = Noesis_RoutedPropertyChangedEventArgs_GetOldValue(cPtr, type);
            return (T)Extend.GetProxy(value, true);
        }

        public static T GetNewValue<T>(HandleRef cPtr)
        {
            int type = Extend.GetNativePropertyType(typeof(T));
            IntPtr value = Noesis_RoutedPropertyChangedEventArgs_GetNewValue(cPtr, type);
            return (T)Extend.GetProxy(value, true);
        }

        [DllImport(Library.Name)]
        private static extern IntPtr Noesis_RoutedPropertyChangedEventArgs_GetOldValue(HandleRef cPtr, int type);

        [DllImport(Library.Name)]
        private static extern IntPtr Noesis_RoutedPropertyChangedEventArgs_GetNewValue(HandleRef cPtr, int type);
    }
    #endregion

    public class InputDevice
    {
        public UIElement Target { get; private set; }

        internal InputDevice(UIElement target)
        {
            Target = target;
        }
    }

    public class TouchDevice : InputDevice
    {
        public ulong Id { get; private set; }

        public UIElement DirectlyOver { get { return Target; } }

        public CaptureMode CaptureMode { get { return CaptureMode.Element; } }

        public UIElement Captured
        {
            get
            {
                IntPtr cPtr = TouchDevice_GetCaptured(BaseComponent.getCPtr(Target), Id);
                return (UIElement)Extend.GetProxy(cPtr, false);
            }
        }

        internal TouchDevice(UIElement target, ulong id): base(target)
        {
            Id = id;
        }

        #region Imports
        [DllImport(Library.Name)]
        private static extern IntPtr TouchDevice_GetCaptured(HandleRef target, ulong id);
        #endregion
    }

    internal class EventHandlerStore
    {
        public UIElement Element { get => (UIElement)Extend.GetProxy(_element, false); }

        #region Routed Events
        public static void AddHandler(UIElement element, IntPtr routedEventPtr, Delegate handler)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            if (!EventManager.IsLegalHandler(routedEventPtr, handler))
            {
                RoutedEvent routedEvent = (RoutedEvent)Extend.GetProxy(routedEventPtr, false);
                throw new ArgumentException("Illegal Handler type for " + routedEvent.Name + " event");
            }

            EventHandlerStore events;
            IntPtr cPtr = BaseComponent.getCPtr(element).Handle;
            long ptr = cPtr.ToInt64();
            if (!_elements.TryGetValue(ptr, out events))
            {
                events = new EventHandlerStore(element);
                _elements.Add(ptr, events);
                element.Destroyed += OnElementDestroyed;
            }

            long routedEventKey = routedEventPtr.ToInt64();

            Delegate eventHandler;
            if (!events._binds.TryGetValue(routedEventKey, out eventHandler))
            {
                events._binds.Add(routedEventKey, null);
                Noesis_RoutedEvent_Bind(_raiseRoutedEvent, events._element, routedEventPtr);
            }

            eventHandler = Delegate.Combine(eventHandler, handler);
            events._binds[routedEventKey] = eventHandler;
        }

        public static void RemoveHandler(UIElement element, IntPtr routedEventPtr, Delegate handler)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            if (!EventManager.IsLegalHandler(routedEventPtr, handler))
            {
                RoutedEvent routedEvent = (RoutedEvent)Extend.GetProxy(routedEventPtr, false);
                throw new ArgumentException("Illegal Handler type for " + routedEvent.Name + " event");
            }

            EventHandlerStore events;
            IntPtr cPtr = BaseComponent.getCPtr(element).Handle;
            long ptr = cPtr.ToInt64();
            if (!_elements.TryGetValue(ptr, out events))
            {
                return;
            }

            long routedEventKey = routedEventPtr.ToInt64();

            Delegate eventHandler;
            if (!events._binds.TryGetValue(routedEventKey, out eventHandler))
            {
                return;
            }

            eventHandler = Delegate.Remove(eventHandler, handler);
            events._binds[routedEventKey] = eventHandler;

            if (eventHandler == null)
            {
                Noesis_RoutedEvent_Unbind(_raiseRoutedEvent, events._element, routedEventPtr);
                events._binds.Remove(routedEventKey);

                if (events._binds.Count == 0)
                {
                    element.Destroyed -= OnElementDestroyed;
                    _elements.Remove(ptr);
                }
            }
        }

        #region RoutedEvent callback
        internal delegate void RaiseRoutedEventCallback(IntPtr cPtrType, IntPtr cPtr,
            IntPtr routedEvent, IntPtr sender, IntPtr e);
        private static RaiseRoutedEventCallback _raiseRoutedEvent = RaiseRoutedEvent;

        [MonoPInvokeCallback(typeof(RaiseRoutedEventCallback))]
        private static void RaiseRoutedEvent(IntPtr cPtrType, IntPtr cPtr, IntPtr routedEvent,
            IntPtr sender, IntPtr e)
        {
            try
            {
                if (Extend.Initialized)
                {
                    EventHandlerStore events;
                    long ptr = cPtr.ToInt64();
                    if (!_elements.TryGetValue(ptr, out events))
                    {
                        return;
                    }

                    long routedEventKey = routedEvent.ToInt64();

                    // check if we are called to unbind the event
                    if (sender == IntPtr.Zero && e == IntPtr.Zero)
                    {
                        events._binds.Remove(routedEventKey);
                        return;
                    }

                    // invoke handler
                    Delegate eventHandler;
                    if (!events._binds.TryGetValue(routedEventKey, out eventHandler))
                    {
                        return;
                    }

                    EventManager.InvokeHandler(routedEvent, eventHandler, sender, e);
                }
            }
            catch (Exception exception)
            {
                Error.UnhandledException(exception);
            }
        }
        #endregion
        #endregion

        #region CLR Events
        public static void AddHandler(UIElement element, string eventId, Delegate handler)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            if (string.IsNullOrEmpty(eventId))
            {
                throw new ArgumentNullException("eventId");
            }
            if (handler == null)
            {
                throw new ArgumentNullException("handler");
            }
            if (!EventManager.IsLegalHandler(eventId, handler))
            {
                throw new ArgumentException("Illegal Handler type for " + eventId + " event");
            }

            EventHandlerStore events;
            IntPtr cPtr = BaseComponent.getCPtr(element).Handle;
            long ptr = cPtr.ToInt64();
            if (!_elements.TryGetValue(ptr, out events))
            {
                events = new EventHandlerStore(element);
                _elements.Add(ptr, events);
                element.Destroyed += OnElementDestroyed;
            }

            long eventKey = eventId.GetHashCode();

            Delegate eventHandler;
            if (!events._binds.TryGetValue(eventKey, out eventHandler))
            {
                events._binds.Add(eventKey, null);
                Noesis_Event_Bind(_raiseEvent, events._element, eventId);
            }

            eventHandler = Delegate.Combine(eventHandler, handler);
            events._binds[eventKey] = eventHandler;
        }

        public static void RemoveHandler(UIElement element, string eventId, Delegate handler)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            if (string.IsNullOrEmpty(eventId))
            {
                throw new ArgumentNullException("eventId");
            }
            if (handler == null)
            {
                throw new ArgumentNullException("handler");
            }
            if (!EventManager.IsLegalHandler(eventId, handler))
            {
                throw new ArgumentException("Illegal Handler type for " + eventId + " event");
            }

            EventHandlerStore events;
            IntPtr cPtr = BaseComponent.getCPtr(element).Handle;
            long ptr = cPtr.ToInt64();
            if (!_elements.TryGetValue(ptr, out events))
            {
                return;
            }

            long eventKey = eventId.GetHashCode();

            Delegate eventHandler;
            if (!events._binds.TryGetValue(eventKey, out eventHandler))
            {
                return;
            }

            eventHandler = Delegate.Remove(eventHandler, handler);

            if (eventHandler == null)
            {
                Noesis_Event_Unbind(_raiseEvent, events._element, eventId);
                events._binds.Remove(eventKey);

                if (events._binds.Count == 0)
                {
                    element.Destroyed -= OnElementDestroyed;
                    _elements.Remove(ptr);
                }
            }
        }

        #region CLR Event callback
        internal delegate void RaiseEventCallback(IntPtr cPtrType, IntPtr cPtr,
            string eventId, IntPtr sender, IntPtr e);
        private static RaiseEventCallback _raiseEvent = RaiseEvent;

        [MonoPInvokeCallback(typeof(RaiseEventCallback))]
        private static void RaiseEvent(IntPtr cPtrType, IntPtr cPtr, string eventId,
            IntPtr sender, IntPtr e)
        {
            try
            {
                if (Extend.Initialized)
                {
                    EventHandlerStore events;
                    long ptr = cPtr.ToInt64();
                    if (!_elements.TryGetValue(ptr, out events))
                    {
                        return;
                    }

                    long eventKey = eventId.GetHashCode();

                    // check if we are called to unbind the event
                    if (sender == IntPtr.Zero && e == IntPtr.Zero)
                    {
                        events._binds.Remove(eventKey);
                        return;
                    }

                    // invoke handler
                    Delegate eventHandler;
                    if (!events._binds.TryGetValue(eventKey, out eventHandler))
                    {
                        return;
                    }

                    EventManager.InvokeHandler(eventId, eventHandler, sender, e);
                }
            }
            catch (Exception exception)
            {
                Error.UnhandledException(exception);
            }
        }
        #endregion
        #endregion

        #region Private members
        private EventHandlerStore(UIElement element)
        {
            _element = BaseComponent.getCPtr(element).Handle;
        }

        internal static void Clear()
        {
            foreach (var entry in _elements)
            {
                entry.Value._binds.Clear();
            }
            _elements.Clear();

            DependencyObject._Destroyed.Clear();
            Clock._Completed.Clear();
            CommandBinding._PreviewCanExecute.Clear();
            CommandBinding._PreviewExecuted.Clear();
            CommandBinding._CanExecute.Clear();
            CommandBinding._Executed.Clear();
            ItemContainerGenerator._ItemsChanged.Clear();
            ItemContainerGenerator._StatusChanged.Clear();
            Popup._Closed.Clear();
            Popup._Opened.Clear();
            Timeline._Completed.Clear();
            VisualStateGroup._CurrentStateChanging.Clear();
            VisualStateGroup._CurrentStateChanged.Clear();
            View._Rendering.Clear();
        }

        private static void OnElementDestroyed(IntPtr d)
        {
            EventHandlerStore events;
            long ptr = d.ToInt64();
            if (_elements.TryGetValue(ptr, out events))
            {
                events._binds.Clear();
                _elements.Remove(ptr);
            }
        }

        private static Dictionary<long, EventHandlerStore> _elements = new Dictionary<long, EventHandlerStore>();

        private IntPtr _element;
        private Dictionary<long, Delegate> _binds = new Dictionary<long, Delegate>();
        #endregion

        #region Imports
        [DllImport(Library.Name)]
        private static extern void Noesis_RoutedEvent_Bind(RaiseRoutedEventCallback callback,
            IntPtr element, IntPtr routedEvent);

        [DllImport(Library.Name)]
        private static extern void Noesis_RoutedEvent_Unbind(RaiseRoutedEventCallback callback,
            IntPtr element, IntPtr routedEvent);

        [DllImport(Library.Name)]
        private static extern void Noesis_Event_Bind(RaiseEventCallback callback,
            IntPtr element, [MarshalAs(UnmanagedType.LPWStr)]string eventId);

        [DllImport(Library.Name)]
        private static extern void Noesis_Event_Unbind(RaiseEventCallback callback,
            IntPtr element, [MarshalAs(UnmanagedType.LPWStr)]string eventId);
        #endregion
    }

    public static class EventManager
    {
        /// <summary>Registers a new routed event. </summary>
        /// <returns>The identifier for the newly registered routed event. This identifier object can now be stored as a static field in a class and then used as a parameter for methods that attach handlers to the event. The routed event identifier is also used for other event system APIs.</returns>
        /// <param name="name">The name of the routed event. The name must be unique within the owner type and cannot be null or an empty string.</param>
        /// <param name="routingStrategy">The routing strategy of the event as a value of the enumeration.</param>
        /// <param name="handlerType">The type of the event handler. This must be a delegate type and cannot be null.</param>
        /// <param name="ownerType">The owner class type of the routed event. This cannot be null.</param>
        public static RoutedEvent RegisterRoutedEvent(string name, RoutingStrategy routingStrategy, Type handlerType, Type ownerType)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (handlerType == null)
            {
                throw new ArgumentNullException("handlerType");
            }
            if (ownerType == null)
            {
                throw new ArgumentNullException("ownerType");
            }

            return AddRoutedEvent(name, routingStrategy, handlerType, ownerType);
        }

        /// <summary>Registers a class handler for a particular routed event. </summary>
        /// <param name="classType">The type of the class that is declaring class handling.</param>
        /// <param name="routedEvent">The routed event identifier of the event to handle.</param>
        /// <param name="handler">A reference to the class handler implementation.</param>
        public static void RegisterClassHandler(Type classType, RoutedEvent routedEvent, Delegate handler)
        {
            EventManager.RegisterClassHandler(classType, routedEvent, handler, false);
        }

        /// <summary> Registers a class handler for a particular routed event, with the option to handle events where event data is already marked handled.</summary>
        /// <param name="classType">The type of the class that is declaring class handling.</param>
        /// <param name="routedEvent">The routed event identifier of the event to handle.</param>
        /// <param name="handler">A reference to the class handler implementation.</param>
        /// <param name="handledEventsToo">true to invoke this class handler even if arguments of the routed event have been marked as handled; false to retain the default behavior of not invoking the handler on any marked-handled event.</param>
        public static void RegisterClassHandler(Type classType, RoutedEvent routedEvent, Delegate handler, bool handledEventsToo)
        {
            if (classType == null)
            {
                throw new ArgumentNullException("classType");
            }
            if (!typeof(UIElement).IsAssignableFrom(classType))
            {
                throw new ArgumentException("Illegal class type " + classType.FullName);
            }
            IntPtr routedEventPtr = BaseComponent.getCPtr(routedEvent).Handle;
            if (!IsLegalHandler(routedEventPtr, handler))
            {
                throw new ArgumentException("Illegal Handler type for " + routedEvent.Name + " event");
            }

            ClassHandlerInfo c = AddClassHandler(routedEventPtr, handler);

            Noesis_EventManager_RegisterClassHandler(Extend.GetNativeType(classType),
                BaseComponent.getCPtr(routedEvent).Handle, handledEventsToo,
                Extend.GetInstanceHandle(c).Handle, _classHandler);
        }

        #region Routed Events
        internal static bool IsLegalHandler(IntPtr routedEventPtr, Delegate handler)
        {
            if (routedEventPtr == IntPtr.Zero)
            {
                throw new ArgumentNullException("routedEvent");
            }
            if (handler == null)
            {
                throw new ArgumentNullException("handler");
            }

            HandlerInfo info;
            if (!_handlerTypes.TryGetValue(Key(routedEventPtr), out info))
            {
                RoutedEvent routedEvent = (RoutedEvent)Extend.GetProxy(routedEventPtr, false);
                throw new InvalidOperationException("Routed event " + routedEvent.Name + " not registered");
            }

            return info.Type == handler.GetType();
        }

        internal static void InvokeHandler(IntPtr routedEventPtr, Delegate handler,
            IntPtr sender, IntPtr args)
        {
            HandlerInfo info;
            if (!_handlerTypes.TryGetValue(Key(routedEventPtr), out info))
            {
                throw new InvalidOperationException("Routed event not registered");
            }

            info.Invoker(handler, sender, args);
        }

        #region Class handlers
        private delegate void ClassHandlerCallback(IntPtr cPtrType, IntPtr cPtr,
            IntPtr sender, IntPtr args);
        private static ClassHandlerCallback _classHandler = OnClassHandler;

        [MonoPInvokeCallback(typeof(ClassHandlerCallback))]
        private static void OnClassHandler(IntPtr cPtrType, IntPtr cPtr,
            IntPtr sender, IntPtr args)
        {
            try
            {
                ClassHandlerInfo c = (ClassHandlerInfo)Extend.GetProxy(cPtrType, cPtr, false);

                HandlerInfo info;
                if (!_handlerTypes.TryGetValue(Key(c.RoutedEvent), out info))
                {
                    RoutedEvent routedEvent = (RoutedEvent)Extend.GetProxy(c.RoutedEvent, false);
                    throw new InvalidOperationException("Routed event " + routedEvent.Name + " not registered");
                }

                info.Invoker(c.Handler, sender, args);
            }
            catch (Exception exception)
            {
                Error.UnhandledException(exception);
            }
        }

        private static ClassHandlerInfo AddClassHandler(IntPtr routedEventPtr, Delegate handler)
        {
            ClassHandlerInfo c = new ClassHandlerInfo();
            c.RoutedEvent = routedEventPtr;
            c.Handler = handler;

            _classHandlers.Add(c);

            return c;
        }

        private class ClassHandlerInfo
        {
            public IntPtr RoutedEvent;
            public Delegate Handler;
        }

        private static List<ClassHandlerInfo> _classHandlers;
        #endregion
        #endregion

        #region CLR Events
        internal static bool IsLegalHandler(string eventId, Delegate handler)
        {
            if (string.IsNullOrEmpty(eventId))
            {
                throw new ArgumentNullException("eventId");
            }
            if (handler == null)
            {
                throw new ArgumentNullException("handler");
            }

            HandlerInfo info;
            if (!_handlerTypes.TryGetValue(Key(eventId), out info))
            {
                throw new InvalidOperationException("Event " + eventId + " not registered");
            }

            return info.Type == handler.GetType();
        }

        internal static void InvokeHandler(string eventId, Delegate handler,
            IntPtr sender, IntPtr args)
        {
            HandlerInfo info;
            if (!_handlerTypes.TryGetValue(Key(eventId), out info))
            {
                throw new InvalidOperationException("Event " + eventId + " not registered");
            }

            info.Invoker(handler, sender, args);
        }
        #endregion

        #region Private members
        static EventManager()
        {
            _classHandlers = new List<ClassHandlerInfo>();
            _handlerTypes = new Dictionary<long, HandlerInfo>(100);

            // UIElement
            RegisterRoutedEvent(UIElement.GotFocusEvent, typeof(RoutedEventHandler), RoutedEventArgs.InvokeHandler);
            RegisterRoutedEvent(UIElement.LostFocusEvent, typeof(RoutedEventHandler), RoutedEventArgs.InvokeHandler);
            RegisterRoutedEvent(UIElement.GotMouseCaptureEvent, typeof(MouseEventHandler), MouseEventArgs.InvokeHandler);
            RegisterRoutedEvent(UIElement.LostMouseCaptureEvent, typeof(MouseEventHandler), MouseEventArgs.InvokeHandler);
            RegisterRoutedEvent(UIElement.MouseEnterEvent, typeof(MouseEventHandler), MouseEventArgs.InvokeHandler);
            RegisterRoutedEvent(UIElement.MouseLeaveEvent, typeof(MouseEventHandler), MouseEventArgs.InvokeHandler);
            RegisterRoutedEvent(UIElement.PreviewMouseMoveEvent, typeof(MouseEventHandler), MouseEventArgs.InvokeHandler);
            RegisterRoutedEvent(UIElement.MouseMoveEvent, typeof(MouseEventHandler), MouseEventArgs.InvokeHandler);
            RegisterRoutedEvent(UIElement.PreviewMouseDownEvent, typeof(MouseButtonEventHandler), MouseButtonEventArgs.InvokeHandler);
            RegisterRoutedEvent(UIElement.MouseDownEvent, typeof(MouseButtonEventHandler), MouseButtonEventArgs.InvokeHandler);
            RegisterRoutedEvent(UIElement.PreviewMouseUpEvent, typeof(MouseButtonEventHandler), MouseButtonEventArgs.InvokeHandler);
            RegisterRoutedEvent(UIElement.MouseUpEvent, typeof(MouseButtonEventHandler), MouseButtonEventArgs.InvokeHandler);
            RegisterRoutedEvent(UIElement.PreviewMouseLeftButtonDownEvent, typeof(MouseButtonEventHandler), MouseButtonEventArgs.InvokeHandler);
            RegisterRoutedEvent(UIElement.MouseLeftButtonDownEvent, typeof(MouseButtonEventHandler), MouseButtonEventArgs.InvokeHandler);
            RegisterRoutedEvent(UIElement.PreviewMouseLeftButtonUpEvent, typeof(MouseButtonEventHandler), MouseButtonEventArgs.InvokeHandler);
            RegisterRoutedEvent(UIElement.MouseLeftButtonUpEvent, typeof(MouseButtonEventHandler), MouseButtonEventArgs.InvokeHandler);
            RegisterRoutedEvent(UIElement.PreviewMouseRightButtonDownEvent, typeof(MouseButtonEventHandler), MouseButtonEventArgs.InvokeHandler);
            RegisterRoutedEvent(UIElement.MouseRightButtonDownEvent, typeof(MouseButtonEventHandler), MouseButtonEventArgs.InvokeHandler);
            RegisterRoutedEvent(UIElement.PreviewMouseRightButtonUpEvent, typeof(MouseButtonEventHandler), MouseButtonEventArgs.InvokeHandler);
            RegisterRoutedEvent(UIElement.MouseRightButtonUpEvent, typeof(MouseButtonEventHandler), MouseButtonEventArgs.InvokeHandler);
            RegisterRoutedEvent(UIElement.PreviewMouseWheelEvent, typeof(MouseWheelEventHandler), MouseWheelEventArgs.InvokeHandler);
            RegisterRoutedEvent(UIElement.MouseWheelEvent, typeof(MouseWheelEventHandler), MouseWheelEventArgs.InvokeHandler);
            RegisterRoutedEvent(UIElement.PreviewGotKeyboardFocusEvent, typeof(KeyboardFocusChangedEventHandler), KeyboardFocusChangedEventArgs.InvokeHandler);
            RegisterRoutedEvent(UIElement.GotKeyboardFocusEvent, typeof(KeyboardFocusChangedEventHandler), KeyboardFocusChangedEventArgs.InvokeHandler);
            RegisterRoutedEvent(UIElement.PreviewLostKeyboardFocusEvent, typeof(KeyboardFocusChangedEventHandler), KeyboardFocusChangedEventArgs.InvokeHandler);
            RegisterRoutedEvent(UIElement.LostKeyboardFocusEvent, typeof(KeyboardFocusChangedEventHandler), KeyboardFocusChangedEventArgs.InvokeHandler);
            RegisterRoutedEvent(UIElement.PreviewKeyDownEvent, typeof(KeyEventHandler), KeyEventArgs.InvokeHandler);
            RegisterRoutedEvent(UIElement.KeyDownEvent, typeof(KeyEventHandler), KeyEventArgs.InvokeHandler);
            RegisterRoutedEvent(UIElement.PreviewKeyUpEvent, typeof(KeyEventHandler), KeyEventArgs.InvokeHandler);
            RegisterRoutedEvent(UIElement.KeyUpEvent, typeof(KeyEventHandler), KeyEventArgs.InvokeHandler);
            RegisterRoutedEvent(UIElement.PreviewTextInputEvent, typeof(TextCompositionEventHandler), TextCompositionEventArgs.InvokeHandler);
            RegisterRoutedEvent(UIElement.TextInputEvent, typeof(TextCompositionEventHandler), TextCompositionEventArgs.InvokeHandler);
            RegisterRoutedEvent(UIElement.GotTouchCaptureEvent, typeof(TouchEventHandler), TouchEventArgs.InvokeHandler);
            RegisterRoutedEvent(UIElement.LostTouchCaptureEvent, typeof(TouchEventHandler), TouchEventArgs.InvokeHandler);
            RegisterRoutedEvent(UIElement.TouchEnterEvent, typeof(TouchEventHandler), TouchEventArgs.InvokeHandler);
            RegisterRoutedEvent(UIElement.TouchLeaveEvent, typeof(TouchEventHandler), TouchEventArgs.InvokeHandler);
            RegisterRoutedEvent(UIElement.PreviewTouchMoveEvent, typeof(TouchEventHandler), TouchEventArgs.InvokeHandler);
            RegisterRoutedEvent(UIElement.TouchMoveEvent, typeof(TouchEventHandler), TouchEventArgs.InvokeHandler);
            RegisterRoutedEvent(UIElement.PreviewTouchDownEvent, typeof(TouchEventHandler), TouchEventArgs.InvokeHandler);
            RegisterRoutedEvent(UIElement.TouchDownEvent, typeof(TouchEventHandler), TouchEventArgs.InvokeHandler);
            RegisterRoutedEvent(UIElement.PreviewTouchUpEvent, typeof(TouchEventHandler), TouchEventArgs.InvokeHandler);
            RegisterRoutedEvent(UIElement.TouchUpEvent, typeof(TouchEventHandler), TouchEventArgs.InvokeHandler);
            RegisterRoutedEvent(UIElement.TappedEvent, typeof(TappedEventHandler), TappedEventArgs.InvokeHandler);
            RegisterRoutedEvent(UIElement.DoubleTappedEvent, typeof(DoubleTappedEventHandler), DoubleTappedEventArgs.InvokeHandler);
            RegisterRoutedEvent(UIElement.HoldingEvent, typeof(HoldingEventHandler), HoldingEventArgs.InvokeHandler);
            RegisterRoutedEvent(UIElement.RightTappedEvent, typeof(RightTappedEventHandler), RightTappedEventArgs.InvokeHandler);
            RegisterRoutedEvent(UIElement.ManipulationStartingEvent, typeof(ManipulationStartingEventHandler), ManipulationStartingEventArgs.InvokeHandler);
            RegisterRoutedEvent(UIElement.ManipulationStartedEvent, typeof(ManipulationStartedEventHandler), ManipulationStartedEventArgs.InvokeHandler);
            RegisterRoutedEvent(UIElement.ManipulationDeltaEvent, typeof(ManipulationDeltaEventHandler), ManipulationDeltaEventArgs.InvokeHandler);
            RegisterRoutedEvent(UIElement.ManipulationInertiaStartingEvent, typeof(ManipulationInertiaStartingEventHandler), ManipulationInertiaStartingEventArgs.InvokeHandler);
            RegisterRoutedEvent(UIElement.ManipulationCompletedEvent, typeof(ManipulationCompletedEventHandler), ManipulationCompletedEventArgs.InvokeHandler);
            RegisterRoutedEvent(UIElement.QueryCursorEvent, typeof(QueryCursorEventHandler), QueryCursorEventArgs.InvokeHandler);
            RegisterRoutedEvent(UIElement.PreviewQueryContinueDragEvent, typeof(QueryContinueDragEventHandler), QueryContinueDragEventArgs.InvokeHandler);
            RegisterRoutedEvent(UIElement.QueryContinueDragEvent, typeof(QueryContinueDragEventHandler), QueryContinueDragEventArgs.InvokeHandler);
            RegisterRoutedEvent(UIElement.PreviewGiveFeedbackEvent, typeof(GiveFeedbackEventHandler), GiveFeedbackEventArgs.InvokeHandler);
            RegisterRoutedEvent(UIElement.GiveFeedbackEvent, typeof(GiveFeedbackEventHandler), GiveFeedbackEventArgs.InvokeHandler);
            RegisterRoutedEvent(UIElement.PreviewDragEnterEvent, typeof(DragEventHandler), DragEventArgs.InvokeHandler);
            RegisterRoutedEvent(UIElement.DragEnterEvent, typeof(DragEventHandler), DragEventArgs.InvokeHandler);
            RegisterRoutedEvent(UIElement.PreviewDragOverEvent, typeof(DragEventHandler), DragEventArgs.InvokeHandler);
            RegisterRoutedEvent(UIElement.DragOverEvent, typeof(DragEventHandler), DragEventArgs.InvokeHandler);
            RegisterRoutedEvent(UIElement.PreviewDragLeaveEvent, typeof(DragEventHandler), DragEventArgs.InvokeHandler);
            RegisterRoutedEvent(UIElement.DragLeaveEvent, typeof(DragEventHandler), DragEventArgs.InvokeHandler);
            RegisterRoutedEvent(UIElement.PreviewDropEvent, typeof(DragEventHandler), DragEventArgs.InvokeHandler);
            RegisterRoutedEvent(UIElement.DropEvent, typeof(DragEventHandler), DragEventArgs.InvokeHandler);
            RegisterCLREvent("FocusableChanged", typeof(DependencyPropertyChangedEventHandler), DependencyPropertyChangedEventArgs.InvokeHandler);
            RegisterCLREvent("IsEnabledChanged", typeof(DependencyPropertyChangedEventHandler), DependencyPropertyChangedEventArgs.InvokeHandler);
            RegisterCLREvent("IsHitTestVisibleChanged", typeof(DependencyPropertyChangedEventHandler), DependencyPropertyChangedEventArgs.InvokeHandler);
            RegisterCLREvent("IsVisibleChanged", typeof(DependencyPropertyChangedEventHandler), DependencyPropertyChangedEventArgs.InvokeHandler);
            RegisterCLREvent("IsMouseCapturedChanged", typeof(DependencyPropertyChangedEventHandler), DependencyPropertyChangedEventArgs.InvokeHandler);
            RegisterCLREvent("IsMouseCaptureWithinChanged", typeof(DependencyPropertyChangedEventHandler), DependencyPropertyChangedEventArgs.InvokeHandler);
            RegisterCLREvent("IsMouseDirectlyOverChanged", typeof(DependencyPropertyChangedEventHandler), DependencyPropertyChangedEventArgs.InvokeHandler);
            RegisterCLREvent("IsKeyboardFocusedChanged", typeof(DependencyPropertyChangedEventHandler), DependencyPropertyChangedEventArgs.InvokeHandler);
            RegisterCLREvent("IsKeyboardFocusWithinChanged", typeof(DependencyPropertyChangedEventHandler), DependencyPropertyChangedEventArgs.InvokeHandler);
            RegisterCLREvent("LayoutUpdated", typeof(EventHandler), EventArgs.InvokeHandler);

            // FrameworkElement
            RegisterRoutedEvent(FrameworkElement.LoadedEvent, typeof(RoutedEventHandler), RoutedEventArgs.InvokeHandler);
            RegisterRoutedEvent(FrameworkElement.ReloadedEvent, typeof(RoutedEventHandler), RoutedEventArgs.InvokeHandler);
            RegisterRoutedEvent(FrameworkElement.UnloadedEvent, typeof(RoutedEventHandler), RoutedEventArgs.InvokeHandler);
            RegisterRoutedEvent(FrameworkElement.SizeChangedEvent, typeof(SizeChangedEventHandler), SizeChangedEventArgs.InvokeHandler);
            RegisterRoutedEvent(FrameworkElement.RequestBringIntoViewEvent, typeof(RequestBringIntoViewEventHandler), RequestBringIntoViewEventArgs.InvokeHandler);
            RegisterRoutedEvent(FrameworkElement.ContextMenuOpeningEvent, typeof(ContextMenuEventHandler), ContextMenuEventArgs.InvokeHandler);
            RegisterRoutedEvent(FrameworkElement.ContextMenuClosingEvent, typeof(ContextMenuEventHandler), ContextMenuEventArgs.InvokeHandler);
            RegisterRoutedEvent(FrameworkElement.ToolTipOpeningEvent, typeof(ToolTipEventHandler), ToolTipEventArgs.InvokeHandler);
            RegisterRoutedEvent(FrameworkElement.ToolTipClosingEvent, typeof(ToolTipEventHandler), ToolTipEventArgs.InvokeHandler);
            RegisterCLREvent("Initialized", typeof(EventHandler), EventArgs.InvokeHandler);
            RegisterCLREvent("DataContextChanged", typeof(DependencyPropertyChangedEventHandler), DependencyPropertyChangedEventArgs.InvokeHandler);

            // Control
            RegisterRoutedEvent(Control.PreviewMouseDoubleClickEvent, typeof(MouseButtonEventHandler), MouseButtonEventArgs.InvokeHandler);
            RegisterRoutedEvent(Control.MouseDoubleClickEvent, typeof(MouseButtonEventHandler), MouseButtonEventArgs.InvokeHandler);

            // ButtonBase
            RegisterRoutedEvent(ButtonBase.ClickEvent, typeof(RoutedEventHandler), RoutedEventArgs.InvokeHandler);

            // ToggleButton
            RegisterRoutedEvent(ToggleButton.CheckedEvent, typeof(RoutedEventHandler), RoutedEventArgs.InvokeHandler);
            RegisterRoutedEvent(ToggleButton.UncheckedEvent, typeof(RoutedEventHandler), RoutedEventArgs.InvokeHandler);
            RegisterRoutedEvent(ToggleButton.IndeterminateEvent, typeof(RoutedEventHandler), RoutedEventArgs.InvokeHandler);

            // Expander
            RegisterRoutedEvent(Expander.ExpandedEvent, typeof(RoutedEventHandler), RoutedEventArgs.InvokeHandler);
            RegisterRoutedEvent(Expander.CollapsedEvent, typeof(RoutedEventHandler), RoutedEventArgs.InvokeHandler);

            // ContextMenu
            RegisterRoutedEvent(ContextMenu.OpenedEvent, typeof(RoutedEventHandler), RoutedEventArgs.InvokeHandler);
            RegisterRoutedEvent(ContextMenu.ClosedEvent, typeof(RoutedEventHandler), RoutedEventArgs.InvokeHandler);

            // ToolTip
            RegisterRoutedEvent(ToolTip.OpenedEvent, typeof(RoutedEventHandler), RoutedEventArgs.InvokeHandler);
            RegisterRoutedEvent(ToolTip.ClosedEvent, typeof(RoutedEventHandler), RoutedEventArgs.InvokeHandler);

            // Selector
            RegisterRoutedEvent(Selector.SelectionChangedEvent, typeof(SelectionChangedEventHandler), SelectionChangedEventArgs.InvokeHandler);

            // ListBoxItem
            RegisterRoutedEvent(ListBoxItem.SelectedEvent, typeof(RoutedEventHandler), RoutedEventArgs.InvokeHandler);
            RegisterRoutedEvent(ListBoxItem.UnselectedEvent, typeof(RoutedEventHandler), RoutedEventArgs.InvokeHandler);

            // MenuItem
            RegisterRoutedEvent(MenuItem.ClickEvent, typeof(RoutedEventHandler), RoutedEventArgs.InvokeHandler);
            RegisterRoutedEvent(MenuItem.CheckedEvent, typeof(RoutedEventHandler), RoutedEventArgs.InvokeHandler);
            RegisterRoutedEvent(MenuItem.UncheckedEvent, typeof(RoutedEventHandler), RoutedEventArgs.InvokeHandler);
            RegisterRoutedEvent(MenuItem.SubmenuOpenedEvent, typeof(RoutedEventHandler), RoutedEventArgs.InvokeHandler);
            RegisterRoutedEvent(MenuItem.SubmenuClosedEvent, typeof(RoutedEventHandler), RoutedEventArgs.InvokeHandler);

            // TreeViewItem
            RegisterRoutedEvent(TreeViewItem.SelectedEvent, typeof(RoutedEventHandler), RoutedEventArgs.InvokeHandler);
            RegisterRoutedEvent(TreeViewItem.UnselectedEvent, typeof(RoutedEventHandler), RoutedEventArgs.InvokeHandler);
            RegisterRoutedEvent(TreeViewItem.ExpandedEvent, typeof(RoutedEventHandler), RoutedEventArgs.InvokeHandler);
            RegisterRoutedEvent(TreeViewItem.CollapsedEvent, typeof(RoutedEventHandler), RoutedEventArgs.InvokeHandler);

            // TreeView
            RegisterRoutedEvent(TreeView.SelectedItemChangedEvent, typeof(RoutedPropertyChangedEventHandler<object>), RoutedPropertyChangedEventArgs<object>.InvokeHandler);

            // RangeBase
            RegisterRoutedEvent(RangeBase.ValueChangedEvent, typeof(RoutedPropertyChangedEventHandler<float>), RoutedPropertyChangedEventArgs<float>.InvokeHandler);

            // Thumb
            RegisterRoutedEvent(Thumb.DragStartedEvent, typeof(DragStartedEventHandler), DragStartedEventArgs.InvokeHandler);
            RegisterRoutedEvent(Thumb.DragDeltaEvent, typeof(DragDeltaEventHandler), DragDeltaEventArgs.InvokeHandler);
            RegisterRoutedEvent(Thumb.DragCompletedEvent, typeof(DragCompletedEventHandler), DragCompletedEventArgs.InvokeHandler);

            // ScrollBar
            RegisterRoutedEvent(ScrollBar.ScrollEvent, typeof(ScrollEventHandler), ScrollEventArgs.InvokeHandler);

            // ScrollViewer
            RegisterRoutedEvent(ScrollViewer.ScrollChangedEvent, typeof(ScrollChangedEventHandler), ScrollChangedEventArgs.InvokeHandler);

            // TextBoxBase
            RegisterRoutedEvent(TextBoxBase.SelectionChangedEvent, typeof(RoutedEventHandler), RoutedEventArgs.InvokeHandler);
            RegisterRoutedEvent(TextBoxBase.TextChangedEvent, typeof(RoutedEventHandler), RoutedEventArgs.InvokeHandler);

            // PasswordBox
            RegisterRoutedEvent(PasswordBox.PasswordChangedEvent, typeof(RoutedEventHandler), RoutedEventArgs.InvokeHandler);

            // Hyperlink
            RegisterRoutedEvent(Hyperlink.RequestNavigateEvent, typeof(RequestNavigateEventHandler), RequestNavigateEventArgs.InvokeHandler);
        }

        private delegate void InvokeHandlerDelegate(Delegate handler, IntPtr sender, IntPtr args);

        private struct HandlerInfo
        {
            public Type Type { get; set; }
            public InvokeHandlerDelegate Invoker { get; set; }
        }

        private static RoutedEvent AddRoutedEvent(string name, RoutingStrategy routingStrategy,
            Type handlerType, Type ownerType)
        {
            IntPtr routedEventPtr = Noesis_EventManager_RegisterRoutedEvent(name,
                (int)routingStrategy, Extend.GetNativeType(ownerType));

            RoutedEvent routedEvent = (RoutedEvent)Extend.GetProxy(routedEventPtr, false);

            RegisterRoutedEvent(routedEvent, handlerType, GetInvoker(handlerType));

            return routedEvent;
        }

        private static InvokeHandlerDelegate GetInvoker(Type handlerType)
        {
            MethodInfo handlerInvoke = handlerType.GetMethod("Invoke");
            if (handlerInvoke == null)
            {
                throw new ArgumentException("handlerType");
            }

            ParameterInfo[] parameters = handlerInvoke.GetParameters();
            if (parameters.Length != 2)
            {
                throw new ArgumentException("handlerType");
            }

            Type argsType = parameters[1].ParameterType;
            MethodInfo invoker = argsType.GetMethod("InvokeHandler", BindingFlags.Static | BindingFlags.NonPublic);
            if (invoker == null)
            {
                throw new InvalidOperationException(
                    "Event args for " + handlerType.Name + " have to define the method " +
                    "'static void InvokeHandler(Delegate handler, IntPtr sender, IntPtr args)'");
            }

            return (InvokeHandlerDelegate)Delegate.CreateDelegate(typeof(InvokeHandlerDelegate),
                invoker);
        }

        private static void RegisterRoutedEvent(RoutedEvent routedEvent,
            Type handlerType, InvokeHandlerDelegate invoker)
        {
            _handlerTypes.Add(Key(routedEvent), new HandlerInfo 
            {
                Type = handlerType, Invoker = invoker
            });
        }

        private static void RegisterCLREvent(string eventId,
            Type handlerType, InvokeHandlerDelegate invoker)
        {
            _handlerTypes.Add(Key(eventId), new HandlerInfo
            {
                Type = handlerType, Invoker = invoker
            });
        }

        private static long Key(RoutedEvent routedEvent)
        {
            return BaseComponent.getCPtr(routedEvent).Handle.ToInt64();
        }

        private static long Key(IntPtr routedEvent)
        {
            return routedEvent.ToInt64();
        }

        private static long Key(string eventId)
        {
            return eventId.GetHashCode();
        }

        private static Dictionary<long, HandlerInfo> _handlerTypes;
        #endregion

        #region Imports
        [DllImport(Library.Name)]
        private static extern IntPtr Noesis_EventManager_RegisterRoutedEvent(
            [MarshalAs(UnmanagedType.LPWStr)]string name, int routingStrategy, IntPtr ownerType);

        [DllImport(Library.Name)]
        private static extern void Noesis_EventManager_RegisterClassHandler(IntPtr classType,
            IntPtr routedEvent, bool handledEventsToo, IntPtr info, ClassHandlerCallback callback);
        #endregion
    }
}

