using PowerShellTools.Common.ServiceManagement;
using System;
using System.Collections.ObjectModel;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace PowerShellTools.HostService
{
    /// <summary>
    /// In WCF, unhandled exception crashes the service, leaving the channel into fault state, which is basically requiring client to re-instantiate proxy in order to continue using the service.
    /// As a result, WCF provides the ability to configure a service to return information from unhandled exceptions.
    /// This is implemetaion of the generic error handler for entire powershell wcf services, by exposing it as a service behavior attribute, so that it is developer friendly
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    class PowerShellServiceHostBehaviorAttribute : Attribute, IErrorHandler, IServiceBehavior
    {
        #region IErrorHandler Members

        /// <summary>
        /// Central place for error handling for service host behaior
        /// </summary>
        /// <param name="error">exception</param>
        /// <returns></returns>
        public bool HandleError(Exception error)
        {
            // Log the error details on server side
            ServiceCommon.Log("PowershellHostService: {0}", error.Message.ToString() + Environment.NewLine + error.StackTrace);

            // Let the other ErrorHandler do their jobs
            return true;
        }

        /// <summary>
        /// Transform error into proper faultexception to client
        /// </summary>
        /// <param name="error">Original exception</param>
        /// <param name="version">Message version</param>
        /// <param name="fault">Fault output</param>
        public void ProvideFault(Exception error, MessageVersion version, ref Message fault)
        {
            // If the error is intented to be send to client, we just let it be
            if (error is FaultException)
                return;

            // Creates the exception we want to send back to the client
            var exception = new FaultException<PowerShellHostServiceExceptionDetails>(
                PowerShellHostServiceExceptionDetails.Default,
                new FaultReason(PowerShellHostServiceExceptionDetails.Default.Message));

            // Creates a message fault
            var messageFault = exception.CreateMessageFault();

            // Creates the new message based on the message fault
            fault = Message.CreateMessage(version, messageFault, exception.Action);
        }

        #endregion

        #region IServiceBehavior Members

        /// <summary>
        /// Hook up the service behavior into service host channel properly
        /// </summary>
        /// <param name="serviceDescription"></param>
        /// <param name="serviceHostBase"></param>
        public void ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
            // Adds a PowershellServiceHostBehavior to each ChannelDispatcher
            foreach (var channelDispatcherBase in serviceHostBase.ChannelDispatchers)
            {
                var channelDispatcher = channelDispatcherBase as ChannelDispatcher;
                channelDispatcher.ErrorHandlers.Add(new PowerShellServiceHostBehaviorAttribute());
            }
        }

        public void AddBindingParameters(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints, BindingParameterCollection bindingParameters) { }
        
        public void Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase) { }
        
        #endregion
    }
}
