using System;

namespace Runner.Base.Lock
{
    public enum LockStyle
    {
        Single = 0,
        Mutex = 1,
        Database = 2,
        MSDTC = 3
    }

    public interface ILockable : IDisposable
    {
        string Key { get; }
        void Acquire(TimeSpan mutexTimeout, TimeSpan acquireTimeout, params object[] contextObjs);
        void Release();
    }

    /// <summary>
    /// MutexException will throw whenever have a conflision on mutil-thread working on same key.
    /// </summary>
    public class MutexScopeException : Exception
    {
        public MutexScopeException() { }
        public MutexScopeException(string message) : base(message) { }
        public MutexScopeException(string message, Exception innerException) : base(message, innerException) { }
    }
}