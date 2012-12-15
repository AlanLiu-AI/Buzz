using System;
using System.Collections.Generic;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Transactions;
using System.Diagnostics;

namespace PSPEServer
{
    internal class PSPEServer
    {
        private readonly static log4net.ILog Log = log4net.LogManager.GetLogger(typeof(PSPEServer));

        public static void Run(IDictionary<string, string> setting)
        {
            TcpChannel tcpChannel = new TcpChannel(8085);
            ChannelServices.RegisterChannel(tcpChannel, false);
            RemotingConfiguration.RegisterWellKnownServiceType(Type.GetType("PSPEServer.PSPEDatabaseServer"),
                "MyDatabase", WellKnownObjectMode.Singleton);
            Log.Info("Server running...");
            System.Console.ReadLine();
        }
    }

    public class PSPEDatabaseServer : MarshalByRefObject
    {
        private readonly static log4net.ILog Log = log4net.LogManager.GetLogger(typeof(PSPEDatabaseServer));
        private int internalTxID = 0;
        private CommittableTransaction tx;
        private InternalServerRM internalServerRM;

        public int Connect()
        {
            Log.Info("client connected");
            return ++internalTxID;
        }

        public void DoWork()
        {
            Log.Info("PSPEDBServer.DoWork");
        }

        public byte[] Promote()
        {
            Log.Info("PSPEDBServer.Promote");
            this.tx = new CommittableTransaction();
            //Debug.Assert(this.internalServerRM == null);
            if (this.internalServerRM != null) Log.Error("this.internalServerRM != null");
            // the following statement will cause the transaction to be promoted to MSDTC
            byte[] txToken = TransactionInterop.GetTransmitterPropagationToken(this.tx);
            Enlist(txToken);
            return txToken;
        }

        public void CommitWork()
        {
            Log.Info("PSPEDBServer.CommitWork");
            if (tx != null)
            {
                Log.InfoFormat("tx.Commit: Status={0}, LocalId={1}, DistributedId={2}.",
                                Transaction.Current.TransactionInformation.Status,
                                Transaction.Current.TransactionInformation.LocalIdentifier,
                                Transaction.Current.TransactionInformation.DistributedIdentifier);
                // we have a distributed transaction, and so we have to commit it
                tx.Commit();
            }
            else
            {
                // we only have an internal tx
                Log.Info("committing internal tx:" + internalTxID);
            }
        }

        public void RollbackWork()
        {
            Log.Info("PSPEDBServer.RollbackWork");
            if (tx != null)
            {
                Log.InfoFormat("tx.Rollback: Status={0}, LocalId={1}, DistributedId={2}.",
                                Transaction.Current.TransactionInformation.Status,
                                Transaction.Current.TransactionInformation.LocalIdentifier,
                                Transaction.Current.TransactionInformation.DistributedIdentifier);
                // we have a distributed transaction, and so we have to rollback it
                tx.Rollback();
            }
            else
            {
                // we only have an internal tx
                Log.Info("aborting internal tx:" + internalTxID);
            }
        }

        public void Enlist(byte[] txToken)
        {
            Log.Info("PSPEDBServer.Enlist");
            this.internalServerRM = new InternalServerRM();
            this.internalServerRM.Enlist(txToken);
        }

        private class InternalServerRM : ISinglePhaseNotification
        {
            private readonly static log4net.ILog Log = log4net.LogManager.GetLogger(typeof(InternalServerRM));

            #region ISinglePhaseNotification Members

            public void SinglePhaseCommit(SinglePhaseEnlistment singlePhaseEnlistment)
            {
                Log.Info("InternalServerRM.SPC");
                singlePhaseEnlistment.Committed();
            }

            #endregion

            #region IEnlistmentNotification Members

            public void Commit(Enlistment enlistment)
            {
                Log.Info("InternalServerRM.Commit");
                enlistment.Done();
            }

            public void InDoubt(Enlistment enlistment)
            {
                Log.Info("InternalServerRM.InDoubt");
                throw new Exception("The method or operation is not implemented.");
            }

            public void Prepare(PreparingEnlistment preparingEnlistment)
            {
                Log.Info("InternalServerRM.Prepare");
                // first a durable RM will log preparingEnlistment.RecoveryInformation(), but this is just a sample
                preparingEnlistment.Prepared();
            }

            public void Rollback(Enlistment enlistment)
            {
                Log.Info("InternalServerRM.Rollback");
                enlistment.Done();
            }

            #endregion

            private Guid rmGuid = new Guid("{B14FF9BB-8419-4dbc-A78C-3C1453D60AC4}");

            public void Enlist(byte[] txToken)
            {
                Log.Info("InternalServerRM.Enlist");
                TransactionInterop.GetTransactionFromTransmitterPropagationToken(txToken).EnlistDurable(
                    this.rmGuid, this, EnlistmentOptions.None);
            }
        }
    }
}