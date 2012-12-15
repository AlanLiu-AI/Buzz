using System;
using System.Data;
using System.Diagnostics;
using System.Transactions;
using Runner.Base.Db;

namespace Runner.Base.Lock
{
    class EnlistmentLocalImpl : IEnlistmentNotification
    {
        private readonly static log4net.ILog Log = log4net.LogManager.GetLogger(typeof(EnlistmentLocalImpl));

        private readonly string _key;
        internal EnlistmentLocalImpl(string key)
        {
            _key = key;
        }

        public void Prepare(PreparingEnlistment preparingEnlistment)
        {
            Log.Info("Prepare notification received");

            //Perform transactional work
            using (var db = Dao.Default.Create())
            {
                using (var dbTransaction = db.Connection.BeginTransaction())
                {
                    using (var cmd = db.Connection.CreateCommand())
                    {
                        try
                        {
                            DbUtil.Execute(
                                "INSERT INTO GlobalSetting (SettingGroup, SettingKey, LastModified, SettingValue, System) VALUES ({0}, {1}, {2}, {3}, {4})",
                                db, cmd,
                                new[] { DbType.String, DbType.String, DbType.DateTime, DbType.String, DbType.Boolean },
                                new object[] { GlobalMutexScope.Group, _key, DateTime.UtcNow, "", true });
                            dbTransaction.Commit();
                            Log.InfoFormat("Trans: Status={0}, LocalId={1}, DistributedId={2}.",
                                           Transaction.Current.TransactionInformation.Status,
                                           Transaction.Current.TransactionInformation.LocalIdentifier,
                                           Transaction.Current.TransactionInformation.DistributedIdentifier);
                            //If work finished correctly, reply prepared
                            preparingEnlistment.Prepared();
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex.Message);
                            // otherwise, do a ForceRollback
                            preparingEnlistment.ForceRollback();
                            throw;
                        }
                    }
                }
            }
        }

        public void Commit(Enlistment enlistment)
        {
            Log.Info("Commit notification received");

            //Do any work necessary when commit notification is received
            using (var db = Dao.Default.Create())
            {
                using (var cmd = db.Connection.CreateCommand())
                {
                    try
                    {
                        DbUtil.Execute("DELETE FROM GlobalSetting WHERE SettingGroup={0} AND SettingKey={1}",
                            db, cmd, new[] { DbType.String, DbType.String }, new object[] { GlobalMutexScope.Group, _key });
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex.Message);
                        throw;
                    }
                }
            }

            //Declare done on the enlistment
            enlistment.Done();
        }

        public void Rollback(Enlistment enlistment)
        {
            Log.Info("Rollback notification received");

            //Do any work necessary when rollback notification is received
            using (var db = Dao.Default.Create())
            {
                using (var cmd = db.Connection.CreateCommand())
                {
                    try
                    {
                        DbUtil.Execute("DELETE FROM GlobalSetting WHERE SettingGroup={0} AND SettingKey={1}",
                            db, cmd, new[] { DbType.String, DbType.String }, new object[] { GlobalMutexScope.Group, _key });
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex.Message);
                        throw;
                    }
                }
            }

            //Declare done on the enlistment
            enlistment.Done();
        }

        public void InDoubt(Enlistment enlistment)
        {
            Log.Info("In doubt notification received");

            //Do any work necessary when indout notification is received
            using (var db = Dao.Default.Create())
            {
                using (var cmd = db.Connection.CreateCommand())
                {
                    try
                    {
                        DbUtil.Execute("DELETE FROM GlobalSetting WHERE SettingGroup={0} AND SettingKey={1}",
                            db, cmd, new[] { DbType.String, DbType.String }, new object[] { GlobalMutexScope.Group, _key });
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex.Message);
                        throw;
                    }
                }
            }

            //Declare done on the enlistment
            enlistment.Done();
        }
    }

    class EnlistmentWCFImpl : IEnlistmentNotification
    {
        private readonly static log4net.ILog Log = log4net.LogManager.GetLogger(typeof(EnlistmentLocalImpl));

        private readonly string _key;
        internal EnlistmentWCFImpl(string key)
        {
            _key = key;
        }

        public void Prepare(PreparingEnlistment preparingEnlistment)
        {
            Log.Info("Prepare notification received");

            //Perform transactional work
            try
            {

                Log.InfoFormat("Trans: Status={0}, LocalId={1}, DistributedId={2}.",
                                Transaction.Current.TransactionInformation.Status,
                                Transaction.Current.TransactionInformation.LocalIdentifier,
                                Transaction.Current.TransactionInformation.DistributedIdentifier);
                //If work finished correctly, reply prepared
                preparingEnlistment.Prepared();
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                // otherwise, do a ForceRollback
                preparingEnlistment.ForceRollback();
                throw;
            }
        }

        public void Commit(Enlistment enlistment)
        {
            Log.Info("Commit notification received");

            //Do any work necessary when commit notification is received


            //Declare done on the enlistment
            enlistment.Done();
        }

        public void Rollback(Enlistment enlistment)
        {
            Log.Info("Rollback notification received");

            //Do any work necessary when rollback notification is received


            //Declare done on the enlistment
            enlistment.Done();
        }

        public void InDoubt(Enlistment enlistment)
        {
            Log.Info("In doubt notification received");

            //Do any work necessary when indout notification is received
            using (var db = Dao.Default.Create())
            {
                using (var cmd = db.Connection.CreateCommand())
                {
                    try
                    {
                        DbUtil.Execute("DELETE FROM GlobalSetting WHERE SettingGroup={0} AND SettingKey={1}",
                            db, cmd, new[] { DbType.String, DbType.String }, new object[] { GlobalMutexScope.Group, _key });
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex.Message);
                        throw;
                    }
                }
            }

            //Declare done on the enlistment
            enlistment.Done();
        }
    }

    class DurableResourceManager : ISinglePhaseNotification
    {
        private readonly static log4net.ILog Log = log4net.LogManager.GetLogger(typeof(DurableResourceManager));

        #region IEnlistmentNotification Members

        public void Commit(Enlistment enlistment)
        {
            Log.Info("DurableRM.Commit");
            enlistment.Done();
        }

        public void InDoubt(Enlistment enlistment)
        {
            Log.Info("DurableRM.InDoubt");
            throw new Exception("The method or operation is not implemented.");
        }

        public void Prepare(PreparingEnlistment preparingEnlistment)
        {
            Log.Info("DurableRM.Prepare");
            _dbLock.Acquire(GlobalMutexScope.MutexTimeout, GlobalMutexScope.AcquireMutexTimeout, null);
            preparingEnlistment.Prepared();
        }

        public void Rollback(Enlistment enlistment)
        {
            Log.Info("DurableRM.Rollback");
            enlistment.Done();
        }

        public void SinglePhaseCommit(SinglePhaseEnlistment singlePhaseEnlistment)
        {

        }
        #endregion

        private readonly ConcurrentDTCDbLock _dbLock;
        public DurableResourceManager(string key)
        {
            if (null == Transaction.Current) throw new Exception("Transaction is null");
            _dbLock = new ConcurrentDTCDbLock { Key = key };
        }
    }

    /// <summary>
    /// Concurrency control based on Distributed Transaction Coordinate service. This implementation is a global locking mechenisame.
    /// </summary>
    internal class ConcurrentDTCLock : ConcurrentLock
    {
        private readonly static log4net.ILog Log = log4net.LogManager.GetLogger(typeof(ConcurrentDTCLock));

        private TransactionScope _globalScope;
        private TransactionScope _mutexScope;
        private TransactionScope _biScope;

        public override void Acquire(TimeSpan mutexTimeout, TimeSpan acquireTimeout, params object[] contextObjs)
        {
            _globalScope = new TransactionScope(TransactionScopeOption.Required);
            var transactionOptions = new TransactionOptions
                                         {
                                             IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted,
                                             Timeout = mutexTimeout
                                         };
            _mutexScope = new TransactionScope(TransactionScopeOption.RequiresNew, transactionOptions, EnterpriseServicesInteropOption.None);
            //Transaction.Current.EnlistDurable(Guid.NewGuid(), new DurableResourceManager(Key), EnlistmentOptions.None);
            //var resMgr = new DurableResourceManager();
            //Transaction.Current.EnlistDurable(Guid.NewGuid(), new EnlistmentLocalImpl(Key), EnlistmentOptions.None);
            //Transaction.Current.EnlistVolatile(new EnlistmentLocalImpl(Key), EnlistmentOptions.None);
            
            //var newTransaction = TransactionInterop.GetDtcTransaction(Transaction.Current);
            //using(var _dtcScope1 = new TransactionScope(newTransaction))

            Log.InfoFormat("Trans: Status={0}, LocalId={1}, DistributedId={2}.", Transaction.Current.TransactionInformation.Status, Transaction.Current.TransactionInformation.LocalIdentifier, Transaction.Current.TransactionInformation.DistributedIdentifier);
            using (var db = Dao.Default.Create())
            {
                var dbTransaction = db.Connection.BeginTransaction(System.Data.IsolationLevel.ReadUncommitted);
                using (var cmd = db.Connection.CreateCommand())
                {
                    try
                    {
                        base.Acquire(mutexTimeout, acquireTimeout, db, cmd);
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
            _biScope = new TransactionScope(TransactionScopeOption.RequiresNew);
        }

        public override bool Lock(out bool acquiredLock, TimeSpan mutexTimeout, params object[] contextObjs)
        {
            var db = (DbCommon)contextObjs[0];
            var cmd = (System.Data.Common.DbCommand)contextObjs[1];
            var value = string.Format("{0},{1}", Environment.MachineName, Process.GetCurrentProcess().Id);
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

        public override void Unlock()
        {
            try
            {
                Log.InfoFormat("Trans: Status={0}, LocalId={1}, DistributedId={2}.", Transaction.Current.TransactionInformation.Status,
                               Transaction.Current.TransactionInformation.LocalIdentifier, Transaction.Current.TransactionInformation.DistributedIdentifier);
                _biScope.Complete();
            }
            catch (Exception ex)
            {
                Log.WarnFormat("Commit biScope failed: {0}", ex.Message);
            }
            try
            {
                Log.InfoFormat("Trans: Status={0}, LocalId={1}, DistributedId={2}.", Transaction.Current.TransactionInformation.Status,
                               Transaction.Current.TransactionInformation.LocalIdentifier, Transaction.Current.TransactionInformation.DistributedIdentifier);
                //Never do _mutexScope.Complete() so that we can automatic rollback insert method
                _mutexScope.Dispose();//Transaction.Current.Rollback();

            }
            catch (Exception ex)
            {
                Log.WarnFormat("Remove Key '{0}' failed: {1}", Key, ex.Message);
            }
            try
            {
                Log.InfoFormat("Trans: Status={0}, LocalId={1}, DistributedId={2}.", Transaction.Current.TransactionInformation.Status,
                               Transaction.Current.TransactionInformation.LocalIdentifier, Transaction.Current.TransactionInformation.DistributedIdentifier);
                _globalScope.Complete();
            }
            catch (Exception ex)
            {
                Log.WarnFormat("Commit globalScope failed: {0}", ex.Message);
            }
        }
    }
}