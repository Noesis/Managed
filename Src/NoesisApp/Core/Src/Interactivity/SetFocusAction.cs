using System;

namespace NoesisGUIExtensions
{
    /// <summary>
    /// An action that will try to focus the associated element.
    /// </summary>
    public class SetFocusAction : NoesisApp.TargetedTriggerAction<Noesis.UIElement>
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

        public static readonly Noesis.DependencyProperty EngageProperty = Noesis.DependencyProperty.Register(
            "Engage", typeof(bool), typeof(SetFocusAction),
            new Noesis.PropertyMetadata(true));

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
                element.Focus(Engage);
            }
        }
    }
}
