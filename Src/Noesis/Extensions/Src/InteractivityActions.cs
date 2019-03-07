////////////////////////////////////////////////////////////////////////////////////////////////////
// Noesis Engine - http://www.noesisengine.com
// Copyright (c) 2009-2010 Noesis Technologies S.L. All Rights Reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Interactivity;

namespace NoesisGUIExtensions
{
    public class SetFocusAction : TargetedTriggerAction<UIElement>
    {
        protected override void Invoke(object parameter)
        {
            UIElement element = Target;
            if (element != null)
            {
                element.Focus();
            }
        }
    }

    public class SelectAction : TriggerAction<Control>
    {
        protected override void Invoke(object parameter)
        {
            Control control = AssociatedObject;
            if (control != null)
            {
                Selector.SetIsSelected(control, true);
            }
        }
    }

    public class SelectAllAction : TriggerAction<Control>
    {
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
