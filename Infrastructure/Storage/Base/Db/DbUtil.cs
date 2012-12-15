using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using System.Data;
using System.Data.Common;

namespace Runner.Base.Db
{
    
    public static class DbUtil
    {
        private readonly static ILog Log = LogManager.GetLogger(typeof(DbUtil));

        public static int Execute(string sql, DbCommon db, DbCommand cmd)
        {
            if (Log.IsDebugEnabled)
            {
                Log.Debug(sql);
            }
            try
            {
                cmd.CommandText = sql;
                return cmd.ExecuteNonQuery();
            }
            catch
            {
                Log.Warn("SQL: " + sql);
                throw;
            }
        }

        public static int Execute(string sql, DbCommon db, DbCommand cmd, DbType[] types, params object[][] values)
        {
            if (Log.IsDebugEnabled)
            {
                Log.Debug(sql);
            }
            try
            {
                var stm = db.CreateParameters(cmd);
                stm.Select(string.Format(sql, types.Select((t, i) => stm.WhereParameter(i, t)).Cast<object>().ToArray()));
                var ret = 0;
                foreach(var objs in values)
                {
                    for (var i = 0; i < types.Length; i++)
                    {
                        stm.Set(i, objs[i]);
                    }
                    ret += cmd.ExecuteNonQuery();
                }
                return ret;
            }
            catch
            {
                Log.Warn("SQL: " + sql);
                throw;
            }
        }


        private static void HookDbParameters(ref DbCommon db, ref DbCommand cmd, string queryFormat, DbType[] types, object[] values)
        {
            var stm = db.CreateParameters(cmd);
            stm.Select(string.Format(queryFormat, types.Select((t, i) => stm.WhereParameter(i, t)).Cast<object>().ToArray()));
            for (var i = 0; i < types.Length; i++)
            {
                stm.Set(i, values[i]);
            }
        }

        public static DbDataReader ExecuteReader(string sql, DbCommon db, DbCommand cmd)
        {
            if (Log.IsDebugEnabled)
            {
                Log.Debug(sql);
            }
            try
            {
                cmd.CommandText = sql;
                return cmd.ExecuteReader();
            }
            catch
            {
                Log.Warn("SQL: " + sql);
                throw;
            }
        }

        public static DbDataReader ExecuteReader(string sql, DbCommon db, DbCommand cmd, DbType[] types, object[] values)
        {
            if (Log.IsDebugEnabled)
            {
                Log.Debug(sql);
            }
            try
            {
                HookDbParameters(ref db, ref cmd, sql, types, values);
                return cmd.ExecuteReader();
            }
            catch
            {
                Log.Warn("SQL: " + sql);
                throw;
            }
        }

        public static object ExecuteScalar(string sql, DbCommon db, DbCommand cmd)
        {
            if (Log.IsDebugEnabled)
            {
                Log.Debug(sql);
            }
            try
            {
                cmd.CommandText = sql;
                object ret = cmd.ExecuteScalar();
                if (ret != null && !Convert.IsDBNull(ret))
                {
                    return ret;
                }
                return null;
            }
            catch
            {
                Log.Warn("SQL: " + sql);
                throw;
            }
        }

        public static T ExecuteScalarEx<T>(string sql, DbCommon db, DbCommand cmd, T defaultValue)
        {
            object obj = ExecuteScalar(sql, db, cmd);
            if (obj != null)
            {
                var ret = (T) obj;
                return ret;
            }
            return defaultValue;
        }

        public static object ExecuteScalar(string sql, DbCommon db, DbCommand cmd, DbType[] types, object[] values)
        {
            if (Log.IsDebugEnabled)
            {
                Log.Debug(sql);
            }
            try
            {
                HookDbParameters(ref db, ref cmd, sql, types, values);
                return cmd.ExecuteScalar();
            }
            catch
            {
                Log.Warn("SQL: " + sql);
                throw;
            }
        }

        public static T ExecuteScalarEx<T>(string sql, DbCommon db, DbCommand cmd, T defaultValue, DbType[] types, object[] values)
        {
            object obj = ExecuteScalar(sql, db, cmd, types, values);
            if (obj != null)
            {
                var ret = (T)obj;
                return ret;
            }
            return defaultValue;
        }

        public static DataTable GetDataTable(string sql, DbCommon db, DbCommand command)
        {
            try
            {
                command.CommandText = sql;
                using(var rd = command.ExecuteReader())
                {
                    var dt = new DataTable();
                    dt.Load(rd);
                    return dt;
                }
            }
            catch (Exception ex)
            {
                Log.Warn("SQL: " + sql + " exception: " + ex);
                throw;
            }
        }

        public static DataSet GetDataSet(string sql, DbCommon db, DbCommand command)
        {
            var ds = new DataSet();
            ds.Tables.Add(GetDataTable(sql, db, command));
            return ds;
        }

        public static DataTable GetDataTable(string sql, DbCommon db, DbCommand command, DbType[] types, object[] values)
        {
            try
            {
                HookDbParameters(ref db, ref command, sql, types, values);
                DataSet ds;
                using (var rd = command.ExecuteReader())
                {
                    var dt = new DataTable();
                    dt.Load(rd);
                    return dt;
                }
            }
            catch
            {
                Log.Warn("SQL: " + sql);
                throw;
            }
        }

        public static DataSet GetDataSet(string sql, DbCommon db, DbCommand command, DbType[] types, object[] values)
        {
            var ds = new DataSet();
            ds.Tables.Add(GetDataTable(sql, db, command, types, values));
            return ds;
        }

        public static object[][] GetRowsEx(string sql, DbCommon db, DbCommand command, DbType[] types, object[] values)
        {
            if (Log.IsDebugEnabled)
            {
                Log.Debug(sql);
            }
            var ret = new List<object[]>();
            try
            {
                HookDbParameters(ref db, ref command, sql, types, values);
                using (var rd = command.ExecuteReader())
                {
                    var length = rd.FieldCount;
                    while (rd.Read())
                    {
                        var row = new object[length];
                        for (var i = 0; i < length; i++)
                        {
                            row[i] = rd.IsDBNull(i) ? null : rd.GetValue(i);
                        }
                        ret.Add(row);
                    }
                }
                return ret.ToArray();
            }
            catch
            {
                Log.Warn("SQL: " + sql);
                throw;
            }
        }

        public static object[][] GetRowsEx(string sql, DbCommon db, DbCommand command)
        {
            return GetRowsSpecific(sql, db, command, false);
        }

        public static object[][] GetRowsSpecific(string sql, DbCommon db, DbCommand command, bool noDateTime)
        {
            if (Log.IsDebugEnabled)
            {
                Log.Debug(sql);
            }
            var ret = new List<object[]>();
            try
            {
                command.CommandText = sql;
                using (var rd = command.ExecuteReader())
                {
                    var length = rd.FieldCount;
                    while (rd.Read())
                    {
                        var row = new object[length];
                        for (var i = 0; i < length; i++)
                        {
                            row[i] = rd.IsDBNull(i) ? null : rd.GetValue(i);
                            if (noDateTime && row[i] is DateTime) row[i] = ((DateTime)row[i]).ToOADate();
                        }
                        ret.Add(row);
                    }
                }
                return ret.ToArray();
            }
            catch
            {
                Log.Warn("SQL: " + sql);
                throw;
            }
        }

        public static object[] GetRowEx(string sql, DbCommon db, DbCommand command, DbType[] types, object[] values)
        {
            try
            {
                HookDbParameters(ref db, ref command, sql, types, values);
                using (var rd = command.ExecuteReader())
                {
                    var length = rd.FieldCount;
                    if (rd.Read())
                    {
                        var row = new object[length];
                        for (var i = 0; i < length; i++)
                        {
                            row[i] = rd.IsDBNull(i) ? null : rd.GetValue(i);
                        }
                        return row;
                    }                    
                }
            }
            catch
            {
                Log.Warn("SQL: " + sql);
                throw;
            }
            return null;
        }

        public static object[] GetRowEx(string sql, DbCommon db, DbCommand command)
        {
            return GetRowSpecific(sql, db, command, false);
        }

        /// <summary>
        /// Executes the query, and returns the results collection
        /// </summary>
        /// <returns></returns>
        public static object[] GetRowSpecific(string sql, DbCommon db, DbCommand command, bool noDateTime)
        {
            try
            {
                command.CommandText = sql;
                using (var rd = command.ExecuteReader())
                {
                    var length = rd.FieldCount;
                    if (rd.Read())
                    {
                        var row = new object[length];
                        for (var i = 0; i < length; i++)
                        {
                            row[i] = rd.IsDBNull(i) ? null : rd.GetValue(i);
                            if (noDateTime && row[i] is DateTime) row[i] = ((DateTime)row[i]).ToOADate();
                        }
                        return row;
                    }
                }
            }
            catch
            {
                Log.Warn("SQL: " + sql);
                throw;
            }
            return null;
        }

        public static object[][] GetRows(string sql, DbCommon db, DbCommand command)
        {
            var dataset = GetDataSet(sql, db, command);
            return ConvertDataSet2Rows(dataset);
        }

        public static object[][] GetRows(string sql, DbCommon db, DbCommand command, DbType[] types, object[] values)
        {
            var dataset = GetDataSet(sql, db, command, types, values);
            return ConvertDataSet2Rows(dataset);
        }

        public static object[] GetRow(string sql, DbCommon db, DbCommand command)
        {
            var dataset = GetDataSet(sql, db, command);
            return ConvertDataSet2Row(dataset);
        }

        public static object[] GetRow(string sql, DbCommon db, DbCommand command, DbType[] types, object[] values)
        {
            var dataset = GetDataSet(sql, db, command, types, values);
            return ConvertDataSet2Row(dataset);
        }

        public static object[][] ConvertDataSet2Rows(DataSet dataset)
        {
            var datas = new List<object[]>();
            foreach (DataTable datatable in dataset.Tables)
            {
                foreach (DataRow row in datatable.Rows)
                {
                    var rowLine = new List<object>();
                    var values = row.ItemArray;
                    if (values.Length > 0)
                    {
                        foreach (var val in values)
                        {
                            if (val == null || Convert.IsDBNull(val))
                            {
                                rowLine.Add(null);
                            }
                            else
                            {
                                rowLine.Add(val);
                            }
                        }
                    }
                    datas.Add(rowLine.ToArray());
                }
            }
            return datas.ToArray();
        }

        public static string[][] ConvertDataSet2StringArray(DataSet dataset)
        {
            var datas = new List<string[]>();
            foreach (DataTable datatable in dataset.Tables)
            {
                foreach (DataRow row in datatable.Rows)
                {
                    var rowLine = new List<string>();
                    var values = row.ItemArray;
                    if (values.Length > 0)
                    {
                        foreach (var val in values)
                        {
                            if (val == null || Convert.IsDBNull(val))
                            {
                                rowLine.Add(string.Empty);
                            }
                            else
                            {
                                rowLine.Add(val.ToString());
                            }
                        }
                    }
                    datas.Add(rowLine.ToArray());
                }
            }
            return datas.ToArray();
        }

        public static string[][] ConvertRows2StringArray(object[][] rows)
        {
            var datas = new List<string[]>();
            foreach (var values in rows)
            {
                var rowLine = new List<string>();
                if (values != null && values.Length > 0)
                {
                    foreach (var val in values)
                    {
                        if (val == null || Convert.IsDBNull(val))
                        {
                            rowLine.Add(string.Empty);
                        }
                        else
                        {
                            rowLine.Add(val.ToString());
                        }
                    }
                }
                datas.Add(rowLine.ToArray());
            }
            return datas.ToArray();
        }

        public static string[] ConvertRow2StringArray(object[] row)
        {
            var rowLine = new List<string>();
            if (row != null && row.Length > 0)
            {
                foreach (var val in row)
                {
                    if (val == null || Convert.IsDBNull(val))
                    {
                        rowLine.Add(string.Empty);
                    }
                    else
                    {
                        rowLine.Add(val.ToString());
                    }
                }
            }
            return rowLine.ToArray();
        }

        public static object[] ConvertDataSet2Row(DataSet dataset)
        {
            foreach (DataTable datatable in dataset.Tables)
            {
                foreach (DataRow row in datatable.Rows)
                {
                    var rowLine = new List<object>();
                    var values = row.ItemArray;
                    if (values.Length > 0)
                    {
                        foreach (var val in values)
                        {
                            if (val == null || Convert.IsDBNull(val))
                            {
                                rowLine.Add(null);
                            }
                            else
                            {
                                rowLine.Add(val);
                            }
                        }
                    }
                    return rowLine.ToArray();
                }
                break;
            }
            return null;
        }

        public static DbType ConvertDataType(Type type)
        {
            if (type == typeof(string)) return DbType.String;
            if (type == typeof(short)) return DbType.Int16;
            if (type == typeof(int)) return DbType.Int32;
            if (type == typeof(long)) return DbType.Int64;
            if(type==typeof(byte[])) return DbType.Binary;
            if (type == typeof(byte)) return DbType.Byte;
            if (type == typeof(ushort)) return DbType.UInt16;
            if (type == typeof(uint)) return DbType.UInt32;
            if (type == typeof(ulong)) return DbType.UInt64;
            if (type == typeof(float)) return DbType.Single;
            if (type == typeof(double)) return DbType.Double;
            if (type == typeof(decimal)) return DbType.Decimal;
            if (type == typeof(bool)) return DbType.Boolean;
            if (type == typeof(DateTime)) return DbType.DateTime;
            if (type == typeof(Guid)) return DbType.Guid;
            throw new Exception("Can not convert type '" + type.FullName + "' to DbType");
        }
    }

}
