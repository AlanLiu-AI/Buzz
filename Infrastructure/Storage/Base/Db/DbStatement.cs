using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Data.SqlServerCe;
using System.Text;
using Oracle.DataAccess.Client;

namespace Runner.Base.Db
{
    public abstract class DbStatement
    {

        protected static readonly log4net.ILog Log = log4net.LogManager.GetLogger(typeof(DbStatement).Name);

        public DbCommand DbCmd;
        readonly List<string> _columns; //List of column name, e.g. Id, Name
        readonly List<string> _values; //List of column value assign, e.g. :0, :1
        readonly List<ExtType> _parameters; //List of object instances, e.g. "example", 12
        protected DbParameter Output;

        protected DbStatement(DbCommand cmd)
        {
            DbCmd = cmd;
            _columns = new List<string>();
            _values = new List<string>();
            _parameters = new List<ExtType>();
        }

        protected void Column(string name, string val)
        {
            _columns.Add(name);
            _values.Add(val);
        }

        public string WhereParameter(int i, DbType type)
        {
            var ext = new ExtType();

            if (type == DbType.Binary)
                ext.Type = DbCommonType.LongBinary;
            else if (type == DbType.Xml)
                ext.Type = DbCommonType.Xml;
            else if (type == DbType.Boolean)
                ext.Type = DbCommonType.Boolean;
            else if (type == DbType.Guid)
                ext.Type = DbCommonType.Guid;

            ext.DBType = type;
            _parameters.Add(ext);
            return Name(i);
        }

        public void Parameter(int i, string name, DbType type)
        {
            Column(name, Name(i));
            var ext = new ExtType();

            if (type == DbType.Binary)
                ext.Type = DbCommonType.LongBinary;
            else if (type == DbType.Xml)
                ext.Type = DbCommonType.Xml;
            else if (type == DbType.Boolean)
                ext.Type = DbCommonType.Boolean;
            else if (type == DbType.Guid)
                ext.Type = DbCommonType.Guid;

            ext.DBType = type;
            _parameters.Add(ext);
        }

        public void VarBinary(int i, string name)
        {
            Column(name, Name(i));
            var ext = new ExtType {Type = DbCommonType.LongBinary};
            _parameters.Add(ext);
        }

        public void LongString(int i, string name)
        {
            Column(name, Name(i));
            var ext = new ExtType {Type = DbCommonType.LongChar};
            _parameters.Add(ext);
        }

        public void Xml(int i, string name)
        {
            Column(name, Name(i));
            var ext = new ExtType {Type = DbCommonType.Xml};
            _parameters.Add(ext);
        }

        public void Boolean(int i, string name)
        {
            Column(name, Name(i));
            var ext = new ExtType {Type = DbCommonType.Boolean};
            _parameters.Add(ext);
        }

        public void Guid(int i, string name)
        {
            Column(name, Name(i));
            var ext = new ExtType {Type = DbCommonType.Guid};
            _parameters.Add(ext);
        }

        void Bind()
        {
            DbCmd.Parameters.Clear();
            DbCmd.Prepare();

            for (var i = 0; i < _parameters.Count; i++)
            {
                var type = _parameters[i];

                DbParameter p;
                switch (type.Type)
                {
                    case DbCommonType.LongBinary:
                        p = LongBinary(i);
                        break;
                    case DbCommonType.LongChar:
                        p = LongChar(i);
                        break;
                    case DbCommonType.Xml:
                        p = Xml(i);
                        break;
                    case DbCommonType.Boolean:
                        p = Boolean(i);
                        break;
                    case DbCommonType.Guid:
                        p = Guid(i);
                        break;
                    default:
                        p = DbCmd.CreateParameter();
                        p.DbType = type.DBType;
                        break;
                }
                p.ParameterName = Name(i);
                p.Direction = ParameterDirection.Input;
                DbCmd.Parameters.Add(p);
            }

            if (Output != null)
            {
                DbCmd.Parameters.Add(Output);
            }
        }

        public string DumpParameters()
        {
            var sb = new StringBuilder();

            for (var i = 0; i < DbCmd.Parameters.Count; i++)
            {
                var type = _parameters[i];

                if (type.Type == DbCommonType.LongBinary)
                {
                    var p = DbCmd.Parameters[i];
                    var buf = p.Value as byte[];
                    sb.AppendFormat("{0}={1}", Name(i), buf == null ? "[NULL]" : "byte[" + buf.Length + "]");

                    if (i < DbCmd.Parameters.Count - 1)
                    {
                        sb.Append(", ");
                    }
                }
                else
                {
                    var p = DbCmd.Parameters[i];
                    sb.AppendFormat("{0}={1}", Name(i), p.Value);
                    if (i < DbCmd.Parameters.Count - 1)
                    {
                        sb.Append(", ");
                    }
                }
            }

            return sb.ToString();
        }

        public void Select(string select)
        {
            DbCmd.CommandText = select;
            Bind();
        }

        public void Set(int i, object value)
        {
            if (value == null)
                value = DBNull.Value;

            if (DbCmd.Parameters[i].DbType == DbType.Guid)
                DbCmd.Parameters[i].Value = new Guid(value.ToString());
            else
                DbCmd.Parameters[i].Value = value;
        }

        public int ExecuteNonQuery()
        {
            try
            {
                return DbCmd.ExecuteNonQuery();
            }
            catch
            {
                Log.ErrorFormat("{0} parameters {1}", DbCmd.CommandText, DumpParameters());
                throw;
            }
        }

        public string LogString
        {
            get
            {
                return string.Format("{0} parameters {1}", DbCmd.CommandText, DumpParameters());
            }
        }

        public DbDataReader ExecuteReader()
        {
            try
            {
                return DbCmd.ExecuteReader();
            }
            catch
            {
                Log.ErrorFormat("{0} parameters {1}", DbCmd.CommandText, DumpParameters());
                throw;
            }
        }
        public object ExecuteScalar()
        {
            try
            {
                return DbCmd.ExecuteScalar();
            }
            catch
            {
                Log.ErrorFormat("{0} parameters {1}", DbCmd.CommandText, DumpParameters());
                throw;
            }
        }

        public abstract string Name(int i);

        public abstract DbParameter LongBinary(int i);

        public abstract DbParameter LongChar(int i);

        public abstract DbParameter Xml(int i);

        public abstract DbParameter Boolean(int i);

        public abstract DbParameter Guid(int i);

        public abstract void Timestamp(string name);
    }

    class SqlStatement : DbStatement
    {
        public SqlStatement(DbCommand cmd)
            : base(cmd)
        {
        }

        public override string Name(int i)
        {
            return "@" + i;
        }

        public override DbParameter LongBinary(int i)
        {
            var p = (SqlParameter)DbCmd.CreateParameter();
            p.SqlDbType = SqlDbType.VarBinary;
            return p;
        }

        public override DbParameter LongChar(int i)
        {
            var p = (SqlParameter)DbCmd.CreateParameter();
            p.SqlDbType = SqlDbType.NText;
            return p;
        }

        public override void Timestamp(string name)
        {
            Column(name, "GETDATE()");
        }

        public override DbParameter Xml(int i)
        {
            var p = (SqlParameter)DbCmd.CreateParameter();
            p.SqlDbType = SqlDbType.Xml;
            return p;
        }

        public override DbParameter Boolean(int i)
        {
            var p = (SqlParameter)DbCmd.CreateParameter();
            p.SqlDbType = SqlDbType.Bit;
            return p;
        }

        public override DbParameter Guid(int i)
        {
            var p = (SqlParameter)DbCmd.CreateParameter();
            p.SqlDbType = SqlDbType.UniqueIdentifier;
            return p;
        }
    }

    class OracleStatement : DbStatement
    {
        public OracleStatement(DbCommand cmd)
            : base(cmd)
        {
        }

        public override string Name(int i)
        {
            return ":" + i;
        }

        public override DbParameter LongBinary(int i)
        {
            var p = (OracleParameter)DbCmd.CreateParameter();
            p.OracleDbType = OracleDbType.Blob;
            return p;
        }

        public override DbParameter LongChar(int i)
        {
            var p = (OracleParameter)DbCmd.CreateParameter();
            p.OracleDbType = OracleDbType.Clob;
            return p;
        }

        public override void Timestamp(string name)
        {
            Column(name, "systimestamp");
        }

        public override DbParameter Xml(int i)
        {
            var p = (OracleParameter)DbCmd.CreateParameter();
            p.OracleDbType = OracleDbType.XmlType;
            return p;
        }

        public override DbParameter Boolean(int i)
        {
            var p = (OracleParameter)DbCmd.CreateParameter();
            p.OracleDbType = OracleDbType.Int32;
            return p;
        }

        public override DbParameter Guid(int i)
        {
            var p = (OracleParameter)DbCmd.CreateParameter();
            p.OracleDbType = OracleDbType.Varchar2;
            return p;
        }
    }

    class SqlCeStatement : DbStatement
    {
        public SqlCeStatement(DbCommand cmd)
            : base(cmd)
        {
        }

        public override string Name(int i)
        {
            return "@" + i;
        }

        public override DbParameter LongBinary(int i)
        {
            var p = (SqlCeParameter)DbCmd.CreateParameter();
            p.SqlDbType = SqlDbType.Image;
            return p;
        }

        public override DbParameter LongChar(int i)
        {
            var p = (SqlCeParameter)DbCmd.CreateParameter();
            p.SqlDbType = SqlDbType.NText;
            return p;
        }

        public override void Timestamp(string name)
        {
            Column(name, "GETDATE()");
        }

        public override DbParameter Xml(int i)
        {
            var p = (SqlCeParameter)DbCmd.CreateParameter();
            p.SqlDbType = SqlDbType.Xml;
            return p;
        }

        public override DbParameter Boolean(int i)
        {
            var p = (SqlCeParameter)DbCmd.CreateParameter();
            p.SqlDbType = SqlDbType.Bit;
            return p;
        }

        public override DbParameter Guid(int i)
        {
            var p = (SqlCeParameter)DbCmd.CreateParameter();
            p.SqlDbType = SqlDbType.UniqueIdentifier;
            return p;
        }
    }
    
}
