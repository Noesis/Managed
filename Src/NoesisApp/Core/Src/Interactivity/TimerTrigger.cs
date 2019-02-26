using Noesis;

namespace NoesisApp
{
    /// <summary>
    /// A trigger that is triggered by a specified event and fires after a delay.
    /// </summary>
    public class TimerTrigger : EventTrigger
    {
        public new TimerTrigger Clone()
        {
            return (TimerTrigger)base.Clone();
        }

        public new TimerTrigger CloneCurrentValue()
        {
            return (TimerTrigger)base.CloneCurrentValue();
        }

        /// <summary>
        /// Gets or sets the number of milliseconds to wait between ticks
        /// </summary>
        public int MillisecondsPerTick
        {
            get { return (int)GetValue(MillisecondsPerTickProperty); }
            set { SetValue(MillisecondsPerTickProperty, value); }
        }

        public static readonly DependencyProperty MillisecondsPerTickProperty = DependencyProperty.Register(
            "MillisecondsPerTick", typeof(int), typeof(TimerTrigger),
            new PropertyMetadata(1000));

        /// <summary>
        /// Gets or sets the total number of ticks to be fired before the trigger is finished
        /// </summary>
        public int TotalTicks
        {
            get { return (int)GetValue(TotalTicksProperty); }
            set { SetValue(TotalTicksProperty, value); }
        }

        public static readonly DependencyProperty TotalTicksProperty = DependencyProperty.Register(
            "TotalTicks", typeof(int), typeof(TimerTrigger),
            new PropertyMetadata(-1));

        protected override void OnDetaching()
        {
            StopTimer();

            base.OnDetaching();
        }

        protected override void OnEvent(object parameter)
        {
            StopTimer();
            _tickCount = 0;
            StartTimer();
        }

        private void StartTimer()
        {
            if (View != null)
            {
                int nextTick = MillisecondsPerTick;
                _timer = View.CreateTimer(nextTick, OnTimerTick);
            }
        }

        private void StopTimer()
        {
            if (_timer >= 0)
            {
                if (View != null)
                {
                    View.CancelTimer(_timer);
                }
                _timer = -1;
            }
        }

        private int OnTimerTick()
        {
            int nextTick = MillisecondsPerTick;
            int totalTicks = TotalTicks;
            if (totalTicks > 0 && ++_tickCount >= totalTicks)
            {
                nextTick = 0; // stop timer
                _timer = -1;
            }

            InvokeActions(null);

            return nextTick;
        }

        private int _timer = -1;
        private int _tickCount = 0;
    }
}
