using System;
using System.Threading;

namespace Runner.Base.Util
{
    public delegate object WatchdogMethod<in T>(T input);
    public delegate void WatchdogFree(object input);

    /*
public object WatchdogMethod(int second)
{
    if (second < 1) throw new Exception("Input is bad.");
    Thread.Sleep(second*1000);
    return true;
}
WatchdogMethodRunner<int> runner1 = new WatchdogMethodRunner<int>(WatchdogMethod, 2);
object ret1;
bool run1 = runner1.DoIt(TimeSpan.FromSeconds(5), out ret1);
     */
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class WatchdogMethodRunner<T>
    {
        // method SampleOperation return variables
        private object _retObj;
        private readonly T _input;
        private readonly WatchdogMethod<T> _methodDelegate;
        private readonly WatchdogFree _methodFree;
        private readonly object _freeObj;

        public WatchdogMethodRunner(WatchdogMethod<T> method, T input)
        {
            _input = input;
            _methodDelegate = method;
        }
        public WatchdogMethodRunner(WatchdogMethod<T> method, T input, WatchdogFree free, object freeState)
        {
            _input = input;
            _methodDelegate = method;
            _methodFree = free;
            _freeObj = freeState;
        }

        // Notifies a waiting thread that an event has occurred
        private readonly AutoResetEvent _evnt = new AutoResetEvent(false);

        public bool DoIt(TimeSpan timeout, out object ret)
        {
            var th = new Thread(RunClientMethod);
            // Sets the state of the specified event to non signaled.
            _evnt.Reset();

            // start thread
            th.Start();

            ret = null;
            // wait with timeout on the event
            if (_evnt.WaitOne(timeout, false))
            {
                // Sucess - timeout did not occurred
                var retObj = _retObj as Exception;
                if (retObj != null)
                {
                    throw retObj;
                }
                ret = _retObj;
                return true;
            }
            // Failure - Timeout - do cleanup
            if (_methodFree != null)
            {
                _methodFree(_freeObj);
            }
            th.Abort();
            return false;
        }

        void RunClientMethod()
        {
            try
            {
                _retObj = _methodDelegate(_input);
            }
            catch (Exception ex)
            {
                _retObj = ex;
            }
            // Sets the state of the specified event to signaled
            _evnt.Set();
        }
    }    
}
