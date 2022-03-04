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
    public class MoveFocusAction : NoesisApp.TriggerAction<Noesis.UIElement>
    {
        /// <summary>
        /// Direction to look for the next control to get focus.
        /// </summary>
        public FocusDirection Direction
        {
            get { return (FocusDirection)GetValue(DirectionProperty); }
            set { SetValue(DirectionProperty, value); }
        }

        public static readonly Noesis.DependencyProperty DirectionProperty = Noesis.DependencyProperty.Register(
            "Direction", typeof(FocusDirection), typeof(MoveFocusAction),
            new Noesis.PropertyMetadata(FocusDirection.Left));

        /// <summary>
        /// A value indicating if control should be engaged when getting the focus. Default is true.
        /// </summary>
        public bool Engage
        {
            get { return (bool)GetValue(EngageProperty); }
            set { SetValue(EngageProperty, value); }
        }

        public static readonly Noesis.DependencyProperty EngageProperty = Noesis.DependencyProperty.Register(
            "Engage", typeof(bool), typeof(MoveFocusAction),
            new Noesis.PropertyMetadata(true));

        protected override void Invoke(object parameter)
        {
            Noesis.UIElement element = AssociatedObject;
            if (element != null)
            {
                Noesis.Keyboard keyboard = element.Keyboard;
                if (keyboard != null)
                {
                    Noesis.UIElement source = keyboard.FocusedElement ?? element;

                    int direction = (int)Noesis.FocusNavigationDirection.Left + (int)Direction;
                    Noesis.UIElement target = (Noesis.UIElement)source.PredictFocus((Noesis.FocusNavigationDirection)direction);

                    if (target != null)
                    {
                        target.Focus(Engage);
                    }
                }
            }
        }
    }
}
