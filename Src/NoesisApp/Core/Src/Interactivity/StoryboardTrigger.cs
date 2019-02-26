using Noesis;
using System;
using System.Reflection;

namespace NoesisApp
{
    /// <summary>
    /// An abstract trigger class that provides the ability to target a Storyboard.
    /// </summary>
    public abstract class StoryboardTrigger : TriggerBase<DependencyObject>
    {
        public new StoryboardTrigger Clone()
        {
            return (StoryboardTrigger)base.Clone();
        }

        public new StoryboardTrigger CloneCurrentValue()
        {
            return (StoryboardTrigger)base.CloneCurrentValue();
        }

        /// <summary>
        /// The key that must be pressed for the trigger to fire
        /// </summary>
        public Storyboard Storyboard
        {
            get { return (Storyboard)GetValue(StoryboardProperty); }
            set { SetValue(StoryboardProperty, value); }
        }

        public static readonly DependencyProperty StoryboardProperty = DependencyProperty.Register(
            "Storyboard", typeof(Storyboard), typeof(StoryboardTrigger),
            new PropertyMetadata(null, OnStoryboardChanged));

        private static void OnStoryboardChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            StoryboardTrigger trigger = (StoryboardTrigger)d;
            trigger.OnStoryboardChanged(e);
        }

        /// <summary>
        /// Called when the storyboard property has changed
        /// </summary>
        protected virtual void OnStoryboardChanged(DependencyPropertyChangedEventArgs e)
        {
        }
    }
}
