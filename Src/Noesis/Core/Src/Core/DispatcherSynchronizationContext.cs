using System;
using System.Threading;

namespace Noesis
{
    /// <summary>
    /// Provides a synchronization context for Noesis.
    /// </summary>
    public sealed class DispatcherSynchronizationContext : SynchronizationContext
    {
        public DispatcherSynchronizationContext() : this(Dispatcher.CurrentDispatcher)
        {
        }

        public DispatcherSynchronizationContext(Dispatcher dispatcher)
        {
            if (dispatcher == null)
            {
                throw new ArgumentNullException("dispatcher");
            }
            _dispatcher = dispatcher;
            SetWaitNotificationRequired();
        }

        #region SynchronizationContext implementation
        public override void Send(SendOrPostCallback d, object state)
        {
            _dispatcher.Invoke(d, state);
        }

        public override void Post(SendOrPostCallback d, object state)
        {
            _dispatcher.BeginInvoke(d, state);
        }

        public override SynchronizationContext CreateCopy()
        {
            return new DispatcherSynchronizationContext(_dispatcher);
        }
        #endregion

        #region Private members
        private Dispatcher _dispatcher;
        #endregion
    }
}