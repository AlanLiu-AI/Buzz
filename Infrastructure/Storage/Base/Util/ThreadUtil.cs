using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Runner.Base.Util
{
    public static class ThreadUtil
    {
        /// <summary>
        /// log4net log instance
        /// </summary>
        private readonly static log4net.ILog Log = log4net.LogManager.GetLogger(
            typeof(ThreadUtil).Name);

        public static void ForceStop(ref Thread thread, bool setNull)
        {
            try
            {
                if (thread != null && thread.IsAlive)
                {
                    //force stop
                    try
                    {
                        thread.Interrupt();
                    }
                    catch (ThreadInterruptedException ex)
                    {
                        Log.WarnFormat("thread {0} Interrupt error : {1}", thread.Name, ex.Message);
                        try
                        {
                            thread.Abort();
                        }
                        catch (System.Exception ex1)
                        {
                            Log.WarnFormat("thread {0} Abort error : {1}", thread.Name, ex1.Message);
                        }
                    }
                    catch (System.Exception ex)
                    {
                        Log.WarnFormat("thread {0} Interrupt error : {1}", thread.Name, ex.Message);
                        try
                        {
                            thread.Abort();
                        }
                        catch (System.Exception ex1)
                        {
                            Log.WarnFormat("thread {0} Abort error : {1}", thread.Name, ex1.Message);
                        }
                    }
                    finally
                    {
                        if (setNull)
                        {
                            thread = null;
                        }
                    }
                }
                else
                {
                    if (setNull)
                    {
                        thread = null;
                    }
                }
            }
            catch (System.Exception ex)
            {
                Log.Error("Not expectant exception : ", ex);
                throw ex;
            }
        }

        public static void StopSingle(ref Thread thread, int secondWait, bool setNull)
        {
            try
            {
                if (thread != null && thread.IsAlive)
                {
                    if (secondWait > 0)
                    {
                        long timeout = DateTime.Now.Ticks + secondWait * 10000000;
                        while (thread != null && thread.IsAlive) //wait unti
                        {
                            if (DateTime.Now.Ticks > timeout)
                            {
                                break;
                            }
                            Thread.Sleep(TimeSpan.FromMilliseconds(50));
                        }
                    }
                    if (secondWait == 0)
                    {
                        while (thread != null && thread.IsAlive) //wait unti
                        {
                            Thread.Sleep(TimeSpan.FromMilliseconds(50));
                        }
                    }
                    if (thread != null && thread.IsAlive)
                    {
                        ForceStop(ref thread, false);
                    }
                }
                if (setNull && thread != null)
                {
                    thread = null;
                }
            }
            catch (System.Exception ex)
            {
                Log.Error("Not expectant exception : ", ex);
                throw ex;
            }
        }

        public static void StopMulti(ref Thread[] threads, int secondWait)
        {
            try
            {
                if (threads != null)
                {
                    bool needWait = false;
                    for (int i = 0; i < threads.Length; i++)
                    {
                        Thread thread = threads[i];
                        if (thread != null && thread.IsAlive)
                        {
                            needWait = true;
                            break;
                        }
                    }
                    if (needWait)
                    {
                        if (secondWait > 0)
                        {
                            long timeout = DateTime.Now.Ticks + secondWait * 10000000;
                            while (true) //wait until timeout
                            {
                                if (DateTime.Now.Ticks > timeout)
                                {
                                    break;
                                }
                                bool needWait1 = false;
                                for (int i = 0; i < threads.Length; i++)
                                {
                                    Thread thread = threads[i];
                                    if (thread != null && thread.IsAlive)
                                    {
                                        needWait1 = true;
                                        break;
                                    }
                                }
                                if (!needWait1)
                                {
                                    needWait = false;
                                    break;
                                }
                                Thread.Sleep(TimeSpan.FromMilliseconds(50));
                            }
                        }
                    }
                    if (!needWait)
                        return;
                    //force stop threads
                    for (int i = 0; i < threads.Length; i++)
                    {
                        Thread thread = threads[i];
                        ForceStop(ref thread, false);
                    }
                }
            }
            catch (System.Exception ex)
            {
                Log.Error("Not expectant exception : ", ex);
                throw ex;
            }

        }

        public static void StopMulti(ref IList<Thread> threads, int secondWait)
        {
            try
            {
                if (threads != null && threads.Count > 0)
                {
                    Thread[] threadsArray = threads.ToArray();
                    StopMulti(ref threadsArray, secondWait);
                }
            }
            catch (System.Exception ex)
            {
                Log.Error("Not expectant exception : ", ex);
                throw ex;
            }
        }

        public static void STAThreadEnable(ref Thread thread)
        {
            thread.SetApartmentState(ApartmentState.STA);
        }

        public static Thread NewThread(ThreadStart start)
        {
            return new Thread(start);
        }

        public static Thread NewSTAThread(ThreadStart start)
        {
            var ret = new Thread(start);
            ret.SetApartmentState(ApartmentState.STA);
            return ret;
        }

        private static void ConsoleWaitWorker()
        {
            Console.WriteLine("Press <ENTER> to terminate service.");
            Console.WriteLine();
            Console.ReadLine();
        }

        public static void ConsoleWait()
        {
            var waitThread = new Thread(new ThreadStart(ConsoleWaitWorker));
            waitThread.Start();
        }

    }

}
