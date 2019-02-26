using System;

namespace NoesisGUIExtensions
{
    /// <summary>
    /// An action that will select all the text on the text control.
    /// </summary>
    public class SelectAllAction : NoesisApp.TriggerAction<Noesis.Control>
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
            Noesis.Control control = AssociatedObject;

            Noesis.TextBox textBox = control as Noesis.TextBox;
            if (textBox != null)
            {
                textBox.SelectAll();
                return;
            }

            Noesis.PasswordBox passwordBox = control as Noesis.PasswordBox;
            if (passwordBox != null)
            {
                passwordBox.SelectAll();
            }
        }
    }
}
