using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Transactions;
using Runner.Base.Util;

namespace PSPEClient
{
    internal class PSPEClient
    {
        public static void Run(IDictionary<string, string> setting)
        {
            var commit = ParametersUtil.GetBoolParam("commit", setting, false);
            var exit = ParametersUtil.GetBoolParam("exit", setting, false);
            var     sec1 = ParametersUtil.GetBoolParam("sec1", setting, false);
            var sec2 = ParametersUtil.GetBoolParam("sec2", setting, false);
            using (var ts = new TransactionScope())
            {
                //*section1begin
                if (sec1)
                {
                    DurableRM durableRM = new DurableRM();
                    durableRM.OpenConnection();
                    durableRM.DoWork();
                }
                /*section1end*/

                var dbProxy = new DatabaseProxy();
                dbProxy.OpenConnection();
                dbProxy.DoWork();

                //*section2begin
                if (sec2)
                {
                    DurableRM durableRM = new DurableRM();
                    durableRM.OpenConnection();
                    durableRM.DoWork();
                }
                /*section2end*/

                if (exit)
                    Environment.Exit(-1);
                if (commit)
                    ts.Complete();
            }
            //System.Console.ReadLine();
        }

        class DatabaseProxy
        {
            private readonly static log4net.ILog Log = log4net.LogManager.GetLogger(typeof(DatabaseProxy));
            private PSPEServer.PSPEDatabaseServer db;
            private InternalRM internalRM;

            public void OpenConnection()
            {
                Log.Info("DatabaseProxy.OpenConnection");
                // connecting to the remote database
                TcpChannel tcpChannel = new TcpChannel();
                ChannelServices.RegisterChannel(tcpChannel);
                this.db = (PSPEServer.PSPEDatabaseServer)Activator.GetObject(
                    typeof(PSPEServer.PSPEDatabaseServer), "tcp://localhost:8085/MyDatabase");
                if (null == db)
                {
                    Log.Info("Cannot connect to the server");
                }
                else
                {
                    Log.Info("Internal tx id:" + db.Connect());
                }

                // enlisting in the transaction
                if (null != this.internalRM)
                {
                    throw new Exception("we don't support multiple connections, this is just a sample");
                }
                internalRM = new InternalRM(db);
                internalRM.Enlist();
            }

            public void DoWork()
            {
                Log.InfoFormat("DatabaseProxy.DoWork: Status={0}, LocalId={1}, DistributedId={2}.",
                               Transaction.Current.TransactionInformation.Status,
                               Transaction.Current.TransactionInformation.LocalIdentifier,
                               Transaction.Current.TransactionInformation.DistributedIdentifier);
                db.DoWork();
            }

            class InternalRM : IPromotableSinglePhaseNotification
            {
                #region IPromotableSinglePhaseNotification Members

                // This member will be called during the call to EnlistPromotableSinglePhase
                // The RM will usually allocate its internal transaction state here
                public void Initialize()
                {
                    Log.Info("InternalRM.Initialize");
                }

                // This method will be called if the RM should Rollback the
                // transaction.  Note that this method will be called even if
                // the transaction has been promoted to a distributed transaction.
                public void Rollback(SinglePhaseEnlistment singlePhaseEnlistment)
                {
                    Log.Info("InternalRM.Rollback");
                    db.RollbackWork();
                    singlePhaseEnlistment.Aborted();
                }

                // This method will be called when the RM should Commit the
                // transaction.  Note that this method will be called even if
                // the transaction has actually been promoted to a distributed
                // transaction.
                public void SinglePhaseCommit(SinglePhaseEnlistment singlePhaseEnlistment)
                {
                    Log.Info("InternalRM.SinglePhaseCommit");
                    db.CommitWork();
                    singlePhaseEnlistment.Committed();
                }

                #endregion

                #region ITransactionPromoter Members

                // This method will be called if System.Transactions
                // determines that the transaction actually needs the support of
                // a fully distributed transaction manager.  The return value of
                // this method is a promoted representation of the transaction
                // usually in the form of transmitter/receiver propagation token
                public byte[] Promote()
                {
                    Log.Info("InternalRm.Promote");
                    return db.Promote();
                }

                #endregion

                private PSPEServer.PSPEDatabaseServer db;

                public InternalRM(PSPEServer.PSPEDatabaseServer db)
                {
                    this.db = db;
                }

                public void Enlist()
                {
                    Log.Info("InternalRM.Enlist");
                    if (null != Transaction.Current)
                    {
                        if (!Transaction.Current.EnlistPromotableSinglePhase(this))
                        {
                            Log.Info("PSPE failed, doing regular Enlist");
                            // PSPE failed; we need to use the regular enlistment
                            db.Enlist(TransactionInterop.GetTransmitterPropagationToken(Transaction.Current));
                        }
                    }
                }
            }
        }

        class DurableRM : IEnlistmentNotification
        {
            private readonly static log4net.ILog Log = log4net.LogManager.GetLogger(typeof(DurableRM));

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
                // first a durable RM will log preparingEnlistment.RecoveryInformation(), but this is just a sample
                preparingEnlistment.Prepared();
            }

            public void Rollback(Enlistment enlistment)
            {
                Log.Info("DurableRM.Rollback");
                enlistment.Done();
            }

            #endregion

            public void OpenConnection()
            {
                Log.Info("DurableRM.OpenConnection and enlist durable");
                if (null != Transaction.Current)
                {
                    Transaction.Current.EnlistDurable(Guid.NewGuid(), this, EnlistmentOptions.None);
                }
            }
            public void DoWork()
            {
                Log.Info("DurableRM - DoWork");
            }
        }
    }
}