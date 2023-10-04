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

        /// Returns the element that is used as target to interact with the Storyboard
        public static FrameworkElement FindStoryboardTarget(Storyboard storyboard,
            DependencyObject associatedObject)
        {
            // For Storyboards defined in the Template Resources use the associated object
            if (FrameworkElement.FindTreeParent(storyboard) is ResourceDictionary resources)
            {
                if (FrameworkElement.FindTreeParent(resources) is FrameworkTemplate)
                {
                    return FrameworkElement.FindTreeElement(associatedObject);
                }
            }

            // Next try with the scope where Storyboard was defined
            FrameworkElement target = FrameworkElement.FindTreeElement(storyboard);
            if (target != null && target.View != null)
            {
                return target;
            }

            // In case Storyboard was defined in Application resources or the Resources of a
            // template element, we next try with the scope of the associated object
            return FrameworkElement.FindTreeElement(associatedObject);
        }
    }
}
