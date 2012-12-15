using System;
using System.Text;
using System.Threading;

namespace Runner.Base.Util
{

    public delegate bool OnLoadHandle(object state);
    public class AQLoadWorker
    {

        private readonly static log4net.ILog log = log4net.LogManager.GetLogger(
            typeof(AQLoadWorker).Name);

        public OnLoadHandle[] loadHandles = null;
        public ManagedThreadPool threadPool;
        protected object parent;
        private string Name;

        public int StatusSuccess = 0;
        public int StatusFailure = 0;
        public long TotalTime = 0;
        public long AverageTime = 0;
        public Object statusSync = new Object();
        public int threadcnt = 1;
        public int loops = -1;
        public int pause = 10;//10 ms

        public long Passed = 0;
        private bool shouldStop = false;
        public bool ShouldStop
        {
            get { return shouldStop; }
        }
        public object Parent
        {
            set { parent = value; }
            get { return parent; }
        }
        public string ToInstanceString()
        {
            return string.Format("AQLoadWorker@{0:X8}[{1}-{2},{3},'{4}']", GetHashCode(),
                threadPool.ActiveThreads, threadPool.MaxThreads, threadPool.WaitingCallbacks, Name);
        }

        public void AddLoadHandle(OnLoadHandle loadHandle)
        {
            if (loadHandles == null)
            {
                loadHandles = new OnLoadHandle[] { loadHandle };
            }
            else
            {
                OnLoadHandle[] loadHandles1 = new OnLoadHandle[loadHandles.Length + 1];
                for (int i = 0; i < loadHandles.Length; i++)
                {
                    loadHandles1[i] = loadHandles[i];
                }
                loadHandles1[loadHandles.Length] = loadHandle;
                loadHandles = loadHandles1;
                loadHandles1 = null;
            }
        }

        public AQLoadWorker(string name, int threads)
        {
            Init(null, name, threads, -1, -1);
        }
        public AQLoadWorker(object parent, string name, int threads)
        {
            Init(parent, name, threads, -1, -1);
        }
        public AQLoadWorker(string name, int threads, int loops, int pause)
        {
            Init(null, name, threads, loops, pause);
        }
        public AQLoadWorker(object parent, string name, int threads, int loops, int pause)
        {
            Init(parent, name, threads, loops, pause);
        }
        private void Init(object parent, string name, int threads, int loops, int pause)
        {
            try
            {
                this.parent = parent;
                this.Name = name;
                threadcnt = threads;
                this.loops = loops;
                this.pause = pause;
                this.threadPool = ManagedThreadPool.CreateThreadPool(name, threadcnt);
            }
            catch (System.Exception ex)
            {
                log.Error(ex);
                throw ex;
            }
        }

        public void Start()
        {
            StatusSuccess = 0;
            StatusFailure = 0;
            TotalTime = 0;
            AverageTime = 0;
            Passed = 0;
            shouldStop = false;
            threadPool.Start();
            for (int i = 0; i < threadcnt; i++)
            {
                log.DebugFormat("Load test ThreadPool start thread {0}.", i);
                threadPool.QueueUserWorkItem(new WaitCallback(OnThreadProcess), i);
            }
        }

        public void Start(object[] states)
        {
            StatusSuccess = 0;
            StatusFailure = 0;
            TotalTime = 0;
            AverageTime = 0;
            Passed = 0;
            shouldStop = false;
            threadPool.Start(states);
            for (int i = 0; i < threadcnt; i++)
            {
                log.DebugFormat("Load test ThreadPool start thread {0}.", i);
                threadPool.QueueUserWorkItem(new WaitCallback(OnThreadProcess), i);
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
            if (log.IsDebugEnabled)
            {
                log.DebugFormat("Stop({0} called.", secondWait);
            }
            shouldStop = true;
            try
            {
                threadPool.Stop(secondWait);
            }
            catch (System.Exception ex)
            {
                log.Error(ex);
                throw ex;
            }
            MessageAppendText("Stop successful.");
        }

        public void OnThreadProcess()
        {
            OnThreadProcess(null);
        }

        public void OnThreadProcess(object state)
        {
            int times = loops - 1;
            if (times < 0) times = int.MaxValue;//set it to an endless loop
            for (int j = 0; j <= times; j++)
            {
                try
                {
                    if (shouldStop) break;
                    DateTime startTime = DateTime.Now;
                    bool status = false;
                    if (loadHandles != null && loadHandles.Length>0)
                    {
                        foreach(OnLoadHandle loadHandle in loadHandles)
                        {
                            status = loadHandle(state);
                        }
                    }
                    else
                    {
                        status = OnThreadProcessFunctionFake(state);
                    }
                    TimeSpan tspan = DateTime.Now - startTime;
                    lock (ReportSync)
                    {
                        Passed++;
                        if (status)
                        {
                            this.TotalTime += (long)tspan.TotalMilliseconds;
                            StatusSuccess += 1;
                            this.AverageTime = this.TotalTime / StatusSuccess;
                        }
                        else
                        {
                            StatusFailure += 1;
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    log.Error(ex.Message);
                    lock (ReportSync)
                    {
                        Passed++;
                        StatusFailure += 1;
                    }
                }
                if (j != times && pause > 0)
                {
                    Thread.Sleep(pause);
                }
            }
        }

        Random randObj = new Random(1000);
        public bool OnThreadProcessFunctionFake(object state)
        {
            int sleepmillsecond = randObj.Next(50, 1000);
            MessageAppendText(String.Format("Thread '{0}' will sleep {1}.", Thread.CurrentThread.Name, sleepmillsecond));
            Thread.Sleep(sleepmillsecond);
            return true;
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
                    ReportSuccess.Append(msg);
                    ReportSuccess.AppendLine();
                }
                else
                {
                    ReportFailure.Append(msg);
                    ReportFailure.AppendLine();
                }
            }
        }
        public void AppendSuccess(string msg)
        {
            AppendReport(msg, true);
        }
        public void AppendFailure(string msg)
        {
            AppendReport(msg, false);
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
            log.ErrorFormat("{0}:{1}", msg, error);
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
            log.ErrorFormat("{0}", msg);
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
                        case 1: log.Debug(msg); break;
                        case 2: log.Info(msg); break;
                        case 3: log.Warn(msg); break;
                        case 4: log.Error(msg); break;
                        case 5: log.Fatal(msg); break;
                    }
                }
                else
                {
                    switch (loglevel)
                    {
                        case 1: log.Debug(msg, ex); break;
                        case 2: log.Info(msg, ex); break;
                        case 3: log.Warn(msg, ex); break;
                        case 4: log.Error(msg, ex); break;
                        case 5: log.Fatal(msg, ex); break;
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
