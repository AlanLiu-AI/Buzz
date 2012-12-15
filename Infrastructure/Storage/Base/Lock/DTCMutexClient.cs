using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;

namespace Runner.Base.Lock
{

    public class DTCMutexClient : Proxy.DTCMutexServiceClient, IDisposable
    {
        protected DTCMutexClient(Binding binding, EndpointAddress remoteAddress) : base(binding, remoteAddress)
        {
        }

        public void Dispose()
        {
            Close();
        }

        public static DTCMutexClient GetInstance()
        {
            return GetInstance(@"http://localhost:6998/DTCTransaction.svc");
        }

        public static DTCMutexClient GetInstance(string url)
        {
            var wsHttpBinding = new WSHttpBinding
                                    {
                                        //MaxBufferPoolSize = Int32.MaxValue,
                                        //MaxReceivedMessageSize = Int32.MaxValue,
                                        //TextEncoding = Encoding.UTF8,
                                        //ReaderQuotas =
                                           // {
                                           //     MaxArrayLength = Int32.MaxValue,
                                           //     MaxBytesPerRead = Int32.MaxValue,
                                           //     MaxDepth = Int32.MaxValue,
                                           //     MaxStringContentLength = Int32.MaxValue,
                                           //     MaxNameTableCharCount = Int32.MaxValue
                                            //},
                                            
                                        //Security = {Mode = SecurityMode.None},
                                        TransactionFlow = true
                                    };
            var endpointAddr = new EndpointAddress(url);
            return new DTCMutexClient(wsHttpBinding, endpointAddr);
        }
    }
}
