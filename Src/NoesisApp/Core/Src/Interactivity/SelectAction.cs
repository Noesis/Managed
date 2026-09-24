using Noesis;
using Noesis.Interactivity;
using System;

namespace NoesisGUIExtensions
{
    /// <summary>
    /// An action that wiil set Selector.IsSelected property to true on the asssociated object.
    /// </summary>
    public class SelectAction : TriggerAction<Control>
    {
        public new SelectAction Clone()
        {
            return (SelectAction)base.Clone();
        }

        public new SelectAction CloneCurrentValue()
        {
            return (SelectAction)base.CloneCurrentValue();
        }

        protected override void Invoke(object parameter)
        {
            Control control = AssociatedObject;
            if (control != null)
            {
                Selector.SetIsSelected(control, true);
            }
        }
    }
}
