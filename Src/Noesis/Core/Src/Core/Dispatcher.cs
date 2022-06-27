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
        public SynchronizationContext SynchronizationContext
        {
            get { return _context; }
        }

        /// <summary>
        /// Determines whether the calling thread is the thread associated with this Dispatcher.
        /// </summary>
        public bool CheckAccess()
        {
            return ThreadId == CurrentThreadId && ManagedThreadId == CurrentManagedThreadId;
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
            BeginInvoke(DispatcherPriority.Normal, action, null);
        }

        /// <summary>
        /// Executes the specified delegate asynchronously on the thread the Dispatcher is associated with.
        /// </summary>
        public void BeginInvoke(Delegate d, object args)
        {
            BeginInvoke(DispatcherPriority.Normal, d, args);
        }

        /// <summary>
        /// Executes the specified delegate asynchronously on the thread the Dispatcher is associated with.
        /// </summary>
        public void BeginInvoke(DispatcherPriority priority, Action action)
        {
            BeginInvoke(priority, action, null);
        }

        /// <summary>
        /// Executes the specified delegate asynchronously on the thread the Dispatcher is associated with.
        /// </summary>
        public void BeginInvoke(DispatcherPriority priority, Delegate d, object args)
        {
            AddOperation(priority, d, args);
        }

        /// <summary>
        /// Executes the specified action synchronously on the thread the Dispatcher is associated with.
        /// </summary>
        public void Invoke(Action action)
        {
            Invoke(DispatcherPriority.Send, action, null);
        }

        /// <summary>
        /// Executes the specified delegate synchronously on the thread the Dispatcher is associated with.
        /// </summary>
        public void Invoke(Delegate d, object args)
        {
            Invoke(DispatcherPriority.Send, d, args);
        }

        /// <summary>
        /// Executes the specified delegate synchronously on the thread the Dispatcher is associated with.
        /// </summary>
        public void Invoke(DispatcherPriority priority, Action action)
        {
            Invoke(priority, action, null);
        }

        /// <summary>
        /// Executes the specified delegate synchronously on the thread the Dispatcher is associated with.
        /// </summary>
        public void Invoke(DispatcherPriority priority, Delegate d, object args)
        {
            AddOperation(priority, d, args, new AutoResetEvent(false));
        }
        #endregion

        #region Queue processing
        private void AddOperation(DispatcherPriority priority, Delegate d, object args, AutoResetEvent wait = null)
        {
            if (priority == DispatcherPriority.Invalid ||
                priority == DispatcherPriority.Inactive)
            {
                throw new ArgumentException("Invalid Priority");
            }

            if (d == null)
            {
                throw new ArgumentNullException("Null method");
            }

            if (priority == DispatcherPriority.Send && CheckAccess())
            {
                // Fast path: invoking at Send priority
                DispatcherOperation.Invoke(d, args, _context);
            }
            else
            {
                // Slow path: going through the queue
                DispatcherOperation operation = new DispatcherOperation
                {
                    Priority = priority,
                    Callback = d,
                    Args = args,
                    WaitEvent = wait
                };

                lock (_operations)
                {
                    _operations.Enqueue(operation);
                }

                if (wait != null)
                {
                    if (CheckAccess())
                    {
                        // We cannot lock this thread, so process the queue now
                        ProcessQueue();
                    }
                    else
                    {
                        // Block this thread and wait for the operation to get finished
                        operation.Wait();
                    }
                }
            }
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
                    if (_operations.Count == 0)
                    {
                        // queue already processed by a synchronous Invoke
                        break;
                    }

                    operation = _operations.Dequeue();
                }

                operation.Invoke(_context);
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

        #region Managed Thread Id

        private int ManagedThreadId { get => _managedThreadId; }

        private static int CurrentManagedThreadId
        {
            get { return Thread.CurrentThread.ManagedThreadId; }
        }
        #endregion

        #region Private members
        private Dispatcher(int threadId)
        {
            _threadId = threadId;
            _managedThreadId = CurrentManagedThreadId;
            _operations = new Queue<DispatcherOperation>();
            _context = new DispatcherSynchronizationContext(this);
        }

        private static Dictionary<int, Dispatcher> _dispatchers =
            new Dictionary<int, Dispatcher>();

        private int _threadId;
        private int _managedThreadId;
        private Queue<DispatcherOperation> _operations;
        private DispatcherSynchronizationContext _context;

        private struct DispatcherOperation
        {
            public DispatcherPriority Priority;
            public Delegate Callback;
            public object Args;
            public AutoResetEvent WaitEvent;

            public static void Invoke(Delegate callback, object args, SynchronizationContext context)
            {
                SynchronizationContext currentContext = SynchronizationContext.Current;
                try
                {
                    SynchronizationContext.SetSynchronizationContext(context);

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
                catch (OperationCanceledException) { }
                catch (Exception e)
                {
                    Error.UnhandledException(e);
                }
                finally
                {
                    SynchronizationContext.SetSynchronizationContext(currentContext);
                }
            }

            public void Invoke(SynchronizationContext context)
            {
                // Invoke callback
                Invoke(Callback, Args, context);

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

    /// <summary>
    ///     An enunmeration describing the priorities at which
    ///     operations can be invoked via the Dispatcher.
    /// </summary>
    ///
    public enum DispatcherPriority
    {
        /// <summary> This is an invalid priority.</summary>
        Invalid = -1,

        /// <summary>Operations at this priority are not processed.</summary>
        Inactive = 0,

        /// <summary>Operations at this priority are processed when the system is idle.</summary>
        SystemIdle,

        /// <summary>Operations at this priority are processed when the application is idle.</summary>
        ApplicationIdle,

        /// <summary>Operations at this priority are processed when the context is idle.</summary>
        ContextIdle,

        /// <summary>Operations at this priority are processed after all other non-idle operations are done.</summary>
        Background,

        /// <summary>Operations at this priority are processed at the same priority as input.</summary>
        Input,

        /// <summary>
        ///     Operations at this priority are processed when layout and render is
        ///     done but just before items at input priority are serviced. Specifically
        ///     this is used while firing the Loaded event
        /// </summary>
        Loaded,

        /// <summary>Operations at this priority are processed at the same priority as rendering.</summary>
        Render,

        /// <summary>Operations at this priority are processed at the same priority as data binding.</summary>
        DataBind,

        /// <summary>Operations at this priority are processed at normal priority.</summary>
        Normal,

        /// <summary>Operations at this priority are processed before other asynchronous operations.</summary>
        Send
    }
}
