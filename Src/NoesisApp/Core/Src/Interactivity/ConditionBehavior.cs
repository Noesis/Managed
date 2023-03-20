using Noesis;

namespace NoesisApp
{
    /// <summary>
    /// A behavior that attaches to a trigger and controls the conditions to fire the actions.
    /// </summary>
    [ContentProperty("Condition")]
    public class ConditionBehavior : Behavior<TriggerBase>
    {
        public new ConditionBehavior Clone()
        {
            return (ConditionBehavior)base.Clone();
        }

        public new ConditionBehavior CloneCurrentValue()
        {
            return (ConditionBehavior)base.CloneCurrentValue();
        }

        public ICondition Condition
        {
            get { return (ICondition)GetValue(ConditionProperty); }
            set { SetValue(ConditionProperty, value); }
        }

        public static readonly DependencyProperty ConditionProperty = DependencyProperty.Register(
            "Condition", typeof(ICondition), typeof(ConditionBehavior),
            new PropertyMetadata(null));

        protected override void OnAttached()
        {
            TriggerBase trigger = AssociatedObject;
            if (trigger != null)
            {
                trigger.PreviewInvoke += OnPreviewInvoke;
            }
        }

        protected override void OnDetaching()
        {
            TriggerBase trigger = AssociatedObject;
            if (trigger != null)
            {
                trigger.PreviewInvoke -= OnPreviewInvoke;
            }
        }

        private void OnPreviewInvoke(object sender, PreviewInvokeEventArgs e)
        {
            ICondition condition = Condition;
            if (condition != null)
            {
                e.Cancelling = !condition.Evaluate();
            }
        }
    }
}
