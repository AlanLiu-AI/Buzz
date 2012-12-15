using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Runner.Base.Util
{
    public static class EncodingUtil
    {

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(EncodingUtil).Name);

        public static System.Text.Encoding defaultEncoding()
        {
            return System.Text.Encoding.UTF8;
        }
        public static System.Text.Encoding encoding(string charsetName)
        {
            if (charsetName != null && charsetName.Length > 0)
            {
                return System.Text.Encoding.GetEncoding(charsetName);
            }
            return defaultEncoding();
        }

        public static string convertEncoding(string src, string srcEncode, string destEncode)
        {
            if (src == null)
            {
                return null;
            }
            return convertEncoding(src, encoding(srcEncode), encoding(destEncode));
        }
        public static string convertEncoding(string src, System.Text.Encoding srcEncode, System.Text.Encoding destEncode)
        {
            if (src == null)
            {
                return null;
            }
            if (srcEncode == null || destEncode == null)
            {
                return src;
            }
            try
            {
                byte[] srcBytes = srcEncode.GetBytes(src);
                byte[] destBytes = System.Text.Encoding.Convert(destEncode, srcEncode, srcBytes);
                char[] destChars = new char[destEncode.GetCharCount(destBytes, 0, destBytes.Length)];
                destEncode.GetChars(destBytes, 0, destBytes.Length, destChars, 0);
                string destString = new string(destChars);
                return destString;
            }
            catch (Exception ex)
            {
                log.Error("convert encoding failed:", ex);
            }
            return src;
        }
        public static string convertUtf8(string src)
        {
            string ret = src;//convertEncoding(src, System.Text.Encoding.UTF8);
            return ret;
        }
        public static string convertGB(string src)
        {
            //string ret = convertEncoding(src, encoding("GB2312"));
            string ret = src;
            return ret;
        }
        public static string convertEncoding(string src, string encode)
        {
            return convertEncoding(src, encoding(encode));
        }
        public static string convertEncoding(string src, System.Text.Encoding encode)
        {
            if (src == null)
            {
                return null;
            }
            if (encode == null)
            {
                return src;
            }
            System.IO.MemoryStream ms = null;
            System.IO.StreamReader rdr = null;
            System.Text.StringBuilder sb = null;
            System.IO.StringWriter write = null;

            try
            {
                //
                byte[] array = encode.GetBytes(src);
                ms = new System.IO.MemoryStream(array);

                rdr = new System.IO.StreamReader(ms);
                sb = new System.Text.StringBuilder();
                write = new System.IO.StringWriter(sb);
                Char[] readBuff = new Char[1024];//
                int count = 0;
                while ((count = rdr.Read(readBuff, 0, readBuff.Length)) > 0)
                {
                    write.Write(readBuff, 0, count);
                }
                return sb.ToString();
            }
            catch (Exception ex)
            {
                log.Error("convertEncoding error.", ex);
                return null;
            }
            finally
            {
                if (ms != null)
                {
                    try
                    {
                        ms.Close();
                    }
                    catch (Exception e)
                    {
                        log.Warn("ignore ex:" + e.Message);
                    }
                }
                if (rdr != null)
                {
                    try
                    {
                        rdr.Close();
                    }
                    catch (Exception e)
                    {
                        log.Warn("ignore ex::" + e.Message);
                    }
                }
                if (write != null)
                {
                    try
                    {
                        write.Close();
                    }
                    catch (Exception e)
                    {
                        log.Warn("ignore ex::" + e.Message);
                    }
                }
            }
        }
    }
}
