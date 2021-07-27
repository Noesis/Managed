using Noesis;
using System;

namespace NoesisApp
{
    public enum ControlStoryboardOption
    {
        Play,
        Stop,
        TogglePlayPause,
        Pause,
        Resume,
        SkipToFill
    }

    /// <summary>
    /// An action that will change the state of a targeted storyboard when invoked.
    /// </summary>
    public class ControlStoryboardAction : StoryboardAction
    {
        public new ControlStoryboardAction Clone()
        {
            return (ControlStoryboardAction)base.Clone();
        }

        public new ControlStoryboardAction CloneCurrentValue()
        {
            return (ControlStoryboardAction)base.CloneCurrentValue();
        }

        /// <summary>
        /// Gets or sets the targeted Storyboard
        /// </summary>
        public ControlStoryboardOption ControlStoryboardOption
        {
            get { return (ControlStoryboardOption)GetValue(ControlStoryboardOptionProperty); }
            set { SetValue(ControlStoryboardOptionProperty, value); }
        }

        public static readonly DependencyProperty ControlStoryboardOptionProperty = DependencyProperty.Register(
            "ControlStoryboardOption", typeof(ControlStoryboardOption), typeof(ControlStoryboardAction),
            new PropertyMetadata(ControlStoryboardOption.Play));

        protected override void Invoke(object parameter)
        {
            Storyboard storyboard = Storyboard;
            DependencyObject associatedObject = AssociatedObject;
            if (associatedObject != null && storyboard != null)
            {
                // First try with the scope where Storyboard was defined as target
                FrameworkElement target = FrameworkElement.FindTreeElement(storyboard);
                if (target == null || target.View == null)
                {
                    // In case Storyboard was defined in Application resources or the Resources of a
                    // template element, we next try with the scope of the associated object as target
                    target = FrameworkElement.FindTreeElement(associatedObject);
                }

                // Execute the Storyboard action if we have a valid target connected to the UI tree
                if (target != null && target.View != null)
                {
                    switch (ControlStoryboardOption)
                    {
                        case ControlStoryboardOption.Play:
                        {
                            FrameworkElement ns = FrameworkElement.FindTreeElement(associatedObject);
                            storyboard.Begin(target, ns, true);
                            break;
                        }
                        case ControlStoryboardOption.Stop:
                        {
                            storyboard.Stop(target);
                            break;
                        }
                        case ControlStoryboardOption.TogglePlayPause:
                        {
                            if (storyboard.IsPlaying(target))
                            {
                                if (storyboard.IsPaused(target))
                                {
                                    storyboard.Resume(target);
                                }
                                else
                                {
                                    storyboard.Pause(target);
                                }
                            }
                            else
                            {
                                FrameworkElement ns = FrameworkElement.FindTreeElement(associatedObject);
                                storyboard.Begin(target, ns, true);
                            }
                            break;
                        }
                        case ControlStoryboardOption.Pause:
                        {
                            storyboard.Pause(target);
                            break;
                        }
                        case ControlStoryboardOption.Resume:
                        {
                            storyboard.Resume(target);
                            break;
                        }
                        case ControlStoryboardOption.SkipToFill:
                        {
                            throw new NotImplementedException("Storyboard.SkipToFill");
                        }
                    }
                }
            }
        }
    }
}
