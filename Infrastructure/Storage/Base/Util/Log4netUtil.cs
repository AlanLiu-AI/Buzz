using System;
using System.Text;
using log4net;
using log4net.Appender;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Collections;
using log4net.Filter;
using log4net.Core;

namespace Runner.Base.Util
{
    /// <summary>
    /// Log4netUtil utility
    /// </summary>
    public static class Log4netUtil
    {
        private readonly static log4net.ILog Log = log4net.LogManager.GetLogger(typeof(Log4netUtil).Name);

        private static bool _defaultInitialized;
        private static bool _customInitialized;
        private static bool _embbededInitialized;
        private static string _embbededInitializedTo = string.Empty;
        /// <summary>
        /// Initial log4net direct from default config list
        /// </summary>
        public static void Initial()
        {
            if (_defaultInitialized) return;
            //path find log4net with priority
            string[] defaultLogConfigXmlList = {
				@"\log4net.config",
				@"\..\log4net.config",
				@"\..\..\log4net.config",
                @"\..\..\..\log4net.config",
                @"\..\..\..\..\log4net.config",
				@"\..\conf\log4net.config",
				@"\..\..\conf\log4net.config",
				@"\..\..\..\conf\log4net.config",
                @"\..\..\..\..\conf\log4net.config",
				@"\..\config\log4net.config",
				@"\..\..\config\log4net.config",
				@"\..\..\..\config\log4net.config",
                @"\..\..\..\..\config\log4net.config",
				@"\log4net.xml",
				@"\..\log4net.xml",
				@"\..\..\log4net.xml",
				@"\..\..\..\log4net.xml",
                @"\..\..\..\..\log4net.xml",
				@"\..\conf\log4net.xml",
				@"\..\..\conf\log4net.xml",
				@"\..\..\..\conf\log4net.xml",
                @"\..\..\..\..\conf\log4net.xml",
				@"\..\config\log4net.xml",
				@"\..\..\config\log4net.xml",
				@"\..\..\..\config\log4net.xml",
                @"\..\..\..\..\config\log4net.xml"
			};
            foreach (var logxml in defaultLogConfigXmlList)
            {
                if (string.IsNullOrEmpty(logxml)) continue;
                var logxmlpath = Environment.CurrentDirectory + logxml;
                if (!File.Exists(logxmlpath)) continue;
                log4net.Config.XmlConfigurator.ConfigureAndWatch(new FileInfo(logxmlpath));
                var logFileFullPath = GetLogPath();
                CustomizeLogDirectory(logFileFullPath);
                _defaultInitialized = true;
                return;
            }
            log4net.Config.XmlConfigurator.Configure();
            var logFileFullPath1 = GetLogPath();
            CustomizeLogDirectory(logFileFullPath1);
            _defaultInitialized = true;
        }

        public static void DefaultInitial()
        {
            if (_defaultInitialized) return;
            log4net.Config.XmlConfigurator.Configure();
            var logFileFullPath = GetLogPath();
            CustomizeLogDirectory(logFileFullPath);
            _defaultInitialized = true;
        }

        public static void EmbbededInitial(string assemblyName, string ns, string fileName)
        {
            if (_embbededInitialized)
            {
                Log.Warn("EmbbededInitial to " + _embbededInitializedTo);
                return;
            } 
            using (Stream logConfigStream = IOUtil.GetEmbeddedFile(assemblyName, ns, fileName))
            {
                log4net.Config.XmlConfigurator.Configure(logConfigStream);
                string logFileFullPath = GetLogPath();
                CustomizeLogDirectory(logFileFullPath);
                _embbededInitializedTo = string.Format("{0}.{1}[{2}", ns, fileName, assemblyName);
                _embbededInitialized = true;
            }
        }

        public static void EmbbededInitial(string dir, string fileName)
        {
            if (_embbededInitialized)
            {
                Log.Warn("EmbbededInitial to " + _embbededInitializedTo);
                return;
            }
            using (Stream logConfigStream = IOUtil.GetEmbeddedFile(System.Reflection.Assembly.GetCallingAssembly(), "Runner.Base.Util.log4net.config"))
            {
                log4net.Config.XmlConfigurator.Configure(logConfigStream);
                CustomizeLogDirectory(dir, fileName);
                _embbededInitializedTo = string.Format("{0}/{1}[Runner.Base.util.log4net.config]", dir, fileName);
                _embbededInitialized = true;
            }
        }

        public static void EmbbededInitialAssembly(string embbededLogCfg, string fileName)
		{
            EmbbededInitialAssembly(System.Reflection.Assembly.GetCallingAssembly(), embbededLogCfg, fileName);
		}

        public static void EmbbededInitialAssembly(System.Reflection.Assembly assem, string embbededLogCfg, string fileName)
        {
            if (_embbededInitialized)
            {
                Log.Warn("EmbbededInitial to " + _embbededInitializedTo);
                return;
            }
            using (Stream logConfigStream = IOUtil.GetEmbeddedFile(assem, embbededLogCfg))
            {
                log4net.Config.XmlConfigurator.Configure(logConfigStream);
                string logFileFullPath = GetLogPath();
                CustomizeLogDirectory(logFileFullPath, fileName);
                _embbededInitializedTo = string.Format("{0}/{1}[Runner.Base.util.log4net.config]", logFileFullPath, fileName);
                _embbededInitialized = true;
            }
        }

        public static void EmbbededInitial(string fileName)
        {
            //default
            EmbbededInitial(fileName, Level.All);
        }

        public static void EmbbededInitial(string fileName, Level min)
        {
            EmbbededInitial(fileName, min, Level.Off);
        }

        public static void EmbbededInitial(string fileName, Level min, Level max)
        {
            if (_embbededInitialized)
            {
                Log.Warn("EmbbededInitial to " + _embbededInitializedTo);
                return;
            }
            using (var logConfigStream = IOUtil.GetEmbeddedFile(System.Reflection.Assembly.GetCallingAssembly(), "Runner.Base.Util.log4net.config"))
            {
                log4net.Config.XmlConfigurator.Configure(logConfigStream);
                string logFileFullPath = GetLogPath();
                CustomizeLogDirectory(logFileFullPath, fileName, min, max);
                _embbededInitializedTo = string.Format("{0}/{1}[Runner.Base.Util.log4net.config]", logFileFullPath, fileName);
                _embbededInitialized = true;
            }
        }

        public static void EmbbededInitial()
        {
            var fileName = "default.log";
            EmbbededInitial(fileName);
        }

        public static string GetLogPath()
        {
            return FileLogger.GetLogPath();
        }

        /// <summary>
        /// Initial log4net direct from parameter xmlfile
        /// </summary>
        /// <param name="xmlfile">xml file</param>
        public static void Initial(string xmlfile)
        {
            if (_customInitialized) return;
            if (!string.IsNullOrEmpty(xmlfile))
            {
                if (File.Exists(xmlfile))
                {
                    log4net.Config.XmlConfigurator.Configure(new FileInfo(xmlfile));
                    var logFileFullPath = GetLogPath();
                    CustomizeLogDirectory(logFileFullPath);
                    _customInitialized = true;
                    return;
                }
                var logxmlpath = Environment.CurrentDirectory + xmlfile;
                if (File.Exists(logxmlpath))
                {
                    log4net.Config.XmlConfigurator.Configure(new FileInfo(logxmlpath));
                    var logFileFullPath = GetLogPath();
                    CustomizeLogDirectory(logFileFullPath);
                    _customInitialized = true;
                    return;
                }
            }
            throw new Exception(String.Format("load {0} fault!", xmlfile));
        }

        public static void CustomizeLogDirectory()
        {
            string logPath = GetLogPath();
            CustomizeLogDirectory(logPath);
        }

        public static void CustomizeLogDirectory(string logDir)
        {
            CustomizeLogDirectory(logDir, null);
        }

        private static void CustomizeLogDirectory(string logDir, string customFile)
        {
            CustomizeLogDirectory(logDir, customFile, null, null);
        }

        private static void CustomizeLogDirectory(string logDir, string customFile, Level min, Level max)
        {
            if (!string.IsNullOrEmpty(logDir))
            {
                var repository = LogManager.GetRepository();
                var appenders = repository.GetAppenders();
                foreach (var appender in appenders)
                {
                    FileAppender fileAppender = null;
                    if (appender is RollingFileAppender)
                    {
                        fileAppender = (FileAppender)appender;
                    }
                    else if (appender is FileAppender)
                    {
                        fileAppender = (FileAppender)appender;
                    }
                    if (fileAppender != null)
                    {
                        string file = fileAppender.File;
                        int lastPos = file.LastIndexOf(Path.DirectorySeparatorChar);
                        if (lastPos > 0)
                        {
                            file = file.Substring(lastPos + 1, file.Length - lastPos - 1);
                        }
                        if (file == "default.log" && !string.IsNullOrEmpty(customFile)) //Only default.log change to customFile, keep all the others log file
                        {
                            fileAppender.File = logDir + Path.DirectorySeparatorChar + customFile;
                        }
                        else
                        {
                            fileAppender.File = logDir + Path.DirectorySeparatorChar + file;
                        }
                        if (min != null && max != null)
                        {
                            var levelRangeFilter = new LevelRangeFilter { LevelMax = max, LevelMin = min };
                            levelRangeFilter.ActivateOptions();
                            fileAppender.AddFilter(levelRangeFilter);
                        }
                        //make sure to call fileAppender.ActivateOptions() to notify the logging 
                        //sub system that the configuration for this appender has changed. 
                        fileAppender.ActivateOptions();
                    }
                }
                GlobalContext.Properties["pid"] = Process.GetCurrentProcess().Id;
                return;
            }
            throw new Exception(String.Format("CustomizeLogDirectory {0} fault!", logDir));
        }

        public static void Error(ILog log, Exception ex)
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
    }

    public class ConcurrentMinimalLock : FileAppender.MinimalLock
    {
        private string _filename;
        private bool _append;
        private Stream _stream = null;
        private ConcurrentStream _concurrentStream = null;
        public override void OpenFile(string filename, bool append, Encoding encoding)
        {
            _filename = filename;
            this._append = append;
        }
        public override void CloseFile()
        {
            // NOP
        }
        public override Stream AcquireLock()
        {
            if (_stream == null)
            {
                try
                {
                    using (CurrentAppender.SecurityContext.Impersonate(this))
                    {
                        var directoryFullName = Path.GetDirectoryName(_filename);
                        if (directoryFullName != null && !Directory.Exists(directoryFullName))
                        {
                            Directory.CreateDirectory(directoryFullName);
                        }

                        if (_concurrentStream == null)
                        {
                            _concurrentStream = ConcurrentStream.GetInstance(_filename, _append, FileAccess.Write, FileShare.Read);
                        }
                        _stream = _concurrentStream;
                        _append = true;
                    }
                }
                catch (Exception e1)
                {
                    CurrentAppender.ErrorHandler.Error("Unable to acquire lock on file " + _filename + ". " + e1.Message);
                }
            }
            return _stream;
        }
        public override void ReleaseLock()
        {
            using (CurrentAppender.SecurityContext.Impersonate(this))
            {
                _stream.Close();
                _stream = null;
            }
        }
    }

    public class ConcurrentStream :  Stream
    {
        private string _path;
        private bool _append;
        private FileAccess _access;
        private FileShare _share;
        private readonly QueueManager _queueManager;
        private static ConcurrentStream _instance;

        public static ConcurrentStream GetInstance(string path,
         bool append,
         FileAccess access,
         FileShare share)
        {
            if (_instance != null) return _instance;
            _instance = new ConcurrentStream(path, append, access, share);
            return _instance;
        }
        private ConcurrentStream(
            string path,
            bool append,
            FileAccess access,
            FileShare share
         )
        {
            _path = path;
            _append = append;
            _access = access;
            _share = share;
            _queueManager = QueueManager.GetInstance(path, append, access, share);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            var entry = new CachedEntry(buffer, offset, count);
            _queueManager.Enqueue(entry);
        }


        public override bool CanRead
        {
            get { return false; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return true; }
        }


        public override void Flush()
        {
        }

        public override long Length
        {
            get { return 0L; }
        }

        public override long Position
        {
            get { return 0L; }
            set
            {
                ;
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return 0;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return 0L;
        }

        public override void SetLength(long value)
        { } 
    }

    internal class QueueManager
    {

        private string path;
        private bool append;
        private FileAccess access;
        private FileShare share;

        private Queue syncQueue = Queue.Synchronized(new Queue());

        private bool running = false;
        private Random rnd = new Random();
        private DateTime retryTime = DateTime.MaxValue;

        private static TimeSpan RETRY_MAX_SPAN = TimeSpan.FromMinutes(1);
        private static QueueManager instance;
        private const int MAX_BATCH_SIZE = 100;


        public static QueueManager GetInstance(string path,
                  bool append,
                  FileAccess access,
                  FileShare share)
        {
            if (instance == null)
            {
                instance = new QueueManager(path, append, access, share);
            }
            return instance;
        }
        private QueueManager(
         string path,
         bool append,
         FileAccess access,
         FileShare share)
        {
            this.path = path;
            this.append = append;
            this.access = access;
            this.share = share;
        }
        internal void Enqueue(CachedEntry entry)
        {
            syncQueue.Enqueue(entry);

            if (!running)
            {
                lock (this)
                {
                    running = true;
                    Thread th = new Thread(new ThreadStart(this.Dequeue));
                    th.Start();
                }
            }
        }
        private void Dequeue()
        {
            CachedEntry entry = null;
            try
            {
                using (FileStream fs = new FileStream(path, FileMode.Append, access, share))
                {
                    int processedCount = 0;
                    while (true)
                    {
                        processedCount++;
                        if (syncQueue.Count == 0)
                        {
                            //quit when queue is empty
                            lock (this)
                            {
                                running = false;
                                return;
                            }
                        }
                        else
                        {
                            entry = (CachedEntry)syncQueue.Dequeue();
                        }

                        if (entry != null)
                        {
                            Write(entry, fs);
                        }
                    }
                }
            }
            catch// (IOException ioe)
            {
                if (DateTime.Now - retryTime > RETRY_MAX_SPAN)
                {
                    lock (this)
                    {
                        running = false;
                    }
                    throw;
                }
                //When can't aquire lock
                //Wait random time then retry
                Thread.Sleep(rnd.Next(1000));
                Console.WriteLine("Retry:" + DateTime.Now);
                retryTime = DateTime.Now;
                Dequeue();
            }
        }
        private void Write(CachedEntry entry, FileStream fs)
        {
            fs.Write(entry.Buffer, entry.Offset, entry.Count);
            fs.Flush();
        }
    }

    internal class CachedEntry
    {
        private byte[] buffer;
        private int offset;
        private int count;
        internal byte[] Buffer
        {
            get { return buffer; }
        }
        internal int Offset
        {
            get { return offset; }
        }
        internal int Count
        {
            get { return count; }
        }
        internal CachedEntry(byte[] buffer, int offset, int count)
        {
            this.buffer = new byte[buffer.Length];
            buffer.CopyTo(this.buffer, 0);
            this.offset = offset;
            this.count = count;
        }
    }
}

