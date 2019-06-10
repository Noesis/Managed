using System;
using System.Runtime.InteropServices;

namespace Noesis
{
    public enum LogLevel
    {
        Trace = 0,
        Debug = 1,
        Info = 2,
        Warning = 3,
        Error = 4
    }

    public delegate void LogCallback(LogLevel level, string channel, string message);

    public class Log
    {
        public static void SetLogCallback(LogCallback callback)
        {
            _logCallback = callback;
            Noesis_RegisterLogCallback(callback != null ? _noesisLogCallback : null);
        }

        #region Native to managed callback
        private static LogCallback _logCallback;

        private delegate void NativeLogCallback(uint level,
            [MarshalAs(UnmanagedType.LPWStr)]string channel,
            [MarshalAs(UnmanagedType.LPWStr)]string message);
        private static NativeLogCallback _noesisLogCallback = OnLog;

        [MonoPInvokeCallback(typeof(NativeLogCallback))]
        private static void OnLog(uint level, string channel, string message)
        {
            if (_logCallback != null)
            {
                _logCallback((LogLevel)level, channel, message);
            }
        }

        internal static void Error(string message)
        {
            OnLog((int)LogLevel.Error, "", message);
        }

        [DllImport(Library.Name)]
        private static extern void Noesis_RegisterLogCallback(NativeLogCallback callback);
        #endregion
    }
}
