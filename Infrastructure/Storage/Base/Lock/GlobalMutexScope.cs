using System;
using System.Diagnostics;

namespace Runner.Base.Lock
{
    /// <summary>
    /// GlobalMutex
    /// </summary>
    public class GlobalMutexScope : IDisposable
    {
        private readonly static log4net.ILog Log = log4net.LogManager.GetLogger(typeof(GlobalMutexScope));

        public const string Group = "GlobalMutex";
        public readonly static string FromMachine = string.Format("{0},{1}", Environment.MachineName , Process.GetCurrentProcess().Id);

        /// <summary>
        /// Default maximun timeout for locked resource is 5 minutes
        /// </summary>
        public static TimeSpan MutexTimeout = TimeSpan.FromMinutes(5);
        /// <summary>
        /// Default retry timeout for acuire a recource lock is 10 seconds
        /// </summary>
        public static TimeSpan AcquireMutexTimeout = TimeSpan.FromMilliseconds(10000);

        internal static ILockable CreateConcurrentLock(string key, LockStyle style)
        {
            switch (style)
            {
                case LockStyle.Single: return new ConcurrentSyncLock { Key = key };
                case LockStyle.Mutex: return new ConcurrentMutexLock { Key = key };
                case LockStyle.Database: return new ConcurrentDatabaseLock { Key = key };
                case LockStyle.MSDTC: return new ConcurrentDTCLock { Key = key };
                default:
                    {
                        var message = style + " ctor is not implement yet.";
                        Log.Error(message);
                        throw new ArgumentException(message);
                    }
            }
        }

        public static ILockable AquireConcurrentLock(string key)
        {
            return AquireConcurrentLock(key, LockStyle.Single, MutexTimeout, AcquireMutexTimeout);
        }

        public static ILockable AquireConcurrentLock(string key, LockStyle style)
        {
            return AquireConcurrentLock(key, style, MutexTimeout, AcquireMutexTimeout);
        }

        public static ILockable AquireConcurrentLock(string key, LockStyle style, TimeSpan cacheTimeout)
        {
            return AquireConcurrentLock(key, style, cacheTimeout, AcquireMutexTimeout);
        }

        public static ILockable AquireConcurrentLock(string key, LockStyle style, TimeSpan mutexTimeout, TimeSpan acquireMutexTimeout)
        {
            var concurrtLock = CreateConcurrentLock(key, style);
            concurrtLock.Acquire(mutexTimeout, acquireMutexTimeout, null);
            return concurrtLock;
        }

        private readonly ILockable _concurrentLock;
        public GlobalMutexScope(string key)
        {
            _concurrentLock = AquireConcurrentLock(key);
        }

        public GlobalMutexScope(string key, LockStyle style)
        {
            _concurrentLock = AquireConcurrentLock(key, style);
        }

        public GlobalMutexScope(string key, LockStyle style, TimeSpan cacheTimeout)
        {
            _concurrentLock = AquireConcurrentLock(key, style, cacheTimeout);
        }

        public GlobalMutexScope(string key, LockStyle style, TimeSpan cacheTimeout, TimeSpan retryTimeout)
        {
            _concurrentLock = AquireConcurrentLock(key, style, cacheTimeout, retryTimeout);
        }

        public string Key
        {
            get { return _concurrentLock.Key; }
        }

        public void Dispose()
        {
            _concurrentLock.Dispose();
        }

    }
}