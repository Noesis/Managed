using Noesis;
using System;

namespace NoesisApp
{
    public enum KeyTriggerFiredOn
    {
        KeyDown,
        KeyUp
    }

    /// <summary>
    /// A Trigger that is triggered by a keyboard event. If Key and Modifiers are detected, it fires.
    /// </summary>
    public class KeyTrigger : TriggerBase<UIElement>
    {
        public new KeyTrigger Clone()
        {
            return (KeyTrigger)base.Clone();
        }

        public new KeyTrigger CloneCurrentValue()
        {
            return (KeyTrigger)base.CloneCurrentValue();
        }

        /// <summary>
        /// The key that must be pressed for the trigger to fire
        /// </summary>
        public Key Key
        {
            get { return (Key)GetValue(KeyProperty); }
            set { SetValue(KeyProperty, value); }
        }

        public static readonly DependencyProperty KeyProperty = DependencyProperty.Register(
            "Key", typeof(Key), typeof(KeyTrigger),
            new PropertyMetadata(Key.None));

        /// <summary>
        /// The modifiers that must be active for the trigger to fire
        /// </summary>
        public ModifierKeys Modifiers
        {
            get { return (ModifierKeys)GetValue(ModifiersProperty); }
            set { SetValue(ModifiersProperty, value); }
        }

        public static readonly DependencyProperty ModifiersProperty = DependencyProperty.Register(
            "Modifiers", typeof(ModifierKeys), typeof(KeyTrigger),
            new PropertyMetadata(ModifierKeys.None));

        /// <summary>
        /// If true, the Trigger only listens to its trigger Source object, which means that element
        /// must have focus for the trigger to fire. If false, the Trigger listens at the root, so any
        /// unhandled KeyDown/Up messages will be caught
        /// </summary>
        public bool ActiveOnFocus
        {
            get { return (bool)GetValue(ActiveOnFocusProperty); }
            set { SetValue(ActiveOnFocusProperty, value); }
        }

        public static readonly DependencyProperty ActiveOnFocusProperty = DependencyProperty.Register(
            "ActiveOnFocus", typeof(bool), typeof(KeyTrigger),
            new PropertyMetadata(false, OnActiveOnFocusChanged));

        static void OnActiveOnFocusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            KeyTrigger trigger = (KeyTrigger)d;
            trigger.UnregisterSource(trigger.FiredOn);
            trigger.RegisterSource();
        }

        /// <summary>
        /// Determines whether or not to listen to the KeyDown or KeyUp event
        /// </summary>
        public KeyTriggerFiredOn FiredOn
        {
            get { return (KeyTriggerFiredOn)GetValue(FiredOnProperty); }
            set { SetValue(FiredOnProperty, value); }
        }

        public static readonly DependencyProperty FiredOnProperty = DependencyProperty.Register(
            "FiredOn", typeof(KeyTriggerFiredOn), typeof(KeyTrigger),
            new PropertyMetadata(KeyTriggerFiredOn.KeyDown, OnFiredOnChanged));

        static void OnFiredOnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            KeyTrigger trigger = (KeyTrigger)d;
            trigger.UnregisterSource((KeyTriggerFiredOn)e.OldValue);
            trigger.RegisterSource();
        }

        protected override void OnAttached()
        {
            base.OnAttached();

            RegisterSource();
        }

        protected override void OnDetaching()
        {
            UnregisterSource(FiredOn);

            base.OnDetaching();
        }

        private void OnKeyPress(object sender, KeyEventArgs e)
        {
            UIElement source = (UIElement)GetProxy(_source);
            if (Key == e.Key && Modifiers == source.Keyboard.Modifiers)
            {
                InvokeActions(0);
            }
        }

        private void RegisterSource()
        {
            UIElement source = ActiveOnFocus ? AssociatedObject : GetRoot(AssociatedObject);
            if (source != null)
            {
                if (FiredOn == KeyTriggerFiredOn.KeyDown)
                {
                    source.KeyDown += OnKeyPress;
                }
                else
                {
                    source.KeyUp += OnKeyPress;
                }

                source.Destroyed += OnSourceDestroyed;
            }

            _source = GetPtr(source);
        }

        private void UnregisterSource(KeyTriggerFiredOn firedOn)
        {
            UIElement source = (UIElement)GetProxy(_source);
            if (source != null)
            {
                source.Destroyed -= OnSourceDestroyed;

                if (firedOn == KeyTriggerFiredOn.KeyDown)
                {
                    source.KeyDown -= OnKeyPress;
                }
                else
                {
                    source.KeyUp -= OnKeyPress;
                }
            }

            _source = IntPtr.Zero;
        }

        private void OnSourceDestroyed(IntPtr d)
        {
            _source = IntPtr.Zero;
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

        private IntPtr _source = IntPtr.Zero;
    }
}
