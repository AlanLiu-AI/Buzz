using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace Runner.Base.Util
{
    /// <summary>
    /// Converter delegate
    /// </summary>
    /// <typeparam name="TType"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="val"></param>
    /// <returns></returns>
    public delegate TValueTo ConverterDelegate<TTypeFrom, TValueTo>(TTypeFrom val)
        where TTypeFrom : class
        where TValueTo : class
    ;

    /// <summary>
    /// From string converter delegate
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="val"></param>
    /// <returns></returns>
    public delegate TValue FromStringConverterDelegate<TValue>(string val) 
        where TValue : class
    ;
    /// <summary>
    /// To string converter delegate
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="val"></param>
    /// <returns></returns>
    public delegate string ToStringConverterDelegate<TValue>(TValue val) 
        where TValue: class 
    ;

    /// <summary>
    /// convert util
    /// </summary>
    public static class ConvertUtil
    {

        private readonly static log4net.ILog log = log4net.LogManager.GetLogger(
            typeof(ConvertUtil));
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="fromString"></param>
        /// <param name="converter"></param>
        /// <returns></returns>
        public static TValueTo ConvertWithConverter<TTypeFrom, TValueTo>(TTypeFrom from, 
            ConverterDelegate<TTypeFrom, TValueTo> converter)
            where TTypeFrom : class
            where TValueTo : class
        {
            return converter(from);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="fromString"></param>
        /// <param name="converter"></param>
        /// <returns></returns>
        public static TValue FromStringWithConverter<TValue>(string fromString, FromStringConverterDelegate<TValue> converter)
            where TValue : class
        {
            return converter(fromString);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="fromValue"></param>
        /// <param name="converter"></param>
        /// <returns></returns>
        public static string ToStringWithConverter<TValue>(TValue fromValue, ToStringConverterDelegate<TValue> converter)
            where TValue : class
        {
            return converter(fromValue);
        }

        public static TValue To<TValue>(DataRow row, string name, TValue defaultValue)
        {
            object retObj = null;
            if(row!=null&&!string.IsNullOrEmpty(name))
            {
                try
                {
                    object rowitem = row[name];
                    if (Convert.IsDBNull(rowitem))
                    {
                        retObj = null;
                    }
                    else
                    {
                        retObj = To<TValue>(rowitem, defaultValue);
                    }
                }
                catch (System.Exception ex)
                {
                    log.Warn(ex);
                    retObj = null;
                }
            }
            if (retObj != null)
            {
                return (TValue)retObj;
            }
            return defaultValue;
        }

        public static string SqlLiteral(string val)
        {
            if (val == null)
            {
                return "null";
            }
            else
            {
                return "'" + val + "'";
            }
        }

        public static string SqlLiteral(object val)
        {
            if (val == null)
            {
                return "null";
            }
            else
            {
                return val.ToString();
            }
        }

        public static TValue To<TValue>(object instanc, TValue defaultValue)
        {
            object retObj = null;
            TypeCode typeCode = Type.GetTypeCode(typeof(TValue));
            if (instanc != null)
            {
                switch (typeCode)
                {
                    case TypeCode.String://A sealed class type representing Unicode character strings. 
                        retObj = Convert.ToString(instanc); break;
                    case TypeCode.Boolean://A simple type representing Boolean values of true or false. 
                        retObj = Convert.ToBoolean(instanc); break;
                    case TypeCode.Char://An integral type representing unsigned 16-bit integers with values between 0 and 65535
                        retObj = Convert.ToChar(instanc); break;
                    case TypeCode.SByte://An integral type representing signed 8-bit integers with values between -128 and 127. 
                        retObj = Convert.ToSByte(instanc); break;
                    case TypeCode.Byte://An integral type representing unsigned 8-bit integers with values between 0 and 255. 
                        retObj = Convert.ToByte(instanc); break;
                    case TypeCode.Int16://An integral type representing signed 16-bit integers with values between -32768 and 32767. 
                        retObj = Convert.ToInt16(instanc); break;
                    case TypeCode.UInt16://An integral type representing unsigned 16-bit integers with values between 0 and 65535. 
                        retObj = Convert.ToUInt16(instanc); break;
                    case TypeCode.Int32://An integral type representing signed 32-bit integers with values between -2147483648 and 2147483647. 
                        retObj = Convert.ToInt32(instanc); break;
                    case TypeCode.UInt32://An integral type representing unsigned 32-bit integers with values between 0 and 4294967295. 
                        retObj = Convert.ToUInt32(instanc); break;
                    case TypeCode.Int64://An integral type representing signed 64-bit integers with values between -9223372036854775808 and 9223372036854775807.
                        retObj = Convert.ToInt64(instanc); break;
                    case TypeCode.UInt64://An integral type representing unsigned 64-bit integers with values between 0 and 18446744073709551615. 
                        retObj = Convert.ToUInt64(instanc); break;
                    case TypeCode.Single://A floating point type representing values ranging from approximately 1.5 x 10 -45 to 3.4 x 10 38 with a precision of 7 digits. 
                        retObj = Convert.ToSingle(instanc); break;
                    case TypeCode.Double://A floating point type representing values ranging from approximately 5.0 x 10 -324 to 1.7 x 10 308 with a precision of 15-16 digits.
                        retObj = Convert.ToDouble(instanc); break;
                    case TypeCode.Decimal://A simple type representing values ranging from 1.0 x 10 -28 to approximately 7.9 x 10 28 with 28-29 significant digits. 
                        retObj = Convert.ToDecimal(instanc); break;

                    case TypeCode.DateTime://A type representing a date and time value. 
                        retObj = Convert.ToDateTime(instanc); break;

                    case TypeCode.Empty://A null reference. 
                    case TypeCode.DBNull://A database null (column) value. 
                        retObj = null; break;
                    case TypeCode.Object://A general type representing any reference
                    default:
                        if (instanc != null && !Convert.IsDBNull(instanc))
                        {
                            retObj = instanc;
                        }
                        break;
                }
            }
            if(retObj!=null)
            {
                return (TValue)retObj;
            }
            return defaultValue;
        }

        public static TValue FromString<TValue>(string from)
            where TValue : class
        {
            object obj;
            TypeCode typeCode = Type.GetTypeCode( typeof(TValue) );
            switch(typeCode)
            {
                case TypeCode.String://A sealed class type representing Unicode character strings. 
                    obj = from; break;
                case TypeCode.Boolean://A simple type representing Boolean values of true or false. 
                    obj = Boolean.Parse(from); break;
                case TypeCode.Char://An integral type representing unsigned 16-bit integers with values between 0 and 65535
                    obj = Char.Parse(from); break;
                case TypeCode.SByte://An integral type representing signed 8-bit integers with values between -128 and 127. 
                    obj = SByte.Parse(from); break;
                case TypeCode.Byte://An integral type representing unsigned 8-bit integers with values between 0 and 255. 
                    obj = Byte.Parse(from); break;
                case TypeCode.Int16://An integral type representing signed 16-bit integers with values between -32768 and 32767. 
                    obj = Int16.Parse(from); break;
                case TypeCode.UInt16://An integral type representing unsigned 16-bit integers with values between 0 and 65535. 
                    obj = UInt16.Parse(from); break;
                case TypeCode.Int32://An integral type representing signed 32-bit integers with values between -2147483648 and 2147483647. 
                    obj = Int32.Parse(from); break;
                case TypeCode.UInt32://An integral type representing unsigned 32-bit integers with values between 0 and 4294967295. 
                    obj = UInt32.Parse(from); break;
                case TypeCode.Int64://An integral type representing signed 64-bit integers with values between -9223372036854775808 and 9223372036854775807.
                    obj = Int64.Parse(from); break;
                case TypeCode.UInt64://An integral type representing unsigned 64-bit integers with values between 0 and 18446744073709551615. 
                    obj = UInt64.Parse(from); break;
                case TypeCode.Single://A floating point type representing values ranging from approximately 1.5 x 10 -45 to 3.4 x 10 38 with a precision of 7 digits. 
                    obj = Single.Parse(from); break;
                case TypeCode.Double://A floating point type representing values ranging from approximately 5.0 x 10 -324 to 1.7 x 10 308 with a precision of 15-16 digits.
                    obj = Double.Parse(from); break;
                case TypeCode.Decimal://A simple type representing values ranging from 1.0 x 10 -28 to approximately 7.9 x 10 28 with 28-29 significant digits. 
                    obj = Decimal.Parse(from); break;

                case TypeCode.DateTime://A type representing a date and time value. 
                    obj = DateTime.Parse(from); break;

                case TypeCode.Empty://A null reference. 
                case TypeCode.DBNull://A database null (column) value. 
                    obj = null; break;
                case TypeCode.Object://A general type representing any reference
                default:
                    //TODO ConvertUtils.FromString<TValue> maybe have some issue to convert object
                    throw new Exception(string.Format
                    ("Type convert from string{0} to {1} is not accepted.", from, typeof(TValue).FullName));
            }
            return (TValue)obj;
        }

        public static object ConvertToDbType(System.Data.DbType dbType, object val)
        {
            object ret = null;
            switch (dbType)
            {
                case System.Data.DbType.AnsiString:
                case System.Data.DbType.AnsiStringFixedLength:
                case System.Data.DbType.String:
                case System.Data.DbType.StringFixedLength:
                case System.Data.DbType.Xml:
                    ret = (string)val;
                    break;
                case System.Data.DbType.Binary:
                    ret = (byte[])val;
                    break;
                case System.Data.DbType.Boolean:
                    ret = (bool)val;
                    break;
                case System.Data.DbType.SByte:
                case System.Data.DbType.Byte:
                    ret = (byte)val;
                    break;
                case System.Data.DbType.Currency:
                case System.Data.DbType.Double:
                    ret = (double)val;
                    break;
                case System.Data.DbType.Decimal:
                case System.Data.DbType.VarNumeric:
                    ret = (decimal)val;
                    break;
                case System.Data.DbType.Guid:
                    ret = (Guid)val;
                    break;
                case System.Data.DbType.Int16:
                    ret = (short)val;
                    break;
                case System.Data.DbType.Int32:
                    ret = (int)val;
                    break;
                case System.Data.DbType.Int64:
                    ret = (long)val;
                    break;
                case System.Data.DbType.UInt16:
                    ret = (ushort)val;
                    break;
                case System.Data.DbType.UInt32:
                    ret = (uint)val;
                    break;
                case System.Data.DbType.UInt64:
                    ret = (ulong)val;
                    break;
                case System.Data.DbType.Single:
                    ret = (float)val;
                    break;
                case System.Data.DbType.Date:
                case System.Data.DbType.DateTime:
                case System.Data.DbType.DateTime2:
                case System.Data.DbType.Time:
                case System.Data.DbType.DateTimeOffset:
                    ret = (DateTime)val;
                    break;
                case System.Data.DbType.Object:
                default:
                    ret = val;
                    break;
            }
            return ret;
        }

        public static long ToInt64(string longVal)
        {
            if (string.IsNullOrEmpty(longVal))
            {
                return 0;
            }
            else
            {
                double dVal;
                if (double.TryParse(longVal, out dVal))
                {
                    //yous should validate that the value is within the expected range as well
                    return (long)dVal;
                }
                else
                {
                    long lVal;
                    if (long.TryParse(longVal, out lVal))
                    {
                        return lVal;
                    }
                    log.WarnFormat("longVal '{0}' can not convert to Int64!", longVal);
                    return 0;
                }
            }
        }

        public static int ToInt32(string intVal)
        {
            if (string.IsNullOrEmpty(intVal))
            {
                return 0;
            }
            else
            {
                double dVal;
                if (double.TryParse(intVal, out dVal))
                {
                    //yous should validate that the value is within the expected range as well
                    return (int)dVal;
                }
                else
                {
                    int lVal;
                    if (int.TryParse(intVal, out lVal))
                    {
                        return lVal;
                    }
                    log.WarnFormat("intVal '{0}' can not convert to Int32!", intVal);
                    return 0;
                }
            }
        }

        public static double ToDouble(string doubleVal)
        {
            if (string.IsNullOrEmpty(doubleVal))
            {
                return 0.0;
            }
            else
            {
                double dVal;
                if (double.TryParse(doubleVal, out dVal))
                {
                    //yous should validate that the value is within the expected range as well
                    return dVal;
                }
                log.WarnFormat("doubleVal '{0}' can not convert to Double!", doubleVal);
                return 0.0;
            }
        }
    }
}
