using System;
using System.Runtime.InteropServices;

namespace NoesisApp
{
    /// <summary>
    /// Arguments for MediaFailed event.
    /// </summary>
    public class ExceptionRoutedEventArgs: Noesis.RoutedEventArgs
    {
        public System.Exception ErrorException { get; private set; }

        public ExceptionRoutedEventArgs(Noesis.RoutedEvent ev, object source, System.Exception err): base(ev, source)
        {
            ErrorException = err;
        }

        public ExceptionRoutedEventArgs(IntPtr cPtr, bool ownMemory) : base(cPtr, ownMemory)
        {
        }

        // When compiled in Unity in the same assembly it complains about hiding RoutedEventArgs InvokeHandler member
        #pragma warning disable 0108
        private static void InvokeHandler(Delegate handler, IntPtr sender, IntPtr args)
        {
            ExceptionRoutedEventHandler handler_ = (ExceptionRoutedEventHandler)handler;
            if (handler_ != null)
            {
                handler_(Noesis.BaseComponent.GetProxy(sender), new ExceptionRoutedEventArgs(args, false));
            }
        }
        #pragma warning restore 0108
    }

    public delegate void ExceptionRoutedEventHandler(object sender, ExceptionRoutedEventArgs e);

    /// <summary>
    /// Specifies the states that can be applied to a MediaElement for the LoadedBehavior and
    /// UnloadedBehavior properties.
    /// </summary>
    public enum MediaState
    {
        /// The state used to control a MediaElement manually. Interactive methods like Play and Pause
        /// can be used. Media will preroll but not play when the MediaElement is assigned a valid
        /// media source.
        Manual,
        /// The state used to play the media. Media will preroll automatically being playback when the
        /// MediaElement is assigned a valid media source.
        Play,
        /// The state used to close the media. All media resources are released including video memory.
        Close,
        /// The state used to pause the media. Media will preroll but remains paused when the
        /// MediaElement is assigned a valid media source.
        Pause,
        /// The state used to stop the media. Media will preroll but not play when the MediaElement is
        /// assigned a valid media source. Media resources are not released.
        Stop
    };

    /// <summary>
    /// Creation function for MediaPlayer implementation
    /// </summary>
    public delegate MediaPlayer CreateMediaPlayerCallback(MediaElement owner, Uri uri, object user);

    /// <summary>
    /// Represents a control that contains audio and/or video.
    ///
    /// http://msdn.microsoft.com/en-us/library/system.windows.controls.mediaelement.aspx
    /// </summary>
    public class MediaElement : Noesis.Decorator
    {
        public MediaElement()
        {
            Noesis.Image image = new Noesis.Image();
            Noesis.RelativeSource source = new Noesis.RelativeSource { AncestorType = typeof(MediaElement) };
            image.SetBinding(Noesis.Image.StretchProperty, new Noesis.Binding(StretchProperty, source));
            image.SetBinding(Noesis.Image.StretchDirectionProperty, new Noesis.Binding(StretchDirectionProperty, source));
            Child = image;

            Loaded += OnLoadStateChanged;
            Unloaded += OnLoadStateChanged;
        }

        /// <summary>
        /// Sets the creation function for MediaPlayer implementation
        ///</summary>
        public static void SetCreateMediaPlayerCallback(CreateMediaPlayerCallback callback, object user)
        {
            _createMediaPlayerCallback = callback;
            _createMediaPlayerUser = user;
        }

        #region Properties

        #region Source
        public Uri Source
        {
            get { return (Uri)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        public static readonly Noesis.DependencyProperty SourceProperty = Noesis.DependencyProperty.Register(
            "Source", typeof(Uri), typeof(MediaElement),
            new Noesis.FrameworkPropertyMetadata(null,
                Noesis.FrameworkPropertyMetadataOptions.AffectsMeasure | Noesis.FrameworkPropertyMetadataOptions.AffectsRender,
                OnStateChanged));
        #endregion

        #region LoadedBehavior
        public MediaState LoadedBehavior
        {
            get { return (MediaState)GetValue(LoadedBehaviorProperty); }
            set { SetValue(LoadedBehaviorProperty, value); }
        }

        public static readonly Noesis.DependencyProperty LoadedBehaviorProperty = Noesis.DependencyProperty.Register(
            "LoadedBehavior", typeof(MediaState), typeof(MediaElement),
            new Noesis.FrameworkPropertyMetadata(MediaState.Play));

        private static void OnStateChanged(Noesis.DependencyObject d, Noesis.DependencyPropertyChangedEventArgs e)
        {
            MediaElement element = (MediaElement)d;
            element.UpdateState(element._state, e.Property == SourceProperty);
        }
        #endregion

        #region UnloadedBehavior
        public MediaState UnloadedBehavior
        {
            get { return (MediaState)GetValue(UnloadedBehaviorProperty); }
            set { SetValue(UnloadedBehaviorProperty, value); }
        }

        public static readonly Noesis.DependencyProperty UnloadedBehaviorProperty = Noesis.DependencyProperty.Register(
            "UnloadedBehavior", typeof(MediaState), typeof(MediaElement),
            new Noesis.FrameworkPropertyMetadata(MediaState.Close, OnStateChanged));
        #endregion

        #region Stretch
        /// <summary>
        /// Gets or sets a value that describes how a MediaElement fills the destination rectangle.
        /// The default is Stretch.Uniform.
        /// </summary>
        public Noesis.Stretch Stretch
        {
            get { return (Noesis.Stretch)GetValue(StretchProperty); }
            set { SetValue(StretchProperty, value); }
        }

        public static readonly Noesis.DependencyProperty StretchProperty = Noesis.DependencyProperty.Register(
            "Stretch", typeof(Noesis.Stretch), typeof(MediaElement),
            new Noesis.FrameworkPropertyMetadata(Noesis.Stretch.Uniform,
                Noesis.FrameworkPropertyMetadataOptions.AffectsMeasure));
        #endregion

        #region StretchDirection
        /// <summary>
        /// Gets or sets a value that determines the restrictions on scaling that are applied to this
        /// media element. The default is StretchDirection.Both.
        /// </summary>
        public Noesis.StretchDirection StretchDirection
        {
            get { return (Noesis.StretchDirection)GetValue(StretchDirectionProperty); }
            set { SetValue(StretchDirectionProperty, value); }
        }

        public static readonly Noesis.DependencyProperty StretchDirectionProperty = Noesis.DependencyProperty.Register(
            "StretchDirection", typeof(Noesis.StretchDirection), typeof(MediaElement),
            new Noesis.FrameworkPropertyMetadata(Noesis.StretchDirection.Both,
                Noesis.FrameworkPropertyMetadataOptions.AffectsMeasure));
        #endregion

        #region Balance
        public float Balance
        {
            get { return (float)GetValue(BalanceProperty); }
            set { SetValue(BalanceProperty, value); }
        }

        public static readonly Noesis.DependencyProperty BalanceProperty = Noesis.DependencyProperty.Register(
            "Balance", typeof(float), typeof(MediaElement),
            new Noesis.FrameworkPropertyMetadata(0.0f, OnBalanceChanged));

        private static void OnBalanceChanged(Noesis.DependencyObject d, Noesis.DependencyPropertyChangedEventArgs e)
        {
            MediaElement element = (MediaElement)d;
            if (element._player != null)
            {
                element._player.Balance = (float)e.NewValue;
            }
        }
        #endregion

        #region Volume
        public float Volume
        {
            get { return (float)GetValue(VolumeProperty); }
            set { SetValue(VolumeProperty, value); }
        }

        public static readonly Noesis.DependencyProperty VolumeProperty = Noesis.DependencyProperty.Register(
            "Volume", typeof(float), typeof(MediaElement),
            new Noesis.FrameworkPropertyMetadata(0.5f, OnVolumeChanged));

        private static void OnVolumeChanged(Noesis.DependencyObject d, Noesis.DependencyPropertyChangedEventArgs e)
        {
            MediaElement element = (MediaElement)d;
            if (element._player != null)
            {
                element._player.Volume = (float)e.NewValue;
            }
        }
        #endregion

        #region IsMuted
        public bool IsMuted
        {
            get { return (bool)GetValue(IsMutedProperty); }
            set { SetValue(IsMutedProperty, value); }
        }

        public static readonly Noesis.DependencyProperty IsMutedProperty = Noesis.DependencyProperty.Register(
            "IsMuted", typeof(bool), typeof(MediaElement),
            new Noesis.FrameworkPropertyMetadata(false, OnIsMutedChanged));

        private static void OnIsMutedChanged(Noesis.DependencyObject d, Noesis.DependencyPropertyChangedEventArgs e)
        {
            MediaElement element = (MediaElement)d;
            if (element._player != null)
            {
                element._player.IsMuted = (bool)e.NewValue;
            }
        }
        #endregion

        #region IsBuffering
        /// <summary>
        /// Get a value indicating whether the media is buffering.
        /// </summary>
        public bool IsBuffering { get; private set; }
        #endregion

        #region BufferingProgress
        /// <summary>
        /// Gets the percentage of buffering completed for streaming content, represented in a value
        /// between 0 and 1.
        /// </summary>
        public float BufferingProgress
        {
            get { return _player != null ? _player.BufferingProgress : 0.0f; }
        }
        #endregion

        #region DownloadProgress
        /// <summary>
        /// Gets the percentage of download progress for content located at a remote server,
        /// represented by a value between 0 and 1. The default is 1.
        /// </summary>
        public float DownloadProgress
        {
            get { return _player != null ? _player.DownloadProgress : 0.0f; }
        }
        #endregion

        #region ScrubbingEnabled
        public bool ScrubbingEnabled
        {
            get { return (bool)GetValue(ScrubbingEnabledProperty); }
            set { SetValue(ScrubbingEnabledProperty, value); }
        }

        public static readonly Noesis.DependencyProperty ScrubbingEnabledProperty = Noesis.DependencyProperty.Register(
            "ScrubbingEnabled", typeof(bool), typeof(MediaElement),
            new Noesis.FrameworkPropertyMetadata(false));

        private static void OnScrubbingEnabledChanged(Noesis.DependencyObject d, Noesis.DependencyPropertyChangedEventArgs e)
        {
            MediaElement element = (MediaElement)d;
            if (element._player != null)
            {
                element._player.ScrubbingEnabled = (bool)e.NewValue;
            }
        }
        #endregion

        #region NaturalVideoWidth
        /// <summary>
        /// Gets the pixel height of the video.
        /// </summary>
        public uint NaturalVideoWidth
        {
            get { return _player != null ? _player.Width : 0u; }
        }
        #endregion

        #region NaturalVideoHeight
        /// <summary>
        /// Gets the pixel width of the video.
        /// </summary>
        public uint NaturalVideoHeight
        {
            get { return _player != null ? _player.Height : 0u; }
        }
        #endregion

        #region NaturalDuration
        /// <summary>
        /// Gets the natural duration of the media. The default is Duration.Automatic.
        /// </summary>
        public Noesis.Duration NaturalDuration
        {
            get { return TimeSpan.FromSeconds(_player != null ? _player.Duration : 0.0); }
        }
        #endregion

        #region Position
        /// <summary>
        /// Gets or sets the current position of the media (seek operations).
        /// </summary>
        public TimeSpan Position
        {
            get
            {
                if (_player != null)
                {
                    _position = TimeSpan.FromSeconds(_player.Position);
                }

                return _position;
            }
            set
            {
                _position = value;

                if (_player != null)
                {
                    _player.Position = value.TotalSeconds;
                }
            }
        }
        private TimeSpan _position = TimeSpan.FromSeconds(0.0);
        #endregion

        #region SpeedRatio
        /// <summary>
        /// Gets or sets the ratio of speed that media is played at.
        /// Represented by a value between 0 and the largest double value. The default is 1.0.
        /// </summary>
        public float SpeedRatio
        {
            get
            {
                if (_player != null)
                {
                    _speedRatio = _player.SpeedRatio;
                }

                return _speedRatio;
            }
            set
            {
                _speedRatio = value;

                if (_player != null)
                {
                    _player.SpeedRatio = value;
                }
            }
        }
        private float _speedRatio = 1.0f;
        #endregion

        #region CanPause
        /// <summary>
        /// Gets a value indicating whether the media can be paused.
        /// </summary>
        public bool CanPause
        {
            get { return _player != null ? _player.CanPause : false; }
        }
        #endregion

        #region HasAudio
        /// <summary>
        /// Gets a value that indicating whether the media has audio output.
        /// </summary>
        public bool HasAudio
        {
            get { return _player != null ? _player.HasAudio : false; }
        }
        #endregion

        #region HasVideo
        /// <summary>
        /// Gets a value that indicates whether the media has video output.
        /// </summary>
        public bool HasVideo
        {
            get { return _player != null ? _player.HasVideo : false; }
        }
        #endregion

        #endregion

        #region Events

        #region BufferingStarted
        public event Noesis.RoutedEventHandler BufferingStarted
        {
            add { AddHandler(BufferingStartedEvent, value); }
            remove { RemoveHandler(BufferingStartedEvent, value); }
        }

        public static readonly Noesis.RoutedEvent BufferingStartedEvent = Noesis.EventManager.RegisterRoutedEvent(
            "BufferingStarted", Noesis.RoutingStrategy.Bubble, typeof(Noesis.RoutedEventHandler), typeof(MediaElement));
        #endregion

        #region BufferingEnded
        public event Noesis.RoutedEventHandler BufferingEnded
        {
            add { AddHandler(BufferingEndedEvent, value); }
            remove { RemoveHandler(BufferingEndedEvent, value); }
        }

        public static readonly Noesis.RoutedEvent BufferingEndedEvent = Noesis.EventManager.RegisterRoutedEvent(
            "BufferingEnded", Noesis.RoutingStrategy.Bubble, typeof(Noesis.RoutedEventHandler), typeof(MediaElement));
        #endregion

        #region MediaOpened
        public event Noesis.RoutedEventHandler MediaOpened
        {
            add { AddHandler(MediaOpenedEvent, value); }
            remove { RemoveHandler(MediaOpenedEvent, value); }
        }

        public static readonly Noesis.RoutedEvent MediaOpenedEvent = Noesis.EventManager.RegisterRoutedEvent(
            "MediaOpened", Noesis.RoutingStrategy.Bubble, typeof(Noesis.RoutedEventHandler), typeof(MediaElement));
        #endregion

        #region MediaEnded
        public event Noesis.RoutedEventHandler MediaEnded
        {
            add { AddHandler(MediaEndedEvent, value); }
            remove { RemoveHandler(MediaEndedEvent, value); }
        }

        public static readonly Noesis.RoutedEvent MediaEndedEvent = Noesis.EventManager.RegisterRoutedEvent(
            "MediaEnded", Noesis.RoutingStrategy.Bubble, typeof(Noesis.RoutedEventHandler), typeof(MediaElement));
        #endregion

        #region MediaFailed
        public event ExceptionRoutedEventHandler MediaFailed
        {
            add { AddHandler(MediaFailedEvent, value); }
            remove { RemoveHandler(MediaFailedEvent, value); }
        }

        public static readonly Noesis.RoutedEvent MediaFailedEvent = Noesis.EventManager.RegisterRoutedEvent(
            "MediaFailed", Noesis.RoutingStrategy.Bubble, typeof(ExceptionRoutedEventHandler), typeof(MediaElement));
        #endregion

        #endregion

        #region Playback control

        /// Plays media from the current position.
        public void Play()
        {
            SetState(MediaState.Play);
        }

        // Pauses media playback.
        public void Pause()
        {
            SetState(MediaState.Pause);
        }

        /// Stops media playback.
        public void Stop()
        {
            SetState(MediaState.Stop);
        }

        /// Closes the underlying media.
        public void Close()
        {
            SetState(MediaState.Close);
        }

        #endregion

        #region Private members

        private void SetState(MediaState state)
        {
            if (LoadedBehavior != MediaState.Manual && UnloadedBehavior != MediaState.Manual)
            {
                throw new NotSupportedException("Cannot control media unless LoadedBehavior or UnloadedBehavior is set to Manual.");
            }

            UpdateState(state, false);
        }

        private void UpdateState(MediaState action, bool sourceChanged)
        {
            MediaState stateRequested = action;

            if (IsLoaded)
            {
                MediaState loadedBehavior = LoadedBehavior;

                if (loadedBehavior != MediaState.Manual)
                {
                    stateRequested = loadedBehavior;
                }
            }
            else
            {
                MediaState unloadedBehavior = UnloadedBehavior;

                if (unloadedBehavior != MediaState.Manual)
                {
                    stateRequested = unloadedBehavior;
                }
            }

            if (_state != stateRequested || stateRequested == MediaState.Stop || sourceChanged)
            {
                bool opened = false;
                if (stateRequested != MediaState.Close && stateRequested != MediaState.Manual)
                {
                    Uri source = Source;
                    if ((_state == MediaState.Close || sourceChanged) && source.OriginalString.Length > 0)
                    {
                        DestroyMediaPlayer();
                        CreateMediaPlayer(source);
                        opened = true;
                    }
                }

                if (_state != MediaState.Close || opened)
                {
                    _state = stateRequested;

                    switch (_state)
                    {
                        case MediaState.Manual:
                        {
                            break; // ignore
                        }
                        case MediaState.Play:
                        {
                            if (_player != null)
                            {
                                _player.Play();
                            }
                            break;
                        }
                        case MediaState.Close:
                        {
                            DestroyMediaPlayer();
                            break;
                        }
                        case MediaState.Pause:
                        {
                            if (_player != null)
                            {
                                _player.Pause();
                            }
                            break;
                        }
                        case MediaState.Stop:
                        {
                            if (_player != null)
                            {
                                _player.Stop();
                            }
                            break;
                        }
                        default: break;
                    }
                }
            }
        }

        private void CreateMediaPlayer(Uri source)
        {
            if (_createMediaPlayerCallback != null)
            {
                _player = _createMediaPlayerCallback(this, source, _createMediaPlayerUser);

                if (_player != null)
                {
                    _player.BufferingStarted += OnBufferingStarted;
                    _player.BufferingEnded += OnBufferingEnded;
                    _player.MediaOpened += OnMediaOpened;
                    _player.MediaEnded += OnMediaEnded;
                    _player.MediaFailed += OnMediaFailed;
                }
            }
        }

        private void DestroyMediaPlayer()
        {
            if (_player != null)
            {
                _player.BufferingStarted -= OnBufferingStarted;
                _player.BufferingEnded -= OnBufferingEnded;
                _player.MediaOpened -= OnMediaOpened;
                _player.MediaEnded -= OnMediaEnded;
                _player.MediaFailed -= OnMediaFailed;

                _player.Close();
                _player = null;

                ((Noesis.Image)Child).Source = null;
            }
        }

        private void OnLoadStateChanged(object sender, Noesis.RoutedEventArgs e)
        {
            UpdateState(_state, false);
        }

        private void OnBufferingStarted()
        {
            IsBuffering = true;
            RaiseEvent(new Noesis.RoutedEventArgs(BufferingStartedEvent, this));
        }

        private void OnBufferingEnded()
        {
            IsBuffering = false;
            RaiseEvent(new Noesis.RoutedEventArgs(BufferingEndedEvent, this));
        }

        private void OnMediaOpened()
        {
            _player.Volume = Volume;
            _player.Balance = Balance;
            _player.IsMuted = IsMuted;
            _player.ScrubbingEnabled = ScrubbingEnabled;
            _player.Position = _position.TotalSeconds;
            _player.SpeedRatio = _speedRatio;

            ((Noesis.Image)Child).Source = _player.TextureSource;
            RaiseEvent(new Noesis.RoutedEventArgs(MediaOpenedEvent, this));
        }

        private void OnMediaEnded()
        {
            RaiseEvent(new Noesis.RoutedEventArgs(MediaEndedEvent, this));
        }

        private void OnMediaFailed(System.Exception error)
        {
            RaiseEvent(new ExceptionRoutedEventArgs(MediaFailedEvent, this, error));
        }

        private static CreateMediaPlayerCallback _createMediaPlayerCallback;
        private static object _createMediaPlayerUser;
        private MediaPlayer _player;
        private MediaState _state = MediaState.Close;

        #endregion
    }
}
