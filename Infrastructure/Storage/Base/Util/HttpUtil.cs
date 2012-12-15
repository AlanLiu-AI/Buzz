using System;
using System.Net;
using System.Threading;
using System.Text;
using System.IO;

using System.Web;
using System.ServiceModel.Web;
using System.ServiceModel.Channels;
using System.ServiceModel;
using System.Web.SessionState;

namespace Runner.Base.Util
{
    public static class HttpUtil
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(typeof(HttpUtil).Name);

        public static ManualResetEvent AllDone = new ManualResetEvent(false);
        const int BufferSize = 1024;

        public static string GetRequestUrl()
        {
            var url = string.Empty;
            if (HttpContext.Current != null)
            {
                url = HttpContext.Current.Request.Url.AbsoluteUri;
            }
            else if (WebOperationContext.Current != null)
            {
                url = WebOperationContext.Current.IncomingRequest.UriTemplateMatch.RequestUri.AbsoluteUri;
            }
            return url;
        }

        public static bool IsUri(string uri)
        {
            try
            {
                new Uri(uri);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static string GetHttpServiceClientIP()
        {
            string ClientIP = string.Empty;
            if (HttpContext.Current != null)
            {
                HttpContext context = HttpContext.Current;
                if (context.Request.ServerVariables["HTTP_VIA"] != null)
                {
                    ClientIP = context.Request.ServerVariables["HTTP_X_FORWARDED_FOR"].ToString();
                }
                else
                {
                    ClientIP = context.Request.ServerVariables["REMOTE_ADDR"].ToString();
                }
            }
            else if (WebOperationContext.Current != null)
            {
                //string userAgent = WebOperationContext.Current.IncomingRequest.UserAgent;
                if(OperationContext.Current!=null)
                {
                    OperationContext context = OperationContext.Current;
                    MessageProperties messageProperties = context.IncomingMessageProperties;
                    RemoteEndpointMessageProperty endpointProperty = messageProperties[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty;
                    return endpointProperty.Address;
                }
            }
            return ClientIP;
        }

        public static string GetUrlHost(string url)
        {
            if (url == null) return null;
            int pos = url.IndexOf("://");
            if (pos > 0)
            {
                if (url.StartsWith("file:"))//
                {
                    return "localhost";
                }
                string left = url.Substring(pos + 3, url.Length - pos - 3);
                pos = left.IndexOf("/");
                if (pos > 0)
                {
                    left = left.Substring(0, pos);
                }
                //
                left = left.Replace(":", "_");
                return left;
            }
            return null;
        }

        public static string GetUrlPage(string url)
        {
            if (url == null) return null;
            int pos = url.LastIndexOf("/");
            if (pos > 0)
            {
                string left = url.Substring(pos + 1, url.Length - pos - 1);
                pos = left.IndexOf("?");
                if (pos > 0)
                {
                    left = left.Substring(0, pos);
                    return left;
                }
                else
                {
                    return left;
                }
            }
            return null;
        }

        public static string GetUrlPagePath(string url)
        {
            if (url == null) return null;
            int pos = url.IndexOf("//");
            if (pos > 0)
            {
                string left = url.Substring(pos + 2, url.Length - pos - 2);
                pos = left.IndexOf("/");
                if (pos > 0)
                {
                    left = left.Substring(pos + 1, left.Length - pos - 1);
                    if (left.Length > 0)
                    {
                        left = left.Replace("/", "_");
                        pos = left.IndexOf("?");
                        if (pos > 0)
                        {
                            left = left.Substring(0, pos);
                            return left;
                        }
                        else
                        {
                            return left;
                        }
                    }
                }
            }
            return null;
        }

        private static string XSLT_SEP = "@SEPERATE@";
        public static string xsltCall(string url, string xmlfile, string xsltfile, string charsetName)
        {
            try
            {
                Encoding encoding = EncodingUtil.encoding(charsetName);
                string xml = System.IO.File.ReadAllText(xmlfile, encoding);
                string xslt = System.IO.File.ReadAllText(xsltfile, encoding);
                xml = xml.Replace("encoding=\"gb2312\"", "encoding=\"GBK\"");
                xml = xml.Replace("encoding=\"GB2312\"", "encoding=\"GBK\"");
                xslt = xslt.Replace("encoding=\"gb2312\"", "encoding=\"GBK\"");
                xslt = xslt.Replace("encoding=\"GB2312\"", "encoding=\"GBK\"");
                string data = xml + XSLT_SEP + xslt;
                return HttpPost(url, data, charsetName);
                //return data;
            }
            catch (Exception e)
            {
                Log.Error(" xsltcall('" + url + "','" + xmlfile + "','" + xsltfile + "') error:" + e.Message);
                throw e;
            }
        }

        public static string HttpPost(string url, string data, string charsetName)
        {
            return HttpPost(url, data, 0, charsetName);
        }

        public static string HttpPost(string url, string data, int timeout, string charsetName)
        {
            Encoding encoding = EncodingUtil.encoding(charsetName);
            byte[] postArray = encoding.GetBytes(data);
            HttpWebRequest request =
                (HttpWebRequest)WebRequest.Create(url);
            request.ContentType = @"application/x-www-form-urlencoded";
            request.Method = "POST";
            request.ContentLength = postArray.Length;
            Stream requests = null;
            Stream responses = null;
            try
            {
                if (timeout > 0)
                {
                    request.Timeout = timeout;
                    request.ReadWriteTimeout = timeout;
                }
                requests = request.GetRequestStream();
                requests.Write(postArray, 0, postArray.Length);
                WebResponse response = request.GetResponse();
                responses = response.GetResponseStream();
                string ret = IOUtil.ReadStream(responses, encoding);
                return ret;
            }
            catch (Exception e)
            {
                Log.Error(e);
                return null;
            }
            finally
            {
                if (requests != null)
                {
                    try
                    {
                        requests.Close();
                    }
                    catch (Exception e)
                    {
                        Log.Warn("Ignored error:" + e.Message);
                    }
                }
                if (responses != null)
                {
                    try
                    {
                        responses.Close();
                    }
                    catch (Exception e)
                    {
                        Log.Warn("Ignored error:" + e.Message);
                    }
                }
            }
        }

        public static byte[] HttpPost(string url, byte[] data)
        {
            return HttpPost(url, data, 0);
        }

        public static byte[] HttpPost(string url, byte[] data, int timeout)
        {
            HttpWebRequest request =
                (HttpWebRequest)WebRequest.Create(url);
            request.ContentType = @"application/x-www-form-urlencoded";
            request.Method = "POST";
            request.ContentLength = data.Length;
            Stream requests = null;
            Stream responses = null;
            try
            {
                if (timeout > 0)
                {
                    request.Timeout = timeout;
                    request.ReadWriteTimeout = timeout;
                }
                requests = request.GetRequestStream();
                requests.Write(data, 0, data.Length);
                WebResponse response = request.GetResponse();
                responses = response.GetResponseStream();
                byte[] ret = IOUtil.BinReadStream(responses);
                return ret;
            }
            catch (Exception e)
            {
                Log.Error(e);
                return null;
            }
            finally
            {
                if (requests != null)
                {
                    try
                    {
                        requests.Close();
                    }
                    catch (Exception e)
                    {
                        Log.Warn("Ignored error:" + e.Message);
                    }
                }
                if (responses != null)
                {
                    try
                    {
                        responses.Close();
                    }
                    catch (Exception e)
                    {
                        Log.Warn("Ignored error:" + e.Message);
                    }
                }
            }
        }

        public static string HttpGet(string url, Encoding encoding)
        {
            return HttpGet(url, 0, encoding);
        }

        /// <summary>
        ///  Return character set
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string HttpGet(string url, int timeout, Encoding encoding)
        {
            HttpWebRequest request = null;
            WebResponse response = null;
            Stream requests = null;
            Stream responses = null;
            try
            {
                request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "GET";
                if (timeout > 0)
                {
                    request.Timeout = timeout;
                    request.ReadWriteTimeout = timeout;
                }
                response = request.GetResponse();
                //requests = request.GetRequestStream();
                responses = response.GetResponseStream();
                string ret = IOUtil.ReadStream(responses, encoding);
                return ret;
            }
            catch (Exception e)
            {
                Log.Error(e);
                return null;
            }
            finally
            {
                if (requests != null)
                {
                    try
                    {
                        requests.Close();
                    }
                    catch (Exception e)
                    {
                        Log.Warn("Ignored error:" + e.Message);
                    }
                }
                if (responses != null)
                {
                    try
                    {
                        responses.Close();
                    }
                    catch (Exception e)
                    {
                        Log.Warn("Ignored error:" + e.Message);
                    }
                }
            }
        }

        public static byte[] HttpGet(string url)
        {
            return HttpGet(url, 0);
        }

        /// <summary>
        ///  Return character set
        /// </summary>
        /// <param name="url"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public static byte[] HttpGet(string url, int timeout)
        {
            HttpWebRequest request = null;
            WebResponse response = null;
            Stream requests = null;
            Stream responses = null;
            try
            {
                request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "GET";
                if (timeout > 0)
                {
                    request.Timeout = timeout;
                    request.ReadWriteTimeout = timeout;
                }
                response = request.GetResponse();
                //requests = request.GetRequestStream();
                responses = response.GetResponseStream();
                byte[] ret = IOUtil.BinReadStream(responses);
                return ret;
            }
            catch (Exception e)
            {
                Log.Error(e);
                return null;
            }
            finally
            {
                if (requests != null)
                {
                    try
                    {
                        requests.Close();
                    }
                    catch (Exception e)
                    {
                        Log.Warn("Ignored error:" + e.Message);
                    }
                }
                if (responses != null)
                {
                    try
                    {
                        responses.Close();
                    }
                    catch (Exception e)
                    {
                        Log.Warn("Ignored error:" + e.Message);
                    }
                }
            }
        }

        /// <summary>
        /// Return character set
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string downloadUrl(string url, string filepath, string encoding)
        {
            if (Log.IsDebugEnabled)
            {
                Log.DebugFormat("downloadUrl('{0}', '{1}', '{2}') called.", url, filepath, encoding);
            }
            //default character set
            string charset = encoding == null ? "gb2312" : encoding;
            HttpWebRequest request = null;
            WebResponse response = null;
            Stream requests = null;
            Stream responses = null;
            try
            {
                request = (HttpWebRequest)WebRequest.Create(url);
                response = request.GetResponse();
                requests = request.GetRequestStream();
                responses = response.GetResponseStream();
                IOUtil.Stream2File(responses, filepath);
                return GetHtmlEncoding(filepath, charset);
            }
            catch (Exception e)
            {
                Log.Error(e);
                return null;
            }
            finally
            {
                if (requests != null)
                {
                    try
                    {
                        requests.Close();
                    }
                    catch (Exception e)
                    {
                        Log.Warn("ignore ex:" + e.Message);
                    }
                }
                if (responses != null)
                {
                    try
                    {
                        responses.Close();
                    }
                    catch (Exception e)
                    {
                        Log.Warn("ignore ex:" + e.Message);
                    }
                }
            }

        }

        public static void DumpVariables()
        {
            HttpContext context = HttpContext.Current;
            if (context != null)
            {
                HttpRequest Request = context.Request;
                HttpResponse Response = context.Response;
                HttpSessionState session = context.Session;
                if(Request!=null)
                {
                    try
                    {
                        if (Request.ServerVariables != null)
                        {
                            foreach (string name in Request.ServerVariables)
                            {
                                string val = Request.ServerVariables[name];
                                Log.DebugFormat("Request.ServerVariables[{0}] = {1}.", name, val);
                            }
                        }
                    }
                    catch (System.Exception ex)
                    {
                        Log.Warn(ex.Message);
                    }                    
                    try
                    {
                        if (Request.Headers != null)
                        {
                            foreach (string name in Request.Headers)
                            {
                                string val = Request.Headers[name];
                                Log.DebugFormat("Request.Headers[{0}] = {1}.", name, val);
                            }
                        }
                    }
                    catch (System.Exception ex)
                    {
                        Log.Warn(ex.Message);
                    }                    
                    try
                    {
                        if (Request.Cookies != null)
                        {
                            foreach (string name in Request.Cookies)
                            {
                                HttpCookie cookie = Request.Cookies[name];
                                Log.DebugFormat("Request.Cookie[{0}] = {{Name={1},Path={2},Secure={3},Expires={4},Value={5}}}.",
                                    name, cookie.Name, cookie.Path, cookie.Secure, cookie.Expires, cookie.Value);
                            }
                        }
                    }
                    catch (System.Exception ex)
                    {
                        Log.Warn(ex.Message);
                    }
                }
                if(Response!=null)
                {
                    try
                    {
                        if (Response.Headers != null)
                        {
                            foreach (string name in Response.Headers)
                            {
                                string val = Response.Headers[name];
                                Log.DebugFormat("Response.Headers[{0}] = {1}.", name, val);
                            }
                        }
                    }
                    catch (System.Exception ex)
                    {
                        Log.Warn(ex.Message);
                    }
                    try
                    {
                        if (Response.Cookies != null)
                        {
                            foreach (string name in Response.Cookies)
                            {
                                HttpCookie cookie = Response.Cookies[name];
                                Log.DebugFormat("Response.Cookie[{0}] = {{Name={1},Path={2},Secure={3},Expires={4},Value={5}}}.",
                                    name, cookie.Name, cookie.Path, cookie.Secure, cookie.Expires, cookie.Value);
                            }
                        }
                    }
                    catch (System.Exception ex)
                    {
                        Log.Warn(ex.Message);
                    }
                }
                if(session!=null)
                {
                    try
                    {
                        if (session.Keys!=null)
                        {
                            foreach (string key in session.Keys)
                            {
                                object val = session[key];
                                Log.DebugFormat("Session[{0}] = {1}.", key, val.ToString());
                            }
                        }
                    }
                    catch (System.Exception ex)
                    {
                        Log.Warn(ex.Message);
                    }
                }
            }
        }

        public static string GetHtmlEncoding(string file, string defaultEncoding)
        {
            string htmlcontent = System.IO.File.ReadAllText(file, Encoding.ASCII);
            int start = htmlcontent.IndexOf("</head");
            if (start == -1)
            {
                start = htmlcontent.IndexOf("</Head");
            }
            if (start == -1)
            {
                start = htmlcontent.IndexOf("</HEAD");
            }
            if (start > 0)
            {
                htmlcontent = htmlcontent.Substring(0, start);
            }
            else
            {
                return defaultEncoding;
            }
            start = htmlcontent.IndexOf("<head");
            if (start == -1)
            {
                start = htmlcontent.IndexOf("<Head");
            }
            if (start == -1)
            {
                start = htmlcontent.IndexOf("<HEAD");
            }
            if (start > 0)
            {
                htmlcontent = htmlcontent.Substring(start, htmlcontent.Length - start);
            }
            else
            {
                return defaultEncoding;
            }
            htmlcontent.ToLower();
            start = htmlcontent.IndexOf("charset=");
            if (start > 0)
            {
                htmlcontent = htmlcontent.Substring(start, htmlcontent.Length - start);
            }
            else
            {
                return defaultEncoding;
            }
            start = htmlcontent.IndexOf("\"");
            if (start > 0)
            {
                htmlcontent = htmlcontent.Substring(0, start);
            }
            else
            {
                return defaultEncoding;
            }
            return htmlcontent;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        public static void DownloadUrl(string url, string filepath)
        {
            if (Log.IsDebugEnabled)
            {
                Log.DebugFormat("downloadUrl('{0}', '{1}') called.", url, filepath);
            }
            HttpWebRequest request = null;
            WebResponse response = null;
            Stream requests = null;
            Stream responses = null;
            try
            {
                request = (HttpWebRequest)WebRequest.Create(url);
                response = request.GetResponse();
                requests = request.GetRequestStream();
                responses = response.GetResponseStream();
                IOUtil.Stream2File(responses, filepath);
            }
            catch (Exception e)
            {
                Log.Error(e);
                throw e;
            }
            finally
            {
                if (requests != null)
                {
                    try
                    {
                        requests.Close();
                    }
                    catch (Exception e)
                    {
                        Log.Warn("ignore ex:" + e.Message);
                    }
                }
                if (responses != null)
                {
                    try
                    {
                        responses.Close();
                    }
                    catch (Exception e)
                    {
                        Log.Warn("ignore ex:" + e.Message);
                    }
                }
            }

        }

        public static string PostAsyc(string url, string data)
        {
            WebRequest wreq = null;
            try
            {
                // Create the request object.
                wreq = WebRequest.Create(url);

                // Set the 'Method' property to 'POST' to post data to the Uri.
                wreq.Method = "POST";
                wreq.ContentType = "application/x-www-form-urlencoded";

                // Create the state object.
                RequestState rs = new RequestState();

                // Put the request into the state object so it can be passed around.
                rs.Request = wreq;

                // Issue the async request.
                IAsyncResult r = (IAsyncResult)wreq.BeginGetResponse(
                    new AsyncCallback(RespCallback), rs);

                // Wait until the ManualResetEvent is set so that the application 
                // does not exit until after the callback is called.
                AllDone.WaitOne();
                return rs.RequestData.ToString();

                //sendStream = httpClient.GetRequestStream();
                //sendStream.Write(postArray, 0, postArray.Length);

            }
            catch (Exception e)
            {
                Log.Error("open " + url + " failed:", e);
                throw e;
            }
            finally
            {
                // Close the Stream Object.
                /*if(readStream!=null)
                {
                    try
                    {
                        readStream.Close();
                    }
                    catch(Exception e)
                    {
                        Warn("Ignored error:"+e.Message);
                    }
                }*/
            }
        }

        private static void RespCallback(IAsyncResult ar)
        {
            // Get the RequestState object from the async result.
            RequestState rs = (RequestState)ar.AsyncState;

            // Get the WebRequest from RequestState.
            WebRequest req = rs.Request;

            // Call EndGetResponse, which produces the WebResponse object
            //  that came from the request issued above.
            WebResponse resp = req.EndGetResponse(ar);

            //  Start reading data from the response stream.
            Stream ResponseStream = resp.GetResponseStream();

            // Store the response stream in RequestState to read 
            // the stream asynchronously.
            rs.ResponseStream = ResponseStream;

            //  Pass rs.BufferRead to BeginRead. Read data into rs.BufferRead
            IAsyncResult iarRead = ResponseStream.BeginRead(rs.BufferRead, 0,
                BufferSize, new AsyncCallback(ReadCallBack), rs);
        }


        private static void ReadCallBack(IAsyncResult asyncResult)
        {
            // Get the RequestState object from AsyncResult.
            RequestState rs = (RequestState)asyncResult.AsyncState;

            // Retrieve the ResponseStream that was set in RespCallback. 
            Stream responseStream = rs.ResponseStream;

            // Read rs.BufferRead to verify that it contains data. 
            int read = responseStream.EndRead(asyncResult);
            if (read > 0)
            {
                // Prepare a Char array buffer for converting to Unicode.
                Char[] charBuffer = new Char[BufferSize];

                // Convert byte stream to Char array and then to String.
                // len contains the number of characters converted to Unicode.
                int len =
                    rs.StreamDecode.GetChars(rs.BufferRead, 0, read, charBuffer, 0);

                String str = new String(charBuffer, 0, len);

                // Append the recently read data to the RequestData stringbuilder
                // object contained in RequestState.
                rs.RequestData.Append(
                    Encoding.ASCII.GetString(rs.BufferRead, 0, read));

                // Continue reading data until 
                // responseStream.EndRead returns –1.
                IAsyncResult ar = responseStream.BeginRead(
                    rs.BufferRead, 0, BufferSize,
                    new AsyncCallback(ReadCallBack), rs);
            }
            else
            {
                if (rs.RequestData.Length > 0)
                {
                    //  Display data to the console.
                    string strContent;
                    strContent = rs.RequestData.ToString();
                }
                // Close down the response stream.
                responseStream.Close();
                // Set the ManualResetEvent so the main thread can exit.
                AllDone.Set();
            }
            return;
        }

        public static string GetHttpProperty(string key)
        {
            #region HttpContext part
            if (HttpContext.Current != null)
            {
                HttpRequest request = HttpContext.Current.Request;
                HttpResponse response = HttpContext.Current.Response;
                HttpSessionState session = HttpContext.Current.Session;
                string method = request.Path;
                string value = string.Empty;
                if (string.IsNullOrEmpty(method)) 
                {
                    //Does not have request pathInfo, it is and WCF soap call
                    return string.Empty;
                }

                if (String.IsNullOrEmpty(value)&&session!=null)
                {
                    //Add <sessionState mode="InProc" cookieless="false" timeout="30"/> into web.config, we can get a same sessionid, mostly equaling to our authToken
                    value = session[key] as string;
                    if (!string.IsNullOrEmpty(value))
                    {
                        return value;
                    }
                }
                if (String.IsNullOrEmpty(value) && request != null)
                {
                    if (request.QueryString!=null)
                    {
                        value = request.QueryString[key];
                        if (!string.IsNullOrEmpty(value))
                        {
                            return value;
                        }
                    }
                    if (request.Headers != null)
                    {
                        value = request.Headers.Get(key);
                        if (!string.IsNullOrEmpty(value))
                        {
                            return value;
                        }
                    }
                    if (request.Cookies != null)
                    {
                        HttpCookie cookie = request.Cookies.Get(key);
                        if (cookie!=null)
                        {
                            value = cookie.Value;
                            if (!string.IsNullOrEmpty(value))
                            {
                                return value;
                            }
                        }
                    }
                }
            }
            #endregion
            #region WebOperationContext part
            WebOperationContext operationContext = WebOperationContext.Current;
            if (operationContext != null && operationContext.IncomingRequest != null && operationContext.IncomingRequest.Headers != null && operationContext.IncomingRequest.Headers.Count > 0)
            {
                for (int i = 0; i < operationContext.IncomingRequest.Headers.Count; i++)
                {
                    string name = operationContext.IncomingRequest.Headers.GetKey(i);
                    string[] values = operationContext.IncomingRequest.Headers.GetValues(i);
                    if (values!=null && values.Length>0 && String.Compare(key, name, true) == 0)
                    {
                        if (values.Length == 1) return values[0];
                        else return string.Join(",", values);
                    }
                }
            }
            #endregion
            return string.Empty;
        }

        public static bool GetHttpPropertyByBool(string key, bool defaultValue)
        {
            string value = GetHttpProperty(key);
            if (!string.IsNullOrEmpty(value))
            {
                bool b;
                if (bool.TryParse(value, out b)) 
                    return b;
                long l;
                if (long.TryParse(value, out l)) 
                    return l > 0;
            }
            return defaultValue;
        }

        public static bool ResponseUnicode()
        {
            bool useUnicode = HttpUtil.GetHttpPropertyByBool("ResponseUnicode", false);
            return useUnicode;
        }

        public static bool SupportDeflate()
        {
            //Accept-Encoding: gzip,deflate
            string accceptEncoding = HttpUtil.GetHttpProperty("Accept-Encoding");
            if (accceptEncoding.IndexOf("Deflate", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                bool useDeflate = HttpUtil.GetHttpPropertyByBool("ResponseDeflate", false);
                if(useDeflate)
                {
                    return true;
                }
            }
            return false;
        }

        
    }


    // The RequestState class passes data across async calls.
    public class RequestState
    {
        const int BufferSize = 1024;
        public StringBuilder RequestData;
        public byte[] BufferRead;
        public WebRequest Request;
        public Stream ResponseStream;
        // Create Decoder for appropriate enconding type.
        public Decoder StreamDecode = Encoding.UTF8.GetDecoder();

        public RequestState()
        {
            BufferRead = new byte[BufferSize];
            RequestData = new StringBuilder(String.Empty);
            Request = null;
            ResponseStream = null;
        }
    }
}
