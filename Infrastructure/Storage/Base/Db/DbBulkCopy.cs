using System;
using System.Data;
using System.Data.SqlClient;
using Oracle.DataAccess.Client;

namespace Runner.Base.Db
{
    public interface IDbBulkCopy
    {
        string ConnectionString { get; }
        void Load(string table, DataTable records, int bachSize, int timeout);
    }

    abstract class DbBulkCopyBase : IDbBulkCopy
    {
        private readonly string _connectionString;

        protected DbBulkCopyBase(string connectionString)
        {
            _connectionString = connectionString;
        }

        public string ConnectionString
        {
            get { return _connectionString; }
        }

        public abstract void Load(string table, DataTable records, int bachSize, int timeout);
    }

    class OracleDbBulkCopy : DbBulkCopyBase
    {
        public OracleDbBulkCopy(string connectionString)
            : base(connectionString)
        {

        }

        public override void Load(string table, DataTable records, int bachSize, int timeout)
        {
            using (var oraBulkCopy = new OracleBulkCopy(ConnectionString))
            {
                // set bulk copy options
                oraBulkCopy.BulkCopyOptions = OracleBulkCopyOptions.Default;

                // mapping between data source and target database table
                oraBulkCopy.DestinationTableName = table;

                // batch and time options
                oraBulkCopy.BatchSize = bachSize;
                oraBulkCopy.BulkCopyTimeout = timeout;  // in seconds; i've set my time out to 10 minutes

                // do the actual copy
                oraBulkCopy.WriteToServer(records);
            }
        }
    }

    class MsSqlDbBulkCopy : DbBulkCopyBase
    {
        public MsSqlDbBulkCopy(string connectionString)
            : base(connectionString)
        {

        }

        public override void Load(string table, DataTable records, int bachSize, int timeout)
        {
            // this connection is to a sql server instance on my PC; change this if you're going to play with this code!
            using (var sqlservBulkCopy = new SqlBulkCopy(ConnectionString))
            {
                // mapping between data source and target database table
                sqlservBulkCopy.DestinationTableName = table;

                // batch and time options
                sqlservBulkCopy.BatchSize = bachSize;
                sqlservBulkCopy.BulkCopyTimeout = timeout;  // in seconds; i've set my time out to 10 minutes

                // do the actual copy
                sqlservBulkCopy.WriteToServer(records);
                records.Clear();
            }
        }
    }

    public class DbBulkCopy : IDbBulkCopy
    {
        private readonly DbBulkCopyBase _bulkCopy;
        public DbBulkCopy(string connectionString, SqlType type)
        {
            _bulkCopy = GetInstance(connectionString, type);
        }

        private static DbBulkCopyBase GetInstance(string connectionString, SqlType type)
        {
            DbBulkCopyBase bulkCopy;
            switch (type)
            {
                case SqlType.Oracle:
                    bulkCopy = new OracleDbBulkCopy(connectionString);
                    break;
                case SqlType.MsSql:
                    bulkCopy = new MsSqlDbBulkCopy(connectionString);
                    break;
                default:
                    throw new Exception("DbBulkCopy ctor does not support SqlType '" + type + "'");
            }
            return bulkCopy;
        }

        public string ConnectionString
        {
            get { return _bulkCopy.ConnectionString; }
        }

        public void Load(string table, DataTable records)
        {
            //Default each 100000 a commit
            //Timeout to 10 minutes
            Load(table, records, 100000, 600);
        }

        public void Load(string table, DataTable records, int bachSize, int timeout)
        {
            _bulkCopy.Load(table, records, bachSize, timeout);
        }
    }
}
