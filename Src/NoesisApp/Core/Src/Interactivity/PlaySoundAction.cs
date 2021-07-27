using Noesis;
using System;

namespace NoesisApp
{
    /// <summary>
    /// An action that will play a sound to completion.
    /// </summary>
    public class PlaySoundAction : TriggerAction<DependencyObject>
    {
        public new PlaySoundAction Clone()
        {
            return (PlaySoundAction)base.Clone();
        }

        public new PlaySoundAction CloneCurrentValue()
        {
            return (PlaySoundAction)base.CloneCurrentValue();
        }

        /// <summary>
        /// Gets or sets the URI that defines the location of the sound file
        /// </summary>
        public Uri Source
        {
            get { return (Uri)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(
            "Source", typeof(Uri), typeof(PlaySoundAction),
            new PropertyMetadata(null));

        /// <summary>
        /// Controls the volume of the sound
        /// </summary>
        public double Volume
        {
            get { return (double)GetValue(VolumeProperty); }
            set { SetValue(VolumeProperty, value); }
        }

        public static readonly DependencyProperty VolumeProperty = DependencyProperty.Register(
            "Volume", typeof(double), typeof(PlaySoundAction),
            new PropertyMetadata(0.5));

        protected override void Invoke(object parameter)
        {
            Uri source = Source;
            if (AssociatedObject != null && source != null &&
                !string.IsNullOrEmpty(source.OriginalString))
            {
                GUI.PlayAudio(source, (float)Volume);
            }
        }
    }
}
