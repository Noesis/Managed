using NoesisApp;
using Noesis.Interactivity;
using System;

namespace NoesisGUIExtensions
{
    /// <summary>
    /// Play a media element.
    /// </summary>
    public class PlayMediaAction : TargetedTriggerAction<MediaElement>
    {
        protected override void Invoke(object o)
        {
            MediaElement element = Target;
            if (element != null)
            {
                element.Play();
            }
        }
    }

    /// <summary>
    /// Pause a media element.
    /// </summary>
    public class PauseMediaAction : TargetedTriggerAction<MediaElement>
    {
        protected override void Invoke(object o)
        {
            MediaElement element = Target;
            if (element != null)
            {
                element.Pause();
            }
        }
    }

    /// <summary>
    /// Seeks a media element to position 0.
    /// </summary>
    public class RewindMediaAction : TargetedTriggerAction<MediaElement>
    {
        protected override void Invoke(object o)
        {
            MediaElement element = Target;
            if (element != null)
            {
                element.Position = TimeSpan.Zero;
            }
        }
    }

    /// <summary>
    /// Stops a media element.
    /// </summary>
    public class StopMediaAction : TargetedTriggerAction<MediaElement>
    {
        protected override void Invoke(object o)
        {
            MediaElement element = Target;
            if (element != null)
            {
                element.Stop();
            }
        }
    }
}
