using System;

namespace Noesis
{
    public delegate void UnhandledExceptionCallback(Exception exception);

    public class Error
    {
        public static void SetUnhandledCallback(UnhandledExceptionCallback callback)
        {
            _unhandledCallback = callback;
        }

        public static void UnhandledException(Exception exception)
        {
            if (_unhandledCallback != null)
            {
                _unhandledCallback(exception);
            }
        }

        private static UnhandledExceptionCallback _unhandledCallback;
    }
}

