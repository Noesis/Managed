using System;
using Noesis;
using System.Collections.Generic;

namespace NoesisApp
{
    public class GbmDisplay : Display
    {
        public GbmDisplay(string drmDevice)
        {
            _drmFd = LibC.Open(drmDevice, LibC.O_RDWR);

            IntPtr resourcesPtr = Drm.ModeGetResources(_drmFd);
            Drm.ModeRes resources = System.Runtime.InteropServices.Marshal.PtrToStructure<Drm.ModeRes>(resourcesPtr);

            for (int i = 0; i < resources.countConnectors; ++i)
            {
                uint connectorId = (uint)System.Runtime.InteropServices.Marshal.ReadInt32(resources.connectors, i * sizeof(uint));
                IntPtr connectorPtr = Drm.ModeGetConnector(_drmFd, connectorId);
                Drm.ModeConnector connector = System.Runtime.InteropServices.Marshal.PtrToStructure<Drm.ModeConnector>(connectorPtr);
                if (connector.connection == Drm.MODE_CONNECTED)
                {
                    _drmConnectorId = connector.connectorId;

                    int SizeOfDrmModeModeInfo = System.Runtime.InteropServices.Marshal.SizeOf<Drm.ModeModeInfo>();
                    for (int j = 0; j < connector.countModes; ++j)
                    {
                        _drmMode = System.Runtime.InteropServices.Marshal.PtrToStructure<Drm.ModeModeInfo>(IntPtr.Add(connector.modes, j * SizeOfDrmModeModeInfo));
                        if ((_drmMode.type & Drm.MODE_TYPE_PREFERRED) != 0)
                        {
                            _width = _drmMode.hdisplay;
                            _height = _drmMode.vdisplay;
                            break;
                        }
                    }

                    for (int j = 0; j < resources.countEncoders; ++j)
                    {
                        uint encoderId = (uint)System.Runtime.InteropServices.Marshal.ReadInt32(resources.encoders, j * sizeof(uint));
                        IntPtr encoderPtr = Drm.ModeGetEncoder(_drmFd, encoderId);
                        Drm.ModeEncoder encoder = System.Runtime.InteropServices.Marshal.PtrToStructure<Drm.ModeEncoder>(encoderPtr);
                        if (encoder.encoderId == connector.encoderId)
                        {
                            _drmEncoderCrtcId = encoder.crtcId;
                            break;
                        }

                        Drm.ModeFreeEncoder(encoderPtr);
                    }

                    break;
                }

                Drm.ModeFreeConnector(connectorPtr);
            }

            Drm.ModeFreeResources(resourcesPtr);

            Drm.SetClientCap(_drmFd, Drm.CLIENT_CAP_UNIVERSAL_PLANES, 1);

            _gbmDevice = Gbm.CreateDevice(_drmFd);
            _gbmSurface = Gbm.SurfaceCreate(_gbmDevice, (uint)_width, (uint)_height, Drm.FORMAT_ARGB8888, Gbm.BO_USE_SCANOUT | Gbm.BO_USE_RENDERING);

            _gbmBoToDrmFb = new Dictionary<IntPtr, uint>();

            LibEvdev.KeyDown += OnKeyDown;
            LibEvdev.KeyUp += OnKeyUp;

            LibEvdev.TouchDown += OnTouchDown;
            LibEvdev.TouchUp += OnTouchUp;
            LibEvdev.TouchMove += OnTouchMove;
        }

        #region Display overrides
        public override IntPtr NativeHandle { get { return _gbmDevice; } }
        public override IntPtr NativeWindow { get { return _gbmSurface; } }

        public override int ClientWidth { get { return _width; } }
        public override int ClientHeight { get { return _height; } }

        private uint GetFbForBo(IntPtr bo)
        {
            uint fb;
            if (!_gbmBoToDrmFb.TryGetValue(bo, out fb))
            {
                uint width = Gbm.BoGetWidth(bo);
                uint height = Gbm.BoGetHeight(bo);
                uint stride = Gbm.BoGetStride(bo);
                uint format = Gbm.BoGetFormat(bo);
                Gbm.BoHandle handle = Gbm.BoGetHandle(bo);

                uint[] strides = new uint[] { stride, 0, 0, 0};
                uint[] handles = new uint[] { handle.u32, 0, 0, 0};
                uint[] offsets = new uint[] { 0, 0, 0, 0};

                int rc = Drm.ModeAddFB2(_drmFd, width, height, format, handles, strides, offsets, ref fb, 0);
                if (rc != 0)
                {
                    throw new Exception("Failed to create DRM FB");
                }

                _gbmBoToDrmFb.Add(bo, fb);
            }

            return fb;
        }

        public override void EnterMessageLoop(bool runInBackground)
        {
            bool running = true;
            SizeChanged?.Invoke(this, _width, _height);
            Activated?.Invoke(this);

            IntPtr bo = Gbm.SurfaceLockFrontBuffer(_gbmSurface);
            uint fb = GetFbForBo(bo);
            IntPtr connectors = System.Runtime.InteropServices.Marshal.AllocHGlobal(sizeof(uint));
            System.Runtime.InteropServices.Marshal.WriteInt32(connectors, (int)_drmConnectorId);
            int rc = Drm.ModeSetCrtc(_drmFd, _drmEncoderCrtcId, fb, 0, 0, connectors, 1, ref _drmMode);
            System.Runtime.InteropServices.Marshal.FreeHGlobal(connectors);
            if (rc != 0)
            {
                throw new Exception("Failed to create DRM Mode");
            }

            while (running)
            {
                LibEvdev.HandleEvents();
                Render?.Invoke(this);

                IntPtr prevBo = bo;
                bo = Gbm.SurfaceLockFrontBuffer(_gbmSurface);
                fb = GetFbForBo(bo);

                Drm.ModePageFlip(_drmFd, _drmEncoderCrtcId, fb, Drm.MODE_PAGE_FLIP_EVENT, IntPtr.Zero);

                LibC.PollFd fd = new LibC.PollFd();
                fd.fd = _drmFd;
                fd.events = LibC.POLLIN | LibC.POLLPRI;
                LibC.PollFd[] fds = new LibC.PollFd[] { fd };
                LibC.Poll(fds, -1);
                Drm.EventContext evctx = new Drm.EventContext();
                evctx.version = 2;
                Drm.HandleEvent(_drmFd, ref evctx);

                if (prevBo != IntPtr.Zero)
                {
                    Gbm.SurfaceReleaseBuffer(_gbmSurface, prevBo);
                }
            }
        }

        public override void SetSize(int width, int height)
        {
        }

        public override void Show()
        {
        }
        #endregion

        #region Input events
        private void OnKeyDown(Key key)
        {
            KeyDown?.Invoke(this, key);
        }
        
        private void OnKeyUp(Key key)
        {
            KeyUp?.Invoke(this, key);
        }

        private void OnTouchDown(float x, float y, ulong id)
        {
            TouchDown?.Invoke(this, (int)(x * _width), (int)(y * _height), id);
        }

        private void OnTouchUp(float x, float y, ulong id)
        {
            TouchUp?.Invoke(this, (int)(x * _width), (int)(y * _height), id);
        }

        private void OnTouchMove(float x, float y, ulong id)
        {
            TouchMove?.Invoke(this, (int)(x * _width), (int)(y * _height), id);
        }
        #endregion

        #region Private members
        private int _width;
        private int _height;

        private int _drmFd;
        private uint _drmConnectorId;
        private Drm.ModeModeInfo _drmMode;
        private uint _drmEncoderCrtcId;

        private IntPtr _gbmDevice;
        private IntPtr _gbmSurface;

        private Dictionary<IntPtr, uint> _gbmBoToDrmFb;
        #endregion
    }
}
