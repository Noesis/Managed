using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using Noesis;

namespace NoesisApp
{
    internal class LibEvdev
    {
        static private IntPtr _udev;
        static private IntPtr _udevMonitor;
        static private int _udevMonitorFd;

        static LibEvdev()
        {
            InitKeyMap();

            _inputDeviceNames = new List<string>();
            _inputDescriptors = new List<int>();
            _inputDevices = new List<IntPtr>();

            _udev = LibUdev.New();

            IntPtr enumerate = LibUdev.EnumerateNew(_udev);

            LibUdev.EnumerateAddMatchSubsystem(enumerate, "input");
            LibUdev.EnumerateScanDevices(enumerate);

            IntPtr listEntry = LibUdev.EnumerateGetListEntry(enumerate);

            while (listEntry != IntPtr.Zero)
            {
                IntPtr path = LibUdev.ListEntryGetName(listEntry);

                IntPtr device = LibUdev.DeviceNewFromSyspath(_udev, path);

                IntPtr deviceName = LibUdev.DeviceGetDevnode(device);
                if (deviceName != IntPtr.Zero)
                {
                    string devName = System.Runtime.InteropServices.Marshal.PtrToStringAnsi(deviceName);

                    AddDevice(devName);
                }

                LibUdev.DeviceUnref(device);

                listEntry = LibUdev.ListEntryGetNext(listEntry);
            }

            LibUdev.EnumerateUnref(enumerate);

            _udevMonitor = LibUdev.MonitorNewFromNetlink(_udev, "udev");
            
            LibUdev.MonitorFilterAddMatchSubsystemDevtype(_udevMonitor, "input", null);
            LibUdev.MonitorEnableReceiving(_udevMonitor);

            _udevMonitorFd = LibUdev.MonitorGetFd(_udevMonitor);

            _mtSlotIds = new int[MaxSlots];
            for (int i = 0; i < MaxSlots; ++i)
            {
                _mtSlotIds[i] = -1;
            }
            _mtSlotXs = new float[MaxSlots];
            _mtSlotYs = new float[MaxSlots];
            _touchDown = new bool[MaxSlots];
            _touchUp = new bool[MaxSlots];
            _touchMove = new bool[MaxSlots];
        }

        private static void AddDevice(string devName)
        {
            int fd = LibC.Open(devName, LibC.O_RDONLY | LibC.O_NONBLOCK);
            if (fd < 0)
                return;

            LibC.SetFdFlags(fd, LibC.O_NONBLOCK);

            IntPtr dev = IntPtr.Zero;
            int rc = NewFromFd(fd, ref dev);
            if (rc == 0)
            {
                _inputDeviceNames.Add(devName);
                _inputDescriptors.Add(fd);
                _inputDevices.Add(dev);
            }
        }

        private static void RemoveDevice(string devName)
        {
            int index = _inputDeviceNames.IndexOf(devName);
            if (index != -1)
            {
                _inputDeviceNames.RemoveAt(index);
                _inputDescriptors.RemoveAt(index);
                _inputDevices.RemoveAt(index);
            }
        }

        private static void HandleDevices()
        {
            while (true)
            {
                IntPtr device = LibUdev.MonitorReceiveDevice(_udevMonitor);

                if (device == IntPtr.Zero)
                {
                    break;
                }

                IntPtr deviceAction = LibUdev.DeviceGetAction(device);
                IntPtr deviceName = LibUdev.DeviceGetDevnode(device);

                string devAction = System.Runtime.InteropServices.Marshal.PtrToStringAnsi(deviceAction);
                string devName = System.Runtime.InteropServices.Marshal.PtrToStringAnsi(deviceName);

                if (devAction == "remove")
                {
                    RemoveDevice(devName);
                }
                else if (devAction == "add")
                {
                    AddDevice(devName);
                }

                LibUdev.DeviceUnref(device);
            }
        }

        public static void HandleEvents()
        {
            List<IntPtr> inputDevices = new List<IntPtr>(_inputDevices);
            LibC.PollFd[] fds = new LibC.PollFd[inputDevices.Count + 1];
            LibC.PollFd fd = new LibC.PollFd();
            fd.fd = _udevMonitorFd;
            fd.events = LibC.POLLIN | LibC.POLLPRI;
            fds[0] = fd;
            for (int i = 0; i < inputDevices.Count; ++i)
            {
                fd = new LibC.PollFd();
                fd.fd = _inputDescriptors[i];
                fd.events = LibC.POLLIN | LibC.POLLPRI;
                fds[i + 1] = fd;
            }
            int rc = LibC.Poll(fds, 0);
            if (rc > 0)
            {
                if (fds[0].revents != 0)
                {
                    HandleDevices();
                }
                else
                {
                    InputEvent ev = new InputEvent();
                    for (int i = 0; i < inputDevices.Count; ++i)
                    {
                        if (fds[i + 1].revents != 0)
                        {
                            IntPtr dev = inputDevices[i];
                            rc = 0;
                            do
                            {
                                rc = NextEvent(dev, READ_FLAG_NORMAL, ref ev);
                                if (rc < 0)
                                {
                                    if (rc == -LibC.EAGAIN)
                                    {
                                        // No more events
                                        break;
                                    }
                                    System.Console.WriteLine("Error reading input events {0}", rc);
                                    //throw new Exception("Error reading input events");
                                }
                                else if (rc == READ_STATUS_SUCCESS)
                                {
                                    HandleEvent(ev);
                                }
                                else // READ_STATUS_SYNC
                                {
                                    rc = NextEvent(dev, READ_FLAG_SYNC, ref ev);
                                    while (rc == READ_STATUS_SYNC)
                                    {
                                        HandleEvent(ev);
                                        rc = NextEvent(dev, READ_FLAG_SYNC, ref ev);
                                    }
                                }
                            } while (rc == READ_STATUS_SUCCESS);
                        }
                    }
                }
            }

            EmitTouchMoveEvents();
        }

        public delegate void KeyDownEventHandler(Key key);
        public delegate void KeyUpEventHandler(Key key);
        public delegate void TouchMoveEventHandler(float x, float y, ulong id);
        public delegate void TouchDownEventHandler(float x, float y, ulong id);
        public delegate void TouchUpEventHandler(float x, float y, ulong id);

        public static KeyDownEventHandler KeyDown;
        public static KeyUpEventHandler KeyUp;

        public static TouchDownEventHandler TouchDown;
        public static TouchMoveEventHandler TouchMove;
        public static TouchUpEventHandler TouchUp;

        private const int READ_FLAG_SYNC = 1;
        private const int READ_FLAG_NORMAL = 2;
        private const int READ_FLAG_FORCE_SYNC = 4;
        private const int READ_FLAG_BLOCKING = 8;

        private const int READ_STATUS_SUCCESS = 0;
        private const int READ_STATUS_SYNC = 1;

        #region Event codes
        private const ushort EV_SYN = 0x00;
        private const ushort EV_KEY = 0x01;
        private const ushort EV_REL = 0x02;
        private const ushort EV_ABS = 0x03;
        private const ushort EV_MSC = 0x04;
        private const ushort EV_SW = 0x05;
        private const ushort EV_LED = 0x11;
        private const ushort EV_SND = 0x12;
        private const ushort EV_REP = 0x14;
        private const ushort EV_FF = 0x15;
        private const ushort EV_PWR = 0x16;
        private const ushort EV_FF_STATUS = 0x17;
        private const ushort EV_MAX = 0x1f;
        private const ushort EV_CNT = EV_MAX + 1;
        #endregion

        #region Key codes
        private const ushort KEY_RESERVED = 0;
        private const ushort KEY_ESC = 1;
        private const ushort KEY_1 = 2;
        private const ushort KEY_2 = 3;
        private const ushort KEY_3 = 4;
        private const ushort KEY_4 = 5;
        private const ushort KEY_5 = 6;
        private const ushort KEY_6 = 7;
        private const ushort KEY_7 = 8;
        private const ushort KEY_8 = 9;
        private const ushort KEY_9 = 10;
        private const ushort KEY_0 = 11;
        private const ushort KEY_MINUS = 12;
        private const ushort KEY_EQUAL = 13;
        private const ushort KEY_BACKSPACE = 14;
        private const ushort KEY_TAB = 15;
        private const ushort KEY_Q = 16;
        private const ushort KEY_W = 17;
        private const ushort KEY_E = 18;
        private const ushort KEY_R = 19;
        private const ushort KEY_T = 20;
        private const ushort KEY_Y = 21;
        private const ushort KEY_U = 22;
        private const ushort KEY_I = 23;
        private const ushort KEY_O = 24;
        private const ushort KEY_P = 25;
        private const ushort KEY_LEFTBRACE = 26;
        private const ushort KEY_RIGHTBRACE = 27;
        private const ushort KEY_ENTER = 28;
        private const ushort KEY_LEFTCTRL = 29;
        private const ushort KEY_A = 30;
        private const ushort KEY_S = 31;
        private const ushort KEY_D = 32;
        private const ushort KEY_F = 33;
        private const ushort KEY_G = 34;
        private const ushort KEY_H = 35;
        private const ushort KEY_J = 36;
        private const ushort KEY_K = 37;
        private const ushort KEY_L = 38;
        private const ushort KEY_SEMICOLON = 39;
        private const ushort KEY_APOSTROPHE = 40;
        private const ushort KEY_GRAVE = 41;
        private const ushort KEY_LEFTSHIFT = 42;
        private const ushort KEY_BACKSLASH = 43;
        private const ushort KEY_Z = 44;
        private const ushort KEY_X = 45;
        private const ushort KEY_C = 46;
        private const ushort KEY_V = 47;
        private const ushort KEY_B = 48;
        private const ushort KEY_N = 49;
        private const ushort KEY_M = 50;
        private const ushort KEY_COMMA = 51;
        private const ushort KEY_DOT = 52;
        private const ushort KEY_SLASH = 53;
        private const ushort KEY_RIGHTSHIFT = 54;
        private const ushort KEY_KPASTERISK = 55;
        private const ushort KEY_LEFTALT = 56;
        private const ushort KEY_SPACE = 57;
        private const ushort KEY_CAPSLOCK = 58;
        private const ushort KEY_F1 = 59;
        private const ushort KEY_F2 = 60;
        private const ushort KEY_F3 = 61;
        private const ushort KEY_F4 = 62;
        private const ushort KEY_F5 = 63;
        private const ushort KEY_F6 = 64;
        private const ushort KEY_F7 = 65;
        private const ushort KEY_F8 = 66;
        private const ushort KEY_F9 = 67;
        private const ushort KEY_F10 = 68;
        private const ushort KEY_NUMLOCK = 69;
        private const ushort KEY_SCROLLLOCK = 70;
        private const ushort KEY_KP7 = 71;
        private const ushort KEY_KP8 = 72;
        private const ushort KEY_KP9 = 73;
        private const ushort KEY_KPMINUS = 74;
        private const ushort KEY_KP4 = 75;
        private const ushort KEY_KP5 = 76;
        private const ushort KEY_KP6 = 77;
        private const ushort KEY_KPPLUS = 78;
        private const ushort KEY_KP1 = 79;
        private const ushort KEY_KP2 = 80;
        private const ushort KEY_KP3 = 81;
        private const ushort KEY_KP0 = 82;
        private const ushort KEY_KPDOT = 83;
        private const ushort KEY_ZENKAKUHANKAKU = 85;
        private const ushort KEY_102ND = 86;
        private const ushort KEY_F11 = 87;
        private const ushort KEY_F12 = 88;
        private const ushort KEY_RO = 89;
        private const ushort KEY_KATAKANA = 90;
        private const ushort KEY_HIRAGANA = 91;
        private const ushort KEY_HENKAN = 92;
        private const ushort KEY_KATAKANAHIRAGANA = 93;
        private const ushort KEY_MUHENKAN = 94;
        private const ushort KEY_KPJPCOMMA = 95;
        private const ushort KEY_KPENTER = 96;
        private const ushort KEY_RIGHTCTRL = 97;
        private const ushort KEY_KPSLASH = 98;
        private const ushort KEY_SYSRQ = 99;
        private const ushort KEY_RIGHTALT = 100;
        private const ushort KEY_LINEFEED = 101;
        private const ushort KEY_HOME = 102;
        private const ushort KEY_UP = 103;
        private const ushort KEY_PAGEUP = 104;
        private const ushort KEY_LEFT = 105;
        private const ushort KEY_RIGHT = 106;
        private const ushort KEY_END = 107;
        private const ushort KEY_DOWN = 108;
        private const ushort KEY_PAGEDOWN = 109;
        private const ushort KEY_INSERT = 110;
        private const ushort KEY_DELETE = 111;
        private const ushort KEY_MACRO = 112;
        private const ushort KEY_MUTE = 113;
        private const ushort KEY_VOLUMEDOWN = 114;
        private const ushort KEY_VOLUMEUP = 115;
        private const ushort KEY_POWER = 116;
        private const ushort KEY_KPEQUAL = 117;
        private const ushort KEY_KPPLUSMINUS = 118;
        private const ushort KEY_PAUSE = 119;
        private const ushort KEY_SCALE = 120;
        private const ushort KEY_KPCOMMA = 121;
        private const ushort KEY_HANGEUL = 122;
        private const ushort KEY_HANGUEL = KEY_HANGEUL;
        private const ushort KEY_HANJA = 123;
        private const ushort KEY_YEN = 124;
        private const ushort KEY_LEFTMETA = 125;
        private const ushort KEY_RIGHTMETA = 126;
        private const ushort KEY_COMPOSE = 127;
        private const ushort KEY_STOP = 128;
        private const ushort KEY_AGAIN = 129;
        private const ushort KEY_PROPS = 130;
        private const ushort KEY_UNDO = 131;
        private const ushort KEY_FRONT = 132;
        private const ushort KEY_COPY = 133;
        private const ushort KEY_OPEN = 134;
        private const ushort KEY_PASTE = 135;
        private const ushort KEY_FIND = 136;
        private const ushort KEY_CUT = 137;
        private const ushort KEY_HELP = 138;
        private const ushort KEY_MENU = 139;
        private const ushort KEY_CALC = 140;
        private const ushort KEY_SETUP = 141;
        private const ushort KEY_SLEEP = 142;
        private const ushort KEY_WAKEUP = 143;
        private const ushort KEY_FILE = 144;
        private const ushort KEY_SENDFILE = 145;
        private const ushort KEY_DELETEFILE = 146;
        private const ushort KEY_XFER = 147;
        private const ushort KEY_PROG1 = 148;
        private const ushort KEY_PROG2 = 149;
        private const ushort KEY_WWW = 150;
        private const ushort KEY_MSDOS = 151;
        private const ushort KEY_COFFEE = 152;
        private const ushort KEY_SCREENLOCK = KEY_COFFEE;
        private const ushort KEY_ROTATE_DISPLAY = 153;
        private const ushort KEY_DIRECTION = KEY_ROTATE_DISPLAY;
        private const ushort KEY_CYCLEWINDOWS = 154;
        private const ushort KEY_MAIL = 155;
        private const ushort KEY_BOOKMARKS = 156;
        private const ushort KEY_COMPUTER = 157;
        private const ushort KEY_BACK = 158;
        private const ushort KEY_FORWARD = 159;
        private const ushort KEY_CLOSECD = 160;
        private const ushort KEY_EJECTCD = 161;
        private const ushort KEY_EJECTCLOSECD = 162;
        private const ushort KEY_NEXTSONG = 163;
        private const ushort KEY_PLAYPAUSE = 164;
        private const ushort KEY_PREVIOUSSONG = 165;
        private const ushort KEY_STOPCD = 166;
        private const ushort KEY_RECORD = 167;
        private const ushort KEY_REWIND = 168;
        private const ushort KEY_PHONE = 169;
        private const ushort KEY_ISO = 170;
        private const ushort KEY_CONFIG = 171;
        private const ushort KEY_HOMEPAGE = 172;
        private const ushort KEY_REFRESH = 173;
        private const ushort KEY_EXIT = 174;
        private const ushort KEY_MOVE = 175;
        private const ushort KEY_EDIT = 176;
        private const ushort KEY_SCROLLUP = 177;
        private const ushort KEY_SCROLLDOWN = 178;
        private const ushort KEY_KPLEFTPAREN = 179;
        private const ushort KEY_KPRIGHTPAREN = 180;
        private const ushort KEY_NEW = 181;
        private const ushort KEY_REDO = 182;
        private const ushort KEY_F13 = 183;
        private const ushort KEY_F14 = 184;
        private const ushort KEY_F15 = 185;
        private const ushort KEY_F16 = 186;
        private const ushort KEY_F17 = 187;
        private const ushort KEY_F18 = 188;
        private const ushort KEY_F19 = 189;
        private const ushort KEY_F20 = 190;
        private const ushort KEY_F21 = 191;
        private const ushort KEY_F22 = 192;
        private const ushort KEY_F23 = 193;
        private const ushort KEY_F24 = 194;
        private const ushort KEY_PLAYCD = 200;
        private const ushort KEY_PAUSECD = 201;
        private const ushort KEY_PROG3 = 202;
        private const ushort KEY_PROG4 = 203;
        private const ushort KEY_DASHBOARD = 204;
        private const ushort KEY_SUSPEND = 205;
        private const ushort KEY_CLOSE = 206;
        private const ushort KEY_PLAY = 207;
        private const ushort KEY_FASTFORWARD = 208;
        private const ushort KEY_BASSBOOST = 209;
        private const ushort KEY_PRINT = 210;
        private const ushort KEY_HP = 211;
        private const ushort KEY_CAMERA = 212;
        private const ushort KEY_SOUND = 213;
        private const ushort KEY_QUESTION = 214;
        private const ushort KEY_EMAIL = 215;
        private const ushort KEY_CHAT = 216;
        private const ushort KEY_SEARCH = 217;
        private const ushort KEY_CONNECT = 218;
        private const ushort KEY_FINANCE = 219;
        private const ushort KEY_SPORT = 220;
        private const ushort KEY_SHOP = 221;
        private const ushort KEY_ALTERASE = 222;
        private const ushort KEY_CANCEL = 223;
        private const ushort KEY_BRIGHTNESSDOWN = 224;
        private const ushort KEY_BRIGHTNESSUP = 225;
        private const ushort KEY_MEDIA = 226;
        private const ushort KEY_SWITCHVIDEOMODE = 227;
        private const ushort KEY_KBDILLUMTOGGLE = 228;
        private const ushort KEY_KBDILLUMDOWN = 229;
        private const ushort KEY_KBDILLUMUP = 230;
        private const ushort KEY_SEND = 231;
        private const ushort KEY_REPLY = 232;
        private const ushort KEY_FORWARDMAIL = 233;
        private const ushort KEY_SAVE = 234;
        private const ushort KEY_DOCUMENTS = 235;
        private const ushort KEY_BATTERY = 236;
        private const ushort KEY_BLUETOOTH = 237;
        private const ushort KEY_WLAN = 238;
        private const ushort KEY_UWB = 239;
        private const ushort KEY_UNKNOWN = 240;
        private const ushort KEY_VIDEO_NEXT = 241;
        private const ushort KEY_VIDEO_PREV = 242;
        private const ushort KEY_BRIGHTNESS_CYCLE = 243;
        private const ushort KEY_BRIGHTNESS_AUTO = 244;
        private const ushort KEY_BRIGHTNESS_ZERO = KEY_BRIGHTNESS_AUTO;
        private const ushort KEY_DISPLAY_OFF = 245;
        private const ushort KEY_WWAN = 246;
        private const ushort KEY_WIMAX = KEY_WWAN;
        private const ushort KEY_RFKILL = 247;
        private const ushort KEY_MICMUTE = 248;

        private const ushort KEY_OK = 0x160;
        private const ushort KEY_SELECT = 0x161;
        private const ushort KEY_GOTO = 0x162;
        private const ushort KEY_CLEAR = 0x163;
        private const ushort KEY_POWER2 = 0x164;
        private const ushort KEY_OPTION = 0x165;
        private const ushort KEY_INFO = 0x166;
        private const ushort KEY_TIME = 0x167;
        private const ushort KEY_VENDOR = 0x168;
        private const ushort KEY_ARCHIVE = 0x169;
        private const ushort KEY_PROGRAM = 0x16a;
        private const ushort KEY_CHANNEL = 0x16b;
        private const ushort KEY_FAVORITES = 0x16c;
        private const ushort KEY_EPG = 0x16d;
        private const ushort KEY_PVR = 0x16e;
        private const ushort KEY_MHP = 0x16f;
        private const ushort KEY_LANGUAGE = 0x170;
        private const ushort KEY_TITLE = 0x171;
        private const ushort KEY_SUBTITLE = 0x172;
        private const ushort KEY_ANGLE = 0x173;
        private const ushort KEY_ZOOM = 0x174;
        private const ushort KEY_MODE = 0x175;
        private const ushort KEY_KEYBOARD = 0x176;
        private const ushort KEY_SCREEN = 0x177;
        private const ushort KEY_PC = 0x178;
        private const ushort KEY_TV = 0x179;
        private const ushort KEY_TV2 = 0x17a;
        private const ushort KEY_VCR = 0x17b;
        private const ushort KEY_VCR2 = 0x17c;
        private const ushort KEY_SAT = 0x17d;
        private const ushort KEY_SAT2 = 0x17e;
        private const ushort KEY_CD = 0x17f;
        private const ushort KEY_TAPE = 0x180;
        private const ushort KEY_RADIO = 0x181;
        private const ushort KEY_TUNER = 0x182;
        private const ushort KEY_PLAYER = 0x183;
        private const ushort KEY_TEXT = 0x184;
        private const ushort KEY_DVD = 0x185;
        private const ushort KEY_AUX = 0x186;
        private const ushort KEY_MP3 = 0x187;
        private const ushort KEY_AUDIO = 0x188;
        private const ushort KEY_VIDEO = 0x189;
        private const ushort KEY_DIRECTORY = 0x18a;
        private const ushort KEY_LIST = 0x18b;
        private const ushort KEY_MEMO = 0x18c;
        private const ushort KEY_CALENDAR = 0x18d;
        private const ushort KEY_RED = 0x18e;
        private const ushort KEY_GREEN = 0x18f;
        private const ushort KEY_YELLOW = 0x190;
        private const ushort KEY_BLUE = 0x191;
        private const ushort KEY_CHANNELUP = 0x192;
        private const ushort KEY_CHANNELDOWN = 0x193;
        private const ushort KEY_FIRST = 0x194;
        private const ushort KEY_LAST = 0x195;
        private const ushort KEY_AB = 0x196;
        private const ushort KEY_NEXT = 0x197;
        private const ushort KEY_RESTART = 0x198;
        private const ushort KEY_SLOW = 0x199;
        private const ushort KEY_SHUFFLE = 0x19a;
        private const ushort KEY_BREAK = 0x19b;
        private const ushort KEY_PREVIOUS = 0x19c;
        private const ushort KEY_DIGITS = 0x19d;
        private const ushort KEY_TEEN = 0x19e;
        private const ushort KEY_TWEN = 0x19f;
        private const ushort KEY_VIDEOPHONE = 0x1a0;
        private const ushort KEY_GAMES = 0x1a1;
        private const ushort KEY_ZOOMIN = 0x1a2;
        private const ushort KEY_ZOOMOUT = 0x1a3;
        private const ushort KEY_ZOOMRESET = 0x1a4;
        private const ushort KEY_WORDPROCESSOR = 0x1a5;
        private const ushort KEY_EDITOR = 0x1a6;
        private const ushort KEY_SPREADSHEET = 0x1a7;
        private const ushort KEY_GRAPHICSEDITOR = 0x1a8;
        private const ushort KEY_PRESENTATION = 0x1a9;
        private const ushort KEY_DATABASE = 0x1aa;
        private const ushort KEY_NEWS = 0x1ab;
        private const ushort KEY_VOICEMAIL = 0x1ac;
        private const ushort KEY_ADDRESSBOOK = 0x1ad;
        private const ushort KEY_MESSENGER = 0x1ae;
        private const ushort KEY_DISPLAYTOGGLE = 0x1af;
        private const ushort KEY_BRIGHTNESS_TOGGLE = KEY_DISPLAYTOGGLE;
        private const ushort KEY_SPELLCHECK = 0x1b0;
        private const ushort KEY_LOGOFF = 0x1b1;
        private const ushort KEY_DOLLAR = 0x1b2;
        private const ushort KEY_EURO = 0x1b3;
        private const ushort KEY_FRAMEBACK = 0x1b4;
        private const ushort KEY_FRAMEFORWARD = 0x1b5;
        private const ushort KEY_CONTEXT_MENU = 0x1b6;
        private const ushort KEY_MEDIA_REPEAT = 0x1b7;
        private const ushort KEY_10CHANNELSUP = 0x1b8;
        private const ushort KEY_10CHANNELSDOWN = 0x1b9;
        private const ushort KEY_IMAGES = 0x1ba;
        private const ushort KEY_DEL_EOL = 0x1c0;
        private const ushort KEY_DEL_EOS = 0x1c1;
        private const ushort KEY_INS_LINE = 0x1c2;
        private const ushort KEY_DEL_LINE = 0x1c3;
        private const ushort KEY_FN = 0x1d0;
        private const ushort KEY_FN_ESC = 0x1d1;
        private const ushort KEY_FN_F1 = 0x1d2;
        private const ushort KEY_FN_F2 = 0x1d3;
        private const ushort KEY_FN_F3 = 0x1d4;
        private const ushort KEY_FN_F4 = 0x1d5;
        private const ushort KEY_FN_F5 = 0x1d6;
        private const ushort KEY_FN_F6 = 0x1d7;
        private const ushort KEY_FN_F7 = 0x1d8;
        private const ushort KEY_FN_F8 = 0x1d9;
        private const ushort KEY_FN_F9 = 0x1da;
        private const ushort KEY_FN_F10 = 0x1db;
        private const ushort KEY_FN_F11 = 0x1dc;
        private const ushort KEY_FN_F12 = 0x1dd;
        private const ushort KEY_FN_1 = 0x1de;
        private const ushort KEY_FN_2 = 0x1df;
        private const ushort KEY_FN_D = 0x1e0;
        private const ushort KEY_FN_E = 0x1e1;
        private const ushort KEY_FN_F = 0x1e2;
        private const ushort KEY_FN_S = 0x1e3;
        private const ushort KEY_FN_B = 0x1e4;
        private const ushort KEY_BRL_DOT1 = 0x1f1;
        private const ushort KEY_BRL_DOT2 = 0x1f2;
        private const ushort KEY_BRL_DOT3 = 0x1f3;
        private const ushort KEY_BRL_DOT4 = 0x1f4;
        private const ushort KEY_BRL_DOT5 = 0x1f5;
        private const ushort KEY_BRL_DOT6 = 0x1f6;
        private const ushort KEY_BRL_DOT7 = 0x1f7;
        private const ushort KEY_BRL_DOT8 = 0x1f8;
        private const ushort KEY_BRL_DOT9 = 0x1f9;
        private const ushort KEY_BRL_DOT10 = 0x1fa;
        private const ushort KEY_NUMERIC_0 = 0x200;
        private const ushort KEY_NUMERIC_1 = 0x201;
        private const ushort KEY_NUMERIC_2 = 0x202;
        private const ushort KEY_NUMERIC_3 = 0x203;
        private const ushort KEY_NUMERIC_4 = 0x204;
        private const ushort KEY_NUMERIC_5 = 0x205;
        private const ushort KEY_NUMERIC_6 = 0x206;
        private const ushort KEY_NUMERIC_7 = 0x207;
        private const ushort KEY_NUMERIC_8 = 0x208;
        private const ushort KEY_NUMERIC_9 = 0x209;
        private const ushort KEY_NUMERIC_STAR = 0x20a;
        private const ushort KEY_NUMERIC_POUND = 0x20b;
        private const ushort KEY_NUMERIC_A = 0x20c;
        private const ushort KEY_NUMERIC_B = 0x20d;
        private const ushort KEY_NUMERIC_C = 0x20e;
        private const ushort KEY_NUMERIC_D = 0x20f;
        private const ushort KEY_CAMERA_FOCUS = 0x210;
        private const ushort KEY_WPS_BUTTON = 0x211;
        private const ushort KEY_TOUCHPAD_TOGGLE = 0x212;
        private const ushort KEY_TOUCHPAD_ON = 0x213;
        private const ushort KEY_TOUCHPAD_OFF = 0x214;
        private const ushort KEY_CAMERA_ZOOMIN = 0x215;
        private const ushort KEY_CAMERA_ZOOMOUT = 0x216;
        private const ushort KEY_CAMERA_UP = 0x217;
        private const ushort KEY_CAMERA_DOWN = 0x218;
        private const ushort KEY_CAMERA_LEFT = 0x219;
        private const ushort KEY_CAMERA_RIGHT = 0x21a;
        private const ushort KEY_ATTENDANT_ON = 0x21b;
        private const ushort KEY_ATTENDANT_OFF = 0x21c;
        private const ushort KEY_ATTENDANT_TOGGLE = 0x21d;
        private const ushort KEY_LIGHTS_TOGGLE = 0x21e;
        private const ushort BTN_DPAD_UP = 0x220;
        private const ushort BTN_DPAD_DOWN = 0x221;
        private const ushort BTN_DPAD_LEFT = 0x222;
        private const ushort BTN_DPAD_RIGHT = 0x223;
        private const ushort KEY_ALS_TOGGLE = 0x230;
        private const ushort KEY_ROTATE_LOCK_TOGGLE = 0x231;
        private const ushort KEY_BUTTONCONFIG = 0x240;
        private const ushort KEY_TASKMANAGER = 0x241;
        private const ushort KEY_JOURNAL = 0x242;
        private const ushort KEY_CONTROLPANEL = 0x243;
        private const ushort KEY_APPSELECT = 0x244;
        private const ushort KEY_SCREENSAVER = 0x245;
        private const ushort KEY_VOICECOMMAND = 0x246;
        private const ushort KEY_ASSISTANT = 0x247;
        private const ushort KEY_BRIGHTNESS_MIN = 0x250;
        private const ushort KEY_BRIGHTNESS_MAX = 0x251;
        private const ushort KEY_KBDINPUTASSIST_PREV = 0x260;
        private const ushort KEY_KBDINPUTASSIST_NEXT = 0x261;
        private const ushort KEY_KBDINPUTASSIST_PREVGROUP = 0x262;
        private const ushort KEY_KBDINPUTASSIST_NEXTGROUP = 0x263;
        private const ushort KEY_KBDINPUTASSIST_ACCEPT = 0x264;
        private const ushort KEY_KBDINPUTASSIST_CANCEL = 0x265;

        private const ushort KEY_RIGHT_UP = 0x266;
        private const ushort KEY_RIGHT_DOWN = 0x267;
        private const ushort KEY_LEFT_UP = 0x268;
        private const ushort KEY_LEFT_DOWN = 0x269;
        private const ushort KEY_ROOT_MENU = 0x26a;

        private const ushort KEY_MEDIA_TOP_MENU = 0x26b;
        private const ushort KEY_NUMERIC_11 = 0x26c;
        private const ushort KEY_NUMERIC_12 = 0x26d;

        private const ushort KEY_AUDIO_DESC = 0x26e;
        private const ushort KEY_3D_MODE = 0x26f;
        private const ushort KEY_NEXT_FAVORITE = 0x270;
        private const ushort KEY_STOP_RECORD = 0x271;
        private const ushort KEY_PAUSE_RECORD = 0x272;
        private const ushort KEY_VOD = 0x273;
        private const ushort KEY_UNMUTE = 0x274;
        private const ushort KEY_FASTREVERSE = 0x275;
        private const ushort KEY_SLOWREVERSE = 0x276;

        private const ushort KEY_DATA = 0x277;
        private const ushort KEY_ONSCREEN_KEYBOARD = 0x278;

        private const ushort KEY_MIN_INTERESTING = KEY_MUTE;
        private const ushort KEY_MAX = 0x2ff;
        private const ushort KEY_CNT = KEY_MAX + 1;
        #endregion

        #region Button codes
        private const ushort BTN_MISC = 0x100;
        private const ushort BTN_0 = 0x100;
        private const ushort BTN_1 = 0x101;
        private const ushort BTN_2 = 0x102;
        private const ushort BTN_3 = 0x103;
        private const ushort BTN_4 = 0x104;
        private const ushort BTN_5 = 0x105;
        private const ushort BTN_6 = 0x106;
        private const ushort BTN_7 = 0x107;
        private const ushort BTN_8 = 0x108;
        private const ushort BTN_9 = 0x109;
        private const ushort BTN_MOUSE = 0x110;
        private const ushort BTN_LEFT = 0x110;
        private const ushort BTN_RIGHT = 0x111;
        private const ushort BTN_MIDDLE = 0x112;
        private const ushort BTN_SIDE = 0x113;
        private const ushort BTN_EXTRA = 0x114;
        private const ushort BTN_FORWARD = 0x115;
        private const ushort BTN_BACK = 0x116;
        private const ushort BTN_TASK = 0x117;
        private const ushort BTN_JOYSTICK = 0x120;
        private const ushort BTN_TRIGGER = 0x120;
        private const ushort BTN_THUMB = 0x121;
        private const ushort BTN_THUMB2 = 0x122;
        private const ushort BTN_TOP = 0x123;
        private const ushort BTN_TOP2 = 0x124;
        private const ushort BTN_PINKIE = 0x125;
        private const ushort BTN_BASE = 0x126;
        private const ushort BTN_BASE2 = 0x127;
        private const ushort BTN_BASE3 = 0x128;
        private const ushort BTN_BASE4 = 0x129;
        private const ushort BTN_BASE5 = 0x12a;
        private const ushort BTN_BASE6 = 0x12b;
        private const ushort BTN_DEAD = 0x12f;
        private const ushort BTN_GAMEPAD = 0x130;
        private const ushort BTN_SOUTH = 0x130;
        private const ushort BTN_A = BTN_SOUTH;
        private const ushort BTN_EAST = 0x131;
        private const ushort BTN_B = BTN_EAST;
        private const ushort BTN_C = 0x132;
        private const ushort BTN_NORTH = 0x133;
        private const ushort BTN_X = BTN_NORTH;
        private const ushort BTN_WEST = 0x134;
        private const ushort BTN_Y = BTN_WEST;
        private const ushort BTN_Z = 0x135;
        private const ushort BTN_TL = 0x136;
        private const ushort BTN_TR = 0x137;
        private const ushort BTN_TL2 = 0x138;
        private const ushort BTN_TR2 = 0x139;
        private const ushort BTN_SELECT = 0x13a;
        private const ushort BTN_START = 0x13b;
        private const ushort BTN_MODE = 0x13c;
        private const ushort BTN_THUMBL = 0x13d;
        private const ushort BTN_THUMBR = 0x13e;
        private const ushort BTN_DIGI = 0x140;
        private const ushort BTN_TOOL_PEN = 0x140;
        private const ushort BTN_TOOL_RUBBER = 0x141;
        private const ushort BTN_TOOL_BRUSH = 0x142;
        private const ushort BTN_TOOL_PENCIL = 0x143;
        private const ushort BTN_TOOL_AIRBRUSH = 0x144;
        private const ushort BTN_TOOL_FINGER = 0x145;
        private const ushort BTN_TOOL_MOUSE = 0x146;
        private const ushort BTN_TOOL_LENS = 0x147;
        private const ushort BTN_TOOL_QUINTTAP = 0x148;
        private const ushort BTN_STYLUS3 = 0x149;
        private const ushort BTN_TOUCH = 0x14a;
        private const ushort BTN_STYLUS = 0x14b;
        private const ushort BTN_STYLUS2 = 0x14c;
        private const ushort BTN_TOOL_DOUBLETAP = 0x14d;
        private const ushort BTN_TOOL_TRIPLETAP = 0x14e;
        private const ushort BTN_TOOL_QUADTAP = 0x14f;
        private const ushort BTN_WHEEL = 0x150;
        private const ushort BTN_GEAR_DOWN = 0x150;
        private const ushort BTN_GEAR_UP = 0x151;

        private const ushort BTN_TRIGGER_HAPPY = 0x2c0;
        private const ushort BTN_TRIGGER_HAPPY1 = 0x2c0;
        private const ushort BTN_TRIGGER_HAPPY2 = 0x2c1;
        private const ushort BTN_TRIGGER_HAPPY3 = 0x2c2;
        private const ushort BTN_TRIGGER_HAPPY4 = 0x2c3;
        private const ushort BTN_TRIGGER_HAPPY5 = 0x2c4;
        private const ushort BTN_TRIGGER_HAPPY6 = 0x2c5;
        private const ushort BTN_TRIGGER_HAPPY7 = 0x2c6;
        private const ushort BTN_TRIGGER_HAPPY8 = 0x2c7;
        private const ushort BTN_TRIGGER_HAPPY9 = 0x2c8;
        private const ushort BTN_TRIGGER_HAPPY10 = 0x2c9;
        private const ushort BTN_TRIGGER_HAPPY11 = 0x2ca;
        private const ushort BTN_TRIGGER_HAPPY12 = 0x2cb;
        private const ushort BTN_TRIGGER_HAPPY13 = 0x2cc;
        private const ushort BTN_TRIGGER_HAPPY14 = 0x2cd;
        private const ushort BTN_TRIGGER_HAPPY15 = 0x2ce;
        private const ushort BTN_TRIGGER_HAPPY16 = 0x2cf;
        private const ushort BTN_TRIGGER_HAPPY17 = 0x2d0;
        private const ushort BTN_TRIGGER_HAPPY18 = 0x2d1;
        private const ushort BTN_TRIGGER_HAPPY19 = 0x2d2;
        private const ushort BTN_TRIGGER_HAPPY20 = 0x2d3;
        private const ushort BTN_TRIGGER_HAPPY21 = 0x2d4;
        private const ushort BTN_TRIGGER_HAPPY22 = 0x2d5;
        private const ushort BTN_TRIGGER_HAPPY23 = 0x2d6;
        private const ushort BTN_TRIGGER_HAPPY24 = 0x2d7;
        private const ushort BTN_TRIGGER_HAPPY25 = 0x2d8;
        private const ushort BTN_TRIGGER_HAPPY26 = 0x2d9;
        private const ushort BTN_TRIGGER_HAPPY27 = 0x2da;
        private const ushort BTN_TRIGGER_HAPPY28 = 0x2db;
        private const ushort BTN_TRIGGER_HAPPY29 = 0x2dc;
        private const ushort BTN_TRIGGER_HAPPY30 = 0x2dd;
        private const ushort BTN_TRIGGER_HAPPY31 = 0x2de;
        private const ushort BTN_TRIGGER_HAPPY32 = 0x2df;
        private const ushort BTN_TRIGGER_HAPPY33 = 0x2e0;
        private const ushort BTN_TRIGGER_HAPPY34 = 0x2e1;
        private const ushort BTN_TRIGGER_HAPPY35 = 0x2e2;
        private const ushort BTN_TRIGGER_HAPPY36 = 0x2e3;
        private const ushort BTN_TRIGGER_HAPPY37 = 0x2e4;
        private const ushort BTN_TRIGGER_HAPPY38 = 0x2e5;
        private const ushort BTN_TRIGGER_HAPPY39 = 0x2e6;
        private const ushort BTN_TRIGGER_HAPPY40 = 0x2e7;
        #endregion

        #region Touch codes
        private const ushort ABS_X = 0x00;
        private const ushort ABS_Y = 0x01;
        private const ushort ABS_Z = 0x02;
        private const ushort ABS_RX = 0x03;
        private const ushort ABS_RY = 0x04;
        private const ushort ABS_RZ = 0x05;
        private const ushort ABS_THROTTLE = 0x06;
        private const ushort ABS_RUDDER = 0x07;
        private const ushort ABS_WHEEL = 0x08;
        private const ushort ABS_GAS = 0x09;
        private const ushort ABS_BRAKE = 0x0a;
        private const ushort ABS_HAT0X = 0x10;
        private const ushort ABS_HAT0Y = 0x11;
        private const ushort ABS_HAT1X = 0x12;
        private const ushort ABS_HAT1Y = 0x13;
        private const ushort ABS_HAT2X = 0x14;
        private const ushort ABS_HAT2Y = 0x15;
        private const ushort ABS_HAT3X = 0x16;
        private const ushort ABS_HAT3Y = 0x17;
        private const ushort ABS_PRESSURE = 0x18;
        private const ushort ABS_DISTANCE = 0x19;
        private const ushort ABS_TILT_X = 0x1a;
        private const ushort ABS_TILT_Y = 0x1b;
        private const ushort ABS_TOOL_WIDTH = 0x1c;

        private const ushort ABS_VOLUME = 0x20;

        private const ushort ABS_MISC = 0x28;

        private const ushort ABS_RESERVED = 0x2e;

        private const ushort ABS_MT_SLOT = 0x2f;
        private const ushort ABS_MT_TOUCH_MAJOR = 0x30;
        private const ushort ABS_MT_TOUCH_MINOR = 0x31;
        private const ushort ABS_MT_WIDTH_MAJOR = 0x32;
        private const ushort ABS_MT_WIDTH_MINOR = 0x33;
        private const ushort ABS_MT_ORIENTATION = 0x34;
        private const ushort ABS_MT_POSITION_X = 0x35;
        private const ushort ABS_MT_POSITION_Y = 0x36;
        private const ushort ABS_MT_TOOL_TYPE = 0x37;
        private const ushort ABS_MT_BLOB_ID = 0x38;
        private const ushort ABS_MT_TRACKING_ID = 0x39;
        private const ushort ABS_MT_PRESSURE = 0x3a;
        private const ushort ABS_MT_DISTANCE = 0x3b;
        private const ushort ABS_MT_TOOL_X = 0x3c;
        private const ushort ABS_MT_TOOL_Y = 0x3d;

        private const ushort ABS_MAX = 0x3f;
        private const ushort ABS_CNT = ABS_MAX + 1;
        #endregion

        [StructLayout(LayoutKind.Sequential)]
        internal struct InputEvent
        {
            internal IntPtr input_event_sec;
            internal IntPtr input_event_usec;
            internal ushort type;
            internal ushort code;
            internal int @value;
        }

        private static void HandleKeyEvent(InputEvent ev)
        {
            Key key = _keyMap[ev.code];
            if (key != Key.None)
            {
                if (ev.value == 0)
                {
                    KeyUp?.Invoke(key);
                }
                else if (ev.value == 1 || ev.value == 2)
                {
                    KeyDown?.Invoke(key);
                }
            }
        }

        private static void HandleTouchEvent(InputEvent ev)
        {
            if (ev.code == ABS_MT_SLOT && ev.value < MaxSlots)
            {
                _currentMtSlot = ev.value;
                return;
            }

            if (_currentMtSlot >= 0)
            {
                switch (ev.code)
                {
                    case ABS_MT_TRACKING_ID:
                        if (ev.value != -1)
                        {
                            _mtSlotIds[_currentMtSlot] = ev.value;
                            _touchDown[_currentMtSlot] = true;
                        }
                        else
                        {
                            _touchUp[_currentMtSlot] = true;
                        }
                        break;
                    case ABS_MT_POSITION_X:
                        _mtSlotXs[_currentMtSlot] = (float)ev.value / 4096.0f;
                        _touchMove[_currentMtSlot] = true;
                        break;
                    case ABS_MT_POSITION_Y:
                        _mtSlotYs[_currentMtSlot] = (float)ev.value / 4096.0f;
                        _touchMove[_currentMtSlot] = true;
                        break;
                    default:
                        break;
                }
            }
        }

        private static void EmitEvents()
        {
            for (int i = 0; i < MaxSlots; ++i)
            {
                if (_touchDown[i])
                {
                    TouchDown?.Invoke(_mtSlotXs[i], _mtSlotYs[i], (ulong)_mtSlotIds[i]);
                    _touchDown[i] = false;
                    _touchMove[i] = false;
                }
                if (_touchUp[i])
                {
                    TouchUp?.Invoke(_mtSlotXs[i], _mtSlotYs[i], (ulong)_mtSlotIds[i]);
                    _touchUp[i] = false;
                    _touchMove[i] = false;
                }
            }
        }

        private static void EmitTouchMoveEvents()
        {
            for (int i = 0; i < MaxSlots; ++i)
            {
                if (_touchMove[i])
                {
                    TouchMove?.Invoke(_mtSlotXs[i], _mtSlotYs[i], (ulong)_mtSlotIds[i]);
                    _touchMove[i] = false;
                }
            }
        }

        private static void HandleEvent(InputEvent ev)
        {
            switch (ev.type)
            {
                case EV_KEY:
                    HandleKeyEvent(ev);
                    break;
                case EV_ABS:
                    HandleTouchEvent(ev);
                    break;
                case EV_SYN:
                    EmitEvents();
                    break;
            }
        }

        #region Key mapping
        private static void InitKeyMap()
        {
            _keyMap = new Key[KEY_CNT];

            _keyMap[KEY_CANCEL] = Key.Cancel;
            _keyMap[KEY_BACKSPACE] = Key.Back;
            _keyMap[KEY_TAB] = Key.Tab;
            _keyMap[KEY_LINEFEED] = Key.LineFeed;
            _keyMap[KEY_CLEAR] = Key.Clear;
            _keyMap[KEY_ENTER] = Key.Return;
            _keyMap[KEY_PAUSE] = Key.Pause;
            _keyMap[KEY_CAPSLOCK] = Key.CapsLock;
            _keyMap[KEY_ESC] = Key.Escape;
            _keyMap[KEY_SPACE] = Key.Space;
            _keyMap[KEY_PAGEUP] = Key.PageUp;
            _keyMap[KEY_PAGEDOWN] = Key.PageDown;
            _keyMap[KEY_END] = Key.End;
            _keyMap[KEY_HOME] = Key.Home;
            _keyMap[KEY_LEFT] = Key.Left;
            _keyMap[KEY_UP] = Key.Up;
            _keyMap[KEY_RIGHT] = Key.Right;
            _keyMap[KEY_DOWN] = Key.Down;
            _keyMap[KEY_SELECT] = Key.Select;
            //_keyMap[KEY_EXECUTE] = Key.Execute;
            _keyMap[KEY_PRINT] = Key.PrintScreen;
            _keyMap[KEY_INSERT] = Key.Insert;
            _keyMap[KEY_DELETE] = Key.Delete;
            _keyMap[KEY_HELP] = Key.Help;
            _keyMap[KEY_0] = Key.D0;
            _keyMap[KEY_1] = Key.D1;
            _keyMap[KEY_2] = Key.D2;
            _keyMap[KEY_3] = Key.D3;
            _keyMap[KEY_4] = Key.D4;
            _keyMap[KEY_5] = Key.D5;
            _keyMap[KEY_6] = Key.D6;
            _keyMap[KEY_7] = Key.D7;
            _keyMap[KEY_8] = Key.D8;
            _keyMap[KEY_9] = Key.D9;
            _keyMap[KEY_A] = Key.A;
            _keyMap[KEY_B] = Key.B;
            _keyMap[KEY_C] = Key.C;
            _keyMap[KEY_D] = Key.D;
            _keyMap[KEY_E] = Key.E;
            _keyMap[KEY_F] = Key.F;
            _keyMap[KEY_G] = Key.G;
            _keyMap[KEY_H] = Key.H;
            _keyMap[KEY_I] = Key.I;
            _keyMap[KEY_J] = Key.J;
            _keyMap[KEY_K] = Key.K;
            _keyMap[KEY_L] = Key.L;
            _keyMap[KEY_M] = Key.M;
            _keyMap[KEY_N] = Key.N;
            _keyMap[KEY_O] = Key.O;
            _keyMap[KEY_P] = Key.P;
            _keyMap[KEY_Q] = Key.Q;
            _keyMap[KEY_R] = Key.R;
            _keyMap[KEY_S] = Key.S;
            _keyMap[KEY_T] = Key.T;
            _keyMap[KEY_U] = Key.U;
            _keyMap[KEY_V] = Key.V;
            _keyMap[KEY_W] = Key.W;
            _keyMap[KEY_X] = Key.X;
            _keyMap[KEY_Y] = Key.Y;
            _keyMap[KEY_Z] = Key.Z;
            _keyMap[KEY_NUMERIC_0] = Key.NumPad0;
            _keyMap[KEY_NUMERIC_1] = Key.NumPad1;
            _keyMap[KEY_NUMERIC_2] = Key.NumPad2;
            _keyMap[KEY_NUMERIC_3] = Key.NumPad3;
            _keyMap[KEY_NUMERIC_4] = Key.NumPad4;
            _keyMap[KEY_NUMERIC_5] = Key.NumPad5;
            _keyMap[KEY_NUMERIC_6] = Key.NumPad6;
            _keyMap[KEY_NUMERIC_7] = Key.NumPad7;
            _keyMap[KEY_NUMERIC_8] = Key.NumPad8;
            _keyMap[KEY_NUMERIC_9] = Key.NumPad9;
            //_keyMap[KEY_NUM] = Key.Multiply;
            //_keyMap[KEY_] = Key.Add;
            //_keyMap[KEY_] = Key.Separator;
            //_keyMap[KEY_] = Key.Subtract;
            //_keyMap[KEY_] = Key.Decimal;
            //_keyMap[KEY_] = Key.Divide;
            _keyMap[KEY_F1] = Key.F1;
            _keyMap[KEY_F2] = Key.F2;
            _keyMap[KEY_F3] = Key.F3;
            _keyMap[KEY_F4] = Key.F4;
            _keyMap[KEY_F5] = Key.F5;
            _keyMap[KEY_F6] = Key.F6;
            _keyMap[KEY_F7] = Key.F7;
            _keyMap[KEY_F8] = Key.F8;
            _keyMap[KEY_F9] = Key.F9;
            _keyMap[KEY_F10] = Key.F10;
            _keyMap[KEY_F11] = Key.F11;
            _keyMap[KEY_F12] = Key.F12;
            _keyMap[KEY_F13] = Key.F13;
            _keyMap[KEY_F14] = Key.F14;
            _keyMap[KEY_F15] = Key.F15;
            _keyMap[KEY_F16] = Key.F16;
            _keyMap[KEY_F17] = Key.F17;
            _keyMap[KEY_F18] = Key.F18;
            _keyMap[KEY_F19] = Key.F19;
            _keyMap[KEY_F20] = Key.F20;
            _keyMap[KEY_F21] = Key.F21;
            _keyMap[KEY_F22] = Key.F22;
            _keyMap[KEY_F23] = Key.F23;
            _keyMap[KEY_F24] = Key.F24;
            _keyMap[KEY_NUMLOCK] = Key.NumLock;
            _keyMap[KEY_SCROLLLOCK] = Key.Scroll;
            _keyMap[KEY_LEFTSHIFT] = Key.LeftShift;
            _keyMap[KEY_RIGHTSHIFT] = Key.RightShift;
            _keyMap[KEY_LEFTCTRL] = Key.LeftCtrl;
            _keyMap[KEY_RIGHTCTRL] = Key.RightCtrl;
            _keyMap[KEY_LEFTALT] = Key.LeftAlt;
            _keyMap[KEY_RIGHTALT] = Key.RightAlt;
        }
        #endregion

        #region Imports
        [DllImport("libevdev.so.2", EntryPoint = "libevdev_new_from_fd")]
        private static extern int NewFromFd(int fd, ref IntPtr dev);

        [DllImport("libevdev.so.2", EntryPoint = "libevdev_has_event_pending")]
        private static extern int HasEventPending(IntPtr dev);

        [DllImport("libevdev.so.2", EntryPoint = "libevdev_next_event")]
        private static extern int NextEvent(IntPtr dev, uint flags, ref InputEvent ev);
        
        [DllImport("libevdev.so.2", EntryPoint = "libevdev_event_type_get_name")]
        private static extern IntPtr EventTypeGetName(ushort type);

        [DllImport("libevdev.so.2", EntryPoint = "libevdev_event_code_get_name")]
        private static extern IntPtr EventCodeGetName(ushort type, ushort code);
        #endregion

        #region Private members
        private static Key[] _keyMap;

        private static List<string> _inputDeviceNames;
        private static List<int> _inputDescriptors;
        private static List<IntPtr> _inputDevices;

        private static int _currentMtSlot;
        private const int MaxSlots = 16;
        private static int[] _mtSlotIds;
        private static float[] _mtSlotXs;
        private static float[] _mtSlotYs;
        private static bool[] _touchMove;
        private static bool[] _touchDown;
        private static bool[] _touchUp;
        #endregion
    }
}
