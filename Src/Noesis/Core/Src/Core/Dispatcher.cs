using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

namespace Noesis
{
    /// <summary>
    /// Provides services for managing the queue of work items for a thread.
    /// </summary>
    public sealed class Dispatcher
    {
        /// <summary>
        /// Gets the Dispatcher for the thread currently executing and creates a new Dispatcher
        /// if one is not already associated with the thread.
        /// </summary>
        public static Dispatcher CurrentDispatcher
        {
            get { return FromThreadId(CurrentThreadId); }
        }

        /// <summary>
        /// Gets the thread Id this Dispatcher is associated with.
        /// </summary>
        public int ThreadId
        {
            get { return _threadId; }
        }

        /// <summary>
        /// Gets the synchronization context associated with this Dispatcher.
        /// </summary>
        internal SynchronizationContext SynchronizationContext
        {
            get { return _context; }
        }

        /// <summary>
        /// Determines whether the calling thread is the thread associated with this Dispatcher.
        /// </summary>
        public bool CheckAccess()
        {
            return ThreadId == CurrentThreadId;
        }

        /// <summary>
        /// Determines whether the calling thread has access to this Dispatcher.
        /// </summary>
        public void VerifyAccess()
        {
            if (!CheckAccess())
            {
                throw new InvalidOperationException(
                    "The calling thread cannot access this object because a different thread owns it");
            }
        }

        #region Invoke
        /// <summary>
        /// Executes the specified action asynchronously on the thread the Dispatcher is associated with.
        /// </summary>
        public void BeginInvoke(Action action)
        {
            BeginInvoke(action, null);
        }

        /// <summary>
        /// Executes the specified delegate asynchronously on the thread the Dispatcher is associated with.
        /// </summary>
        public void BeginInvoke(Delegate d, object args)
        {
            AddOperation(d, args);
        }

        /// <summary>
        /// Executes the specified action synchronously on the thread the Dispatcher is associated with.
        /// </summary>
        public void Invoke(Action action)
        {
            Invoke(action, null);
        }

        /// <summary>
        /// Executes the specified delegate synchronously on the thread the Dispatcher is associated with.
        /// </summary>
        public void Invoke(Delegate d, object args)
        {
            if (CheckAccess())
            {
                DispatcherOperation.Invoke(d, args);
            }
            else
            {
                DispatcherOperation operation = AddOperation(d, args, new AutoResetEvent(false));
                operation.Wait();
            }
        }
        #endregion

        #region Queue processing
        private DispatcherOperation AddOperation(Delegate d, object args, AutoResetEvent wait = null)
        {
            DispatcherOperation operation = new DispatcherOperation
            {
                Callback = d,
                Args = args,
                WaitEvent = wait
            };

            lock (_operations)
            {
                _operations.Enqueue(operation);
            }

            return operation;
        }

        internal void ProcessQueue()
        {
            VerifyAccess();

            int pendingOperations;
            lock (_operations)
            {
                pendingOperations = _operations.Count;
            }

            for (int i = 0; i < pendingOperations; ++i)
            {
                DispatcherOperation operation;

                lock (_operations)
                {
                    operation = _operations.Dequeue();
                }

                SynchronizationContext currentContext = SynchronizationContext.Current;
                try
                {
                    SynchronizationContext.SetSynchronizationContext(_context);

                    operation.Invoke();
                }
                finally
                {
                    SynchronizationContext.SetSynchronizationContext(currentContext);
                }
            }
        }
        #endregion

        #region Thread Id
        internal static Dispatcher FromThreadId(int threadId)
        {
            Dispatcher dispatcher;

            lock (_dispatchers)
            {
                if (!_dispatchers.TryGetValue(threadId, out dispatcher))
                {
                    dispatcher = new Dispatcher(threadId);
                    _dispatchers.Add(threadId, dispatcher);
                }
            }

            return dispatcher;
        }

        private static int CurrentThreadId
        {
            get { return Noesis_GetCurrentThreadId(); }
        }

        [DllImport(Library.Name)]
        private static extern int Noesis_GetCurrentThreadId();
        #endregion

        #region Private members
        private Dispatcher(int threadId)
        {
            _threadId = threadId;
            _operations = new Queue<DispatcherOperation>();
            _context = new DispatcherSynchronizationContext(this);
        }

        private static Dictionary<int, Dispatcher> _dispatchers =
            new Dictionary<int, Dispatcher>();

        private int _threadId;
        private Queue<DispatcherOperation> _operations;
        private DispatcherSynchronizationContext _context;

        private struct DispatcherOperation
        {
            public Delegate Callback;
            public object Args;
            public AutoResetEvent WaitEvent;

            public static void Invoke(Delegate callback, object args)
            {
                if (callback is Action)
                {
                    Action action = (Action)callback;
                    action();
                }
                else if (callback is SendOrPostCallback)
                {
                    SendOrPostCallback sendOrPost = (SendOrPostCallback)callback;
                    sendOrPost(args);
                }
                else
                {
                    callback.DynamicInvoke(args);
                }
            }

            public void Invoke()
            {
                // Invoke callback
                Invoke(Callback, Args);

                // Signal waiting event
                if (WaitEvent != null)
                {
                    WaitEvent.Set();
                }
            }

            public void Wait()
            {
                if (WaitEvent != null)
                {
                    WaitEvent.WaitOne();
                }
            }
        }
        #endregion
    }
}
