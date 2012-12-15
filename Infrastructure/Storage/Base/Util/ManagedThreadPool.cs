// Stephen Toub
// stoub@microsoft.com
// 
// ManagedThreadPool.cs
// ThreadPool written in 100% managed code.  Mimics the core functionality of
// the System.Threading.ThreadPool class.
//
// HISTORY:
// v1.0.1 - Disposes of items remaining in queue when the queue is emptied
//		  - Catches errors thrown during execution of delegates
//		  - Added reset to semaphore, called during empty queue
//		  - Catches errors when unable to dequeue delegates
// v1.0.0 - Original version
// 
// August 27, 2002
// v1.0.1

// http://www.gotdotnet.com/community/usersamples/Default.aspx?query=ManagedThreadPool

#region Namespaces
using System;
using System.Linq;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
#endregion

namespace Runner.Base.Util
{
    /// <summary>Implementation of Dijkstra's PV Semaphore based on the Monitor class.</summary>
    public class CustomSemaphore
    {
        /// <summary>
        /// log4net log instance
        /// </summary>
        private readonly static log4net.ILog log = log4net.LogManager.GetLogger(
            typeof(CustomSemaphore).Name);

        #region Member Variables
        /// <summary>The number of units alloted by this semaphore.</summary>
        private int _count;
        private volatile bool _shouldBreak;
        #endregion

        #region Construction
        /// <summary> Initialize the semaphore as a binary semaphore.</summary>
        public CustomSemaphore()
            : this(1)
        {
            _shouldBreak = false;
        }

        /// <summary> Initialize the semaphore as a counting semaphore.</summary>
        /// <param name="count">Initial number of threads that can take out units from this semaphore.</param>
        /// <exception cref="ArgumentException">Throws if the count argument is less than 1.</exception>
        public CustomSemaphore(int count)
        {
            if (count < 0) throw new ArgumentException("Semaphore must have a count of at least 0.", "count");
            _count = count;
        }
        #endregion

        #region Synchronization Operations
        /// <summary>V the semaphore (add 1 unit to it).</summary>
        public void AddOne() { V(); }

        public void StopWait() 
        {
            _shouldBreak = true;
        }

        /// <summary>P the semaphore (take out 1 unit from it).</summary>
        public void WaitOne() { P(); }

        /// <summary>P the semaphore (take out 1 unit from it).</summary>
        private void P()
        {
            try
            {
                // Lock so we can work in peace.  This works because lock is actually
                // built around Monitor.
                //lock (this){
                    // Wait until a unit becomes available.  We need to wait
                    // in a loop in case someone else wakes up before us.  This could
                    // happen if the Monitor.Pulse statements were changed to Monitor.PulseAll
                    // statements in order to introduce some randomness into the order
                    // in which threads are woken.
                    try
                    {
                        /*/
                        while (_count <= 0)Monitor.Wait(this, Timeout.Infinite);
                        _count--;
                        /*/
                        while (_count <= 0 || !_shouldBreak)
                        {
                            if (this!=null)
                            {
                                lock (this)
                                {
                                    Monitor.Wait(this, 100);
                                    //Monitor.Wait(this, Timeout.Infinite);
                                    if (_count > 0)
                                    {
                                        _count--;
                                        break;
                                    }
                                }
                            }else
                            {
                                break;
                            }
                        }
                        if (!_shouldBreak)
                        {
                        }
                        //*/
                    }
                    catch (System.Exception ex)
                    {
                        log.WarnFormat("Not expectant exception : ", ex.Message);
                        //log.Warn("Not expectant exception : ", ex);
                    }
                //}
            }
            catch (System.Exception ex)
            {
                log.Warn("Not expectant exception : ", ex);
            }            
        }

        /// <summary>V the semaphore (add 1 unit to it).</summary>
        private void V()
        {
            try
            {
                // Lock so we can work in peace.  This works because lock is actually
                // built around Monitor.
                lock (this)
                {
                    // Release our hold on the unit of control.  Then tell everyone
                    // waiting on this object that there is a unit available.
                    _count++;
                    Monitor.Pulse(this);
                }
            }
            catch (System.Exception ex)
            {
                log.Warn("Not expectant exception : ", ex);
            }
        }

        /// <summary>Resets the semaphore to the specified count.  Should be used cautiously.</summary>
        public void Reset(int count)
        {
            lock (this) { _count = count; }
        }
        #endregion
    }

    /// <summary>Managed thread pool.</summary>
    public class ManagedThreadPool : IDisposable
    {
        #region Properties
        /// <summary>
        /// log4net log instance
        /// </summary>
        private readonly static log4net.ILog Log = log4net.LogManager.GetLogger(
            typeof(ManagedThreadPool).Name);

        /// <summary>Maximum number of threads the thread pool has at its disposal.</summary>
        private readonly int _maxWorkerThreads = 15;

        private readonly string _name;

        /// <summary>Gets the number of threads at the disposal of the thread pool.</summary>
        public int MaxThreads { get { return _maxWorkerThreads; } }
        /// <summary>Gets the number of currently active threads in the thread pool.</summary>
        public int ActiveThreads { get { return _inUseThreads; } }
        /// <summary>Gets the number of callback delegates currently waiting in the thread pool.</summary>
        public int WaitingCallbacks { get { lock (_waitingCallbacks.SyncRoot) { return _waitingCallbacks.Count; } } }

        #endregion

        public static ManagedThreadPool CreateThreadPool(string name, int max) 
        {
            return new ManagedThreadPool(name, max);
        }

        public string ToInstanceString()
        {
            return string.Format("MTP@{0:X8}[{1}-{2},{3},'{4}']", GetHashCode(), ActiveThreads, MaxThreads, 
                WaitingCallbacks, _name);
        }

        #region Member Variables
        /// <summary>Queue of all the callbacks waiting to be executed.</summary>
        Queue _waitingCallbacks;
        /// <summary>
        /// Used to signal that a worker thread is needed for processing.  Note that multiple
        /// threads may be needed simultaneously and as such we use a semaphore instead of
        /// an auto reset event.
        /// </summary>
        CustomSemaphore _workerThreadNeeded;
        /// <summary>List of all worker threads at the disposal of the thread pool.</summary>
        IList<Thread> _workerThreads;
        /// <summary>Number of threads currently active.</summary>
        int _inUseThreads;

        protected volatile bool _shouldStop;
        #endregion

        #region Construction
        /// <summary>Initialize the thread pool.</summary>
        ManagedThreadPool(string name, int max)
        {
            _name = name;
            _maxWorkerThreads = max;
            // Create our thread stores; we handle synchronization ourself
            // as we may run into situtations where multiple operations need to be atomic.
            // We keep track of the threads we've created just for good measure; not actually
            // needed for any core functionality.
            _waitingCallbacks = new Queue();
            _workerThreads = new List<Thread>();
            _inUseThreads = 0;
            _shouldStop = true;

            // Create our "thread needed" event
            _workerThreadNeeded = new CustomSemaphore(0);

        }
        #endregion

        #region IDisposable
        public void Dispose()
        {
            Stop();
        }
        #endregion

        public bool IsAlive 
        {
            get
            {
                if(_workerThreads!=null&&_workerThreads.Count>0)
                {
                    return _workerThreads.Any(t => t.IsAlive);
                }
                return false;
            }
        }

        public void Start()
        {
            Start(null);
        }

        public void Start(object[] states)
        {
            if (states != null && states.Length != _maxWorkerThreads)
            {
                throw new Exception("Start states not validate.");
            }
            try
            {
                _shouldStop = false;
                // Create all of the worker threads
                for (var i = 0; i < _maxWorkerThreads; i++)
                {
                    // Create a new thread and add it to the list of threads.
                    var newThread = new Thread(ProcessQueuedItemsWithParam);
                    _workerThreads.Add(newThread);

                    // Configure the new thread and start it
                    newThread.Name = string.Format("{0}[@MPT#{1}]", _name, i);
                    newThread.IsBackground = true;
                    if (states == null)
                    {
                        newThread.Start();
                    }
                    else
                    {
                        newThread.Start(states[i]);
                    }
                }
            }
            catch (System.Exception ex)
            {
                Log.Warn("Not expectant exception : ", ex);
            }            
        }

        public void Stop()
        {
            Stop(-1);
            if (Log.IsDebugEnabled)
            {
                Log.Info("ManagedThreadPool stoped.");
            }
        }
        public void Stop(int secondWait)
        {
            try
            {
                if (Log.IsDebugEnabled)
                {
                    Log.DebugFormat("Stop({0} called.", secondWait);
                }
                if (secondWait > 0)
                {
                    this._shouldStop = true;
                    long timeout = DateTime.Now.Ticks + secondWait * 10000000;
                    while (WaitingCallbacks > 0)
                    {
                        if (DateTime.Now.Ticks > timeout)
                        {
                            break;
                        }
                        Thread.Sleep(TimeSpan.FromMilliseconds(10));
                    }
                }
                else if (secondWait == 0)
                {
                    while (WaitingCallbacks > 0 )
                    {
                        Thread.Sleep(TimeSpan.FromMilliseconds(10));
                    }
                    this._shouldStop = true;
                    while(ActiveThreads >0)
                    {
                        Thread.Sleep(TimeSpan.FromMilliseconds(10));
                    }
                }
                foreach (Thread thread in _workerThreads)
                {
                    Thread t = thread;
                    try
                    {
                        ThreadUtil.ForceStop(ref t, false);
                    }
                    catch (System.Exception ex)
                    {
                        Log.Error(ex);                    	
                    }
                }
                _workerThreadNeeded.Reset(0);
                _workerThreadNeeded.StopWait();
                _workerThreads.Clear();
                if (Log.IsDebugEnabled)
                {
                    Log.DebugFormat("Stop({0}) called over.", secondWait);
                }
            }
            catch (System.Exception ex)
            {
                Log.Warn("Not expectant exception : ", ex);
            }            
        }

        #region Public Methods
        /// <summary>Queues a user work item to the thread pool.</summary>
        /// <param name="callback">
        /// A WaitCallback representing the delegate to invoke when the thread in the 
        /// thread pool picks up the work item.
        /// </param>
        public void QueueUserWorkItem(WaitCallback callback)
        {
            // Queue the delegate with no state
            QueueUserWorkItem(callback, null);
        }

        /// <summary>Queues a user work item to the thread pool.</summary>
        /// <param name="callback">
        /// A WaitCallback representing the delegate to invoke when the thread in the 
        /// thread pool picks up the work item.
        /// </param>
        /// <param name="state">
        /// The object that is passed to the delegate when serviced from the thread pool.
        /// </param>
        public void QueueUserWorkItem(WaitCallback callback, object state)
        {
            try
            {
                // Create a waiting callback that contains the delegate and its state.
                // Add it to the processing queue, and signal that data is waiting.
                WaitingCallback waiting = new WaitingCallback(callback, state);
                lock (_waitingCallbacks.SyncRoot) { _waitingCallbacks.Enqueue(waiting); }
                _workerThreadNeeded.AddOne();
            }
            catch (System.Exception ex)
            {
                Log.Warn("Not expectant exception : ", ex);
            }
        }

        /// <summary>Empties the work queue of any queued work items.</summary>
        public void EmptyQueue()
        {
            try
            {
                lock (_waitingCallbacks.SyncRoot)
                {
                    try
                    {
                        // Try to dispose of all remaining state
                        foreach (object obj in _waitingCallbacks)
                        {
                            WaitingCallback callback = (WaitingCallback)obj;
                            if (callback.State is IDisposable) ((IDisposable)callback.State).Dispose();
                        }
                    }
                    catch (System.Exception ex)
                    {
                        // Make sure an error isn't thrown.
                        Log.Error("EmptyQueue() occurs error : ", ex);
                    }

                    // Clear all waiting items and reset the number of worker threads currently needed
                    // to be 0 (there is nothing for threads to do)
                    _waitingCallbacks.Clear();
                    _workerThreadNeeded.Reset(0);
                }
            }
            catch (System.Exception ex1)
            {
                Log.Warn("Not expectant exception : ", ex1);
            }            
        }
        #endregion


        #region Thread Processing
        private void ProcessQueuedItems()
        {
            ProcessQueuedItemsWithParam(null);
        }
        /// <summary>A thread worker function that processes items from the work queue.</summary>
        private void ProcessQueuedItemsWithParam(object state)
        {
            try
            {
                // Process indefinitely
                while (true)
                {
                    // Get the next item in the queue.  If there is nothing there, go to sleep
                    // for a while until we're woken up when a callback is waiting.
                    WaitingCallback callback = null;
                    while (callback == null)
                    {
                        if (_shouldStop) break;
                        // Try to get the next callback available.  We need to lock on the 
                        // queue in order to make our count check and retrieval atomic.
                        lock (_waitingCallbacks.SyncRoot)
                        {
                            if (_waitingCallbacks.Count > 0)
                            {
                                try { 
                                    callback = (WaitingCallback)_waitingCallbacks.Dequeue(); 
                                }catch(Exception dequeEx)
                                {
                                    // make sure not to fail here
                                    Log.Warn("Not expectable exception : ", dequeEx);
                                } 
                            }
                        }
                        if (_shouldStop) break;
                        // If we can't get one, go to sleep.
                        if (callback == null) _workerThreadNeeded.WaitOne();
                    }
                    if (_shouldStop) break;

                    // We now have a callback.  Execute it.  Make sure to accurately
                    // record how many callbacks are currently executing.
                    try
                    {
                        Interlocked.Increment(ref _inUseThreads);
                        if (state == null)
                        {
                            callback.Callback(callback.State);
                        }
                        else
                        {
                            object[] newStates = new object[2];
                            newStates[0] = callback.State;
                            newStates[1] = state;
                            callback.Callback(newStates);
                        }
                    }
                    catch
                    {
                        // Make sure we don't throw here.  Errors are not our problem.
                    }
                    finally
                    {
                        Interlocked.Decrement(ref _inUseThreads);
                    }
                }
            }
            catch (System.Exception ex)
            {
                Log.Warn("Not expectant exception : ", ex);
            }
            
        }
        #endregion

        /// <summary>Used to hold a callback delegate and the state for that delegate.</summary>
        private class WaitingCallback
        {
            #region Member Variables
            /// <summary>Callback delegate for the callback.</summary>
            private WaitCallback _callback;
            /// <summary>State with which to call the callback delegate.</summary>
            private object _state;
            #endregion

            #region Construction
            /// <summary>Initialize the callback holding object.</summary>
            /// <param name="callback">Callback delegate for the callback.</param>
            /// <param name="state">State with which to call the callback delegate.</param>
            public WaitingCallback(WaitCallback callback, object state)
            {
                _callback = callback;
                _state = state;
            }
            #endregion

            #region Properties
            /// <summary>Gets the callback delegate for the callback.</summary>
            public WaitCallback Callback { get { return _callback; } }
            /// <summary>Gets the state with which to call the callback delegate.</summary>
            public object State { get { return _state; } }
            #endregion
        }
    }
}
