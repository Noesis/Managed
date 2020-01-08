using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace NoesisApp
{
    /// <summary>
    /// Use the CLR thread pool to execute a work in the background
    /// </summary>
    public struct ThreadPoolThreadSwitcher : INotifyCompletion
    {
        public ThreadPoolThreadSwitcher GetAwaiter()
        {
            return this;
        }

        public bool IsCompleted
        {
            get { return SynchronizationContext.Current == null; }
        }

        public void GetResult()
        {
        }

        public void OnCompleted(Action continuation)
        {
            ThreadPool.QueueUserWorkItem(state => continuation());
        }
    }
}
