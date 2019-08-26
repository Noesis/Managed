using Noesis;
using System;

namespace NoesisGUIExtensions
{
    public enum GamepadTriggerFiredOn
    {
        ButtonDown,
        ButtonUp
    }

    public enum GamepadButton
    {
        Left,
        Up,
        Right,
        Down,
        Accept,
        Cancel,
        Menu,
        View,
        PageUp,
        PageDown,
        PageLeft,
        PageRight,
        Context1,
        Context2,
        Context3,
        Context4,
    }

    /// <summary>
    /// A Trigger that is triggered by a specified gamepad button.
    /// </summary>
    public class GamepadTrigger : NoesisApp.TriggerBase<UIElement>
    {
        public new GamepadTrigger Clone()
        {
            return (GamepadTrigger)base.Clone();
        }

        public new GamepadTrigger CloneCurrentValue()
        {
            return (GamepadTrigger)base.CloneCurrentValue();
        }

        /// <summary>
        /// The button that must be pressed or released for the trigger to fire
        /// </summary>
        public GamepadButton Button
        {
            get { return (GamepadButton)GetValue(ButtonProperty); }
            set { SetValue(ButtonProperty, value); }
        }

        public static readonly DependencyProperty ButtonProperty = DependencyProperty.Register(
            "Button", typeof(GamepadButton), typeof(GamepadTrigger),
            new PropertyMetadata(GamepadButton.Accept));

        /// <summary>
        /// If true, the Trigger only listens to its trigger Source object, which means that element
        /// must have focus for the trigger to fire. If false, the Trigger listens at the root, so
        /// any unhandled KeyDown and KeyUp messages will be caught
        /// </summary>
        public bool ActiveOnFocus
        {
            get { return (bool)GetValue(ActiveOnFocusProperty); }
            set { SetValue(ActiveOnFocusProperty, value); }
        }

        public static readonly DependencyProperty ActiveOnFocusProperty = DependencyProperty.Register(
            "ActiveOnFocus", typeof(bool), typeof(GamepadTrigger),
            new PropertyMetadata(false));

        /// <summary>
        /// Determines if trigger happens when gamepad button is pressed or released
        /// </summary>
        public GamepadTriggerFiredOn FiredOn
        {
            get { return (GamepadTriggerFiredOn)GetValue(FiredOnProperty); }
            set { SetValue(FiredOnProperty, value); }
        }

        public static readonly DependencyProperty FiredOnProperty = DependencyProperty.Register(
            "FiredOn", typeof(GamepadTriggerFiredOn), typeof(GamepadTrigger),
            new PropertyMetadata(GamepadTriggerFiredOn.ButtonDown));

        protected override void OnAttached()
        {
            base.OnAttached();

            _source = ActiveOnFocus ? AssociatedObject: GetRoot(AssociatedObject);

            if (_source != null)
            {
                if (FiredOn == GamepadTriggerFiredOn.ButtonDown)
                {
                    _source.KeyDown += OnButtonPress;
                }
                else
                {
                    _source.KeyUp += OnButtonPress;
                }
            }
        }

        protected override void OnDetaching()
        {
            if (_source != null)
            {
                if (FiredOn == GamepadTriggerFiredOn.ButtonDown)
                {
                    _source.KeyDown -= OnButtonPress;
                }
                else
                {
                    _source.KeyUp -= OnButtonPress;
                }

                _source = null;
            }

            base.OnDetaching();
        }

        private void OnButtonPress(object sender, KeyEventArgs e)
        {
            if (Button == (GamepadButton)(e.Key - Key.GamepadLeft))
            {
                InvokeActions(0);
            }
        }

        private UIElement GetRoot(Visual current)
        {
            UIElement root = null;
            while (current != null)
            {
                root = current as UIElement;
                current = (Visual)VisualTreeHelper.GetParent(current);
            }
            return root;
        }

        private UIElement _source;
    }
}
