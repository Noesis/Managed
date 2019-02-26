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
            if (AssociatedObject != null && storyboard != null)
            {
                switch (ControlStoryboardOption)
                {
                    case ControlStoryboardOption.Play:
                    {
                        storyboard.Begin();
                        break;
                    }
                    case ControlStoryboardOption.Stop:
                    {
                        storyboard.Stop();
                        break;
                    }
                    case ControlStoryboardOption.TogglePlayPause:
                    {
                        if (storyboard.IsPlaying())
                        {
                            if (storyboard.IsPaused())
                            {
                                storyboard.Resume();
                            }
                            else
                            {
                                storyboard.Pause();
                            }
                        }
                        else
                        {
                            storyboard.Begin();
                        }
                        break;
                    }
                    case ControlStoryboardOption.Pause:
                    {
                        storyboard.Pause();
                        break;
                    }
                    case ControlStoryboardOption.Resume:
                    {
                        storyboard.Resume();
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
