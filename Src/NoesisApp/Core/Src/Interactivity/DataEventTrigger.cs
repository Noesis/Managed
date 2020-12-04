// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

using Noesis;
using NoesisApp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace NoesisGUIExtensions
{
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
