using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Threading;
using Runner.Base.Db;

namespace Runner.Base.Lock
{
   
    /// <summary>
    /// Concurrency control to avoid multi-threading handling a specific job inside same process. E.g, append and save same timeseries at the same time.
    /// </summary>
    internal abstract class ConcurrentLock : ILockable, IDisposable
    {
        private readonly static log4net.ILog Log = log4net.LogManager.GetLogger(typeof(ConcurrentLock));
        private string _key = string.Empty;

        public string Key
        {
            internal set { _key = value; }
            get { return _key; }
        }

        /// <summary>
        /// Default retry timeout for acuire a recource lock is 50 seconds
        /// </summary>
        private const int AcquireMutexInternvalMilliseconds = 50;

        public virtual void Acquire(TimeSpan mutexTimeout, TimeSpan acquireTimeout, params object[] contextObjs)
        {
            bool acquiredLock;
            var tryActiveKeyTime = DateTime.Now;
            var dumpFault = false;
            var sleepCount = 0;
            while (!Lock(out acquiredLock, mutexTimeout, contextObjs))
            {
                var acquireLeftTimeInMillseconds = (int)(acquireTimeout.TotalMilliseconds - (DateTime.Now - tryActiveKeyTime).TotalMilliseconds);
                if (acquireLeftTimeInMillseconds <= 1)
                {
                    break;
                }
                if (!dumpFault)
                {
                    Log.WarnFormat("Wait {1} seconds for another thread processing '{0}' .", _key, acquireTimeout.TotalSeconds);
                    dumpFault = true;
                }
                //make sleepInterval in exponent increasement 
                var sleepInterval = AcquireMutexInternvalMilliseconds * ((int)Math.Pow(2, sleepCount));
                sleepCount++;
                Log.DebugFormat("{0},{1}", sleepInterval, acquireLeftTimeInMillseconds);
                if (sleepInterval > acquireLeftTimeInMillseconds) sleepInterval = acquireLeftTimeInMillseconds-1;
                Log.DebugFormat("Sleep {0}", sleepInterval);
                Thread.Sleep(sleepInterval);
            }
            if (!acquiredLock)
            {
                throw new MutexScopeException(string.Format("Concurrent violation: cannot have multiple mutex which key is '{0}' at same time.", _key));
            }
            if (Log.IsDebugEnabled) Log.DebugFormat("Active Key {0}.", _key);
        }

        public void Release()
        {
            Unlock();
        }

        public void Dispose()
        {
            Release();
        }

        public abstract bool Lock(out bool added, TimeSpan mutexTimeout, params object[] contextObjs);
        public abstract void Unlock();
    }

    /// <summary>
    /// Concurrency control single process sync implementation.
    /// </summary>
    internal class ConcurrentSyncLock : ConcurrentLock
    {
        private readonly static log4net.ILog Log = log4net.LogManager.GetLogger(typeof(ConcurrentSyncLock));

        internal static readonly Dictionary<string, DateTime> MutexCache = new Dictionary<string, DateTime>();//key, expireTimestamp
        internal static readonly Object LockMutexCache = new Object();

        public override void Unlock()
        {
            try
            {
                lock (LockMutexCache)
                {
                    MutexCache.Remove(Key);
                }
                if (Log.IsDebugEnabled) Log.DebugFormat("Remove Key '{0}'", Key);
            }
            catch (Exception ex)
            {
                Log.WarnFormat("Remove Key '{0}' failed: {1}", Key, ex.Message);
            }
        }

        public override bool Lock(out bool added, TimeSpan mutexTimeout, params object[] contextObjs)
        {
            lock (LockMutexCache)
            {
                if (!MutexCache.ContainsKey(Key))
                {
                    MutexCache.Add(Key, DateTime.Now + mutexTimeout);
                    added = true;
                    return true;
                }
                if (DateTime.Now > MutexCache[Key])
                {
                    Log.WarnFormat("Expire '{1}' for an exist job processing '{0}', ignore it and accept current job.", Key, mutexTimeout);
                    MutexCache[Key] = DateTime.Now + mutexTimeout;
                    added = true;
                    return true;
                }
            }
            added = false;
            return false;
        }
    }

    /// <summary>
    /// Concurrency control in same machine.
    /// </summary>
    internal class ConcurrentMutexLock : ConcurrentLock
    {
        private readonly static log4net.ILog Log = log4net.LogManager.GetLogger(typeof(ConcurrentMutexLock));

        private Mutex _mutex;

        public override void Unlock()
        {
            _mutex.ReleaseMutex();
        }

        public override bool Lock(out bool added, TimeSpan mutexTimeout, params object[] contextObjs)
        {
            try
            {
                bool createNew;
                _mutex = new Mutex(true, Key, out createNew);
                added = true;
                return true;
            }
            catch (Exception ee)
            {
                Log.Warn(ee);
                added = false;
                return false;
            }
        }
    }

    /// <summary>
    /// Concurrency control based on central database transaction/locking/unique index resource. This implementation is a global locking mechenisame.
    /// </summary>
    internal class ConcurrentDatabaseLock : ConcurrentLock
    {
        private readonly static log4net.ILog Log = log4net.LogManager.GetLogger(typeof(ConcurrentDatabaseLock));

        public readonly static string CreatedBy = GetCreateBy();

        private int _acquireRetryTimes;
        public override void Acquire(TimeSpan mutexTimeout, TimeSpan acquireTimeout, params object[] contextObjs)
        {
            using (var db = Dao.Default.Create())
            {
                using (var cmd = db.Connection.CreateCommand())
                {
                    base.Acquire(mutexTimeout, acquireTimeout, db, cmd);
                }
            }
        }

        public override void Unlock()
        {
            Release(Key);
        }

        public static void Release(string key)
        {
            using (var db = Dao.Default.Create())
            {
                using (var cmd = db.Connection.CreateCommand())
                {
                    try
                    {
                        DbUtil.Execute("DELETE FROM GlobalSetting WHERE SettingGroup={0} AND SettingKey={1}",
                            db, cmd, new[] { DbType.String, DbType.String }, new object[] { GlobalMutexScope.Group, key });
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex.Message);
                        throw;
                    }
                }
            }
        }

        public override bool Lock(out bool acquiredLock, TimeSpan mutexTimeout, params object[] contextObjs)
        {
            var db = (DbCommon)contextObjs[0];
            var cmd = (System.Data.Common.DbCommand)contextObjs[1];
            var value = string.Format("{0},{1}", Environment.MachineName , Process.GetCurrentProcess().Id);
            if (_acquireRetryTimes < 1)
            {
                //first time try direct add the entry into database
                try
                {
                    DbUtil.Execute(
                        "INSERT INTO GlobalSetting (SettingGroup, SettingKey, LastModified, SettingValue, System) VALUES ({0}, {1}, {2}, {3}, {4})",
                        db, cmd,
                        new[] { DbType.String, DbType.String, DbType.DateTime, DbType.String, DbType.Boolean },
                        new object[] { GlobalMutexScope.Group, Key, DateTime.UtcNow + mutexTimeout, value, true });
                    acquiredLock = true;
                    return true;
                }
                catch (Exception ex)
                {
                    if (Log.IsDebugEnabled) Log.Debug(ex.Message);
                }
            }
            _acquireRetryTimes++;
            //retry, go futher to check the lastmodified time see if previous GlobalMutex is timeout
            var dbtime = DbUtil.ExecuteScalar("SELECT LastModified FROM GlobalSetting WHERE SettingGroup={0} AND SettingKey={1}",
                db, cmd, new[] { DbType.String, DbType.String }, new object[] { GlobalMutexScope.Group, Key });

            if (dbtime == null)
            {
                //previous job is release mutex, direct add again
                DbUtil.Execute(
                    "INSERT INTO GlobalSetting (SettingGroup, SettingKey, LastModified, SettingValue, System) VALUES ({0}, {1}, {2}, {3}, {4})",
                    db, cmd,
                    new[] { DbType.String, DbType.String, DbType.DateTime, DbType.String, DbType.Boolean },
                    new object[] { GlobalMutexScope.Group, Key, DateTime.UtcNow + mutexTimeout, value, true });
                acquiredLock = true;
                return true;
            }
            if (DateTime.UtcNow > Convert.ToDateTime(dbtime))
            {
                //previous job timeout, update LastModified time and return true.
                DbUtil.Execute(
                    "UPDATE GlobalSetting SET LastModified={2},SettingValue={3} WHERE SettingGroup={0} AND SettingKey={1}",
                    db, cmd,
                    new[] { DbType.String, DbType.String, DbType.DateTime, DbType.String },
                    new object[] { GlobalMutexScope.Group, Key, DateTime.UtcNow + mutexTimeout, value });
                acquiredLock = true;
                return true;
            }
            acquiredLock = false;
            return false;
        }

        public static string GetCreateBy()
        {
            return GetCreateBy(Process.GetCurrentProcess().Id);
        }

        public static string GetCreateBy(int pid)
        {
            return string.Format("{0}-{1}", Environment.MachineName, pid);
        }

        /// <summary>
        /// This method will be used for Process Monitor, which can remove period dead locks caused by process crashing.
        /// </summary>
        /// <param name="processId"></param>
        public static void Cleanup(int processId)
        {
            try
            {
                var value = GetCreateBy(processId);
                using (var db = Dao.Default.Create())
                {
                    using (var cmd = db.Connection.CreateCommand())
                    {
                        try
                        {
                            DbUtil.Execute("DELETE FROM GlobalSetting WHERE SettingGroup={0} AND SettingValue={1}",
                                db, cmd, new[] { DbType.String, DbType.String }, new object[] { GlobalMutexScope.Group, value });
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex.Message);
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }
    }

    internal class ConcurrentDTCDbLock : ConcurrentLock
    {
        private readonly static log4net.ILog Log = log4net.LogManager.GetLogger(typeof(ConcurrentDTCDbLock));

        public override void Acquire(TimeSpan mutexTimeout, TimeSpan acquireTimeout, params object[] contextObjs)
        {
            using (var db = Dao.Default.Create())
            {
                var dbTransaction = db.Connection.BeginTransaction(IsolationLevel.ReadUncommitted);
                using (var cmd = db.Connection.CreateCommand())
                {
                    try
                    {
                        DbUtil.Execute(
                            "INSERT INTO GlobalSetting (SettingGroup, SettingKey, LastModified, SettingValue, System) VALUES ({0}, {1}, {2}, {3}, {4})",
                            db, cmd,
                            new[] { DbType.String, DbType.String, DbType.DateTime, DbType.String, DbType.Boolean },
                            new object[] { GlobalMutexScope.Group, Key, DateTime.UtcNow + mutexTimeout, "", true });
                        dbTransaction.Commit(); //comit insertion so that other process can read it by IsolationLevel.ReadUncommitted.
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex);
                        dbTransaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public override void Unlock()
        {
        }

        public override bool Lock(out bool acquiredLock, TimeSpan mutexTimeout, params object[] contextObjs)
        {
            acquiredLock = false;
            return false;
        }
    }

    
}
