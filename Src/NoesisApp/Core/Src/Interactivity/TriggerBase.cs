using Noesis;
using System;

namespace NoesisApp
{
    /// <summary>
    /// PreviewInvoke event args. Setting Cancelling to True will cancel invoking the trigger.
    /// </summary>
    public class PreviewInvokeEventArgs : Noesis.EventArgs
    {
        public bool Cancelling { get; set; }
    }

    public delegate void PreviewInvokeEventHandler(object sender, PreviewInvokeEventArgs e);

    /// <summary>
    /// Represents an object that can invoke Actions conditionally.
    /// </summary>
    [ContentProperty("Actions")]
    public abstract class TriggerBase : AttachableObject
    {
        protected TriggerBase(Type associatedType) : base(associatedType)
        {
            SetValue(ActionsProperty, new TriggerActionCollection());
        }

        public new TriggerBase Clone()
        {
            return (TriggerBase)base.Clone();
        }

        public new TriggerBase CloneCurrentValue()
        {
            return (TriggerBase)base.CloneCurrentValue();
        }

        /// <summary>
        /// Raised just before invoking all associated actions
        /// </summary>
        public event PreviewInvokeEventHandler PreviewInvoke;

        /// <summary>
        /// Gets the actions associated with this trigger
        /// </summary>
        public TriggerActionCollection Actions
        {
            get { return (TriggerActionCollection)GetValue(ActionsProperty); }
        }

        public static readonly DependencyProperty ActionsProperty = DependencyProperty.Register(
            "Actions", typeof(TriggerActionCollection), typeof(TriggerBase),
            new PropertyMetadata(null));

        protected override void OnAttached()
        {
            Actions.Attach(base.AssociatedObject);
        }

        protected override void OnDetaching()
        {
            Actions.Detach();
        }

        /// <summary>
        /// Invoke all actions associated with this trigger
        /// </summary>
        protected void InvokeActions(object parameter)
        {
            if (PreviewInvoke != null)
            {
                PreviewInvokeEventArgs previewInvokeEventArgs = new PreviewInvokeEventArgs();
                PreviewInvoke(this, previewInvokeEventArgs);

                if (previewInvokeEventArgs.Cancelling)
                {
                    return;
                }
            }

            foreach (TriggerAction triggerAction in Actions)
            {
                triggerAction.CallInvoke(parameter);
            }
        }
    }

    public abstract class TriggerBase<T> : TriggerBase where T : DependencyObject
    {
        protected TriggerBase() : base(typeof(T)) { }

        protected new T AssociatedObject { get { return (T)base.AssociatedObject; } }
    }
}
