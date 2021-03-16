using System;

namespace NoesisApp
{
    public delegate void BufferingStartedHandler();
    public delegate void BufferingEndedHandler();
    public delegate void MediaFailedHandler(Exception error);
    public delegate void MediaOpenedHandler();
    public delegate void MediaEndedHandler();

    /// <summary>
    /// Controls the playback of audio/video media.
    /// </summary>
    public abstract class MediaPlayer
    {
        /// <summary>
        /// Gets the texture for the latest generated frame.
        /// </summary>
        public virtual uint Width { get { return 0; } }

        /// <summary>
        /// Gets the pixel height of the video.
        /// </summary>
        public virtual uint Height { get { return 0; } }

        /// <summary>
        /// Gets a value indicating whether the media can be paused.
        /// </summary>
        public virtual bool CanPause { get { return false; } }

        /// <summary>
        /// Gets a value that indicating whether the media has audio output.
        /// </summary>
        public virtual bool HasAudio { get { return false; } }

        /// <summary>
        /// Gets a value that indicates whether the media has video output.
        /// </summary>
        public virtual bool HasVideo { get { return false; } }

        /// <summary>
        /// Gets the percentage of buffering completed for streaming content, represented in a value
        /// between 0 and 1.
        /// </summary>
        public virtual float BufferingProgress { get { return 0.0f; } }

        /// <summary>
        /// Gets the percentage of download progress for content located at a remote server,
        /// represented by a value between 0 and 1. The default is 1.
        /// </summary>
        public virtual float DownloadProgress { get { return 0.0f; } }

        /// <summary>
        /// Gets the duration in seconds of the media.
        /// </summary>
        public virtual double Duration { get { return 0.0; } }

        /// <summary>
        /// Gets or sets the current position in seconds of the media.
        /// </summary>
        public virtual double Position
        {
            get { return 0.0f; }
            set { }
        }

        /// <summary>
        /// Gets or sets the ratio of speed that media is played at, represented by a value between 0
        /// and the largest float. The default is 1.
        /// </summary>
        public virtual float SpeedRatio
        {
            get { return 1.0f; }
            set { }
        }

        /// <summary>
        /// Gets or sets the media's volume, represented on a linear scale between 0 and 1. The default
        /// is 0.5.
        /// </summary>
        public virtual float Volume
        {
            get { return 0.5f; }
            set { }
        }

        /// <summary>
        /// Gets or sets the balance between the left and right speaker volumes, represented in a range
        /// between -1 and 1. The default is 0.
        /// </summary>
        public virtual float Balance
        {
            get { return 0.0f; }
            set { }
        }

        /// <summary>
        /// Gets or sets a value that indicates whether the media is muted. The default is false.
        /// </summary>
        public virtual bool IsMuted
        {
            get { return false; }
            set { }
        }

        /// <summary>
        /// Gets or sets a value that indicates whether the media player will update frames for seek
        /// operations while paused. The default is false.
        /// </summary>
        public virtual bool ScrubbingEnabled
        {
            get { return false; }
            set { }
        }

        /// <summary>
        /// Starts or resumes media playback
        /// </summary>
        public virtual void Play() { }

        /// <summary>
        /// Pauses media playback
        /// </summary>
        public virtual void Pause() { }

        /// <summary>
        /// Stops media playback and moves position to the begining
        /// </summary>
        public virtual void Stop() { }

        /// <summary>
        /// Closes media and frees resources
        /// </summary>
        public virtual void Close() { }

        /// <summary>
        /// Gets the texture source for rendering the video
        /// </summary>
        public abstract Noesis.ImageSource TextureSource { get; }


        #region Events
        /// Occurs when buffering has started.
        public event BufferingStartedHandler BufferingStarted;
        protected void RaiseBufferingStarted() { BufferingStarted?.Invoke(); }

        /// Occurs when buffering has finished.
        public event BufferingEndedHandler BufferingEnded;
        protected void RaiseBufferingEnded() { BufferingEnded?.Invoke(); }

        /// Occurs when the media is opened.
        public event MediaOpenedHandler MediaOpened;
        protected void RaiseMediaOpened() { MediaOpened?.Invoke(); }

        /// Occurs when the media has finished playback.
        public event MediaEndedHandler MediaEnded;
        protected void RaiseMediaEnded() { MediaEnded?.Invoke(); }

        /// Occurs when an error is encountered.
        public event MediaFailedHandler MediaFailed;
        protected void RaiseMediaFailed(Exception error) { MediaFailed?.Invoke(error); }
        #endregion
    }
}
