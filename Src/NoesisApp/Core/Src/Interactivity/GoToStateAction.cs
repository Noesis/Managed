using Noesis;
using System;

namespace NoesisApp
{
    /// <summary>
    /// An action that will transition a FrameworkElement to a specified VisualState when invoked.
    /// If the TargetName property is set, this action will attempt to change the state of the
    /// targeted element. If not, it walks the element tree in an attempt to locate an alternative
    /// target that defines states. ControlTemplate and UserControl are two common possibilities.
    /// </summary>
    public class GoToStateAction : TargetedTriggerAction<FrameworkElement>
    {
        public new GoToStateAction Clone()
        {
            return (GoToStateAction)base.Clone();
        }

        public new GoToStateAction CloneCurrentValue()
        {
            return (GoToStateAction)base.CloneCurrentValue();
        }

        /// <summary>
        /// Gets or sets the name of the visual state
        /// </summary>
        public string StateName
        {
            get { return (string)GetValue(StateNameProperty); }
            set { SetValue(StateNameProperty, value); }
        }

        public static readonly DependencyProperty StateNameProperty = DependencyProperty.Register(
            "StateName", typeof(string), typeof(GoToStateAction),
            new PropertyMetadata(string.Empty));

        /// <summary>
        /// Determines whether or not to use a VisualTransition to transition between states
        /// </summary>
        public bool UseTransitions
        {
            get { return (bool)GetValue(UseTransitionsProperty); }
            set { SetValue(UseTransitionsProperty, value); }
        }

        public static readonly DependencyProperty UseTransitionsProperty = DependencyProperty.Register(
            "UseTransitions", typeof(bool), typeof(GoToStateAction),
            new PropertyMetadata(true));

        protected override void Invoke(object parameter)
        {
            if (AssociatedObject != null && _stateTarget != null)
            {
                string stateName = StateName;
                bool useTransitions = UseTransitions;

                if (!string.IsNullOrEmpty(stateName))
                {
                    Control control = _stateTarget as Control;
                    if (control != null)
                    {
                        VisualStateManager.GoToState(control, stateName, useTransitions);
                    }
                    else
                    {
                        VisualStateManager.GoToElementState(_stateTarget, stateName, useTransitions);
                    }
                }
            }
        }

        protected override void OnTargetChanged(FrameworkElement oldTarget, FrameworkElement newTarget)
        {
            if (TargetObject == null && string.IsNullOrEmpty(TargetName))
            {
                _stateTarget = FindStateGroup(AssociatedObject as FrameworkElement);
            }
            else
            {
                _stateTarget = Target;
            }
        }

        private FrameworkElement FindStateGroup(FrameworkElement context)
        {
            if (context != null)
            {
                FrameworkElement current = context;
                FrameworkElement parent = context.Parent;

                while (!HasStateGroup(current) && ShouldWalkTree(parent))
                {
                    current = parent;
                    parent = parent.Parent;
                }

                if (HasStateGroup(current))
                {
                    FrameworkElement templatedParent = current.TemplatedParent;
                    if (templatedParent != null && templatedParent is Control)
                    {
                        return templatedParent;
                    }
                    if (parent != null && parent is UserControl)
                    {
                        return parent;
                    }

                    return current;
                }
            }

            return null;
        }

        private bool HasStateGroup(FrameworkElement element)
        {
            if (element != null)
            {
                VisualStateGroupCollection groups = VisualStateManager.GetVisualStateGroups(element);
                return groups != null && groups.Count > 0;
            }
            else
            {
                return false;
            }
        }

        private bool ShouldWalkTree(FrameworkElement element)
        {
            if (element == null)
            {
                return false;
            }
            if (element is UserControl)
            {
                return false;
            }
            if (element.Parent == null)
            {
                FrameworkElement p = element.TemplatedParent;
                if (p == null || !(p is Control || p is ContentPresenter))
                {
                    return false;
                }
            }
            return true;
        }

        FrameworkElement _stateTarget;
    }
}
