using System;

namespace NoesisGUIExtensions
{
    /// <summary>
    /// An action that will fire a trigger in a RiveControl.
    /// </summary>
    public class RiveTriggerAction : NoesisApp.TargetedTriggerAction<Noesis.RiveControl>
    {
        /// <summary>
        /// Gets or sets the name of the trigger in the Rive scene
        /// </summary>
        public string TriggerName
        {
            get { return (string)GetValue(TriggerNameProperty); }
            set { SetValue(TriggerNameProperty, value); }
        }

        public static readonly Noesis.DependencyProperty TriggerNameProperty = Noesis.DependencyProperty.Register(
            "TriggerName", typeof(string), typeof(RiveTriggerAction),
            new Noesis.PropertyMetadata(string.Empty));

        public new RiveTriggerAction Clone()
        {
            return (RiveTriggerAction)base.Clone();
        }

        public new RiveTriggerAction CloneCurrentValue()
        {
            return (RiveTriggerAction)base.CloneCurrentValue();
        }

        protected override void Invoke(object parameter)
        {
            Noesis.RiveControl rive = Target;
            if (rive != null)
            {
                rive.FireInputTrigger(TriggerName);
            }
        }
    }
}
