using Noesis;
using System;

namespace NoesisApp
{
    /// <summary>
    /// Represents an attachable object that encapsulates a unit of functionality.
    /// </summary>
    public abstract class TriggerAction : AttachableObject
    {
        protected TriggerAction(Type associatedType) : base(associatedType)
        {
        }

        public new TriggerAction Clone()
        {
            return (TriggerAction)base.Clone();
        }

        public new TriggerAction CloneCurrentValue()
        {
            return (TriggerAction)base.CloneCurrentValue();
        }

        /// <summary>
        /// Indicates whether this action will run when invoked
        /// </summary>
        public bool IsEnabled
        {
            get { return (bool)GetValue(IsEnabledProperty); }
            set { SetValue(IsEnabledProperty, value); }
        }

        public static readonly DependencyProperty IsEnabledProperty = DependencyProperty.Register(
            "IsEnabled", typeof(bool), typeof(TriggerAction), new PropertyMetadata(true));

        /// <summary>
        /// Attempts to invoke the action
        /// </summary>
        public void CallInvoke(object parameter)
        {
            if (IsEnabled)
            {
                Invoke(parameter);
            }
        }

        /// <summary>
        /// Invokes the action
        /// </summary>
        protected abstract void Invoke(object parameter);
    }

    public abstract class TriggerAction<T> : TriggerAction where T : DependencyObject
    {
        protected TriggerAction() : base(typeof(T)) { }

        protected new T AssociatedObject { get { return (T)base.AssociatedObject; } }
    }
}
