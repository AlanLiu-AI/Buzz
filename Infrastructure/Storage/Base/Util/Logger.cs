using System;
using System.Threading;
#if !SILVERLIGHT
using System.Text;
using System.IO;
using System.Configuration;
using System.Security.AccessControl;
#endif

namespace Runner.Base.Util
{

    #region class BaseLogger
    public abstract class BaseLogger
    {
        protected LoggerLevel LogLevel;
        protected string Name = string.Empty;
        protected static Object LoggerSync = new Object();

        public bool IsTraceEnabled { get { return LogLevel >= LoggerLevel.All; } }
        public bool IsDebugEnabled { get { return LogLevel >= LoggerLevel.Debug; } }
        public bool IsInfoEnabled { get { return LogLevel >= LoggerLevel.Info; } }
        public bool IsWarnEnabled { get { return LogLevel >= LoggerLevel.Warn; } }
        public bool IsErrorEnabled { get { return LogLevel >= LoggerLevel.Error; } }
        public bool IsFatalEnabled { get { return LogLevel >= LoggerLevel.Fatal; } }

        protected abstract string Prefix(string prefix);
        protected abstract void Log(string prefix, string str, Exception ex);

        public void SetLevel(int level)
        {
            if (level > (int)LoggerLevel.All) level = (int) LoggerLevel.All;
            if (level < (int)LoggerLevel.Off) level = (int)LoggerLevel.Off;
            LogLevel = (LoggerLevel)level;
        }

        public void Trace(Exception ex)
        {
            if (IsTraceEnabled)
            {
                Log(Prefix("TRACE"), string.Empty, ex);
            }
        }

        public void Trace(string str)
        {
            if (IsTraceEnabled)
            {
                Log(Prefix("TRACE"), str, null);
            }
        }

        public void Trace(string str, Exception ex)
        {
            if (IsTraceEnabled)
            {
                Log(Prefix("TRACE"), str, ex);
            }
        }

        public void TraceFormat(string fmt, params object[] args)
        {
            if (IsTraceEnabled)
            {
                Log(Prefix("TRACE"), string.Format(fmt, args), null);
            }
        }

        public void Debug(Exception ex)
        {
            if (IsDebugEnabled)
            {
                Log(Prefix("DEBUG"), string.Empty, ex);
            }
        }

        public void Debug(string str)
        {
            if (IsDebugEnabled)
            {
                Log(Prefix("DEBUG"), str, null);
            }
        }

        public void Debug(string str, Exception ex)
        {
            if (IsDebugEnabled)
            {
                Log(Prefix("DEBUG"), str, ex);
            }
        }

        public void DebugFormat(string fmt, params object[] args)
        {
            if (!IsDebugEnabled) return;
            Log(Prefix("DEBUG"), string.Format(fmt, args), null);
        }

        public void Info(Exception ex)
        {
            if (IsInfoEnabled)
            {
                Log(Prefix("INFO"), string.Empty, ex);
            }
        }

        public void Info(string str)
        {
            if (IsInfoEnabled)
            {
                Log(Prefix("INFO"), str, null);
            }
        }

        public void Info(string str, Exception ex)
        {
            if (IsInfoEnabled)
            {
                Log(Prefix("INFO"), str, ex);
            }
        }

        public void InfoFormat(string fmt, params object[] args)
        {
            if (IsInfoEnabled)
            {
                Log(Prefix("INFO"), string.Format(fmt, args), null);
            }
        }

        public void Warn(Exception ex)
        {
            if (IsWarnEnabled)
            {
                Log(Prefix("WARN"), string.Empty, ex);
            }
        }

        public void Warn(string str)
        {
            if (IsWarnEnabled)
            {
                Log(Prefix("WARN"), str, null);
            }
        }

        public void Warn(string str, Exception ex)
        {
            if (IsWarnEnabled)
            {
                Log(Prefix("WARN"), str, ex);
            }
        }

        public void WarnFormat(string fmt, params object[] args)
        {
            if (IsWarnEnabled)
            {
                Log(Prefix("WARN"), string.Format(fmt, args), null);
            }
        }

        public void Error(Exception ex)
        {
            if (IsErrorEnabled)
            {
                Log(Prefix("ERROR"), string.Empty, ex);
            }
        }

        public void Error(string str)
        {
            if (IsErrorEnabled)
            {
                Log(Prefix("ERROR"), str, null);
            }
        }

        public void Error(string str, Exception ex)
        {
            if (IsErrorEnabled)
            {
                Log(Prefix("ERROR"), str, ex);
            }
        }

        public void ErrorFormat(string fmt, params object[] args)
        {
            if (IsErrorEnabled)
            {
                Log(Prefix("ERROR"), string.Format(fmt, args), null);
            }
        }

        public void Fatal(Exception ex)
        {
            if (IsFatalEnabled)
            {
                Log(Prefix("FATAL"), string.Empty, ex);
            }
        }

        public void Fatal(string str)
        {
            if (IsFatalEnabled)
            {
                Log(Prefix("FATAL"), str, null);
            }
        }

        public void Fatal(string str, Exception ex)
        {
            if (IsFatalEnabled)
            {
                Log(Prefix("FATAL"), str, ex);
            }
        }

        public void FatalFormat(string fmt, params object[] args)
        {
            if (IsFatalEnabled)
            {
                Log(Prefix("FATAL"), string.Format(fmt, args), null);
            }
        }

        public static void Error(Logger log, Exception ex)
        {
            Exception actualEx = null;
            if (ex.InnerException != null)
            {
                actualEx = ex.InnerException;
                while (actualEx.InnerException != null)
                {
                    actualEx = actualEx.InnerException;
                }
            }
            log.Error("Exception: ", ex);
            if (actualEx != null)
            {
                log.Error("Inner exception: ", actualEx);
            }
        }

        public static void Fatal(Logger log, Exception ex)
        {
            Exception actualEx = null;
            if (ex.InnerException != null)
            {
                actualEx = ex.InnerException;
                while (actualEx.InnerException != null)
                {
                    actualEx = actualEx.InnerException;
                }
            }
            log.Error("Exception: ", ex);
            if (actualEx != null)
            {
                log.Fatal("Inner exception: ", actualEx);
            }
        }
    }
    #endregion

#if !SILVERLIGHT
    public class FileLogger : BaseLogger
    {
        public string LogPath = string.Empty;

        public static FileLogger GetLogger(Type type, string logPath)
        {
            return GetLogger(type.FullName, logPath);
        }

        public static FileLogger GetLogger(Type type, string logPath, LoggerLevel level)
        {
            return GetLogger(type.FullName, logPath, level);
        }

        public static FileLogger GetLogger(string name, string logFilePath)
        {
            return new FileLogger(name, logFilePath);
        }

        public static FileLogger GetLogger(string name, string logFilePath, LoggerLevel level)
        {
            return new FileLogger(name, logFilePath, level);
        }

        protected FileLogger(string name, string path, LoggerLevel logLevel)
        {
            Name = name;
            LogPath = path;
			LogLevel = logLevel;
        }

        protected FileLogger(string name, string path)
        {
            Name = name;
            LogPath = path;
            var regLogLevel = GetLoggerRegInt(Name, "LogLevel", "OnOff");
            SetLevel(regLogLevel.HasValue ? regLogLevel.Value : 0);
        }

        protected FileLogger(string name, LoggerLevel logLevel)
        {
            Name = name;
            var logger = GetLogPath();
            LogPath = Path.Combine(logger, GetLoggerRegStr("LogFile", "AQShare.log"));
            LogLevel = logLevel;
        }

        protected FileLogger(string name)
        {
            Name = name;
            var logger = GetLogPath();
            LogPath = Path.Combine(logger, GetLoggerRegStr("LogFile", "AQShare.log"));
            var regLogLevel = GetLoggerRegInt(Name, "LogLevel", "OnOff");
            SetLevel(regLogLevel.HasValue ? regLogLevel.Value : 0);
        }

        protected override string Prefix(string prefix)
        {
            //2010-07-26 12:59:11.781 [5580-2424] DEBUG 
            return string.Format("{0} [{1}-{2}] {3}.NET {4} - ",  //We mix CShareLogger and this Logger to the same target file, so here we added .NET for the difference
                DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                System.Diagnostics.Process.GetCurrentProcess().Id,
                Thread.CurrentThread.ManagedThreadId,
                prefix, Name);
        }

        protected override void Log(string prefix, string str, Exception ex)
        {
            var msg = prefix;
            if (!string.IsNullOrEmpty(str))
            {
                msg += str + Environment.NewLine;
            }
            if (ex != null)
            {
                msg += ex.Message + Environment.NewLine + ex.StackTrace + Environment.NewLine;
            }
            Log(LogPath, msg);
        }

        public static void Log(string filePath, string str)
        {
#if DEBUG
            System.Diagnostics.Trace.Write(str);
#endif
            lock (LoggerSync)
            {
                using (var fs = new FileStream(filePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                {
                    using (TextWriter tw = new StreamWriter(fs, Encoding.UTF8))
                    {
                        tw.Write(str);
                    }
                }
            }
        }

        public static string GetLogPath()
        {
            //Top level, config file logFileFullPath setting
            string logFileFullPath = ConfigurationManager.AppSettings["logFileFullPath"];
            if (string.IsNullOrEmpty(logFileFullPath))
            {
                logFileFullPath = Environment.GetEnvironmentVariable("LOGS");
                if (string.IsNullOrEmpty(logFileFullPath))
                {
                    //Default level, using setup folder.
                    logFileFullPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), @"Logs\");
                }
            }
            CreateDirectoryForEveryone(logFileFullPath);
            return logFileFullPath;
        }

        public static void CreateDirectoryForEveryone(string logPath)
        {
            DirectorySecurity dirInfoSec = null;
            if (!Directory.Exists(logPath))
            {
                var dirInfo = Directory.CreateDirectory(logPath);
                dirInfoSec = dirInfo.GetAccessControl();
            }

            if(dirInfoSec == null)
            {
                dirInfoSec = Directory.GetAccessControl(logPath);
            }

            {
                var everyone = new System.Security.Principal.SecurityIdentifier("S-1-1-0").Translate(typeof(System.Security.Principal.NTAccount)).ToString();
                dirInfoSec.AddAccessRule(new FileSystemAccessRule(everyone, FileSystemRights.FullControl, InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit, PropagationFlags.None, AccessControlType.Allow));
                dirInfoSec.AddAccessRule(new FileSystemAccessRule(everyone, FileSystemRights.ReadAndExecute, AccessControlType.Allow));
                dirInfoSec.AddAccessRule(new FileSystemAccessRule(everyone, FileSystemRights.CreateFiles, AccessControlType.Allow));
                Directory.SetAccessControl(logPath, dirInfoSec);
            }
        }

        internal static class LoggerStaticVariables
        {
            internal static string[] LogRegEntrys = {
                        @"HKEY_LOCAL_MACHINE\SOFTWARE\Runner\Log" //Logs
                    };
        }

        private static int? GetLoggerRegInt(params string[] names)
        {
            const int regIntNull = -999999;
            var value = regIntNull;
            foreach(var name in names)
            {
                foreach (var key in LoggerStaticVariables.LogRegEntrys)
                {
                    try
                    {
                        value = (Int32)Microsoft.Win32.Registry.GetValue(key, name, regIntNull);
                        if (value != regIntNull)
                        {
                            break;
                        }
                    }
                    catch
                    {
                        value = regIntNull;
                    }
                }
                if (value != regIntNull) break;
            }
            if (value == regIntNull) return null;
            return value;
        }

        private static string GetLoggerRegStr(string name, string defaultValue)
        {
            var ret = "";
            foreach (var key in LoggerStaticVariables.LogRegEntrys)
            {
                try
                {
                    ret = (string)Microsoft.Win32.Registry.GetValue(key, name, "");
                    if (!string.IsNullOrEmpty(ret))
                    {
                        break;
                    }
                }
                catch
                {
                    ret = "";
                }
            }
            if (string.IsNullOrEmpty(ret))
            {
                ret = defaultValue;
            }
            return ret;
        }
    }

#else
    public class SilverlightLogger : BaseLogger
    {
        private static LoggerLevel? _logLevel;
        protected SilverlightLogger(string name)
        {
            Name = name;
            if (!_logLevel.HasValue)
            {
                lock (LoggerSync)
                {
                    var logLevelParam = 0;
                    // We need this try/catch to avoid the exception when loading the xaml designer.
                    try
                    {
                        var debugKeys = new[] {"Debug", "debug", "DEBUG"};
                        foreach (var debugKey in debugKeys)
                        {
                            if (System.Windows.Browser.HtmlPage.Document.QueryString.ContainsKey(debugKey))
                            {
                                var debugParam = System.Windows.Browser.HtmlPage.Document.QueryString[debugKey];
                                if (!string.IsNullOrEmpty(debugParam))
                                {
                                    bool debug;
                                    if (bool.TryParse(debugParam, out debug) && debug)
                                    {
                                        logLevelParam = 5;
                                        break;
                                    }
                                    int debugLevel;
                                    if (int.TryParse(debugParam, out debugLevel))
                                    {
                                        logLevelParam = debugLevel;
                                        break;
                                    }
                                    logLevelParam = 0;
                                    break;
                                }
                            }
                        }
                        if (logLevelParam < 1)
                        {
                            logLevelParam = 0;
                        }
                    }
                    catch
                    {
                        logLevelParam = 0;
                    }
                    //value specify
                    if (logLevelParam > (int) LoggerLevel.All) logLevelParam = (int) LoggerLevel.All;
                    if (logLevelParam < (int) LoggerLevel.Off) logLevelParam = (int) LoggerLevel.Off;
                    _logLevel = (LoggerLevel) logLevelParam;
                }
            }
            LogLevel = _logLevel.Value;
        }

        protected override string Prefix(string prefix)
        {
            //2010-07-26 12:59:11.781 [5580-2424] DEBUG 
            return string.Format("{0} [{1}-{2}] {3} {4} - ",
                DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                "SL",
                Thread.CurrentThread.ManagedThreadId,
                prefix, Name);
        }

        protected override void Log(string prefix, string str, Exception ex)
        {
            var msg = prefix;
            if (!string.IsNullOrEmpty(str))
            {
                msg += str + Environment.NewLine;
            }

            if (ex != null)
            {
                msg += ex.Message + Environment.NewLine + ex.StackTrace + Environment.NewLine;
            }

            lock (LoggerSync)
            {
                System.Windows.Browser.HtmlPage.Window.Invoke("Log", LogStringHtmlProcessing(msg));
            }
        }

        private static string LogStringHtmlProcessing(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;
            input = input.Replace("&", "&amp;");
            input = input.Replace("<", "&lt;");
            input = input.Replace(">", "&gt;");
            input = input.Replace("\"", "&quot;");
            //input = input.Replace("'", "&apos;");
            input = input.Replace("\n", "<br/>");
            if (input.IndexOf("FATAL", StringComparison.Ordinal) > 0)
            {
                input = "<font color='red'><b>" + input + "</b></font>";
            }
            else if (input.IndexOf("ERROR", StringComparison.Ordinal) > 0)
            {
                input = "<font color='red'>" + input + "</font>";
            }
            else if (input.IndexOf("WARN", StringComparison.Ordinal) > 0)
            {
                input = "<font color='orange'>" + input + "</font>";
            }
            else if (input.IndexOf("INFO", StringComparison.Ordinal) > 0)
            {
                input = input.Replace("INFO", "<font color='green'>INFO</font>");
            }
            else if (input.IndexOf("DEBUG", StringComparison.Ordinal) > 0 || input.IndexOf("TRACE", StringComparison.Ordinal) > 0)
            {
                input = "<font color='gray'>" + input + "</font>";
            }
            return input;
        }
    }
#endif

#if !SILVERLIGHT
    /// <summary>
    /// CSharp Log Utility for Dotnet COM
    /// To use this log, you need add some register key to active it.
    /// 
    /// [HKEY_LOCAL_MACHINE\SOFTWARE\Logs]
    /// "LogLevel"=dword:00000006
    /// "LogFile"="Runner.log"
    /// 
    /// OnOff also means log level.
    /// Trace 6
    /// Debug 5
    /// Info  4
    /// Warn  3
    /// Error 2
    /// Fatal 1
    /// None  0 or other value.
    /// </summary>
    public class Logger : FileLogger
#else
    /// <summary>
    /// Silverlight Log Utility 
    /// To use this log, you need add a specific param into query string for SILVERLIGHT application
    /// 
    /// Log=1~6
    /// Trace 6
    /// Debug 5
    /// Info  4
    /// Warn  3
    /// Error 2
    /// Fatal 1
    /// None  0 or other value.
    /// </summary>
    public class Logger : SilverlightLogger
#endif
    {
        public static readonly Logger Default = GetLogger("Default");

        public static bool IsTraceLevel { get { return Default.IsTraceEnabled; } }
        public static bool IsDebugLevel { get { return Default.IsDebugEnabled; } }
        public static bool IsInfoLevel { get { return Default.IsInfoEnabled; } }
        public static bool IsWarnLevel { get { return Default.IsWarnEnabled; } }
        public static bool IsErrorLevel { get { return Default.IsErrorEnabled; } }
        public static bool IsFatalLevel { get { return Default.IsFatalEnabled; } }
        public static bool IsEnabled { get { return Default.LogLevel != LoggerLevel.Off; } }

        protected Logger(string name) : base(name)
        {
        }

#if !SILVERLIGHT
        protected Logger(string name, LoggerLevel level)
            : base(name, level)
        {
        }
#endif
        public static Logger GetLogger(Type type)
        {
            return GetLogger(type.FullName);
        }

        public static Logger GetLogger(string name)
        {
            var ret = new Logger(name);
            return ret;
        }

#if !SILVERLIGHT
        public static Logger GetLogger(string name, LoggerLevel level)
        {
            var ret = new Logger(name, level);
            return ret;
        }
#endif

        public static void Log(string str)
        {
            if (IsEnabled)
            {
                Default.Log(Default.Prefix(""), str, null);
            }
        }

        public void Output(string str)
        {
            if (LogLevel!=LoggerLevel.Off)
            {
                Log(Prefix(""), str, null);
            }
        }
    }

    public delegate void LoggingDelegate(string text);
}
