using System;

namespace NoesisGUIExtensions
{
    /// <summary>
    /// Play a media element.
    /// </summary>
    public class PlayMediaAction : NoesisApp.TargetedTriggerAction<NoesisApp.MediaElement>
    {
        protected override void Invoke(object o)
        {
            NoesisApp.MediaElement element = Target;
            if (element != null)
            {
                element.Play();
            }
        }
    }

    /// <summary>
    /// Pause a media element.
    /// </summary>
    public class PauseMediaAction : NoesisApp.TargetedTriggerAction<NoesisApp.MediaElement>
    {
        protected override void Invoke(object o)
        {
            NoesisApp.MediaElement element = Target;
            if (element != null)
            {
                element.Pause();
            }
        }
    }

    /// <summary>
    /// Seeks a media element to position 0.
    /// </summary>
    public class RewindMediaAction : NoesisApp.TargetedTriggerAction<NoesisApp.MediaElement>
    {
        protected override void Invoke(object o)
        {
            NoesisApp.MediaElement element = Target;
            if (element != null)
            {
                element.Position = TimeSpan.Zero;
            }
        }
    }

    /// <summary>
    /// Stops a media element.
    /// </summary>
    public class StopMediaAction : NoesisApp.TargetedTriggerAction<NoesisApp.MediaElement>
    {
        protected override void Invoke(object o)
        {
            NoesisApp.MediaElement element = Target;
            if (element != null)
            {
                element.Stop();
            }
        }
    }
}
