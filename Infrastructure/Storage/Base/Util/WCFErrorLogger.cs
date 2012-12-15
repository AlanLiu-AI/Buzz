using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace Runner.Base.Util
{

    public class WCFErrorHandler : IErrorHandler
    {
        private readonly log4net.ILog _log;

        public WCFErrorHandler(string logName)
        {
            _log = log4net.LogManager.GetLogger(logName);
        }

        // Provide a fault. The Message fault parameter can be replaced, or set to
        // null to suppress reporting a fault.

        public void ProvideFault(Exception error, MessageVersion version, ref Message fault)
        {
        }

        // HandleError. Log an error, then allow the error to be handled as usual.
        // Return true if the error is considered as already handled

        public bool HandleError(Exception error)
        {
            _log.Error(error);
            return true;
        }
    }

    // This attribute can be used to install a custom error handler for a service.
    public class WCFLogErrorBehaviorAttribute : Attribute, IServiceBehavior
    {
        string _logName;

        public WCFLogErrorBehaviorAttribute(Type errorHandlerType)
        {
            _logName = errorHandlerType.Name;
        }

        public WCFLogErrorBehaviorAttribute(string name)
        {
            _logName = name;
        }

        public WCFLogErrorBehaviorAttribute()
        {
        }

        public string Name
        {
            get { return _logName; }
            set { _logName = value; }
        }

        void IServiceBehavior.Validate(ServiceDescription description, ServiceHostBase serviceHostBase)
        {
        }

        void IServiceBehavior.AddBindingParameters(ServiceDescription description, ServiceHostBase serviceHostBase, System.Collections.ObjectModel.Collection<ServiceEndpoint> endpoints, BindingParameterCollection parameters)
        {
        }

        void IServiceBehavior.ApplyDispatchBehavior(ServiceDescription description, ServiceHostBase serviceHostBase)
        {
            IErrorHandler errorHandler;

            try
            {
                errorHandler = new WCFErrorHandler(_logName);
            }
            catch (MissingMethodException e)
            {
                throw new ArgumentException("The errorHandlerType specified in the ErrorBehaviorAttribute constructor must have a public empty constructor.", e);
            }
            catch (InvalidCastException e)
            {
                throw new ArgumentException("The errorHandlerType specified in the ErrorBehaviorAttribute constructor must implement System.ServiceModel.Dispatcher.IErrorHandler.", e);
            }

            foreach (var channelDispatcherBase in serviceHostBase.ChannelDispatchers)
            {
                var channelDispatcher = channelDispatcherBase as ChannelDispatcher;
                if (channelDispatcher != null) channelDispatcher.ErrorHandlers.Add(errorHandler);
            }
        }
    }
}
