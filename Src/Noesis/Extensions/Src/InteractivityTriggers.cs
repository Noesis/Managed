////////////////////////////////////////////////////////////////////////////////////////////////////
// Noesis Engine - http://www.noesisengine.com
// Copyright (c) 2009-2010 Noesis Technologies S.L. All Rights Reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Xaml.Behaviors;

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
    public class GamepadTrigger : TriggerBase<UIElement>
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

            _source = ActiveOnFocus ? AssociatedObject : GetRoot(AssociatedObject);

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
            const int GamepadLeft = 175;
            if (Button == (GamepadButton)((int)e.Key - GamepadLeft))
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

    /// <summary>
    /// Trigger which fires when a CLR event is raised on an object. Can be used to trigger from
    /// events on the data context, as opposed to a standard EventTrigger which uses routed events
    /// on a FrameworkElement.
    /// </summary>
    public class DataEventTrigger : TriggerBase<FrameworkElement>
    {
        /// <summary>
        /// The source object for the event
        /// </summary>
        public object Source
        {
            get { return this.GetValue(DataEventTrigger.SourceProperty); }
            set { this.SetValue(DataEventTrigger.SourceProperty, value); }
        }

        /// <summary>Backing DP for the Source property</summary>
        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(
            "Source", typeof(object), typeof(DataEventTrigger),
            new PropertyMetadata(null, DataEventTrigger.OnSourceChanged));

        private static void OnSourceChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            ((DataEventTrigger)sender).UpdateHandler();
        }

        /// <summary>
        /// The name of the event which triggers this
        /// </summary>
        public string EventName
        {
            get { return (string)this.GetValue(DataEventTrigger.EventNameProperty); }
            set { this.SetValue(DataEventTrigger.EventNameProperty, value); }
        }

        /// <summary>Backing DP for the EventName property</summary>
        public static readonly DependencyProperty EventNameProperty = DependencyProperty.Register(
            "EventName", typeof(string), typeof(DataEventTrigger),
            new PropertyMetadata(null, DataEventTrigger.OnEventNameChanged));

        private static void OnEventNameChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            ((DataEventTrigger)sender).UpdateHandler();
        }

        private void UpdateHandler()
        {
            if (this.currentEvent != null)
            {
                this.currentEvent.RemoveEventHandler(this.currentTarget, this.currentDelegate);

                this.currentEvent = null;
                this.currentTarget = null;
                this.currentDelegate = null;
            }

            this.currentTarget = this.Source;

            if (this.currentTarget != null && !string.IsNullOrEmpty(this.EventName))
            {

                Type targetType = this.currentTarget.GetType();
                this.currentEvent = targetType.GetEvent(this.EventName);
                if (this.currentEvent != null)
                {

                    MethodInfo handlerMethod = this.GetType().GetMethod("OnEvent", BindingFlags.NonPublic | BindingFlags.Instance);
                    this.currentDelegate = this.GetDelegate(this.currentEvent, this.OnMethod);
                    this.currentEvent.AddEventHandler(this.currentTarget, this.currentDelegate);
                }
            }
        }

        private Delegate GetDelegate(EventInfo eventInfo, Action action)
        {
            if (typeof(System.EventHandler).IsAssignableFrom(eventInfo.EventHandlerType))
            {
                MethodInfo method = this.GetType().GetMethod("OnEvent", BindingFlags.NonPublic | BindingFlags.Instance);
                return Delegate.CreateDelegate(eventInfo.EventHandlerType, this, method);
            }

            Type handlerType = eventInfo.EventHandlerType;
            ParameterInfo[] eventParams = handlerType.GetMethod("Invoke").GetParameters();

            IEnumerable<ParameterExpression> parameters = eventParams.Select(p => System.Linq.Expressions.Expression.Parameter(p.ParameterType, "x"));

            MethodCallExpression methodExpression = System.Linq.Expressions.Expression.Call(System.Linq.Expressions.Expression.Constant(action), action.GetType().GetMethod("Invoke"));
            LambdaExpression lambdaExpression = System.Linq.Expressions.Expression.Lambda(methodExpression, parameters.ToArray());
            return Delegate.CreateDelegate(handlerType, lambdaExpression.Compile(), "Invoke", false);
        }

        private void OnMethod()
        {
            this.InvokeActions(null);
        }

        private void OnEvent(object sender, System.EventArgs e)
        {
            this.InvokeActions(e);
        }

        private EventInfo currentEvent;
        private Delegate currentDelegate;
        private object currentTarget;
    }
}
