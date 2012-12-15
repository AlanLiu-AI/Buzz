using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Runner.Base.Util
{
    /// <summary>
    /// A local message queue to exchange message from consumer/producer thread pool
    /// Use a static syncLock, not so good
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class LocalEventQueue<T>
    {

        /// <summary>
        /// log4net log instance
        /// </summary>
        private readonly static log4net.ILog log = log4net.LogManager.GetLogger(
            typeof(LocalEventQueue<T>).Name);


        public delegate void MessageDequeuedEventHandler(T message);
        public event MessageDequeuedEventHandler OnMessageDequeued;

        private Semaphore _semaphore;
        private Queue<T> _internalQueue;
        private Thread _dequeueThread;
        private readonly int __QUEUE_SIZE;
        private object __syncLock;

        private volatile bool _shouldStop;

        public int Count 
        {
            get 
            {
                lock (__syncLock)
                {
                    try
                    {
                        //_semaphore.Release();
                        return _internalQueue.Count;
                    }
                    catch(Exception ex) 
                    {
                        log.Warn("Not expectable exception : ", ex);
                    }
                }
                return -1;
            }
        }

        public LocalEventQueue(int queueSize)
        {
            __syncLock = new object();
            __QUEUE_SIZE = queueSize;
            _internalQueue = new Queue<T>(__QUEUE_SIZE);
            _dequeueThread = new Thread(new ThreadStart(_dequeueMessages));
            _semaphore = null;
        }
        public void Start()
        {
            if (_semaphore == null)
            {
                _semaphore = new Semaphore(0, __QUEUE_SIZE);
            }
            _shouldStop = false;
            _dequeueThread.Start();
        }
        public void Stop()
        {
            Stop(-1);
        }

        public void Stop(int timeout)
        {
            if (timeout<0)
            {
                try
                {
                    _dequeueThread.Abort();
                }
                catch(Exception ex) 
                {
                    log.Warn("Not expectable exception : ", ex);
                }
            }
            else
            {
                _shouldStop = true;
                ThreadUtil.StopSingle(ref _dequeueThread, timeout, false);
            }
            try
            {
                if (_semaphore != null)
                {
                    _semaphore.Close();
                    _semaphore = null;
                }
                _internalQueue.Clear();
            }
            catch (System.Exception ex)
            {
                log.Warn("Not expectable exception : ", ex);
            }            
        }

        public void EnqueueMessage(T message)
        {
            lock (__syncLock)
            {
                if (_semaphore != null && message != null)
                {
                    try
                    {
                        _semaphore.Release();
                        _internalQueue.Enqueue(message);
                    }
                    catch { }
                }
            }
        }

        private void _dequeueMessages()
        {
            try
            {
                while (true)
                {
                    if (_semaphore!=null)
                    {
                        //if(_semaphore.WaitOne(TimeSpan.FromMilliseconds(10)))
                        _semaphore.WaitOne();
                        {
                            lock (__syncLock)
                            {
                                try
                                {
                                    T message = _internalQueue.Dequeue();
                                    OnMessageDequeued(message);
                                }
                                catch (Exception ex1)
                                {
                                    log.Warn("Not expectable exception : ", ex1);
                                }
                            }
                        }
                        if(_shouldStop)
                            break;
                    }                    
                }
            }
            catch (Exception ex)
            {
                log.Warn("Not expectable exception : ", ex);
            }
        }
    }
}
