using System;
using System.Runtime.InteropServices;

namespace Noesis
{
    public enum PlatformID
    {
        Unknown = -1,
        Windows,
        Linux,
        OSX,
        iPhone,
        Android,
        XboxOne,
        XboxSeries,
        PlayStation4,
        PlayStation5,
        Switch
    }

    public static class Platform
    {
        public static PlatformID ID { get; private set; }

        static Platform()
        {
            ID = (PlatformID)Noesis_GetPlatformID();
        }

        [DllImport(Library.Name)]
        static extern int Noesis_GetPlatformID();
    }
}
