using Noesis;
using System;
using System.Collections.Generic;

namespace NoesisApp
{
    /// <summary>
    /// Represents an action that can be targeted to affect an object other than its AssociatedObject.
    /// </summary>
    public abstract class TargetedTriggerAction : TriggerAction
    {
        protected TargetedTriggerAction(Type targetType) : base(typeof(DependencyObject))
        {
            _targetType = targetType;
            _target = IntPtr.Zero;
        }

        public new TargetedTriggerAction Clone()
        {
            return (TargetedTriggerAction)base.Clone();
        }

        public new TargetedTriggerAction CloneCurrentValue()
        {
            return (TargetedTriggerAction)base.CloneCurrentValue();
        }

        /// <summary>
        /// Gets or sets the target object
        /// </summary>
        public object TargetObject
        {
            get { return GetValue(TargetObjectProperty); }
            set { SetValue(TargetObjectProperty, value); }
        }

        public static readonly DependencyProperty TargetObjectProperty = DependencyProperty.Register(
            "TargetObject", typeof(object), typeof(TargetedTriggerAction),
            new PropertyMetadata(null, OnTargetObjectChanged));

        private static void OnTargetObjectChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TargetedTriggerAction action = (TargetedTriggerAction)d;
            action.UpdateTarget(action.AssociatedObject);
        }

        /// <summary>
        /// Gets or sets the name of the object this action targets
        /// </summary>
        public string TargetName
        {
            get { return (string)GetValue(TargetNameProperty); }
            set { SetValue(TargetNameProperty, value); }
        }

        public static readonly DependencyProperty TargetNameProperty = DependencyProperty.Register(
            "TargetName", typeof(string), typeof(TargetedTriggerAction),
            new PropertyMetadata(string.Empty, OnTargetNameChanged));

        private static void OnTargetNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TargetedTriggerAction action = (TargetedTriggerAction)d;
            Binding binding = new Binding("", action.TargetName);
            BindingOperations.SetBinding(action, TargetNameResolverProperty, binding);
        }

        /// <summary>
        /// If TargetObject is not set, the Target will look for the object specified by TargetName
        /// If TargetName element cannot be found, the Target will default to the AssociatedObject
        /// </summary>
        protected object Target
        {
            get { return GetProxy(_target); }
        }

        /// <summary>
        /// Called when the target changes
        /// </summary>
        protected virtual void OnTargetChangedImpl(object oldTarget, object newTarget)
        {
        }

        protected override void OnAttached()
        {
            base.OnAttached();

            UpdateTarget(AssociatedObject);
        }

        protected override void OnDetaching()
        {
            UpdateTarget(null);

            base.OnDetaching();
        }

        private void UpdateTarget(object associatedObject)
        {
            object oldTarget = Target;
            object newTarget = associatedObject;

            if (associatedObject != null)
            {
                object targetObject = TargetObject;
                if (targetObject != null)
                {
                    if (targetObject.GetType().IsValueType)
                    {
                        throw new InvalidOperationException(string.Format(
                            "Value type not supported for {0}.TargetObject (value was '{1}')",
                            GetType(), targetObject.ToString()));
                    }
                    newTarget = targetObject;
                }
                else if (!string.IsNullOrEmpty(TargetName))
                {
                    if (BindingOperations.GetBinding(this, TargetNameResolverProperty) != null)
                    {
                        newTarget = TargetNameResolver;
                        if (newTarget != null)
                        {
                            // Remove binding once we have found the TargetName object because keeping it
                            // stored in this property could create circular references and memory leaks
                            BindingOperations.ClearBinding(this, TargetNameResolverProperty);
                        }
                    }
                    else
                    {
                        newTarget = Target;
                    }
                }
            }

            if (oldTarget != newTarget)
            {
                if (newTarget != null && !_targetType.IsAssignableFrom(newTarget.GetType()))
                {
                    throw new InvalidOperationException(string.Format(
                        "Invalid target element type '{0}' for '{1}'",
                        newTarget.GetType(), GetType()));
                }

                UnregisterTarget(oldTarget);

                _target = GetPtr(newTarget);

                RegisterTarget(newTarget);

                if (AssociatedObject != null)
                {
                    OnTargetChangedImpl(oldTarget, newTarget);
                }
            }
        }

        #region Target registration
        private void RegisterTarget(object target)
        {
            if (target is DependencyObject)
            {
                DependencyObject dob = (DependencyObject)target;
                dob.Destroyed += OnTargetDestroyed;
            }
            else
            {
                _keepTarget = target;
            }
        }

        private void UnregisterTarget(object target)
        {
            if (target is DependencyObject)
            {
                DependencyObject dob = (DependencyObject)target;
                dob.Destroyed -= OnTargetDestroyed;
            }
            else
            {
                _keepTarget = null;
            }
        }

        private void OnTargetDestroyed(IntPtr d)
        {
            _target = IntPtr.Zero;
        }
        #endregion

        #region TargetName resolver
        public object TargetNameResolver
        {
            get { return GetValue(TargetNameResolverProperty); }
        }

        public static readonly DependencyProperty TargetNameResolverProperty = DependencyProperty.Register(
            ".TargetNameResolver", typeof(object), typeof(TargetedTriggerAction),
            new PropertyMetadata(null, OnTargetNameResolverChanged));

        static void OnTargetNameResolverChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TargetedTriggerAction action = (TargetedTriggerAction)d;
            if (BindingOperations.GetBinding(action, TargetNameResolverProperty) != null)
            {
                action.UpdateTarget(action.AssociatedObject);
            }
        }
        #endregion

        #region Private members
        Type _targetType;
        IntPtr _target;
        object _keepTarget;
        #endregion
    }

    public abstract class TargetedTriggerAction<T> : TargetedTriggerAction where T : class
    {
        protected TargetedTriggerAction() : base(typeof(T)) { }

        protected new T Target { get { return (T)base.Target; } }

        protected sealed override void OnTargetChangedImpl(object oldTarget, object newTarget)
        {
            base.OnTargetChangedImpl(oldTarget, newTarget);
            OnTargetChanged((T)oldTarget, (T)newTarget);
        }

        protected virtual void OnTargetChanged(T oldTarget, T newTarget)
        {
        }
    }
}
