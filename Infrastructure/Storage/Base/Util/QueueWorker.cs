using System;
using System.Text;
using System.Threading;

namespace Runner.Base.Util
{

    public delegate void AQDequeHandle<T>(T message);

    public class AQQueueWorker<T>
        where T : class
    {
        /// <summary>
        /// log4net Log instance
        /// </summary>
        private readonly static log4net.ILog Log = log4net.LogManager.GetLogger("AQQueueWorker<" + typeof(T).Name + ">");

        protected AQDequeHandle<T>[] dequeHandles = null;
        public ManagedThreadPool consumers;
        public ThreadStart[] publishers;
        public LocalEventQueue<T> queue = null;
        protected object parent;
        private readonly string Name;

        public int StatusSuccess = 0;
        public int StatusFailure = 0;
        public int enquesum = 0;
        public int dequesum = 0;

        private bool shouldStop = false;
        public bool ShouldStop
        {
            get { return shouldStop; }
        }

        public void AddDequeHandle(AQDequeHandle<T> dequeHandle)
        {
            if (dequeHandles == null)
            {
                dequeHandles = new AQDequeHandle<T>[] { dequeHandle };
            }
            else
            {
                AQDequeHandle<T>[] dequeHandles1 = new AQDequeHandle<T>[dequeHandles.Length + 1];
                for (int i = 0; i < dequeHandles.Length; i++)
                {
                    dequeHandles1[i] = dequeHandles[i];
                }
                dequeHandles1[dequeHandles.Length] = dequeHandle;
                dequeHandles = dequeHandles1;
                dequeHandles1 = null;
            }
        }

        /*
        public void AddEnqueHandle(AQEnqueHandle<T> enqueHandle)
        {
            Thread thread = new Thread(new ThreadStart(() =>
            {

            }));
            AddEnqueThread(thread);
        }*/

        public void AddEnqueThread(ThreadStart threadStart)
        {
            if (publishers == null)
            {
                publishers = new ThreadStart[] { threadStart };
            }
            else
            {
                ThreadStart[] thread1 = new ThreadStart[publishers.Length + 1];
                for (int i = 0; i < publishers.Length; i++)
                {
                    thread1[i] = publishers[i];
                }
                thread1[publishers.Length] = threadStart;
                publishers = thread1;
                thread1 = null;
            }
        }

        public int WaitingDequeue
        {
            get 
            {
                if (consumers != null)
                    return consumers.WaitingCallbacks;
                return 0;
            }
        }

        public object Parent
        {
            set {parent = value;}
            get {return parent;}
        }

        public string ToInstanceString()
        {
            return string.Format("AQQueueWorker<{1}>@{0:X8}[{2}-{3},{4},'{5}']", GetHashCode(), typeof(T).Name, 
                consumers.ActiveThreads, consumers.MaxThreads,consumers.WaitingCallbacks, Name);
        }

        public virtual void OnMessageProcess(object state)
        {
            if(Log.IsInfoEnabled)
            {
                Log.InfoFormat("OnMessageProcess({0}) called, thread: {1} ", state, Thread.CurrentThread.Name);
            }
            dequesum++;
            if (dequeHandles != null && dequeHandles.Length > 0)
            {
                foreach (AQDequeHandle<T> dequeHandle in dequeHandles)
                {
                    dequeHandle(state as T);
                }
            }
        }
        public void OnMessageConsume(T msg)
        {
            if(Log.IsDebugEnabled)Log.DebugFormat("OnMessageConsume onCall, queue size = {0}...", consumers.WaitingCallbacks);
            consumers.QueueUserWorkItem(new WaitCallback(OnMessageProcess), msg);
            enquesum++;
            //if (AQGlobalTokens.IsDebugEnabled)Log.DebugFormat("OnMessageConsume called, queue size = {0}.", consumers.WaitingCallbacks);
        }
        public AQQueueWorker(string name, int max)
        {
            try
            {
                Name = name;
                consumers = ManagedThreadPool.CreateThreadPool(name, max);
                //queue = new LocalEventQueue<T>(65536);
                //queue.OnMessageDequeued += new LocalEventQueue<T>.MessageDequeuedEventHandler(OnMessageConsume);
            }
            catch (System.Exception ex)
            {
                Log.Error(ex);
                throw ex;
            }
        }

        public AQQueueWorker(object parent, string name, int max)
        {
            try
            {
                this.parent = parent;
                this.Name = name;
                this.consumers = ManagedThreadPool.CreateThreadPool(name, max);
                //queue = new LocalEventQueue<T>(65536);
                //queue.OnMessageDequeued += new LocalEventQueue<T>.MessageDequeuedEventHandler(OnMessageConsume);
            }
            catch (System.Exception ex)
            {
                Log.Error(ex);
                throw ex;
            }
        }

        private Thread[] enqueThreads = null;
        public void Start()
        {
            StatusSuccess = 0;
            StatusFailure = 0;
            enquesum = 0;
            dequesum = 0;
            shouldStop = false;
            consumers.Start();
            //queue.Start();
            if(publishers!=null && publishers.Length>0)
            {
                enqueThreads = new Thread[publishers.Length];
                for(int i=0;i<publishers.Length;i++)
                {
                    enqueThreads[i] = new Thread(publishers[i]);
                    enqueThreads[i].Start();
                }
            }
        }

        public void Start(object[] states)
        {
            StatusSuccess = 0;
            StatusFailure = 0;
            enquesum = 0;
            dequesum = 0;
            shouldStop = false;
            consumers.Start(states);
            //queue.Start();
            if (publishers != null && publishers.Length > 0)
            {
                enqueThreads = new Thread[publishers.Length];
                for (int i = 0; i < publishers.Length; i++)
                {
                    enqueThreads[i] = new Thread(publishers[i]);
                    enqueThreads[i].Start(states);
                }
            }
            MessageAppendText("Start successful.");
        }

        public void Stop()
        {
            Stop(-1);
        }

        public void Stop(int secondWait)
        {
            shouldStop = true;
            if(Log.IsDebugEnabled)
            {
                Log.DebugFormat("Stop({0} called.", secondWait);
            }
            shouldStop = true;
            if (enqueThreads != null && enqueThreads.Length > 0)
            {
                ThreadUtil.StopMulti(ref enqueThreads, secondWait);
            }
            enqueThreads = null;
            if (queue != null)
            {
                try
                {
                    queue.Stop(secondWait);
                    if (secondWait > 0)
                    {
                        long timeout = DateTime.Now.Ticks + secondWait * 10000000;
                        while (queue.Count > 0)
                        {
                            if (DateTime.Now.Ticks > timeout)
                            {
                                break;
                            }
                            Thread.Sleep(TimeSpan.FromMilliseconds(100));
                        }
                        consumers.Stop(secondWait);
                    }
                    else if (secondWait== 0)
                    {
                        while (queue.Count > 0)
                        {
                            Thread.Sleep(TimeSpan.FromMilliseconds(100));
                        }
                        consumers.Stop();
                    }
                    else
                    {
                        consumers.Stop();
                    }
                }
                catch (System.Exception ex)
                {
                    Log.Error(ex);
                    throw ex;
                }
            }
            else
            {
                try
                {
                    consumers.Stop(secondWait);
                }
                catch (System.Exception ex)
                {
                    Log.Error(ex);
                    throw ex;
                }
            }
            MessageAppendText("Stop successful.");
        }

        public void EnqueueMessage(T message)
        {
            //*/
            try
            {
                OnMessageConsume(message);
            }
            catch (System.Exception ex)
            {
                Log.Error(ex);
                throw ex;
            }
            /*/
            try
            {
                queue.EnqueueMessage(message);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                throw ex;
            }
            /*/
        }

        #region Message, Report
        public StringBuilder ReportSuccess = new StringBuilder();
        public StringBuilder ReportFailure = new StringBuilder();
        public System.Object ReportSync = new System.Object();
        public void AppendReport(string msg, bool success)
        {
            lock (ReportSync)
            {
                if (success)
                {
                    StatusSuccess++;
                    ReportSuccess.Append(msg);
                    ReportSuccess.AppendLine();
                }
                else
                {
                    StatusFailure++;
                    ReportFailure.Append(msg);
                    ReportFailure.AppendLine();
                }
            }
        }
        public bool MessageBufferEnabled = false;
        public StringBuilder Message = new StringBuilder();
        public System.Object MsgSync = new System.Object();
        public void MessageAppendText(string msg)
        {
            MessageAppendText(msg, 2, null);
        }

        public void MessageAppendError(string msg, string error)
        {
            Log.ErrorFormat("{0}:{1}", msg, error);
            if (MessageBufferEnabled)
            {
                lock (MsgSync)
                {
                    Message.AppendFormat(string.Format("{0}:{1}{2}", Thread.CurrentThread.Name, msg, Environment.NewLine));
                    Message.AppendFormat(string.Format("{0}:{1}{2}", Thread.CurrentThread.Name, error, Environment.NewLine));
                }
            }
        }

        public void MessageAppendError(string msg)
        {
            Log.ErrorFormat("{0}", msg);
            if (MessageBufferEnabled)
            {
                lock (MsgSync)
                {
                    Message.AppendFormat(string.Format("{0}:{1}{2}", Thread.CurrentThread.Name, msg, Environment.NewLine));
                }
            }
        }

        public void MessageAppendText(string msg, int loglevel, Exception ex)
        {
            if (loglevel < 1) loglevel = 0;
            if (loglevel > 5) loglevel = 5;
            if (loglevel > 0)
            {
                if (ex == null)
                {
                    switch (loglevel)
                    {
                        case 1: Log.Debug(msg); break;
                        case 2: Log.Info(msg); break;
                        case 3: Log.Warn(msg); break;
                        case 4: Log.Error(msg); break;
                        case 5: Log.Fatal(msg); break;
                    }
                }
                else
                {
                    switch (loglevel)
                    {
                        case 1: Log.Debug(msg, ex); break;
                        case 2: Log.Info(msg, ex); break;
                        case 3: Log.Warn(msg, ex); break;
                        case 4: Log.Error(msg, ex); break;
                        case 5: Log.Fatal(msg, ex); break;
                    }
                }
            }

            if (MessageBufferEnabled)
            {
                lock (MsgSync)
                {
                    //message.Append(DateTime.Now.ToString("o"));
                    Message.Append(Thread.CurrentThread.Name);
                    Message.Append(":");
                    Message.AppendLine(msg);
                }
            }
        }
        #endregion

    }
}
