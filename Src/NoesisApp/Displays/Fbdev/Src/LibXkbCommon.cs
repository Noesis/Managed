using System;
using System.Runtime.InteropServices;

namespace NoesisApp
{
    internal class LibXkbCommon
    {
        internal const int CONTEXT_NO_FLAGS = 0;
        internal const int CONTEXT_NO_DEFAULT_INCLUDES = (1 << 0);
        internal const int CONTEXT_NO_ENVIRONMENT_NAMES = (1 << 1);

        internal const int KEYMAP_COMPILE_NO_FLAGS = 0;

        internal const int COMPOSE_COMPILE_NO_FLAGS = 0;

        internal const int COMPOSE_STATE_NO_FLAGS = 0;

        internal const int COMPOSE_NOTHING = 0;
        internal const int COMPOSE_COMPOSING = 1;
        internal const int COMPOSE_COMPOSED = 2;
        internal const int COMPOSE_CANCELLED = 3;

        internal const int KEY_UP = 0;
        internal const int KEY_DOWN = 1;

        internal const uint KEY_NoSymbol = 0;

        [StructLayout(LayoutKind.Sequential)]
        internal struct RuleNames
        {
            [MarshalAs(UnmanagedType.LPStr)]
            internal string rules;
            
            [MarshalAs(UnmanagedType.LPStr)]
            internal string model;
            
            [MarshalAs(UnmanagedType.LPStr)]
            internal string layout;
            
            [MarshalAs(UnmanagedType.LPStr)]
            internal string variant;
            
            [MarshalAs(UnmanagedType.LPStr)]
            internal string options;
        }

        internal static int StateKeyGetSyms(IntPtr state, uint keycode, out uint[] keysyms)
        {
            IntPtr keysyms_;
            int numKeysyms = xkb_state_key_get_syms(state, keycode, out keysyms_);
            if (numKeysyms > 0)
            {
                keysyms = new uint[numKeysyms];
                for (int i = 0; i < numKeysyms; ++i)
                {
                    keysyms[i] = (uint)Marshal.ReadInt32(keysyms_, i * 4);
                }
            }
            else
            {
                keysyms = null;
            }
            return numKeysyms;
        }

        [DllImport("libxkbcommon.so.0", EntryPoint = "xkb_context_new")]
        internal static extern IntPtr ContextNew(int flags);

        [DllImport("libxkbcommon.so.0", EntryPoint = "xkb_keymap_new_from_names")]
        internal static extern IntPtr KeymapNewFromNames(IntPtr context, ref RuleNames names, int flags);

        [DllImport("libxkbcommon.so.0", EntryPoint = "xkb_compose_table_new_from_locale")]
        internal static extern IntPtr ComposeTableNewFromLocale(IntPtr context, [MarshalAs(UnmanagedType.LPStr)] string locale, int flags);

        [DllImport("libxkbcommon.so.0", EntryPoint = "xkb_state_new")]
        internal static extern IntPtr StateNew(IntPtr keymap);

        [DllImport("libxkbcommon.so.0", EntryPoint = "xkb_compose_state_new")]
        internal static extern IntPtr ComposeStateNew(IntPtr table, int flags);

        [DllImport("libxkbcommon.so.0", EntryPoint = "xkb_keymap_key_repeats")]
        internal static extern int KeymapKeyRepeats(IntPtr keymap, uint key);

        [DllImport("libxkbcommon.so.0", EntryPoint = "xkb_state_key_get_one_sym")]
        internal static extern uint StateKeyGetOneSym(IntPtr state, uint key);

        [DllImport("libxkbcommon.so.0", EntryPoint = "xkb_compose_state_feed")]
        internal static extern int ComposeStateFeed(IntPtr state, uint keysym);

        [DllImport("libxkbcommon.so.0", EntryPoint = "xkb_compose_state_get_status")]
        internal static extern int ComposeStateGetStatus(IntPtr state);

        [DllImport("libxkbcommon.so.0", EntryPoint = "xkb_compose_state_reset")]
        internal static extern void ComposeStateReset(IntPtr state);

        [DllImport("libxkbcommon.so.0", EntryPoint = "xkb_state_key_get_syms")]
        private static extern int xkb_state_key_get_syms(IntPtr state, uint keycode, out IntPtr keysyms);
        
        [DllImport("libxkbcommon.so.0", EntryPoint = "xkb_compose_state_get_one_sym")]
        internal static extern uint ComposeStateGetOneSym(IntPtr state);
        
        [DllImport("libxkbcommon.so.0", EntryPoint = "xkb_keysym_to_utf32")]
        internal static extern uint KeysymToUtf32(uint keysym);
        
        [DllImport("libxkbcommon.so.0", EntryPoint = "xkb_state_update_key")]
        internal static extern int StateUpdateKey(IntPtr state, uint keycode, int direction);
    }
}