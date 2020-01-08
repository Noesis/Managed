using Noesis;
using System;

namespace NoesisApp
{
    /// <summary>
    /// Helper class to switch context to a background thread
    /// </summary>
    public class ThreadSwitcher
    {
        static public ThreadPoolThreadSwitcher ResumeBackgroundAsync()
        {
            return new ThreadPoolThreadSwitcher();
        }

        static public DispatcherThreadSwitcher ResumeForegroundAsync(Dispatcher dispatcher)
        {
            return new DispatcherThreadSwitcher(dispatcher);
        }
    }
}
