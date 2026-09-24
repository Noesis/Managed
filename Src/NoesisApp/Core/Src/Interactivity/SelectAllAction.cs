using Noesis;
using Noesis.Interactivity;
using System;

namespace NoesisGUIExtensions
{
    /// <summary>
    /// An action that will select all the text on the text control.
    /// </summary>
    public class SelectAllAction : TriggerAction<Control>
    {
        public new SelectAllAction Clone()
        {
            return (SelectAllAction)base.Clone();
        }

        public new SelectAllAction CloneCurrentValue()
        {
            return (SelectAllAction)base.CloneCurrentValue();
        }

        protected override void Invoke(object parameter)
        {
            Control control = AssociatedObject;

            TextBox textBox = control as TextBox;
            if (textBox != null)
            {
                textBox.SelectAll();
                return;
            }

            PasswordBox passwordBox = control as PasswordBox;
            if (passwordBox != null)
            {
                passwordBox.SelectAll();
            }
        }
    }
}
