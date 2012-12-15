using System.Collections.Generic;
using System.Configuration;

namespace Runner.Base.Util
{
    public static  class ConfigurationUtil
    {
        /// <summary>
        /// Append key value from srcSettings to destSettings
        /// </summary>
        /// <param name="srcSettings"></param>
        /// <param name="destSettings"></param>
        public static void AppendSettings(IDictionary<string, string> srcSettings, IDictionary<string, string> destSettings)
        {
            AppendSettings(srcSettings, destSettings, true);
        }

        /// <summary>
        /// Append key value from srcSettings to destSettings
        /// </summary>
        /// <param name="srcSettings"></param>
        /// <param name="destSettings"></param>
        /// <param name="overwrite"></param>
        public static void AppendSettings(IDictionary<string, string> srcSettings, IDictionary<string, string> destSettings, bool overwrite)
        {
            if (srcSettings != null && destSettings != null)
            {
                foreach (KeyValuePair<string, string> kv in srcSettings)
                {
                    if (destSettings.ContainsKey(kv.Key))
                    {
                        if (overwrite) destSettings[kv.Key] = kv.Value;
                    }
                    else
                    {
                        destSettings.Add(kv.Key, kv.Value);
                    }
                }
            }
        }


        /// <summary>
        /// Load appsettings from args
        /// </summary>
        /// <param name="args"></param>
        public static IDictionary<string, string> LoadArgsSettings(string[] args)
        {
            //load args setting
            
            IDictionary<string, string> argSettings = new Dictionary<string, string>();
            ArgumentsUtil.RegexParser(args, argSettings);

            //load app.config setting            
            IDictionary<string, string> appSettings = new Dictionary<string, string>();
            if (argSettings.ContainsKey("app.config"))
            {
                string runtimeconfigfile = argSettings["app.config"];
                LoadAppSettingsAtRunTime(runtimeconfigfile, appSettings, true);
            }

            //append args setting to app.config setting, and will overwrite 
            AppendSettings(argSettings, appSettings, true);

            //release some unuse instance
            argSettings.Clear();
            return appSettings;
        }

        public static IDictionary<string, string> LoadAppSettings(string[] args)
        {
            IDictionary<string, string> settings = LoadAppSettings();
            ArgumentsUtil.RegexParser(args, null, settings);
            return settings;
        }

        public static IDictionary<string, string> LoadAppSettings(string[] args, string argsLine)
        {
            IDictionary<string, string> settings = LoadAppSettings();
            ArgumentsUtil.RegexParser(args, argsLine, settings);
            return settings;
        }

        /// <summary>
        /// Load appsettings
        /// </summary>
        public static IDictionary<string, string> LoadAppSettings()
        {
            IDictionary<string, string> settings = new Dictionary<string, string>();
            LoadAppConfigSettings(settings, true);
            return settings;
        }

        /// <summary>
        /// Load appsettings
        /// </summary>
        /// <param name="configFile"></param>
        /// <param name="settings"></param>
        /// <param name="overwrite"></param>
        public static void LoadAppSettingsAtRunTime(string configFile, IDictionary<string, string> settings, bool overwrite)
        {
            if(configFile!=null && System.IO.File.Exists(configFile))
            {
                // Specify config settings at runtime.
                var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                config.AppSettings.File = configFile;
                config.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("appSettings");
                if (settings!=null)
                {
                    LoadAppConfigSettings(settings, overwrite);
                }
            }
        }

        /// <summary>
        /// For read access you do not need to call OpenExeConfiguraton
        /// Some extra configuration information in app.config
        /// &lt;configuration&gt;
        ///     &lt;appSettings&gt;
        ///         &lt;add key="Setting1" value="Very" /&gt;
        //          &lt;add key="Setting2" value="Easy" /&gt;
        ///     &lt;/appSettings&gt;
        ///     ...
        /// &lt;/configuration&gt;
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="overwrite"></param>
        public static void LoadAppConfigSettings(IDictionary<string, string> settings, bool overwrite)
        {
            if (settings != null)
            {
                foreach (string key in ConfigurationManager.AppSettings)
                {
                    string value = ConfigurationManager.AppSettings[key];
                    if (settings.ContainsKey(key))
                    {
                        if (overwrite) settings[key] = value;
                    }
                    else
                    {
                        settings.Add(key, value);
                    }
                }
            }
        }

        public static Configuration GetConfiguration()
        {
            return ConfigurationManager.OpenMachineConfiguration();
        }
    }
}
