using Noesis;
using Noesis.Interactivity;
using System;

namespace NoesisGUIExtensions
{
    public enum FocusDirection
    {
        Left,
        Right,
        Up,
        Down
    }

    /// <summary>
    /// An action that will try to move the focus to the control positioned in the specified direction.
    /// </summary>
    public class MoveFocusAction : TriggerAction<UIElement>
    {
        /// <summary>
        /// Direction to look for the next control to get focus.
        /// </summary>
        public FocusDirection Direction
        {
            get { return (FocusDirection)GetValue(DirectionProperty); }
            set { SetValue(DirectionProperty, value); }
        }

        public static readonly DependencyProperty DirectionProperty = DependencyProperty.Register(
            "Direction", typeof(FocusDirection), typeof(MoveFocusAction),
            new PropertyMetadata(FocusDirection.Left));

        /// <summary>
        /// A value indicating if control should be engaged when getting the focus. Default is true.
        /// </summary>
        public bool Engage
        {
            get { return (bool)GetValue(EngageProperty); }
            set { SetValue(EngageProperty, value); }
        }

        public static readonly DependencyProperty EngageProperty = DependencyProperty.Register(
            "Engage", typeof(bool), typeof(MoveFocusAction),
            new PropertyMetadata(true));

        protected override void Invoke(object parameter)
        {
            UIElement element = AssociatedObject;
            if (element != null)
            {
                Keyboard keyboard = element.Keyboard;
                if (keyboard != null)
                {
                    UIElement source = keyboard.FocusedElement ?? element;

                    int direction = (int)FocusNavigationDirection.Left + (int)Direction;
                    UIElement target = (UIElement)source.PredictFocus((FocusNavigationDirection)direction);

                    if (target != null)
                    {
                        target.Focus(Engage);
                    }
                }
            }
        }
    }
}
