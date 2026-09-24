using Noesis;
using NoesisApp;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace NoesisApp
{
    public class GEMediaPlayer : MediaPlayer
    {
        public static MediaPlayer Create(MediaElement owner, Uri uri, object user)
        {
            return new GEMediaPlayer(owner, uri);
        }

        static GEMediaPlayer()
        {
            InitMediaPlayer();
        }

        GEMediaPlayer(MediaElement owner, Uri uri)
        {
            _stream = Noesis.GUI.LoadXamlResource(uri.OriginalString);
            if (_stream != null)
            {
                _state = CreateState();

                _renderer = new Renderer(_state);

                _streamHandle = GCHandle.Alloc(_stream);
                StreamReadDelegate readFn = new StreamReadDelegate(StreamRead);
                _readFnHandle = GCHandle.Alloc(readFn);
                StreamSeekDelegate seekFn = new StreamSeekDelegate(StreamSeek);
                _seekFnHandle = GCHandle.Alloc(seekFn);

                MediaOpenedDelegate mediaOpenedFn = new MediaOpenedDelegate(this.OnMediaOpened);
                _mediaOpenedFnHandle = GCHandle.Alloc(mediaOpenedFn);
                MediaEndedDelegate mediaEndedFn = new MediaEndedDelegate(this.OnMediaEnded);
                _mediaEndedFnHandle = GCHandle.Alloc(mediaEndedFn);
                MediaFailedDelegate mediaFailedFn = new MediaFailedDelegate(this.OnMediaFailed);
                _mediaFailedFnHandle = GCHandle.Alloc(mediaFailedFn);

                long streamSize = _stream.Length;
                OpenMedia(_state, GCHandle.ToIntPtr(_streamHandle), uri.GetPath(), streamSize,
                    readFn, seekFn, mediaOpenedFn, mediaEndedFn, mediaFailedFn);
            }
        }

        ~GEMediaPlayer()
        {
            Close();
        }

        public override void Close()
        {
            if (_stream != null)
            {
                _renderer = null;

                DestroyState(_state);

                _streamHandle.Free();
                _readFnHandle.Free();
                _seekFnHandle.Free();

                _mediaOpenedFnHandle.Free();
                _mediaEndedFnHandle.Free();
                _mediaFailedFnHandle.Free();

                _stream.Close();
                _stream = null;
            }
        }

        public override uint Width
        {
            get { return (_stream != null) ? GetWidth(_state) : 0; }
        }

        public override uint Height
        {
            get { return (_stream != null) ? GetHeight(_state) : 0; }
        }

        public override bool CanPause
        {
            get { return (_stream != null) ? GetCanPause(_state) : false; }
        }

        public override bool HasAudio
        {
            get { return (_stream != null) ? GetHasAudio(_state) : false; }
        }

        public override bool HasVideo
        {
            get { return (_stream != null) ? GetHasVideo(_state) : false; }
        }

        public override float BufferingProgress
        {
            get { return (_stream != null) ? GetBufferingProgress(_state) : 0; }
        }

        public override float DownloadProgress
        {
            get { return (_stream != null) ? GetDownloadProgress(_state) : 0; }
        }

        public override double Duration
        {
            get { return (_stream != null) ? GetDuration(_state) : 0; }
        }

        public override double Position
        {
            get { return (_stream != null) ? GetTime(_state) : 0; }
            set { if (_stream != null) Seek(_state, value); }
        }

        public override float SpeedRatio
        {
            get { return (_stream != null) ? GetSpeedRatio(_state) : 0; }
            set { if (_stream != null) SetSpeedRatio(_state, value); }
        }

        public override float Volume
        {
            get { return (_stream != null) ? GetVolume(_state) : 0.5f; }
            set { if (_stream != null) SetVolume(_state, value); }
        }

        public override float Balance
        {
            get { return (_stream != null) ? GetBalance(_state) : 0.5f; }
            set { if (_stream != null) SetBalance(_state, value); }
        }

        public override bool IsMuted
        {
            get { return (_stream != null) ? GetIsMuted(_state) : false; }
            set { if (_stream != null) SetIsMuted(_state, value); }
        }

        public override bool ScrubbingEnabled
        {
            get { return (_stream != null) ? GetScrubbingEnabled(_state) : false; }
            set { if (_stream != null) SetScrubbingEnabled(_state, value); }
        }

        public override void Play()
        {
            if (_stream != null) Play(_state);
        }

        public override void Pause()
        {
            if (_stream != null) Pause(_state);
        }

        public override void Stop()
        {
            if (_stream != null) Stop(_state);
        }

        public override void Update()
        {
            if (_stream != null) Update(_state);
        }

        public override ImageSource TextureSource
        {
            get { return _textureSource; }
        }

        private class Renderer : DynamicTextureSource.Renderer
        {
            public Renderer(IntPtr state)
            {
                _state = state;
            }

            public override void Init(RenderDevice device)
            {
                _device = device;
            }

            public override Texture Render(uint width, uint height)
            {
                if (_renderTarget == null ||
                    _renderTarget.Texture.Width != width ||
                    _renderTarget.Texture.Height != height)
                {
                    _renderTarget = _device.CreateRenderTarget("MediaPlayer", width, height, 1, false);
                }

                if (_state != IntPtr.Zero)
                {
                    if (HasNewFrame(_state))
                    {
                        Tile tile = new Tile() { X = 0, Y = 0, Width = width, Height = height };
                        Tile[] tiles = { tile };

                        _device.SetRenderTarget(_renderTarget);

                        RenderFrame(_state);

                        _device.ResolveRenderTarget(_renderTarget, tiles);
                    }
                }

                return _renderTarget.Texture;
            }

            public override void Shutdown()
            {
                _device = null;
            }

            private IntPtr _state;
            private RenderDevice _device;
            private RenderTarget _renderTarget;
        }

        #region Event delegates
        private delegate uint StreamReadDelegate(IntPtr streamPtr, IntPtr buffer, uint size);

        private static uint StreamRead(IntPtr streamPtr, IntPtr buffer, uint size)
        {
            try
            {
                GCHandle handle = GCHandle.FromIntPtr(streamPtr);
                Stream stream = (Stream)handle.Target;
                if (stream != null)
                {
                    byte[] bytes = new byte[size];
                    int count = stream.Read(bytes, 0, (int)size);
                    System.Runtime.InteropServices.Marshal.Copy(bytes, 0, buffer, count);
                    return (uint)count;
                }
            }
            catch (Exception)
            {
            }

            return 0;
        }

        private delegate uint StreamSeekDelegate(IntPtr streamPtr, uint offset);

        private static uint StreamSeek(IntPtr streamPtr, uint offset)
        {
            try
            {
                GCHandle handle = GCHandle.FromIntPtr(streamPtr);
                Stream stream = (Stream)handle.Target;
                if (stream != null)
                {
                    return (uint)stream.Seek(offset, SeekOrigin.Begin);
                }
            }
            catch (Exception)
            {
            }

            return 0;
        }

        private delegate void MediaOpenedDelegate();

        private void OnMediaOpened()
        {
            _textureSource = new DynamicTextureSource(Width, Height, _renderer);
            RaiseMediaOpened();
        }

        private delegate void MediaEndedDelegate();

        private void OnMediaEnded()
        {
            RaiseMediaEnded();
        }

        private delegate void MediaFailedDelegate();

        private void OnMediaFailed()
        {
            RaiseMediaFailed(new Exception("UpdateMedia failed"));
        }
        #endregion

        #region Private members
        private IntPtr _state;
        private Renderer _renderer;
        private DynamicTextureSource _textureSource;
        private Stream _stream;
        private GCHandle _streamHandle;
        private GCHandle _readFnHandle;
        private GCHandle _seekFnHandle;
        private GCHandle _mediaOpenedFnHandle;
        private GCHandle _mediaEndedFnHandle;
        private GCHandle _mediaFailedFnHandle;
        #endregion

        #region Imports
        [DllImport("MediaPlayer")]
        private static extern void InitMediaPlayer();

        [DllImport("MediaPlayer")]
        private static extern IntPtr CreateState();

        [DllImport("MediaPlayer")]
        private static extern void DestroyState(IntPtr state);

        [DllImport("MediaPlayer")]
        private static extern void OpenMedia(IntPtr state, IntPtr streamPtr, string streamName, long streamSize,
            StreamReadDelegate readFn, StreamSeekDelegate seekFn, MediaOpenedDelegate mediaOpenedFn, MediaEndedDelegate mediaEndedFn, MediaFailedDelegate mediaFailedFn);

        [DllImport("MediaPlayer")]
        private static extern uint GetWidth(IntPtr state);

        [DllImport("MediaPlayer")]
        private static extern uint GetHeight(IntPtr state);

        [DllImport("MediaPlayer")]
        private static extern bool GetCanPause(IntPtr state);

        [DllImport("MediaPlayer")]
        private static extern bool GetHasAudio(IntPtr state);

        [DllImport("MediaPlayer")]
        private static extern bool GetHasVideo(IntPtr state);

        [DllImport("MediaPlayer")]
        private static extern float GetBufferingProgress(IntPtr state);

        [DllImport("MediaPlayer")]
        private static extern float GetDownloadProgress(IntPtr state);

        [DllImport("MediaPlayer")]
        private static extern double GetDuration(IntPtr state);

        [DllImport("MediaPlayer")]
        private static extern double GetTime(IntPtr state);

        [DllImport("MediaPlayer")]
        private static extern float GetSpeedRatio(IntPtr state);

        [DllImport("MediaPlayer")]
        private static extern void SetSpeedRatio(IntPtr state, float speedRatio);

        [DllImport("MediaPlayer")]
        private static extern float GetVolume(IntPtr state);

        [DllImport("MediaPlayer")]
        private static extern void SetVolume(IntPtr state, float volume);

        [DllImport("MediaPlayer")]
        private static extern float GetBalance(IntPtr state);

        [DllImport("MediaPlayer")]
        private static extern void SetBalance(IntPtr state, float balance);

        [DllImport("MediaPlayer")]
        private static extern bool GetIsMuted(IntPtr state);

        [DllImport("MediaPlayer")]
        private static extern void SetIsMuted(IntPtr state, bool isMuted);

        [DllImport("MediaPlayer")]
        private static extern bool GetScrubbingEnabled(IntPtr state);

        [DllImport("MediaPlayer")]
        private static extern void SetScrubbingEnabled(IntPtr state, bool scrubbingEnabled);

        [DllImport("MediaPlayer")]
        private static extern void Play(IntPtr state);

        [DllImport("MediaPlayer")]
        private static extern void Pause(IntPtr state);

        [DllImport("MediaPlayer")]
        private static extern void Seek(IntPtr state, double time);

        [DllImport("MediaPlayer")]
        private static extern void Stop(IntPtr state);

        [DllImport("MediaPlayer")]
        private static extern bool HasNewFrame(IntPtr state);

        [DllImport("MediaPlayer")]
        private static extern void RenderFrame(IntPtr state);

        [DllImport("MediaPlayer")]
        private static extern bool IsValid(IntPtr state);

        [DllImport("MediaPlayer")]
        private static extern bool Update(IntPtr state);
        #endregion
    }
}
