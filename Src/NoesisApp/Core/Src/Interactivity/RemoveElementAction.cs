using Noesis;
using System;

namespace NoesisApp
{
    /// <summary>
    /// An action that will remove the targeted element from the tree when invoked.
    /// </summary>
    public class RemoveElementAction : TargetedTriggerAction<FrameworkElement>
    {
        public new RemoveElementAction Clone()
        {
            return (RemoveElementAction)base.Clone();
        }

        public new RemoveElementAction CloneCurrentValue()
        {
            return (RemoveElementAction)base.CloneCurrentValue();
        }

        protected override void Invoke(object parameter)
        {
            FrameworkElement target = Target;
            if (AssociatedObject != null && target != null)
            {
                FrameworkElement parent = target.Parent;

                Panel panel = parent as Panel;
                if (panel != null)
                {
                    panel.Children.Remove(target);
                    return;
                }

                ItemsControl itemsControl = parent as ItemsControl;
                if (itemsControl != null)
                {
                    itemsControl.Items.Remove(target);
                    return;
                }

                ContentControl contentControl = parent as ContentControl;
                if (contentControl != null)
                {
                    if (contentControl.Content as FrameworkElement == target)
                    {
                        contentControl.Content = null;
                    }
                    return;
                }

                Decorator decorator = parent as Decorator;
                if (decorator != null)
                {
                    if (decorator.Child as FrameworkElement == target)
                    {
                        decorator.Child = null;
                    }
                    return;
                }

                if (parent != null)
                {
                    throw new InvalidOperationException(string.Format(
                        "RemoveElementAction: Unsupported parent type '{0}' trying to remove '{1}'",
                        parent.GetType(), target.GetType()));
                }
            }
        }
    }
}
