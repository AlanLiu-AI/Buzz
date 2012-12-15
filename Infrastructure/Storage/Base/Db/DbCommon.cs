using System;
using System.Data.Common;
using System.Data.SqlClient;
using System.Data.SqlServerCe;
using Oracle.DataAccess.Client;

namespace Runner.Base.Db
{
    public abstract class DbCommon : IDisposable
    {
        protected static readonly log4net.ILog Log = log4net.LogManager.GetLogger(typeof(DbCommon).Name);

        readonly DbConnection _conntion;

        protected DbCommon(string connectionString, SqlType sqlType)
        {
            ConnectionString = connectionString;
            SqlType = sqlType;
            _conntion = CreateConnection();
        }

        public SqlType SqlType { get; private set; }

        public String ConnectionString { get; private set; }

        public DbConnection Connection
        {
            get { return _conntion; }
            //set { _conntion = value; }
        }

        public static DbCommon Create(string scon, SqlType type)
        {
            DbCommon db = null;
            switch (type)
            {
                case SqlType.MsSql:
                    db = new DbMsSql(scon, type);
                    break;
                case SqlType.Oracle:
                    db = new DbOracle(scon, type);
                    break;
                case SqlType.MsSqlCe:
                    db = new DbMsSqlCe(scon, type);
                    break;
            }
            if (db == null) throw new Exception("DbCommon do not support type " + type);
            db._conntion.ConnectionString = scon;
            db._conntion.Open();
            return db;
        }

        DbConnection CreateConnection()
        {
            return NewConnection();
        }

        public abstract DbStatement CreateParameters(DbCommand cmd);
        public abstract double GetDouble(DbDataReader rd, int i);
        public abstract int GetInteger(DbDataReader rd, int i);
        public abstract DbConnection NewConnection();
        public abstract DbDataAdapter DataAdapter(DbCommand cmd);
        public abstract bool GetBoolean(DbDataReader rd, int i);

        public string GetString(DbDataReader rd, int i)
        {
            return rd.IsDBNull(i) ? string.Empty : rd.GetString(i);
        }

        public byte[] GetBlob(DbDataReader rd, int i)
        {
            return rd.IsDBNull(i) ? null : rd.GetValue(i) as byte[];
        }

        public long GetInt64(DbDataReader rd, int i, long defaultValue)
        {
            return rd.IsDBNull(i) ? defaultValue : Convert.ToInt64(rd.GetValue(i));
        }

        public int GetInt32(DbDataReader rd, int i, int defaultValue)
        {
            return rd.IsDBNull(i) ? defaultValue : Convert.ToInt32(rd.GetValue(i));
        }
        
        public void Dispose()
        {
            using (_conntion) { }
        }
    }

    class DbOracle : DbCommon
    {
        internal DbOracle(string connectionString, SqlType sqlType)
            : base(connectionString, sqlType)
        {

        }
        public override DbStatement CreateParameters(DbCommand cmd)
        {
            return new OracleStatement(cmd);
        }

        public override double GetDouble(DbDataReader rd, int i)
        {
            var ord = (OracleDataReader)rd;
            return (double)ord.GetOracleDecimal(i);
        }

        public override bool GetBoolean(DbDataReader rd, int i)
        {
            return rd.GetDecimal(i) > 0;
        }

        public override DbConnection NewConnection()
        {
            return new OracleConnection { ConnectionString = ConnectionString };
        }

        public override DbDataAdapter DataAdapter(DbCommand cmd)
        {
            var oracleDA = new OracleDataAdapter((OracleCommand)cmd);
            try
            {
                oracleDA.UpdateCommand = new OracleCommandBuilder(oracleDA).GetUpdateCommand();
                oracleDA.InsertCommand = new OracleCommandBuilder(oracleDA).GetInsertCommand();
                oracleDA.DeleteCommand = new OracleCommandBuilder(oracleDA).GetDeleteCommand();
            }
            catch (Exception ex)
            {
                Log.Warn(ex);
            }
            return oracleDA;
        }

        public override int GetInteger(DbDataReader rd, int i)
        {
            return (int)rd.GetDecimal(i);
        }
    }

    class DbMsSql : DbCommon
    {
        internal DbMsSql(string connectionString, SqlType sqlType)
            : base(connectionString, sqlType)
        {
            
        }
        public override DbStatement CreateParameters(DbCommand cmd)
        {
            return new SqlStatement(cmd);
        }

        public override double GetDouble(DbDataReader rd, int i)
        {
            return rd.GetDouble(i);
        }

        public override bool GetBoolean(DbDataReader rd, int i)
        {
            return rd.GetBoolean(i);
        }

        public override DbConnection NewConnection()
        {
            return new SqlConnection { ConnectionString = ConnectionString };
        }

        public override DbDataAdapter DataAdapter(DbCommand cmd)
        {
            var sqlDA = new SqlDataAdapter((SqlCommand)cmd);
            try
            {
                sqlDA.UpdateCommand = new SqlCommandBuilder(sqlDA).GetUpdateCommand();
                sqlDA.InsertCommand = new SqlCommandBuilder(sqlDA).GetInsertCommand();
                sqlDA.DeleteCommand = new SqlCommandBuilder(sqlDA).GetDeleteCommand();
            }
            catch (Exception ex)
            {
                Log.Warn(ex);
            }
            return sqlDA;
        }

        public override int GetInteger(DbDataReader rd, int i)
        {
            return rd.GetInt32(i);
        }
    }

    class DbMsSqlCe : DbMsSql
    {
        internal DbMsSqlCe(string connectionString, SqlType sqlType)
            : base(connectionString, sqlType)
        {
            
        }
        public override DbStatement CreateParameters(DbCommand cmd)
        {
            return new SqlCeStatement(cmd);
        }

        public override double GetDouble(DbDataReader rd, int i)
        {
            return rd.GetDouble(i);
        }

        public override bool GetBoolean(DbDataReader rd, int i)
        {
            return rd.GetBoolean(i);
        }
        public override DbConnection NewConnection()
        {
            return new SqlCeConnection { ConnectionString = ConnectionString };
        }

        public override DbDataAdapter DataAdapter(DbCommand cmd)
        {
            var sqlCeDA = new SqlCeDataAdapter((SqlCeCommand)cmd);
            try
            {
                sqlCeDA.UpdateCommand = new SqlCeCommandBuilder(sqlCeDA).GetUpdateCommand();
                sqlCeDA.InsertCommand = new SqlCeCommandBuilder(sqlCeDA).GetInsertCommand();
                sqlCeDA.DeleteCommand = new SqlCeCommandBuilder(sqlCeDA).GetDeleteCommand();
            }
            catch (Exception ex)
            {
                Log.Warn(ex);
            }
            return sqlCeDA;
        }

        public override int GetInteger(DbDataReader rd, int i)
        {
            return rd.GetInt32(i);
        }
    }
}
