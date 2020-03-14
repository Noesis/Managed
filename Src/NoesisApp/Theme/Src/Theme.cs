using System;
using System.Reflection;

namespace NoesisApp
{
    public static class Theme
    {
        public static Assembly Assembly
        {
            get { return typeof(Theme).Assembly; }
        }

        public static string Namespace
        {
            get { return typeof(Theme).Namespace; }
        }

        public static string[] FontFallbacks { get; } = new string[] { "Theme/Fonts/#PT Root UI" };

        public static float DefaultFontSize { get; } = 15.0f;
        public static Noesis.FontWeight DefaultFontWeight { get; } = Noesis.FontWeight.Normal;
        public static Noesis.FontStretch DefaultFontStretch { get; } = Noesis.FontStretch.Normal;
        public static Noesis.FontStyle DefaultFontStyle { get; } = Noesis.FontStyle.Normal;
    }
}
