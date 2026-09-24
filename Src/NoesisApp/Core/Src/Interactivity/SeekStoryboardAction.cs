using Noesis;
using Noesis.Interactivity;
using System;

namespace NoesisGUIExtensions
{
    /// <summary>
    /// An action that will change the state of a targeted storyboard when invoked.
    /// </summary>
    public class SeekStoryboardAction : StoryboardAction
    {
        /// <summary>
        /// Gets or sets the amount (positive or negative) by which the storyboard should move
        /// forward or backward from the seek origin. The default value is 0
        /// </summary>
        public TimeSpan Offset
        {
            get { return (TimeSpan)GetValue(OffsetProperty); }
            set { SetValue(OffsetProperty, value); }
        }

        public static readonly DependencyProperty OffsetProperty = DependencyProperty.Register(
            "Offset", typeof(TimeSpan), typeof(SeekStoryboardAction),
            new PropertyMetadata(TimeSpan.Zero));

        /// <summary>
        /// Gets or sets the position from which this seek operation's offset is applied. The
        /// default value is BeginTime, the beginning of the storyboard
        /// </summary>
        public TimeSeekOrigin Origin
        {
            get { return (TimeSeekOrigin)GetValue(OriginProperty); }
            set { SetValue(OriginProperty, value); }
        }

        public static readonly DependencyProperty OriginProperty = DependencyProperty.Register(
            "Origin", typeof(TimeSeekOrigin), typeof(SeekStoryboardAction),
            new PropertyMetadata(TimeSeekOrigin.BeginTime));

        public new SeekStoryboardAction Clone()
        {
            return (SeekStoryboardAction)base.Clone();
        }

        public new SeekStoryboardAction CloneCurrentValue()
        {
            return (SeekStoryboardAction)base.CloneCurrentValue();
        }

        protected override void CloneCommonCore(Freezable source)
        {
            base.CloneCommonCore(source);

            SeekStoryboardAction action = (SeekStoryboardAction)source;
            Storyboard storyboard = action.Storyboard;
            if (storyboard != null &&
                FrameworkElement.FindTreeParent(storyboard) is ResourceDictionary)
            {
                // use original Storyboard resource, not a clone
                Storyboard = storyboard;
            }
        }

        protected override void Invoke(object parameter)
        {
            Storyboard storyboard = Storyboard;
            DependencyObject associatedObject = AssociatedObject;
            if (associatedObject != null && storyboard != null)
            {
                FrameworkElement target = FindStoryboardTarget(storyboard, associatedObject);

                // Execute the Storyboard action if we have a valid target connected to the UI tree
                if (target != null && target.View != null)
                {
                    storyboard.Seek(target, Offset, Origin);
                }
            }
        }
    }
}
