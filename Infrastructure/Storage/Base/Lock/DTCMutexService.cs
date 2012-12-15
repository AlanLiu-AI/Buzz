using System;
using System.ServiceModel;
using System.Transactions;
using Runner.Base.Db;
using Runner.Base.Util;

namespace Runner.Base.Lock
{
    [ServiceContract(Namespace = "http://IDTCMutexService")]
    public interface IDTCMutexService
    {
        [OperationContract]
        [TransactionFlow(TransactionFlowOption.Mandatory)]
        bool Acquire(string key);

        [OperationContract]
        [TransactionFlow(TransactionFlowOption.Mandatory)]
        bool AcquireEx(string key, TimeSpan mutexTimeout, TimeSpan acquireTimeout);
    }

    [WCFLogErrorBehavior(Name = "DTCMutexService")]
    [ServiceBehavior(TransactionIsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted, InstanceContextMode = InstanceContextMode.PerCall)]
    public class DTCMutexService : IDTCMutexService
    {
        private readonly static log4net.ILog Log = log4net.LogManager.GetLogger(typeof(DTCMutexService).Name);

        [OperationBehavior(TransactionScopeRequired = true)]
        public bool Acquire(string key)
        {
            return AcquireEx(key, GlobalMutexScope.MutexTimeout, GlobalMutexScope.AcquireMutexTimeout);
        }

        [OperationBehavior(TransactionScopeRequired = true)]
        public bool AcquireEx(string key, TimeSpan mutexTimeout, TimeSpan acquireTimeout)
        {
            Log.InfoFormat("Trans: Status={0}, LocalId={1}, DistributedId={2}.", Transaction.Current.TransactionInformation.Status,
                Transaction.Current.TransactionInformation.LocalIdentifier, Transaction.Current.TransactionInformation.DistributedIdentifier);
            var dbLock = GlobalMutexScope.AquireConcurrentLock(key, LockStyle.Database, mutexTimeout, acquireTimeout);
            return true;
        }

        [OperationBehavior(TransactionScopeRequired = true, TransactionAutoComplete = true)]
        public void Release(string key)
        {
            ConcurrentDatabaseLock.Release(key);
        }
    }
}