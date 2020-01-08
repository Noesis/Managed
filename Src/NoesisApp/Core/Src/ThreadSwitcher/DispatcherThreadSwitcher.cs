using Noesis;
using System;
using System.Runtime.CompilerServices;

namespace NoesisApp
{
    /// <summary>
    /// Allows executing some work on the Dispatcher thread
    /// </summary>
    public struct DispatcherThreadSwitcher : INotifyCompletion
    {
        public DispatcherThreadSwitcher GetAwaiter()
        {
            return this;
        }

        public bool IsCompleted
        {
            get { return dispatcher.CheckAccess(); }
        }

        public void GetResult()
        {
        }

        public void OnCompleted(Action continuation)
        {
            dispatcher.BeginInvoke(continuation);
        }

        internal DispatcherThreadSwitcher(Dispatcher dispatcher)
        {
            this.dispatcher = dispatcher;
        }

        private Dispatcher dispatcher;
    }
}
