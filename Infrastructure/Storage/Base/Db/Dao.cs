using System;
using System.Data.Common;
using System.Data;
using System.Linq;

namespace Runner.Base.Db
{
    public class Dao : IDisposable
    {
        private readonly static log4net.ILog Log = log4net.LogManager.GetLogger(typeof(Dao).Name);

        public string ConnectionString;
        public SqlType SqlType;

        public Dao(string connectionString, SqlType sqlType)
        {
            ConnectionString = connectionString;
            SqlType = sqlType;
        }

        public void Dispose()
        {
        }

        public DbCommon Create()
        {
            return DbCommon.Create(ConnectionString, SqlType);
        }

        public static string DefaultConnectionString = "";
        public static SqlType DefaultDbType = SqlType.MsSql;
        private static Dao _defaultDao;
        public static Dao Default
        {
            get
            {
                if(_defaultDao!=null) return _defaultDao;
                if (string.IsNullOrEmpty(DefaultConnectionString))
                {
                    throw new Exception("Dao.ConnectionString is empty.");
                }
                _defaultDao = new Dao(DefaultConnectionString, DefaultDbType);
                return _defaultDao;
            }
        }

        

        #region General db access
        public DbDataReader ExecuteReader(string sql)
        {
            using (var db = Create())
            {
                using (var cmd = db.Connection.CreateCommand())
                {
                    return DbUtil.ExecuteReader(sql, db, cmd);
                }
            }
        }

        public DbDataReader ExecuteReader(string sql, DbType[] types, object[] values)
        {
            using (var db = Create())
            {
                using (var cmd = db.Connection.CreateCommand())
                {
                    return DbUtil.ExecuteReader(sql, db, cmd, types, values);
                }
            }
        }

        public int Execute(string sql)
        {
            using (var db = Create())
            {
                using (var command = db.Connection.CreateCommand())
                {
                    return DbUtil.Execute(sql, db, command);
                }
            }
        }

        public int Execute(string sql, DbType[] types, params object[][] values)
        {
            using (var db = Create())
            {
                using (var command = db.Connection.CreateCommand())
                {
                    return DbUtil.Execute(sql, db, command, types, values);
                }
            }
        }

        public object ExecuteScalar(string sql)
        {
            using (var db = Create())
            {
                using (var cmd = db.Connection.CreateCommand())
                {
                    return DbUtil.ExecuteScalar(sql, db, cmd);
                }
            }
        }

        public T ExecuteScalarEx<T>(string sql, T defaultValue)
        {
            using (var db = Create())
            {
                using (var cmd = db.Connection.CreateCommand())
                {
                    return DbUtil.ExecuteScalarEx(sql, db, cmd, defaultValue);
                }
            }
        }

        public object ExecuteScalar(string sql, DbType[] types, object[] values)
        {
            using (var db = Create())
            {
                using (var cmd = db.Connection.CreateCommand())
                {
                    return DbUtil.ExecuteScalar(sql, db, cmd, types, values);
                }
            }
        }

        public T ExecuteScalarEx<T>(string sql, T defaultValue, DbType[] types, object[] values)
        {
            using (var db = Create())
            {
                using (var cmd = db.Connection.CreateCommand())
                {
                    return DbUtil.ExecuteScalarEx(sql, db, cmd, defaultValue, types, values);
                }
            }
        }

        public DataTable GetDataTable(string sql)
        {
            using (var db = Create())
            {
                using (var cmd = db.Connection.CreateCommand())
                {
                    return DbUtil.GetDataTable(sql, db, cmd);
                }
            }
        }

        public DataTable GetDataTable(string sql, DbType[] types, object[] values)
        {
            using (var db = Create())
            {
                using (var cmd = db.Connection.CreateCommand())
                {
                    return DbUtil.GetDataTable(sql, db, cmd, types, values);
                }
            }
        }

        public DataSet GetDataSet(string sql)
        {
            using (var db = Create())
            {
                using (var cmd = db.Connection.CreateCommand())
                {
                    return DbUtil.GetDataSet(sql, db, cmd);
                }
            }
        }

        public DataSet GetDataSet(string sql, DbType[] types, object[] values)
        {
            using (var db = Create())
            {
                using (var cmd = db.Connection.CreateCommand())
                {
                    return DbUtil.GetDataSet(sql, db, cmd, types, values);
                }
            }
        }

        public object[][] GetRows(string sql)
        {
            using (var db = Create())
            {
                using (var cmd = db.Connection.CreateCommand())
                {
                    return DbUtil.GetRows(sql, db, cmd);
                }
            }
        }

        public object[][] GetRows(string sql, DbType[] types, object[] values)
        {
            using (var db = Create())
            {
                using (var cmd = db.Connection.CreateCommand())
                {
                    return DbUtil.GetRows(sql, db, cmd, types, values);
                }
            }
        }

        public object[][] GetRowsEx(string sql)
        {
            using (var db = Create())
            {
                using (var cmd = db.Connection.CreateCommand())
                {
                    return DbUtil.GetRowsEx(sql, db, cmd);
                }
            }
        }

        public object[][] GetRowsEx(string sql, DbType[] types, object[] values)
        {
            using (var db = Create())
            {
                using (var cmd = db.Connection.CreateCommand())
                {
                    return DbUtil.GetRowsEx(sql, db, cmd, types, values);
                }
            }
        }

        public object[] GetRowsEx(string sql, int colPos)
        {
            using (var db = Create())
            {
                using (var cmd = db.Connection.CreateCommand())
                {
                    var rows = DbUtil.GetRowsEx(sql, db, cmd);
                    return rows.Select(row => row[colPos]).ToArray();
                }
            }
        }

        public object[] GetRowsEx(string sql, int colPos, DbType[] types, object[] values)
        {
            using (var db = Create())
            {
                using (var cmd = db.Connection.CreateCommand())
                {
                    var rows = DbUtil.GetRowsEx(sql, db, cmd, types, values);
                    return rows.Select(row => row[colPos]).ToArray();
                }
            }
        }

        public object[] GetRow(string sql)
        {
            using (var db = Create())
            {
                using (var cmd = db.Connection.CreateCommand())
                {
                    return DbUtil.GetRow(sql, db, cmd);
                }
            }
        }

        public object[] GetRow(string sql, DbType[] types, object[] values)
        {
            using (var db = Create())
            {
                using (var cmd = db.Connection.CreateCommand())
                {
                    return DbUtil.GetRow(sql, db, cmd, types, values);
                }
            }
        }

        public object[] GetRowEx(string sql)
        {
            using (var db = Create())
            {
                using (var cmd = db.Connection.CreateCommand())
                {
                    return DbUtil.GetRowEx(sql, db, cmd);
                }
            }
        }

        public object[] GetRowEx(string sql, DbType[] types, object[] values)
        {
            using (var db = Create())
            {
                using (var cmd = db.Connection.CreateCommand())
                {
                    return DbUtil.GetRowEx(sql, db, cmd, types, values);
                }
            }
        }

        public string[][] GetRows2StringArray(string sql)
        {
            var ds = GetDataSet(sql);
            return DbUtil.ConvertDataSet2StringArray(ds);
        }

        public string[][] GetRows2StringArray(string sql, DbType[] types, object[] values)
        {
            DataSet ds = GetDataSet(sql, types, values);
            return DbUtil.ConvertDataSet2StringArray(ds);
        }

        public string[][] GetRows2StringArrayEx(string sql)
        {
            var rows = GetRowsEx(sql);
            return DbUtil.ConvertRows2StringArray(rows);
        }

        public string[][] GetRows2StringArrayEx(string sql, DbType[] types, object[] values)
        {
            var rows = GetRowsEx(sql, types, values);
            return DbUtil.ConvertRows2StringArray(rows);
        }

        public string[] GetRows2StringArrayEx(string sql, int cloPos)
        {
            var rows = GetRowsEx(sql);
            var strRows = DbUtil.ConvertRows2StringArray(rows);
            return strRows.Select(strRow => strRow[cloPos]).ToArray();
        }

        public string[] GetRows2StringArrayEx(string sql, int cloPos, DbType[] types, object[] values)
        {
            var rows = GetRowsEx(sql, types, values);
            var strRows = DbUtil.ConvertRows2StringArray(rows);
            return strRows.Select(strRow => strRow[cloPos]).ToArray();
        }

        public string[] GetStringRowEx(string sql)
        {
            var row = GetRow(sql);
            return DbUtil.ConvertRow2StringArray(row);
        }

        public string[] GetStringRowEx(string sql, DbType[] types, object[] values)
        {
            var row = GetRow(sql, types, values);
            return DbUtil.ConvertRow2StringArray(row);
        }

        public string[] GetStringRow(string sql)
        {
            var dataset = GetDataSet(sql);
            var stringArrays = DbUtil.ConvertDataSet2StringArray(dataset);
            if (stringArrays != null && stringArrays.Length > 0) return stringArrays[0];
            return null;
        }
        #endregion

        #region extend db access
        public bool IsTableExist(string tableName)
        {
            string tbl;
            switch (SqlType)
            {
                case SqlType.MsSql:
                case SqlType.MsSqlCe:
                    tbl = ExecuteScalarEx(string.Format("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{0}'", tableName), "");
                    break;
                case SqlType.Oracle:
                    tbl = ExecuteScalarEx(string.Format("SELECT TABLE_NAME FROM USER_TABLES WHERE TABLE_NAME = '{0}'", tableName.ToUpper()), "");
                    break;
                default:
                    throw new Exception("Not implement yet.");
            }
            return !string.IsNullOrEmpty(tbl);
        }

        public bool IsViewExist(string viewName)
        {
            string view;
            switch (SqlType)
            {
                case SqlType.MsSql:
                case SqlType.MsSqlCe:
                    {
                        view = ExecuteScalarEx(string.Format("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.VIEWS WHERE TABLE_NAME = '{0}'", viewName), "");
                        break;
                    }
                case SqlType.Oracle:
                    {
                        view = ExecuteScalarEx(string.Format("SELECT VIEW_NAME FROM USER_VIEWS WHERE VIEW_NAME = '{0}'", viewName.ToUpper()), "");
                        break;
                    }
                default:
                    throw new Exception("Not implement yet.");
            }
            return !string.IsNullOrEmpty(view);
        }


        public void GatherStats(string[] tables)
        {
            switch (SqlType)
            {
                case SqlType.MsSql:
                case SqlType.MsSqlCe:
                    {
                        foreach (var table in tables)
                        {
                            try
                            {
                                Execute(string.Format("UPDATE STATISTICS {0} WITH SAMPLE", table));
                            }
                            catch(Exception ex)
                            {
                                Log.Error("GatherStats(" + table + ") failed: ", ex);
                            }
                        }
                        break;
                    }
                case SqlType.Oracle:
                    {
                        foreach(var table in tables)
                        {
                            try
                            {
                                Execute(string.Format("begin DBMS_STATS.GATHER_TABLE_STATS (ownname => sys_context('USERENV','CURRENT_SCHEMA') , tabname => '{0}'); end;", table.ToUpper()));
                            }
                            catch(Exception ex)
                            {
                                Log.Error("GatherStats" + table + " failed: ", ex);
                            }
                        }
                        break;
                    }
                default:
                    throw new Exception("Not implement yet.");
            }
        }

        public string[] GetPKColumns(string tableName)
        {
            switch (SqlType)
            {
                case SqlType.MsSql:
                case SqlType.MsSqlCe:
                    {
                        string table = tableName;
                        return GetRows2StringArrayEx(string.Format(@"select col.column_name
from INFORMATION_SCHEMA.columns col, INFORMATION_SCHEMA.table_constraints cons, INFORMATION_SCHEMA.tables tbl, INFORMATION_SCHEMA.key_column_usage usage
where col.table_name=tbl.table_name and tbl.table_name=cons.table_name and usage.table_name=tbl.table_name and 
usage.column_name=col.column_name and usage.constraint_name=cons.constraint_name and 
tbl.table_name='{0}' and cons.constraint_type='PRIMARY KEY'", table), 0);
                    }
                case SqlType.Oracle:
                    {
                        throw new Exception("Not implement yet.");
                    }
            }
            return null;
        }

        public string[] GetFKColumns(string tableName)
        {
            switch (SqlType)
            {
                case SqlType.MsSql:
                case SqlType.MsSqlCe:
                    {
                        string table = tableName;
                        return GetRows2StringArrayEx(string.Format(@"select col.column_name
from INFORMATION_SCHEMA.columns col, INFORMATION_SCHEMA.table_constraints cons, INFORMATION_SCHEMA.tables tbl, INFORMATION_SCHEMA.key_column_usage usage
where col.table_name=tbl.table_name and tbl.table_name=cons.table_name and usage.table_name=tbl.table_name and 
usage.column_name=col.column_name and usage.constraint_name=cons.constraint_name and 
tbl.table_name='{0}' and cons.constraint_type='FOREIGN KEY'", table), 0);
                    }
                case SqlType.Oracle:
                    {
                        throw new Exception("Not implement yet.");
                    }
            }
            return null;
        }

        public DataTable GetColumnsSchema(string tableName)
        {
            var dataTbl = new DataTable(tableName);
            switch (SqlType)
            {
                #region MsSql and MsSqlCe
                case SqlType.MsSql:
                case SqlType.MsSqlCe:
                    {
                        string table = tableName;
                        string[][] rowDefs = GetRows2StringArrayEx(string.Format("SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE, CHARACTER_MAXIMUM_LENGTH, NUMERIC_PRECISION FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{0}' ORDER BY ORDINAL_POSITION ASC", table));
                        if (rowDefs != null && rowDefs.Length > 0)
                        {
                            foreach (var rowDef in rowDefs)
                            {
                                var columnName = rowDef[0].ToUpper(); //use big case for columnName
                                if (columnName.StartsWith("__SYS"))
                                {
                                    Log.InfoFormat("Ignore column {0} in table {1}.", columnName, table);
                                    continue;
                                }
                                var dataType = rowDef[1];
                                var nullable = rowDef[2] == "YES";
                                var nullableMark = !nullable ? " NOT NULL" : "";
                                Type type = null;
                                string expression = null;
                                #region type, expression assignment
                                if (String.Compare(dataType, "uniqueidentifier", StringComparison.OrdinalIgnoreCase) == 0) 
                                {
                                    type = typeof(Guid);
                                    //int length = Int32.Parse(rowDef[3]);
                                    expression = string.Format("{0} UNIQUEIDENTIFIER{1}", columnName, nullableMark);
                                    //dataTbl.Columns.Add(columnName, typeof(string), string.Format("{0} UNIQUEIDENTIFIER{1}", columnName, nullableMark));
                                }
                                else if (String.Compare(dataType, "char", StringComparison.OrdinalIgnoreCase) == 0)
                                {
                                    type = typeof(string);
                                    var length = Int32.Parse(rowDef[3]);
                                    expression = string.Format("{0} char({1}){2}", columnName, length, nullableMark);
                                }
                                else if (String.Compare(dataType, "varchar", StringComparison.OrdinalIgnoreCase) == 0)
                                {
                                    type = typeof(string);
                                    var length = Int32.Parse(rowDef[3]);
                                    expression = length < 1 ? string.Format("{0} VARCHAR(max){1}", columnName, nullableMark) : string.Format("{0} VARCHAR({1}){2}", columnName, length, nullableMark);
                                }
                                else if (String.Compare(dataType, "nvarchar", StringComparison.OrdinalIgnoreCase) == 0)
                                {
                                    type = typeof(string);
                                    var length = Int32.Parse(rowDef[3]);
                                    expression = length < 1 ? string.Format("{0} NVARCHAR(max){1}", columnName, nullableMark) : string.Format("{0} NVARCHAR({1}){2}", columnName, length, nullableMark);
                                }
                                else if (String.Compare(dataType, "int", StringComparison.OrdinalIgnoreCase) == 0)
                                {
                                    type = typeof(Int32);
                                    expression = string.Format("{0} INT{1}", columnName, nullableMark);
                                }
                                else if (String.Compare(dataType, "bigint", StringComparison.OrdinalIgnoreCase) == 0)
                                {
                                    type = typeof(Int64);
                                    expression = string.Format("{0} BIGINT{1}", columnName, nullableMark);
                                }
                                else if (String.Compare(dataType, "smallint", StringComparison.OrdinalIgnoreCase) == 0)
                                {
                                    type = typeof(Int16);
                                    expression = string.Format("{0} SMALLINT{1}", columnName, nullableMark);
                                }
                                else if (String.Compare(dataType, "TINYINT", StringComparison.OrdinalIgnoreCase) == 0)
                                {
                                    type = typeof(Byte);
                                    expression = string.Format("{0} TINYINT{1}", columnName, nullableMark);
                                }
                                else if (String.Compare(dataType, "float", StringComparison.OrdinalIgnoreCase) == 0)
                                {
                                    type = typeof(float);
                                    expression = string.Format("{0} FLOAT{1}", columnName, nullableMark);
                                }
                                else if (String.Compare(dataType, "numeric", StringComparison.OrdinalIgnoreCase) == 0)
                                {
                                    type = typeof(Int64);
                                    int length = Int32.Parse(rowDef[4]);
                                    expression = string.Format("{0} NUMERIC({1}) {2}", columnName, length, nullableMark);
                                }
                                else if (String.Compare(dataType, "datetime", StringComparison.OrdinalIgnoreCase) == 0)
                                {
                                    type = typeof(DateTime);
                                    expression = string.Format("{0} DATETIME{1}", columnName, nullableMark);
                                }
                                else if (String.Compare(dataType, "binary", StringComparison.OrdinalIgnoreCase) == 0)
                                {
                                    type = typeof(byte[]);
                                    expression = string.Format("{0} BINARY(MAX){1}", columnName, nullableMark);
                                }
                                else if (String.Compare(dataType, "varbinary", StringComparison.OrdinalIgnoreCase) == 0)
                                {
                                    type = typeof(byte[]);
                                    expression = string.Format("{0} VARBINARY(MAX){1}", columnName, nullableMark);
                                }
                                else if (String.Compare(dataType, "image", StringComparison.OrdinalIgnoreCase) == 0)
                                {
                                    type = typeof(byte[]);
                                    expression = string.Format("{0} IMAGE{1}", columnName, nullableMark);
                                }
                                else if (String.Compare(dataType, "xml", StringComparison.OrdinalIgnoreCase) == 0)
                                {
                                    type = typeof(string);
                                    expression = string.Format("{0} XML{1}", columnName, nullableMark);
                                }
                                else if (String.Compare(dataType, "bit", StringComparison.OrdinalIgnoreCase) == 0)
                                {
                                    type = typeof(bool);
                                    expression = string.Format("{0} BIT{1}", columnName, nullableMark);
                                }
                                else if (String.Compare(dataType, "text", StringComparison.OrdinalIgnoreCase) == 0)
                                {
                                    type = typeof(string);
                                    expression = string.Format("{0} text{1}", columnName, nullableMark);
                                }
                                else if (String.Compare(dataType, "ntext", StringComparison.OrdinalIgnoreCase) == 0)
                                {
                                    type = typeof(string);
                                    expression = string.Format("{0} ntext{1}", columnName, nullableMark);
                                }
                                else if (String.Compare(dataType, "decimal", StringComparison.OrdinalIgnoreCase) == 0)
                                {
                                    type = typeof(decimal);
                                    int length = Int32.Parse(rowDef[4]);
                                    expression = string.Format("{0} decimal({1}){2}", columnName, length, nullableMark);
                                }
                                else if (String.Compare(dataType, "geography", StringComparison.OrdinalIgnoreCase) == 0)
                                {
                                    //...
                                }
                                else
                                {
                                    Log.WarnFormat("Not recognized dbType '{0}' to .NET dataType.", dataType);
                                }
                                #endregion
                                if (type != null)
                                {
                                    //constraint checking
                                    //string constraintName = ExecuteScalarEx<string>(string.Format("SELECT CONSTRAINT_NAME FROM INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE WHERE TABLE_NAME='{0}' AND COLUMN_NAME='{1}'", table, columnName), "");
                                    //if(!string.IsNullOrEmpty(constraintName))
                                    {
                                        //TODO primary key, foreign key, reference key checking
                                    }
                                    DataColumn col = dataTbl.Columns.Add(columnName, type);
                                    col.Caption = expression;
                                    col.AllowDBNull = nullable;
                                }
                                else
                                {
                                    //ignore for now.
                                    //dataTbl.Columns.Add(columnName);
                                    Log.WarnFormat("Column '{0}' in table '{1}' is ignored.", columnName, table);
                                }
                            }
                        }
                        break;
                    }
                #endregion
                #region Oracle
                case SqlType.Oracle:
                    {
                        string table = tableName.ToUpper();
                        //Note: Here we use ALL_TAB_COLUMNS not USER_TAB_COLUMNS, cause ALL_TAB_COLUMNS also contains columns for VIEW.
                        string[][] rowDefs = GetRows2StringArray(string.Format("SELECT COLUMN_NAME, DATA_TYPE, NULLABLE, DATA_LENGTH, DATA_PRECISION, DATA_SCALE FROM ALL_TAB_COLUMNS WHERE TABLE_NAME = '{0}' ORDER BY COLUMN_ID ASC", table));
                        if (rowDefs != null && rowDefs.Length > 0)
                        {
                            foreach (string[] rowDef in rowDefs)
                            {
                                string columnName = rowDef[0].ToUpper(); //use big case for columnName
                                string dataType = rowDef[1];
                                bool nullable = rowDef[2] == "Y";
                                string nullableMark = !nullable ? " NOT NULL" : "";
                                Type type = null;
                                string expression = null;
                                #region type, expression assignment
                                if (string.Compare(dataType, "CHAR", StringComparison.OrdinalIgnoreCase) == 0)
                                {
                                    type = typeof(string);
                                    int length = Int32.Parse(rowDef[3]);
                                    expression = string.Format("{0} CHAR({1}){2}", columnName, length, nullableMark);
                                }
                                else if (string.Compare(dataType, "VARCHAR", StringComparison.OrdinalIgnoreCase) == 0)
                                {
                                    type = typeof(string);
                                    int length = Int32.Parse(rowDef[3]);
                                    expression = string.Format("{0} VARCHAR({1}){2}", columnName, length, nullableMark);
                                }
                                else if (string.Compare(dataType, "VARCHAR2", StringComparison.OrdinalIgnoreCase) == 0)
                                {
                                    type = typeof(string);
                                    int length = Int32.Parse(rowDef[3]);
                                    expression = string.Format("{0} VARCHAR2({1}){2}", columnName, length, nullableMark);
                                }
                                else if (string.Compare(dataType, "NVARCHAR2", StringComparison.OrdinalIgnoreCase) == 0)
                                {
                                    type = typeof(string);
                                    int length = Int32.Parse(rowDef[3]);
                                    expression = string.Format("{0} NVARCHAR2({1}){2}", columnName, length, nullableMark);
                                }
                                else if (string.Compare(dataType, "XMLTYPE", StringComparison.OrdinalIgnoreCase) == 0)
                                {
                                    type = typeof(string);
                                    expression = string.Format("{0} XMLTYPE{1}", columnName, nullableMark);
                                }
                                else if (string.Compare(dataType, "FLOAT", StringComparison.OrdinalIgnoreCase) == 0)
                                {
                                    type = typeof(float);
                                    expression = string.Format("{0} FLOAT{1}", columnName, nullableMark);
                                }
                                else if (string.Compare(dataType, "LONG", StringComparison.OrdinalIgnoreCase) == 0)
                                {
                                    type = typeof(float);
                                    expression = string.Format("{0} LONG{1}", columnName, nullableMark);
                                }
                                else if (string.Compare(dataType, "TIMESTAMP(3)", StringComparison.OrdinalIgnoreCase) == 0)
                                {
                                    type = typeof(DateTime);
                                    expression = string.Format("{0} TIMESTAMP(3){1}", columnName, nullableMark);
                                }
                                else if (string.Compare(dataType, "DATE", StringComparison.OrdinalIgnoreCase) == 0)
                                {
                                    type = typeof(DateTime);
                                    expression = string.Format("{0} DATE{1}", columnName, nullableMark);
                                }
                                else if (string.Compare(dataType, "TIMESTAMP(6)", StringComparison.OrdinalIgnoreCase) == 0)
                                {
                                    type = typeof(DateTime);
                                    expression = string.Format("{0} TIMESTAMP(6){1}", columnName, nullableMark);
                                }
                                else if (string.Compare(dataType, "BLOB", StringComparison.OrdinalIgnoreCase) == 0)
                                {
                                    type = typeof(byte[]);
                                    expression = string.Format("{0} BLOB{1}", columnName, nullableMark);
                                }
                                else if (string.Compare(dataType, "CLOB", StringComparison.OrdinalIgnoreCase) == 0)
                                {
                                    type = typeof(string);
                                    expression = string.Format("{0} CLOB{1}", columnName, nullableMark);
                                }
                                else if (string.Compare(dataType, "NUMBER", StringComparison.OrdinalIgnoreCase) == 0)
                                {
                                    //.......
                                    var precision = rowDef[4];
                                    var scale = rowDef[5];
                                    if (string.IsNullOrEmpty(precision)) precision = "*";
                                    if (string.IsNullOrEmpty(scale)) scale = "*";
                                    if (precision == "*")
                                    {
                                        if (scale == "*")
                                        {
                                            type = typeof(long);
                                            expression = string.Format("{0} NUMBER{1}", columnName, nullableMark);
                                        }
                                        else
                                        {
                                            type = typeof(long);
                                            expression = string.Format("{0} NUMBER(*,{2}){1}", columnName, nullableMark, scale);
                                        }
                                    }
                                    else
                                    {
                                        if (scale != "*")
                                        {
                                            type = typeof(long);
                                            expression = string.Format("{0} NUMBER({2},{3}){1}", columnName, nullableMark, precision, scale);
                                        }
                                        else
                                        {
                                            type = typeof(long);
                                            expression = string.Format("{0} NUMBER{1}", columnName, nullableMark);
                                        }
                                    }
                                }
                                else if (string.Compare(dataType, "SDO_GEOMETRY", StringComparison.OrdinalIgnoreCase) == 0)
                                {
                                    //type = typeof(byte[]);
                                    //expression = string.Format("{0} SDO_GEOMETRY{1}", columnName, nullableMark);
                                    //...
                                }
                                else if (string.Compare(dataType, "BINARY_DOUBLE", StringComparison.OrdinalIgnoreCase) == 0)
                                {
                                    //...
                                    //type = typeof(byte[]);
                                    //expression = string.Format("{0} BINARY_DOUBLE{1}", columnName, nullableMark);
                                }                           
                                #endregion
                                if (type != null)
                                {
                                    DataColumn col = dataTbl.Columns.Add(columnName, type);
                                    col.Caption = expression;
                                    col.AllowDBNull = nullable;
                                }
                                else
                                {
                                    //ignore for now.
                                    //dataTbl.Columns.Add(columnName);
                                    Log.WarnFormat("Column '{0}' in table '{1}' is ignored.", columnName, table);
                                }
                            }
                        }
                        break;
                    }
                #endregion
                default:
                    throw new Exception("Not implement yet.");
            }
            return dataTbl;
        }
        #endregion

        public static DataSet UnifyDataSet(DataSet ds)
        {
            if (ds.Tables.Count > 0)
            {
                foreach (DataTable table in ds.Tables)
                {
                    if (table.Columns.Count > 0)
                    {
                        for(int i=0;i<table.Columns.Count;i++)
                        {
                            table.Columns[i].ColumnName = table.Columns[i].ColumnName.ToUpper();
                        }
                    }
                }
            }
            return ds;
        }
    }

}
