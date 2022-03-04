using Noesis;
using System;

namespace NoesisApp
{
    /// <summary>
    /// A trigger that listens for a specified event on its source and fires when that event is fired.
    /// </summary>
    public class EventTrigger : EventTriggerBase<object>
    {
        public EventTrigger()
        {
        }

        public EventTrigger(string eventName)
        {
            EventName = eventName;
        }

        public new EventTrigger Clone()
        {
            return (EventTrigger)base.Clone();
        }

        public new EventTrigger CloneCurrentValue()
        {
            return (EventTrigger)base.CloneCurrentValue();
        }

        /// <summary>
        /// Gets or sets the name of the event to listen for
        /// </summary>
        public string EventName
        {
            get { return (string)GetValue(EventNameProperty); }
            set { SetValue(EventNameProperty, value); }
        }

        public static readonly DependencyProperty EventNameProperty = DependencyProperty.Register(
            "EventName", typeof(string), typeof(EventTrigger),
            new PropertyMetadata("Loaded", OnEventNamePropertyChanged));

        static void OnEventNamePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            EventTrigger trigger = (EventTrigger)d;
            trigger.OnEventNameChanged((string)e.OldValue, (string)e.NewValue);
        }

        protected override string GetEventName()
        {
            return EventName;
        }
    }
}
