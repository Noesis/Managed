using Noesis;
using System;

namespace NoesisApp
{
    /// <summary>
    /// An abstract action class that provides the ability to target a Storyboard.
    /// </summary>
    public abstract class StoryboardAction : TriggerAction<DependencyObject>
    {
        public new StoryboardAction Clone()
        {
            return (StoryboardAction)base.Clone();
        }

        public new StoryboardAction CloneCurrentValue()
        {
            return (StoryboardAction)base.CloneCurrentValue();
        }

        /// <summary>
        /// Gets or sets the targeted Storyboard
        /// </summary>
        public Storyboard Storyboard
        {
            get { return (Storyboard)GetValue(StoryboardProperty); }
            set { SetValue(StoryboardProperty, value); }
        }

        public static readonly DependencyProperty StoryboardProperty = DependencyProperty.Register(
            "Storyboard", typeof(Storyboard), typeof(StoryboardAction),
            new PropertyMetadata(null, OnStoryboardChanged));

        private static void OnStoryboardChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            StoryboardAction action = (StoryboardAction)d;
            action.OnStoryboardChanged(e);
        }

        /// <summary>
        /// Called when the Storyboard property changes
        /// </summary>
        protected virtual void OnStoryboardChanged(DependencyPropertyChangedEventArgs e)
        {
        }
    }
}
