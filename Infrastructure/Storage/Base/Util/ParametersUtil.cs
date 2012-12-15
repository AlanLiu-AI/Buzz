using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Runner.Base.Util
{

    /// <summary>
    /// Parameters Utility Class
    /// </summary>
    public static class ParametersUtil
    {
        /// <summary>
        /// log4net log instance
        /// </summary>
        private readonly static log4net.ILog log = log4net.LogManager.GetLogger(
            typeof(ParametersUtil).Name);

        /// <summary>
        /// Check if a parameter exists 
        /// </summary>
        /// <param name="param"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        public static bool ContainsKey(string param, IDictionary<string, string> settings)
        {
            if (settings == null || string.IsNullOrEmpty(param)) return false;
            return settings.ContainsKey(param);
        }

        // 
        /// <summary>
        /// Retrieve a parameter value with a defaultValue
        /// </summary>
        /// <param name="param"></param>
        /// <param name="defaultValue"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        public static string GetParam(string param, IDictionary<string, string> settings, string defaultValue)
        {
            if (settings!=null && settings.ContainsKey(param))
            {
                return (settings[param]);
            }
            else
            {
                return defaultValue;
            }
        }

        public static string GetParams(IDictionary<string, string> settings, string defaultValue, params string[] possibleParamKeys)
        {
            if (possibleParamKeys != null && possibleParamKeys.Length > 0)
            {
                foreach (string key in possibleParamKeys)
                {
                    if (settings.ContainsKey(key))
                    {
                        return settings[key];
                    }
                }
            }
            return defaultValue;
        }
        /// <summary>
        /// Retrieve a parameter value.
        /// </summary>
        /// <param name="param"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        public static string GetParam(string param, IDictionary<string, string> settings)
        {
            return GetParam(param, settings, null);
        }

        /// <summary>
        /// Retrieve a parameter value with a defaultValue
        /// </summary>
        /// <param name="param"></param>
        /// <param name="defaultValue"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        public static TValue GetParamWithConverter<TValue>(string param, 
            FromStringConverterDelegate<TValue> converter, 
            IDictionary<string, string> settings)
            where TValue : class
        {
            return GetParamWithConverter<TValue>(param, converter, settings, null);
        }

        /// <summary>
        /// Retrieve a parameter value with a defaultValue
        /// </summary>
        /// <param name="param"></param>
        /// <param name="defaultValue"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        public static TValue GetParamWithConverter<TValue>(string param, 
            FromStringConverterDelegate<TValue> converter, 
            IDictionary<string, string> settings, TValue defaultValue)
            where TValue : class
        {
            if (settings != null && settings.ContainsKey(param))
            {
                string value = settings[param];
                return converter(value);
            }
            else
            {
                return defaultValue;
            }
        }
        
        /// <summary>
        /// Retrieve a parameter value with a defaultValue
        /// </summary>
        /// <param name="param"></param>
        /// <param name="defaultValue"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        public static int GetIntParam(string param, IDictionary<string, string> settings, int defaultValue)
        {
            if (settings!=null && settings.ContainsKey(param))
            {
                string value = settings[param];
                int ret;
                if (int.TryParse(value, out ret))
                {
                    return ret;
                }
            }
            return defaultValue;
        }

        public static int GetIntParams(IDictionary<string, string> settings, int defaultValue, params string[] possibleParamKeys)
        {
            if (possibleParamKeys != null && possibleParamKeys.Length > 0)
            {
                int ret;
                foreach (string key in possibleParamKeys)
                {
                    if (settings.ContainsKey(key))
                    {
                        string value = settings[key];
                        if (int.TryParse(value, out ret))
                        {
                            return ret;
                        }
                    }
                }
            }
            return defaultValue;
        }

        /// <summary>
        /// Retrieve a parameter value with a defaultValue
        /// </summary>
        /// <param name="param"></param>
        /// <param name="defaultValue"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        public static long GetLongParam(string param, IDictionary<string, string> settings, long defaultValue)
        {
            if (settings != null && settings.ContainsKey(param))
            {
                string value = settings[param];
                if(!string.IsNullOrEmpty(value))
                {
                    long ret;
                    if(long.TryParse(value, out ret))
                    {
                        return ret;
                    }
                    log.WarnFormat("String '{0}' Format is not long type.", value);
                }
                return defaultValue;
            }
            else
            {
                return defaultValue;
            }
        }

        public static long GetLongParams(IDictionary<string, string> settings, int defaultValue, params string[] possibleParamKeys)
        {
            if (possibleParamKeys != null && possibleParamKeys.Length > 0)
            {
                long ret;
                foreach (string key in possibleParamKeys)
                {
                    if (settings.ContainsKey(key))
                    {
                        string value = settings[key];
                        if (long.TryParse(value, out ret))
                        {
                            return ret;
                        }
                    }
                }
            }
            return defaultValue;
        }

        /// <summary>
        /// Retrieve a parameter value with a defaultValue
        /// </summary>
        /// <param name="param"></param>
        /// <param name="defaultValue"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        public static int CheckIntValue(int value, int defaut, int min, int max)
        {
            if (value >= min && value <= max) return value;
            return defaut;
        }

        /// <summary>
        /// Retrieve a parameter value with a defaultValue
        /// </summary>
        /// <param name="param"></param>
        /// <param name="defaultValue"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        public static int GetIntParam(string param, IDictionary<string, string> settings, int defaultValue, int min, int max)
        {
            int ret = GetIntParam(param, settings, defaultValue);
            return CheckIntValue(ret, defaultValue, min, max);
        }


        /// <summary>
        /// Retrieve a parameter value with a defaultValue
        /// </summary>
        /// <param name="param"></param>
        /// <param name="defaultValue"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        public static bool GetBoolParam(string param, IDictionary<string, string> settings, bool defaultValue)
        {
            if (settings != null && settings.ContainsKey(param))
            {
                string value = settings[param];
                bool ret;
                if (bool.TryParse(value, out ret))
                {
                    return ret;
                }
            }
            return defaultValue;
        }

        public static bool GetBoolParams(IDictionary<string, string> settings, bool defaultValue, params string[] possibleParamKeys)
        {
            if (possibleParamKeys != null && possibleParamKeys.Length > 0)
            {
                bool ret;
                foreach (string key in possibleParamKeys)
                {
                    if (settings.ContainsKey(key))
                    {
                        string value = settings[key];
                        if (bool.TryParse(value, out ret))
                        {
                            return ret;
                        }
                        continue;
                    }
                }
            }
            return defaultValue;
        }

        /// <summary>
        /// Retrieve a parameter value 
        /// </summary>
        /// <param name="param"></param>
        /// <param name="defaultValue"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        public static TValue GetParam<TValue>(string param, IDictionary<string, string> settings)
            where TValue : class
        {
            return GetParam<TValue>(param, settings, null);
        }


        /// <summary>
        /// Retrieve a parameter value with a defaultValue
        /// </summary>
        /// <param name="param"></param>
        /// <param name="defaultValue"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        public static TValue GetParam<TValue>(string param, IDictionary<string, string> settings, TValue defaultValue)
            where TValue : class
        {
            if (settings != null && settings.ContainsKey(param))
            {
                string value = settings[param];
                return ConvertUtil.FromString<TValue>(value);
            }
            else
            {
                return defaultValue;
            }
        }
        public static int GetPrefixIntParam(string prefix, string param, IDictionary<string, string> settings, int defaultValue)
        {
            int ret = GetIntParam(prefix + param, settings, -10000000);
            if (ret == -10000000) ret = ParametersUtil.GetIntParam(param, settings, -10000000);
            if (ret == -10000000) ret = defaultValue;
            return ret;
        }

        public static int GetPrefixIntParam(string prefix, string param, IDictionary<string, string> settings, int defaultValue, int min, int max)
        {
            int ret = GetPrefixIntParam(prefix, param, settings, defaultValue);
            ret = CheckIntValue(ret, defaultValue, min, max);
            return ret;
        }

        public static string GetPrefixParam(string prefix, string param, IDictionary<string, string> settings, string defaultValue)
        {
            if (settings != null && settings.ContainsKey(prefix + param))
            {
                string value = settings[prefix + param];
                return value;
            }
            else if (settings != null && settings.ContainsKey(param))
            {
                string value = settings[param];
                return value;
            }
            else
            {
                return defaultValue;
            }
        }

        public static bool GetPrefixBoolParam(string prefix, string param, IDictionary<string, string> settings, bool defaultValue)
        {
            if (settings != null && settings.ContainsKey(prefix + param))
            {
                string value = settings[prefix + param];
                return bool.Parse(value);
            }
            else if (settings != null && settings.ContainsKey(param))
            {
                string value = settings[param];
                return bool.Parse(value);
            }
            else
            {
                return defaultValue;
            }
        }

        public static Object GetRegisterParam(string keyName, string valueName, Object defaultValue)
        {
            Object ret = Microsoft.Win32.Registry.GetValue(keyName, valueName, defaultValue);
            if (ret == null)
            {
                return defaultValue;
            }
            return ret;
        }

        public static void SetRegisterParam(string keyName, string valueName, Object value)
        {
            Microsoft.Win32.Registry.SetValue(keyName, valueName, value);
        }
    }
}
