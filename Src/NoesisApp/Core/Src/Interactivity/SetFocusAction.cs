using System;

namespace NoesisGUIExtensions
{
    /// <summary>
    /// An action that will try to focus the associated element.
    /// </summary>
    public class SetFocusAction : NoesisApp.TargetedTriggerAction<Noesis.UIElement>
    {
        public new SetFocusAction Clone()
        {
            return (SetFocusAction)base.Clone();
        }

        public new SetFocusAction CloneCurrentValue()
        {
            return (SetFocusAction)base.CloneCurrentValue();
        }

        protected override void Invoke(object parameter)
        {
            Noesis.UIElement element = Target;
            if (element != null)
            {
                element.Focus();
            }
        }
    }
}
