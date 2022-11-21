using System;
using System.Reflection;

namespace NoesisApp
{
    public static class Theme
    {
        static Theme()
        {
            if (Noesis.Platform.ID == Noesis.PlatformID.Windows)
            {
                FontFallbacks = new string[]
                {
                    "/Noesis.GUI.Extensions;component/Theme/Fonts/#PT Root UI",
                    "Arial",
                    "Segoe UI Emoji",           // Windows 10 Emojis
                    "Arial Unicode MS",         // Almost everything (but part of MS Office, not Windows)
                    "Microsoft Sans Serif",     // Unicode scripts excluding Asian scripts
                    "Microsoft YaHei",          // Chinese
                    "Gulim",                    // Korean
                    "MS Gothic"                 // Japanese
                };
            }
            else if (Noesis.Platform.ID == Noesis.PlatformID.OSX)
            {
                FontFallbacks = new string[]
                {
                    "/Noesis.GUI.Extensions;component/Theme/Fonts/#PT Root UI",
                    "Arial",
                    "Arial Unicode MS"          // MacOS 10.5+
                };
            }
            else if (Noesis.Platform.ID == Noesis.PlatformID.iPhone)
            {
                FontFallbacks = new string[]
                {
                    "/Noesis.GUI.Extensions;component/Theme/Fonts/#PT Root UI",
                    "PingFang SC",              // Simplified Chinese (iOS 9+)
                    "Apple SD Gothic Neo",      // Korean (iOS 7+)
                    "Hiragino Sans"             // Japanese (iOS 9+)
                };
            }
            else if (Noesis.Platform.ID == Noesis.PlatformID.Android)
            {
                FontFallbacks = new string[]
                {
                    "/Noesis.GUI.Extensions;component/Theme/Fonts/#PT Root UI",
                    "Noto Sans CJK SC",         // Simplified Chinese
                    "Noto Sans CJK KR",         // Korean
                    "Noto Sans CJK JP"          // Japanese
                };
            }
            else if (Noesis.Platform.ID == Noesis.PlatformID.Linux)
            {
                FontFallbacks = new string[]
                {
                    "/Noesis.GUI.Extensions;component/Theme/Fonts/#PT Root UI",
                    "Noto Sans CJK SC",         // Simplified Chinese
                    "Noto Sans CJK KR",         // Korean
                    "Noto Sans CJK JP"          // Japanese
                };
            }
            else
            {
                FontFallbacks = new string[]
                {
                    "/Noesis.GUI.Extensions;component/Theme/Fonts/#PT Root UI"
                };
            }
        }

        public static Assembly Assembly
        {
            get { return typeof(Theme).Assembly; }
        }

        public static string Namespace
        {
            get { return typeof(Theme).Namespace; }
        }

        public static string[] FontFallbacks { get; private set; }

        public static float DefaultFontSize { get; } = 15.0f;
        public static Noesis.FontWeight DefaultFontWeight { get; } = Noesis.FontWeight.Normal;
        public static Noesis.FontStretch DefaultFontStretch { get; } = Noesis.FontStretch.Normal;
        public static Noesis.FontStyle DefaultFontStyle { get; } = Noesis.FontStyle.Normal;
    }
}
