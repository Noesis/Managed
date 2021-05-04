using System;
using Noesis;
using System.Collections.Generic;

namespace NoesisApp
{
    public class GbmDisplay : Display
    {
        public GbmDisplay(string drmDevice)
        {
            _close = false;

            IntPtr udev = LibUdev.New();

            IntPtr enumerate = LibUdev.EnumerateNew(udev);

            LibUdev.EnumerateAddMatchSubsystem(enumerate, "drm");
            LibUdev.EnumerateScanDevices(enumerate);

            IntPtr listEntry = LibUdev.EnumerateGetListEntry(enumerate);

            while (_drmFd == -1 && listEntry != IntPtr.Zero)
            {
                IntPtr path = LibUdev.ListEntryGetName(listEntry);

                IntPtr device = LibUdev.DeviceNewFromSyspath(udev, path);

                IntPtr deviceName = LibUdev.DeviceGetDevnode(device);
                if (deviceName != IntPtr.Zero)
                {
                    string devName = System.Runtime.InteropServices.Marshal.PtrToStringAnsi(deviceName);

                    if (devName == drmDevice)
                    {
                        _drmFd = LibC.Open(drmDevice, LibC.O_RDWR);
                    }
                }

                LibUdev.DeviceUnref(device);

                listEntry = LibUdev.ListEntryGetNext(listEntry);
            }

            LibUdev.EnumerateUnref(enumerate);

            if (_drmFd == -1)
            {
                IntPtr udevMonitor = LibUdev.MonitorNewFromNetlink(udev, "udev");
            
                LibUdev.MonitorFilterAddMatchSubsystemDevtype(udevMonitor, "drm", null);
                LibUdev.MonitorEnableReceiving(udevMonitor);

                int udevMonitorFd = LibUdev.MonitorGetFd(udevMonitor);

                LibC.PollFd fd = new LibC.PollFd();
                fd.fd = udevMonitorFd;
                fd.events = LibC.POLLIN | LibC.POLLPRI;
                LibC.PollFd[] fds = { fd };
                while (_drmFd == -1)
                {
                    int rc = LibC.Poll(fds, -1);
                    if (rc > 0)
                    {
                        IntPtr device = LibUdev.MonitorReceiveDevice(udevMonitor);

                        if (device == IntPtr.Zero)
                        {
                            break;
                        }

                        IntPtr deviceAction = LibUdev.DeviceGetAction(device);
                        IntPtr deviceName = LibUdev.DeviceGetDevnode(device);

                        string devAction = System.Runtime.InteropServices.Marshal.PtrToStringAnsi(deviceAction);
                        string devName = System.Runtime.InteropServices.Marshal.PtrToStringAnsi(deviceName);

                        if (devAction == "add")
                        {
                            if (devName == drmDevice)
                            {
                                _drmFd = LibC.Open(drmDevice, LibC.O_RDWR);
                            }
                        }

                        LibUdev.DeviceUnref(device);
                    }
                }

                LibUdev.MonitorUnref(udevMonitor);
            }

            LibUdev.Unref(udev);

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
        }

        #region Display overrides
        public override IntPtr NativeHandle { get { return _gbmDevice; } }
        public override IntPtr NativeWindow { get { return _gbmSurface; } }

        public override int ClientWidth { get { return _width; } }
        public override int ClientHeight { get { return _height; } }

        public override void Show()
        {
            SizeChanged?.Invoke(this, _width, _height);
            Activated?.Invoke(this);

            SetCrtc();
        }

        public override void Close()
        {
            _close = true;
        }

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

        private bool PollReadable(int timeoutMs)
        {
            LibC.PollFd fd = new LibC.PollFd();
            fd.fd = _drmFd;
            fd.events = LibC.POLLIN | LibC.POLLPRI;
            LibC.PollFd[] fds = new LibC.PollFd[] { fd };
            int rc = LibC.Poll(fds, timeoutMs);
            return rc > 0;
        }

        public override void EnterMessageLoop(bool runInBackground)
        {
            LibEvdev.KeyDown += OnKeyDown;
            LibEvdev.KeyUp += OnKeyUp;

            LibEvdev.TouchDown += OnTouchDown;
            LibEvdev.TouchUp += OnTouchUp;
            LibEvdev.TouchMove += OnTouchMove;

            bool running = true;
            while (running)
            {
                LibEvdev.HandleEvents();
                Render?.Invoke(this);

                PageFlip();

                if (_close)
                {
                    bool cancel = false;
                    Closing?.Invoke(this, ref cancel);
                    if (!cancel)
                    {
                        Closed?.Invoke(this);
                        running = false;
                    }
                    _close = false;
                }
            }
        }
        #endregion

        public void SetCrtc()
        {
            _bo = Gbm.SurfaceLockFrontBuffer(_gbmSurface);
            uint fb = GetFbForBo(_bo);
            IntPtr connectors = System.Runtime.InteropServices.Marshal.AllocHGlobal(sizeof(uint));
            System.Runtime.InteropServices.Marshal.WriteInt32(connectors, (int)_drmConnectorId);
            int rc = Drm.ModeSetCrtc(_drmFd, _drmEncoderCrtcId, fb, 0, 0, connectors, 1, ref _drmMode);
            System.Runtime.InteropServices.Marshal.FreeHGlobal(connectors);
            if (rc != 0)
            {
                throw new Exception("Failed to create DRM Mode");
            }
        }

        public void PageFlip()
        {
            if (_bo != IntPtr.Zero)
            {
                Gbm.SurfaceReleaseBuffer(_gbmSurface, _bo);
            }
            SetCrtc();
        }

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
        private bool _close;

        private int _drmFd = -1;
        private uint _drmConnectorId;
        private Drm.ModeModeInfo _drmMode;
        private uint _drmEncoderCrtcId;

        private IntPtr _gbmDevice;
        private IntPtr _gbmSurface;
        private IntPtr _bo;

        private Dictionary<IntPtr, uint> _gbmBoToDrmFb;
        #endregion
    }
}
