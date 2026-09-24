using Noesis;
using Noesis.Interactivity;
using System;

namespace NoesisGUIExtensions
{
    /// <summary>
    /// An action that will try to focus the associated element.
    /// </summary>
    public class SetFocusAction : TargetedTriggerAction<UIElement>
    {
        /// <summary>
        /// Indicates if control should be engaged or not when getting the focus.
        /// Default value is true.
        /// </summary>
        public bool Engage
        {
            get { return (bool)GetValue(EngageProperty); }
            set { SetValue(EngageProperty, value); }
        }

        public static readonly DependencyProperty EngageProperty = DependencyProperty.Register(
            "Engage", typeof(bool), typeof(SetFocusAction),
            new PropertyMetadata(true));

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
            UIElement element = Target;
            if (element != null)
            {
                element.Focus(Engage);
            }
        }
    }
}
