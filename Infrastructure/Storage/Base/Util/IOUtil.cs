using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using System.Threading;

namespace Runner.Base.Util
{
    public static class IOUtil
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(typeof(IOUtil).Name);

        public static string GetFullPath(string file)
        {
            if(File.Exists(file))
            {
                if(Path.IsPathRooted(file))
                    return file;
                var path = Path.GetFullPath(file);
                return path;
            }
            return null;
        }

        public static string GetFullDirectory(string file)
        {
            if (File.Exists(file) || Directory.Exists(file))
            {
                var path = Path.IsPathRooted(file) ? file : Path.GetFullPath(file);
                var dir = Path.GetDirectoryName(path);
                return dir;
            }
            return null;
        }

        public static string[] GetFileFromFolder(string[] pathes, string filter)
        {
            return GetFileFromFolder(pathes, filter, true);
        }

        public static string[] GetFileFromFolder(string[] pathes, string filter, bool inlcudeSubDirs)
        {
            var list = new List<string>();
            foreach(var path in pathes)
            {
                if (Directory.Exists(path))
                {
                    GetFileFromFolder(path, filter, inlcudeSubDirs, ref list);
                }
            }            
            return list.ToArray();
        }

        public static string[] GetFileFromFolder(string path, string filter)
        {
            return GetFileFromFolder(path, filter, true);
        }

        public static string[] GetFileFromFolder(string path, string filter, bool inlcudeSubDirs)
        {
            if (!Directory.Exists(path))
            {
                return null;
            }
            var list = new List<string>();
            GetFileFromFolder(path, filter, inlcudeSubDirs, ref list);
            return list.ToArray();
        }

        public static long GetDirectorySize(string path)
        {
            long totalBytes = 0;
            try
            {
                var dir = new DirectoryInfo(path);
                // Get array of all FileInfo.
                var fileList = dir.GetFiles("*.*", SearchOption.AllDirectories);

                // Calculate total bytes of all files in a loop.
                totalBytes += fileList.Sum(fileInfo => fileInfo.Length);
                // Return total size
                return totalBytes;
            }
            catch (Exception ex)
            {
                Log.Warn(ex.Message);
                throw;
            }
        }

        private static void GetFileFromFolder(string path, string filter, bool inlcudeSubDirs, ref List<string> list)
        {
            if(list==null)
            {
                list = new List<string>();
            }
            try
            {
                var dir = new DirectoryInfo(path);
                var fileList = dir.GetFiles(filter, inlcudeSubDirs ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
                list.AddRange(fileList.Select(fileInfo => fileInfo.FullName));
            }
            catch (Exception ex)
            {
                Log.Warn(ex.Message);
                throw;
            }
        }

        public static int BufferSize = 8192*4;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static bool FileExist(string file)
        {
            return File.Exists(file);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="file"></param>
        public static void FileDelete(string file)
        {
            try
            {
                if (FileExist(file))
                {
                    File.Delete(file);
                }
            }
            catch (Exception ex)
            {
                Log.Error("FileDelete(" + file + ") error.", ex);
            }
        }

        private static readonly Encoding DefaulatEncoding = Encoding.UTF8;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static string ReadFile(string file)
        {
            return ReadFile(file, DefaulatEncoding);
        }

        public static byte[] BinReadFile(string file)
        {
            FileStream files = null;
            try
            {
                files = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                return BinReadStream(files);
            }
            finally
            {
                if (files != null)
                {
                    try
                    {
                        files.Close();
                    }
                    catch (Exception e)
                    {
                        Log.Warn("ignore ex:" + e.Message);
                    }
                }
            }
        }
        public static byte[] BinReadStream(Stream stream)
        {
            var memory = new MemoryStream();
            var readBuff = new byte[BufferSize];
            var count = 0;
            while ((count = stream.Read(readBuff, 0, BufferSize)) > 0)
            {
                memory.Write(readBuff, 0, count);
            }
            return memory.ToArray();
        }

        public static byte[] BinReadStreamL(Stream stream, out int length)
        {
            using(var br = new BinaryReader(stream))
            {
                length = br.ReadInt32();
                var readBuff = br.ReadBytes(length);
                return readBuff;
            }
        }

        public static byte[] BinReadStreamByReader(Stream stream)
        {
            var sr = new StreamReader(stream);
            var memory = new MemoryStream();

            var readBuff = new byte[BufferSize];
            var count = 0;
                
            while ( (count = stream.Read(readBuff, 0, BufferSize)) > 0)
            {
                Log.Debug(count);
                memory.Write(readBuff, 0, count);
            }
            return memory.ToArray();
        }

        public static byte[] BinAsyncReadStream(Stream stream)
        {
            var reader = new AsyncStreamReader(stream);
            var readBuff = reader.Buffer;
            return readBuff;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static string ReadStream(Stream stream)
        {
            return ReadStream(stream, Encoding.Default);
        }
        /// <summary>
        /// 
        /// 
        /// </summary>
        /// <param name="file"></param>
        /// <param name="start"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string ReadFile(string file, int start, int length)
        {
            return ReadFile(file, DefaulatEncoding, start, length);
        }

        public static string ReadFileFromEnd(string file, int kb)
        {
            int kbsize = 1024;//1kb
            int bufferSize = kb * kbsize;
            string buffer = string.Empty;
            //assumption, one line average length is 256 bytes
            using(FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                long fileLen = fs.Length;
                int startPos = 0;
                if(fileLen>bufferSize)
                {
                    startPos = (int)fileLen-bufferSize;
                }
                string line;
                using(StreamReader sr = new StreamReader(fs, Encoding.UTF8))
                {
                    if(startPos>0)
                    {
                        sr.BaseStream.Seek(startPos, SeekOrigin.Begin);
                    }
                    StringBuilder sb = new StringBuilder();
                    StringWriter write = new StringWriter(sb);
                    int i = 0;
                    while( (line = sr.ReadLine())!=null)
                    {
                        i++;
                        if (i != 1)
                        {
                            sb.AppendLine(line);
                        }
                    }
                    return sb.ToString();
                }
            }
        }


        /// <summary>
        /// 
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="start"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string ReadStream(Stream stream, int start, int length)
        {
            return ReadStream(stream, DefaulatEncoding, start, length);
        }

        public static string ReadStream(Stream stream, Encoding encoding)
        {
            StreamReader sr = null;
            try
            {
                sr = new StreamReader(stream, encoding);
                return Read(sr);
            }
            finally
            {
                if (sr != null)
                {
                    try
                    {
                        sr.Close();
                    }
                    catch (Exception e)
                    {
                        Log.Warn("ignore ex:" + e.Message);
                    }
                }
            }
        }

        public static string ReadStream(Stream stream, Encoding encoding, int start, int length)
        {
            StreamReader sr = null;
            try
            {
                sr = new StreamReader(stream, encoding);
                return Read(sr, start, length);
            }
            finally
            {
                if (sr != null)
                {
                    try
                    {
                        sr.Close();
                    }
                    catch (Exception e)
                    {
                        Log.Warn("ignore ex:" + e.Message);
                    }
                }
            }
        }

        public static string ReadFile(string file, Encoding encoding)
        {
            StreamReader files = null;
            try
            {
                files = new StreamReader(file, encoding);
                return Read(files);
            }
            finally
            {
                if (files != null)
                {
                    try
                    {
                        files.Close();
                    }
                    catch (Exception e)
                    {
                        Log.Warn("ignore ex:" + e.Message);
                    }
                }
            }
        }

        public static string ReadFile(string file, Encoding encoding, string firstStartStr, string end)
        {
            StreamReader files = null;
            try
            {
                files = new StreamReader(file, encoding);
                return Read(files);
            }
            finally
            {
                if (files != null)
                {
                    try
                    {
                        files.Close();
                    }
                    catch (Exception e)
                    {
                        Log.Warn("ignore ex:" + e.Message);
                    }
                }
            }
        }

        public static string ReadFile(string file, Encoding encoding, int start, int length)
        {
            StreamReader files = null;
            try
            {
                files = new StreamReader(file, encoding);
                return Read(files, start, length);
            }
            finally
            {
                if (files != null)
                {
                    try
                    {
                        files.Close();
                    }
                    catch (Exception e)
                    {
                        Log.Warn("ignore ex:" + e.Message);
                    }
                }
            }
        }

        public static string Read(StreamReader sr, int start, int length)
        {
            StringWriter write = null;
            try
            {
                sr.BaseStream.Seek(start, SeekOrigin.Begin);
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                write = new StringWriter(sb);
                Char[] readBuff = new Char[length];
                sr.Read(readBuff, 0, length);
                write.Write(readBuff, 0, length);
                return sb.ToString();
            }
            finally
            {
                if (write != null)
                {
                    try
                    {
                        write.Close();
                    }
                    catch (Exception e)
                    {
                        Log.Warn("ignore ex:" + e.Message);
                    }
                }
            }
        }

        public static string Read(StreamReader sr)
        {
            StringWriter write = null;
            try
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                write = new StringWriter(sb);
                Char[] readBuff = new Char[BufferSize];
                int count = 0;
                while ((count = sr.Read(readBuff, 0, BufferSize)) > 0)
                {
                    write.Write(readBuff, 0, count);
                }
                return sb.ToString();
            }
            finally
            {
                if (write != null)
                {
                    try
                    {
                        write.Close();
                    }
                    catch (Exception e)
                    {
                        Log.Warn("ignore ex:" + e.Message);
                    }
                }
            }
        }

        public static void ResetFile(string file)
        {
            if (File.Exists(file))
            {
                File.Delete(file);
            }
            writeFile(file, "open file failed.");
        }

        public static void writeFile(string file, string data)
        {
            WriteFileImpl(file, false, data, Encoding.Default);
        }
        public static void AppendFile(string file, string data)
        {
            WriteFileImpl(file, true, data, Encoding.Default);
        }
        public static void writeFile(string file, byte[] datas)
        {
            WriteFileImpl(file, false, datas, 0, datas.Length);
        }
        public static void writeFile(string file, byte[] datas, int index, int size)
        {
            WriteFileImpl(file, false, datas, index, size);
        }
        public static void AppendFile(string file, byte[] datas)
        {
            WriteFileImpl(file, false, datas, 0, datas.Length);
        }
        public static void AppendFile(string file, byte[] datas, int index, int size)
        {
            WriteFileImpl(file, true, datas, index, size);
        }


        public static void WriteStream(Stream stream, string data, Encoding encoding)
        {
            StreamWriter sw = null;
            try
            {
                if (encoding == null)
                {
                    sw = new StreamWriter(stream);
                }
                else
                {
                    sw = new StreamWriter(stream, encoding);
                }
                write(sw, data);
            }
            finally
            {
                if (sw != null)
                {
                    try
                    {
                        sw.Close();
                    }
                    catch (Exception e)
                    {
                        Log.Warn("ignore ex:" + e.Message);
                    }
                }
            }
        }

        public static void WriteStream(Stream stream, byte[] data)
        {
            StreamWriter sw = null;
            try
            {
                sw = new StreamWriter(stream);
                write(sw, data);
            }
            finally
            {
                if (sw != null)
                {
                    try
                    {
                        sw.Close();
                    }
                    catch (Exception e)
                    {
                        Log.Warn("ignore ex:" + e.Message);
                    }
                }
            }
        }

        public static void WriteStreamL(Stream stream, byte[] data)
        {
            using(BinaryWriter bw = new BinaryWriter(stream))
            {
                bw.Write(data.Length);
                bw.Write(data);
            }
        }

        public static StreamWriter GetWriteStream(string file, bool append)
        {
            return GetWriteStream(file, append, null);
        }

        public static StreamWriter GetWriteStream(string file, bool append, System.Text.Encoding enc)
        {
            StreamWriter files = null;
            try
            {
                if (enc != null)
                {
                    files = new StreamWriter(file, append, enc);
                }
                else
                {
                    files = new StreamWriter(file, append);
                }
                return files;
            }catch(Exception)
            {
                if (files != null)
                {
                    try
                    {
                        files.Close();
                    }
                    catch (System.Exception)
                    {
                    	
                    }
                    
                }
                return null;
            }
        }

        private static void WriteFileImpl(string file, bool append, string data, System.Text.Encoding enc)
        {
            //Debug(String.Format("writeFile({0},{1},{2},{3}) called",file, append, data, charsetName));
            StreamWriter files = null;
            try
            {
                if (enc != null)
                {
                    files = new StreamWriter(file, append, enc);
                }
                else
                {
                    files = new StreamWriter(file, append);
                }
                write(files, data);
            }
            finally
            {
                if (files != null)
                {
                    try
                    {
                        files.Close();
                    }
                    catch (Exception e)
                    {
                        Log.Warn("ignore ex:" + e.Message);
                    }
                }
            }
        }

        private static void WriteFileImpl(string filename, bool append, byte[] datas, int index, int size)
        {
            //Debug(String.Format("writeFile({0},{1},{2},{3}) called",file, append, data, charsetName));

            FileStream file = null;
            BinaryWriter files = null;
            try
            {
                if (append)
                {
                    file = new FileStream(filename, FileMode.Append);
                }
                else
                {
                    file = new FileStream(filename, FileMode.Create);
                }
                files = new BinaryWriter(file);
                files.Write(datas, index, size);
            }
            finally
            {
                if (files != null)
                {
                    try
                    {
                        files.Close();
                    }
                    catch (Exception e)
                    {
                        Log.Warn("ignore ex:" + e.Message);
                    }
                }
                if (file != null)
                {
                    try
                    {
                        file.Close();
                    }
                    catch (Exception e)
                    {
                        Log.Warn("ignore ex:" + e.Message);
                    }
                }
            }
        }

        public static void writeFile(string file, string data, System.Text.Encoding enc)
        {
            WriteFileImpl(file, false, data, enc);
        }

        public static void AppendFile(string file, string data, System.Text.Encoding enc)
        {
            WriteFileImpl(file, true, data, enc);
        }

#if !COMPACTUTIL
        private static void writeFile(string file, bool append, string data, string charsetName)
        {
            if (charsetName == null || charsetName.Length < 1)
            {
                WriteFileImpl(file, append, data, null);
            }
            else
            {
                WriteFileImpl(file, append, data, EncodingUtil.encoding(charsetName));
            }
        }
        public static void writeFile(string file, string data, string charsetName)
        {
            writeFile(file, false, data, charsetName);
        }

        public static void AppendFile(string file, string data, string charsetName)
        {
            writeFile(file, true, data, charsetName);
        }
#endif

        public static void write(StreamWriter sw, string data)
        {
            sw.Write(data);
        }

        public static void write(StreamWriter sw, byte[]  data)
        {
            sw.Write(data);
        }
        public static void write(StreamWriter sw, long data)
        {
            sw.Write(data);
        }
        public static void write(StreamWriter sw, double data)
        {
            sw.Write(data);
        }
        public static void write(StreamWriter sw, int data)
        {
            sw.Write(data);
        }
        public static void write(StreamWriter sw, short data)
        {
            sw.Write(data);
        }

        public static Stream string2Stream(string content)
        {
            if (content == null) return null;
            byte[] array = System.Text.Encoding.Default.GetBytes(content);
            MemoryStream ms = new MemoryStream(array);
            return ms;
        }

        public static Stream string2Stream(string content, Encoding encoding)
        {
            if (content == null) return null;
            byte[] array = encoding.GetBytes(content);
            MemoryStream ms = new MemoryStream(array);
            return ms;
        }

        public static void Stream2File(Stream ins, Encoding encoding, string outfile)
        {
            string content = ReadStream(ins, encoding);
            writeFile(outfile, content, encoding);
        }

        public static void WriteFile(byte[] buffer, string outfile)
        {
            MemoryStream stream = new MemoryStream(buffer);
            Stream2File(stream, outfile);
        }

        /// <summary>
        /// Extracts an embedded file out of a given assembly.
        /// </summary>
        /// <param name="assemblyName">The name of you assembly.</param>
        /// <param name="ns">The namespace for the file.</param>
        /// <param name="fileName">The name of the file to extract.</param>
        /// <returns>A stream containing the file data.</returns>
        public static Stream GetEmbeddedFile(string assemblyName, string ns, string fileName)
        {
            System.Reflection.Assembly assembly = ClassUtil.LoadAssambly(assemblyName);
            string fullName = ns + "." + fileName;
            return GetEmbeddedFile(assembly, fullName);
        }
        public static Stream GetEmbeddedFile(string fullName)
        {
            Stream ret = null;
            System.Reflection.Assembly assembly = Assembly.GetEntryAssembly();
            ret = GetEmbeddedFile(assembly, fullName);
            if (ret != null) return ret;
            Assembly[] loadedAssemblies = Thread.GetDomain().GetAssemblies();
            foreach (Assembly loadedAssembly in loadedAssemblies)
            {
                ret = GetEmbeddedFile(loadedAssembly, fullName);
                if (ret != null) return ret;
            }
            return null;
        }

        public static Stream GetEmbeddedFile(System.Reflection.Assembly assembly, string fullName)
        {
            if (assembly == null) return null;
            try
            {
                Stream str = assembly.GetManifestResourceStream(fullName);
                if (str == null)
                {
                    string[] names = assembly.GetManifestResourceNames();
                    if (names != null)
                    {
                        string targetName = fullName;
                        targetName = targetName.ToLower();
                        foreach (string name in names)
                        {
                            if (name.ToLower() == targetName)
                            {
                                str = assembly.GetManifestResourceStream(name);
                                if (str != null)
                                {
                                    Log.WarnFormat("It is better use name '{0}' to load embedded resource.", name);
                                }
                                break;
                            }
                        }
                    }
                }
                return str;
            }
            catch (Exception e)
            {
                throw new Exception(assembly.FullName + ": " + e.Message);
            }
        }

        /// <summary>
        /// Extracts an embedded file out of a given assembly.
        /// </summary>
        /// <param name="assemblyName">The namespace of you assembly.</param>
        /// <param name="fileName">The name of the file to extract.</param>
        /// <returns>A stream containing the file data.</returns>
        public static string GetEmbeddedFile2String(string assemblyName, string ns, string fileName)
        {
            Stream stream = GetEmbeddedFile(assemblyName, ns, fileName);
            return ReadStream(stream);
        }

        public static string GetEmbeddedFile2String(string assemblyName, string filename)
        {
            Assembly[] loadedAssemblies = Thread.GetDomain().GetAssemblies();
            foreach (Assembly loadedAssembly in loadedAssemblies)
            {
                string name = loadedAssembly.GetName().Name;
                if (string.Compare(name, assemblyName, true) == 0)
                {
                    Stream str = loadedAssembly.GetManifestResourceStream(filename);
                    return ReadStream(str);
                }
            }
            return null;
        }

        public static string GetEmbeddedFile2String(Assembly assembly, string filename)
        {
            Stream str = assembly.GetManifestResourceStream(filename);
            return ReadStream(str);
        }

        public static string GetEmbeddedFile2String(string filename)
        {
            Stream str = GetEmbeddedFile(filename);
            return ReadStream(str);
        }

        /// <summary>
        /// Extracts an embedded file out of a given assembly.
        /// </summary>
        /// <param name="assemblyName">The namespace of you assembly.</param>
        /// <param name="fileName">The name of the file to extract.</param>
        /// <returns>A stream containing the file data.</returns>
        public static byte[] GetEmbeddedFile2Buffer(string assemblyName, string ns, string fileName)
        {
            Stream stream = GetEmbeddedFile(assemblyName, ns, fileName);
            return BinReadStream(stream);
        }

        public static byte[] GetEmbeddedFile2Buffer(Assembly assembly, string filename)
        {
            Stream stream = assembly.GetManifestResourceStream(filename);
            return BinReadStream(stream);
        }

        public static byte[] GetEmbeddedFile2Buffer(string filename)
        {
            Stream stream = GetEmbeddedFile(filename);
            if (stream != null) return BinReadStream(stream);
            return null;
        }

        public static void Stream2File(Stream ins, String outfile)
        {
            byte[] buffer = new byte[BufferSize];
            int count = 0;
            int readtimes = 0;
            FileStream file = null;
            BinaryWriter files = null;
            try
            {
                while ((count = ins.Read(buffer, 0, BufferSize)) > 0)
                {
                    readtimes++;
                    //if (log.IsDebugEnabled)log.DebugFormat("Stream2File readtimes{0} count{1}.", readtimes, count);
                    if (count > 0)
                    {
                        if (readtimes == 1)//initialize
                        {
                            file = new FileStream(outfile, FileMode.Create);
                            files = new BinaryWriter(file);
                        }
                        else
                        {
                            //file = new FileStream(filename, FileMode.Append);
                            //files = new BinaryWriter(file);
                        }
                        files.Write(buffer, 0, count);
                    }
                }
            }
            finally
            {
                if (files != null)
                {
                    try
                    {
                        files.Close();
                    }
                    catch (Exception e)
                    {
                        Log.Warn("ignore ex:" + e.Message);
                    }
                }
                if (file != null)
                {
                    try
                    {
                        file.Close();
                    }
                    catch (Exception e)
                    {
                        Log.Warn("ignore ex:" + e.Message);
                    }
                }
            }

        }

        public static string AbsoluteToRelativePath(string absolutePath)
        {
            if(Path.IsPathRooted(absolutePath))
            {
                return AbsoluteToRelativePath(Environment.CurrentDirectory, absolutePath);
            }
            return absolutePath;
        }

        /// <summary>
        /// Converts an absolute path to one relative to the given base directory path
        /// </summary>
        /// <param name="basePath">The base directory path</param>
        /// <param name="absolutePath">An absolute path</param>
        /// <returns>A path to the given absolute path, relative to the base path</returns>
        public static string AbsoluteToRelativePath(string basePath, string absolutePath)
        {
            char[] separators = {
                                                                        Path.DirectorySeparatorChar,
                                                                        Path.AltDirectorySeparatorChar,
                                                                        Path.VolumeSeparatorChar
                                                                };

            //split the paths into their component parts
            string[] basePathParts = basePath.Split(separators);
            string[] absPathParts = absolutePath.Split(separators);
            int indx = 0;

            //work out how much they have in common
            int minLength = Math.Min(basePathParts.Length, absPathParts.Length);
            for (; indx < minLength; ++indx)
            {
                if (String.Compare(basePathParts[indx], absPathParts[indx], true) != 0)
                    break;
            }

            //if they have nothing in common, just return the absolute path
            if (indx == 0)
            {
                return absolutePath;
            }


            //start constructing the relative path
            string relPath = "";

            if (indx == basePathParts.Length)
            {
                // the entire base path is in the abs path
                // so the rel path starts with "./"
                relPath += "." + Path.DirectorySeparatorChar;
            }
            else
            {
                //step up from the base to the common root
                for (int i = indx; i < basePathParts.Length; ++i)
                {
                    relPath += ".." + Path.DirectorySeparatorChar;
                }
            }
            //add the path from the common root to the absPath
            relPath += String.Join(Path.DirectorySeparatorChar.ToString(), absPathParts, indx, absPathParts.Length - indx);

            return relPath;
        }

        
        public static string RelativeToAbsolutePath(string absolutePath)
        {
            if (Path.IsPathRooted(absolutePath))
            {
                return absolutePath;
            }
            return RelativeToAbsolutePath(Environment.CurrentDirectory, absolutePath);
        }

        /// <summary>
        /// Converts a given base and relative path to an absolute path
        /// </summary>
        /// <param name="basePath">The base directory path</param>
        /// <param name="relativePath">A path to the base directory path</param>
        /// <returns>An absolute path</returns>
        public static string RelativeToAbsolutePath(string basePath, string relativePath)
        {
            //if the relativePath isn't...
            if (Path.IsPathRooted(relativePath))
            {
                return relativePath;
            }

            //split the paths into their component parts
            string[] basePathParts = basePath.Split(Path.DirectorySeparatorChar);
            string[] relPathParts = relativePath.Split(Path.DirectorySeparatorChar);

            //determine how many we must go up from the base path
            int indx = 0;
            for (; indx < relPathParts.Length; ++indx)
            {
                if (!relPathParts[indx].Equals(".."))
                {
                    break;
                }
            }

            //if the rel path contains no ".." it is below the base
            //therefor just concatonate the rel path to the base
            if (indx == 0)
            {
                int offset = 0;
                //ingnore the first part, if it is a rooting "."
                if (relPathParts[0] == ".") offset = 1;

                return basePath + Path.DirectorySeparatorChar + String.Join(Path.DirectorySeparatorChar.ToString(), relPathParts, offset, relPathParts.Length - offset);
            }

            string absPath = String.Join(Path.DirectorySeparatorChar.ToString(), basePathParts, 0, Math.Max(0, basePathParts.Length - indx));

            absPath += Path.DirectorySeparatorChar + String.Join(Path.DirectorySeparatorChar.ToString(), relPathParts, indx, relPathParts.Length - indx);

            return absPath;
        }

        public static Object SafeLoggerSync = new Object();
        public static void SafeAppendFile(string filename, string str)
        {
            lock (SafeLoggerSync)
            {
                using (FileStream fs = new FileStream(filename, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                {
                    using (TextWriter tw = new StreamWriter(fs, Encoding.UTF8))
                    {
                        tw.WriteLine(str);
                    }
                }
            }
        }
    }

    class AsyncStreamReader
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(AsyncStreamReader).Name);

        public AsyncStreamReader(Stream s)
        {
            Buffer = new byte[IOUtil.BufferSize];
            Length = 0;
            s.BeginRead(Buffer, 0, Buffer.Length, ReadAsyncCallback, s);
        }

        public byte[] Buffer { get; private set; }

        public int Length { get; private set; }

        private void ReadAsyncCallback(IAsyncResult ar)
        {
            try
            {
                Stream media = ar.AsyncState as Stream;

                int read = media.EndRead(ar);
                if (read > 0)
                {
                    Length += read;                    
                }
            }
            catch (Exception exc)
            {
                log.Warn(exc);
            }
        }

    }
}
