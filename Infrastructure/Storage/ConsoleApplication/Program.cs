using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading;
using System.Transactions;
using Runner.Base;
using Runner.Base.Db;
using Runner.Base.Lock;
using Runner.Base.Util;

namespace ConsoleApplication
{
    class Program
    {
        private readonly static log4net.ILog Log = log4net.LogManager.GetLogger(typeof(Program).Name);

        static void Main(string[] args)
        {
            Log4netUtil.EmbbededInitial("Test.log");
            var setting = ConfigurationUtil.LoadAppSettings(args);
            var key = ParametersUtil.GetParam("key", setting, "Key");
            var noPause = ParametersUtil.GetBoolParam("nopause", setting, false);
            Global.ConnectionString = "user id=sa;password=sa123;server=tcp:localhost, 1432; database=AQ30R4Db2;MultipleActiveResultSets=True;";
            Global.DbType = Runner.Base.Db.SqlType.MsSql;
            var go = ParametersUtil.GetParam("go", setting, "");
            try
            {
                if(string.CompareOrdinal(go, "client") == 0)
                {
                    PSPEClient.PSPEClient.Run(setting);
                }
                else if (string.CompareOrdinal(go, "server") == 0)
                {
                    PSPEServer.PSPEServer.Run(setting);
                }
                else if(string.CompareOrdinal(go, "mclient") ==0)
                {
                    using (var client = new ServiceReference1.DTCMutexServiceClient("WSHttpBinding_IDTCMutexService"))
                    {
                        client.Open();
                        var transactionOptions = new TransactionOptions
                        {
                            IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted,
                            Timeout = new TimeSpan(0, 10, 0)
                        };
                        using (new TransactionScope(TransactionScopeOption.Required, transactionOptions))
                        {
                            client.Acquire("123");
                            Console.WriteLine("Enter to continue...");
                            Console.ReadLine();
                        }
                    }
                    /*using (var client = DTCMutexClient.GetInstance())
                    {
                        client.Open();
                        var transactionOptions = new TransactionOptions
                        {
                            IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted,
                            Timeout = new TimeSpan(0,10,0)
                        };
                        using (new TransactionScope(TransactionScopeOption.Required, transactionOptions))
                        {
                            client.Acquire("123");
                            client.Release("123");
                        }
                    }*/
                }
                else
                {
                    using (new GlobalMutexScope(key, LockStyle.MSDTC)) //this will rollback for GlobalMutex violation
                    {
                        using (var db = Dao.Default.Create())
                        {
                            using (var cmd = db.Connection.CreateCommand())
                            {
                                DbUtil.Execute("DELETE FROM _TestTable WHERE Test LIKE '" + key + "%' ", db, cmd);
                                DbUtil.Execute("INSERT INTO _TestTable (Test) VALUES ({0})", db, cmd,
                                                new DbType[] { DbType.String },
                                                new object[] { key + "__" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") });
                            }
                        }
                        Console.WriteLine("Continue...");
                        var keyInfo = Console.ReadKey();
                        switch (keyInfo.Key)
                        {
                            case ConsoleKey.A:
                                Environment.Exit(-1);
                                break;
                        }
                    }
                    //using (var tranScope = new TransactionScope(TransactionScopeOption.Required))
                    //{
                    //    Log.InfoFormat("Trans: Status={0}, LocalId={1}, DistributedId={2}.",
                    //                   Transaction.Current.TransactionInformation.Status,
                    //                   Transaction.Current.TransactionInformation.LocalIdentifier,
                    //                   Transaction.Current.TransactionInformation.DistributedIdentifier);
                    //    var mutexTranScope = new GlobalMutexScope(key, LockStyle.MSDTC);
                    //    try
                    //    {
                    //        using (var biScope = new TransactionScope(TransactionScopeOption.RequiresNew))
                    //        {
                    //            using (var db = Dao.Default.Create())
                    //            //this part will commit for business data processing
                    //            {
                    //                using (var cmd = db.Connection.CreateCommand())
                    //                {
                    //                    DbUtil.Execute("DELETE FROM _TestTable WHERE Test LIKE '" + key + "%' ", db, cmd);
                    //                    DbUtil.Execute("INSERT INTO _TestTable (Test) VALUES ({0})", db, cmd,
                    //                                   new DbType[] { DbType.String },
                    //                                   new object[] { key + "__" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") });
                    //                }
                    //            }
                    //            Console.WriteLine("Continue...");
                    //            var keyInfo = Console.ReadKey();
                    //            switch (keyInfo.Key)
                    //            {
                    //                case ConsoleKey.Y:
                    //                    biScope.Complete();
                    //                    break;
                    //                case ConsoleKey.A:
                    //                    Environment.Exit(-1);
                    //                    break;
                    //                default:
                    //                    biScope.Dispose();
                    //                    break;
                    //            }
                    //        }
                    //    }
                    //    catch (Exception ex1)
                    //    {
                    //        Log.Error(ex1);
                    //        throw;
                    //    }
                    //    finally
                    //    {
                    //        mutexTranScope.Dispose();
                    //    }
                    //    tranScope.Complete();
                    //}
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
            if (noPause) return;
            Console.WriteLine("Enter to exit!");
            Console.ReadLine();
        }
    }
}
