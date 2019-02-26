using Noesis;
using System;

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
            action.UpdateTarget(action.AssociatedObject);
        }

        /// <summary>
        /// If TargetObject is not set, the Target will look for the object specified by TargetName
        /// If TargetName element cannot be found, the Target will default to the AssociatedObject
        /// </summary>
        protected object Target
        {
            get { return _target; }
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
            object oldTarget = _target;
            object newTarget = associatedObject;

            object targetObject = TargetObject;
            string targetName = TargetName;

            if (associatedObject != null)
            {
                if (targetObject != null)
                {
                    newTarget = targetObject;
                }
                else if (!string.IsNullOrEmpty(targetName))
                {
                    FrameworkElement element = associatedObject as FrameworkElement;
                    newTarget = element != null ? element.FindName(targetName) : null;
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

                _target = newTarget;

                if (AssociatedObject != null)
                {
                    OnTargetChangedImpl(oldTarget, newTarget);
                }
            }
        }

        Type _targetType;
        object _target;
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
