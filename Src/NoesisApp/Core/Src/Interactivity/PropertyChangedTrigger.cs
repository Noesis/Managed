using Noesis;
using System;
using System.Reflection;

namespace NoesisApp
{
    /// <summary>
    /// Represents a trigger that performs actions when the bound data have changed.
    /// </summary>
    public class PropertyChangedTrigger : TriggerBase<DependencyObject>
    {
        public new PropertyChangedTrigger Clone()
        {
            return (PropertyChangedTrigger)base.Clone();
        }

        public new PropertyChangedTrigger CloneCurrentValue()
        {
            return (PropertyChangedTrigger)base.CloneCurrentValue();
        }

        /// <summary>
        /// Gets or sets the binding object that the trigger listens to, and causes to fire actions
        /// </summary>
        public object Binding
        {
            get { return GetValue(BindingProperty); }
            set { SetValue(BindingProperty, value); }
        }

        public static readonly DependencyProperty BindingProperty = DependencyProperty.Register(
            "Binding", typeof(object), typeof(PropertyChangedTrigger),
            new PropertyMetadata(null, OnBindingChanged));

        static void OnBindingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PropertyChangedTrigger trigger = (PropertyChangedTrigger)d;
            trigger.EvaluateBindingChange(e);
        }

        protected override void OnAttached()
        {
            base.OnAttached();

            if (Binding != null)
            {
                EvaluateBindingChange(null);
            }
        }

        /// <summary>
        /// Called when the binding property has changed
        /// </summary>
        protected virtual void EvaluateBindingChange(object args)
        {
            if (AssociatedObject != null)
            {
                InvokeActions(args);
            }
        }
    }
}
