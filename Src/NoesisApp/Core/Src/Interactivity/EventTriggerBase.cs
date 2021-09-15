using Noesis;
using System;
using System.Reflection;

namespace NoesisApp
{
    /// <summary>
    /// Represents a trigger that can listen to an object other than its AssociatedObject.
    /// </summary>
    public abstract class EventTriggerBase : TriggerBase
    {
        protected EventTriggerBase(Type sourceType) : base(typeof(DependencyObject))
        {
            _sourceType = sourceType;
            _source = IntPtr.Zero;
            _event = null;
            _handler = null;
        }

        public new EventTriggerBase Clone()
        {
            return (EventTriggerBase)base.Clone();
        }

        public new EventTriggerBase CloneCurrentValue()
        {
            return (EventTriggerBase)base.CloneCurrentValue();
        }

        /// <summary>
        /// Gets or sets the source object
        /// </summary>
        public object SourceObject
        {
            get { return GetValue(SourceObjectProperty); }
            set { SetValue(SourceObjectProperty, value); }
        }

        public static readonly DependencyProperty SourceObjectProperty = DependencyProperty.Register(
            "SourceObject", typeof(object), typeof(EventTriggerBase),
            new PropertyMetadata(null, OnSourceObjectChanged));

        static void OnSourceObjectChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            EventTriggerBase trigger = (EventTriggerBase)d;
            trigger.UpdateSource(trigger.AssociatedObject);
        }

        /// <summary>
        /// Gets or sets the name of the object this trigger listens for as source
        /// </summary>
        public string SourceName
        {
            get { return (string)GetValue(SourceNameProperty); }
            set { SetValue(SourceNameProperty, value); }
        }

        public static readonly DependencyProperty SourceNameProperty = DependencyProperty.Register(
            "SourceName", typeof(string), typeof(EventTriggerBase),
            new PropertyMetadata(string.Empty, OnSourceNameChanged));

        static void OnSourceNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            EventTriggerBase trigger = (EventTriggerBase)d;
            Binding binding = new Binding("", trigger.SourceName);
            BindingOperations.SetBinding(trigger, SourceNameResolverProperty, binding);
        }

        /// <summary>
        /// If SourceObject is not set, the Source will look for the object specified by SourceName
        /// If SourceName element cannot be found, the Source will default to the AssociatedObject
        /// </summary>
        protected object Source
        {
            get { return GetProxy(_source); }
        }

        protected virtual void OnSourceChangedImpl(object oldSource, object newSource)
        {
            UnregisterEvent(oldSource);
            RegisterEvent(newSource, GetEventName());
        }

        protected abstract string GetEventName();

        protected virtual void OnEventNameChanged(string oldName, string newName)
        {
            UnregisterEvent(Source);
            RegisterEvent(Source, newName);
        }

        protected virtual void OnEvent(object parameter)
        {
            InvokeActions(parameter);
        }

        protected override void OnAttached()
        {
            base.OnAttached();

            UpdateSource(AssociatedObject);
        }

        protected override void OnDetaching()
        {
            UpdateSource(null);

            base.OnDetaching();
        }

        private void UpdateSource(object associatedObject)
        {
            object oldSource = Source;
            object newSource = associatedObject;

            if (associatedObject != null)
            {
                object sourceObject = SourceObject;
                if (sourceObject != null)
                {
                    newSource = sourceObject;
                }
                else if (!string.IsNullOrEmpty(SourceName))
                {
                    newSource = SourceNameResolver;

                    if (newSource != null)
                    {
                        // Remove binding once we have found the SourceName object because keeping it
                        // stored in this property could create circular references and memory leaks
                        BindingOperations.ClearBinding(this, SourceNameResolverProperty);
                    }
                }
            }

            if (oldSource != newSource)
            {
                if (newSource != null && !_sourceType.IsAssignableFrom(newSource.GetType()))
                {
                    throw new InvalidOperationException(string.Format(
                        "Invalid source element type '{0}' for '{1}'",
                        newSource.GetType(), GetType()));
                }

                UnregisterSource(oldSource);

                _source = GetPtr(newSource);

                RegisterSource(newSource);

                if (AssociatedObject != null)
                {
                    OnSourceChangedImpl(oldSource, newSource);
                }
            }
        }

        private void RegisterEvent(object source, string eventName)
        {
            if (source != null && !string.IsNullOrEmpty(eventName))
            {
                Type type = source.GetType();
                EventInfo ev = type.GetEvent(eventName);

                if (ev == null)
                {
                    if (SourceObject != null)
                    {
                        throw new ArgumentException(string.Format(
                            "EventTrigger cannot find event '{0}' in SourceObject '{1}'",
                            eventName, type));
                    }
                }

                if (!IsValidEvent(ev))
                {
                    if (SourceObject != null)
                    {
                        throw new ArgumentException(string.Format(
                            "SourceObject event '{0}' is not valid for EventTrigger",
                            eventName));
                    }
                }
                else
                {
                    _event = ev;
                    _handler = Delegate.CreateDelegate(ev.EventHandlerType, this, OnEventMethod);
                    _event.AddEventHandler(source, _handler);
                }
            }
        }

        private void UnregisterEvent(object source)
        {
            if (_event != null && _handler != null)
            {
                _event.RemoveEventHandler(source, _handler);
                _event = null;
                _handler = null;
            }
        }

        private void OnEventImpl(object sender, Noesis.EventArgs eventArgs)
        {
            OnEvent(eventArgs);
        }

        private static bool IsValidEvent(EventInfo eventInfo)
        {
            Type eventHandlerType = eventInfo.EventHandlerType;
            if (typeof(Delegate).IsAssignableFrom(eventInfo.EventHandlerType))
            {
                MethodInfo method = eventHandlerType.GetMethod("Invoke");
                ParameterInfo[] parameters = method.GetParameters();
                return parameters.Length == 2
                    && typeof(object).IsAssignableFrom(parameters[0].ParameterType)
                    && typeof(Noesis.EventArgs).IsAssignableFrom(parameters[1].ParameterType);
            }
            return false;
        }

        #region Source registration
        private void RegisterSource(object source)
        {
            if (source is DependencyObject)
            {
                DependencyObject dob = (DependencyObject)source;
                dob.Destroyed += OnSourceDestroyed;
            }
            else
            {
                _keepSource = source;
            }
        }

        private void UnregisterSource(object source)
        {
            if (source is DependencyObject)
            {
                DependencyObject dob = (DependencyObject)source;
                dob.Destroyed -= OnSourceDestroyed;
            }
            else
            {
                _keepSource = source;
            }
        }

        private void OnSourceDestroyed(IntPtr d)
        {
            _source = IntPtr.Zero;
            _event = null;
            _handler = null;
        }
        #endregion

        #region SourceName resolver
        public object SourceNameResolver
        {
            get { return GetValue(SourceNameResolverProperty); }
        }

        public static readonly DependencyProperty SourceNameResolverProperty = DependencyProperty.Register(
            ".SourceNameResolver", typeof(object), typeof(EventTriggerBase),
            new PropertyMetadata(null, OnSourceNameResolverChanged));

        static void OnSourceNameResolverChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            EventTriggerBase trigger = (EventTriggerBase)d;
            if (BindingOperations.GetBinding(trigger, SourceNameResolverProperty) != null)
            {
                trigger.UpdateSource(trigger.AssociatedObject);
            }
        }
        #endregion

        #region Private members
        public static readonly MethodInfo OnEventMethod = typeof(EventTriggerBase).GetMethod(
                "OnEventImpl", BindingFlags.Instance | BindingFlags.NonPublic);

        Type _sourceType;
        IntPtr _source;
        object _keepSource;
        Delegate _handler;
        EventInfo _event;
        #endregion
    }

    public abstract class EventTriggerBase<T> : EventTriggerBase where T : class
    {
        protected EventTriggerBase() : base(typeof(T)) { }

        protected new T Source { get { return (T)base.Source; } }

        protected sealed override void OnSourceChangedImpl(object oldSource, object newSource)
        {
            base.OnSourceChangedImpl(oldSource, newSource);
            OnSourceChanged((T)oldSource, (T)newSource);
        }

        protected virtual void OnSourceChanged(T oldSource, T newSource)
        {
        }
    }
}
