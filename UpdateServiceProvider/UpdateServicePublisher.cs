using System;
using System.Collections.Generic;
using System.ServiceModel;

using AutoDL.ServiceContracts;

namespace AutoDL.ServiceClients
{
    /// <summary>
    /// Base class for Publisher clients.
    /// </summary>
    public abstract class PublisherClientBase
    {
        /// <summary>
        /// Wrapper for service calls that handles channel creation, disposal,
        /// and common exceptions.
        /// </summary>
        /// <typeparam name="TServiceType">Service contract being used.</typeparam>
        /// <param name="serviceFunction">Service function to call.</param>
        /// <param name="factory">ChannelFactory object for the service contract being used.</param>
        /// <example>
        /// ServiceCall{IServiceContract}( (channel) => { channel.ServiceFunction(); }, factory );
        /// </example>
        /// <remarks>Sessions will not be maintained as IChannel.Close() is called after each service call.</remarks>
        protected virtual void ServiceCall<TServiceType>(
            Action<TServiceType> serviceFunction,
            ChannelFactory<TServiceType> factory)
            where TServiceType : class
        {
            TServiceType channel = factory.CreateChannel();
            try
            {
                serviceFunction(channel);
                (channel as IClientChannel).Close();
            }
            catch (TimeoutException)
            {
                (channel as IClientChannel).Abort();
                throw;
            }
            catch (CommunicationException)
            {
                (channel as IClientChannel).Abort();
                throw;
            }
        }

        public abstract void Open();
        public abstract void Close();

        //Members
        protected string _endpointBase = "net.pipe://localhost/AutoDL/";
        protected NetNamedPipeBinding _binding = new NetNamedPipeBinding();
    }

    public class UpdatePublisherClient : PublisherClientBase, IUpdateStatus
    {
        public UpdatePublisherClient(string serviceExtension)
        {
            _channelFactory = new ChannelFactory<IUpdateStatus>(
                base._binding,
                new EndpointAddress(base._endpointBase + serviceExtension + "/Update/Publish"));
        }

        public void PublishStatusUpdate(DownloadStatus status)
        {
            ServiceCall<IUpdateStatus>((channel) => { channel.PublishStatusUpdate(status); }, _channelFactory);
        }
        public void PublishNextDownload(Download download)
        {
            ServiceCall<IUpdateStatus>((channel) => { channel.PublishNextDownload(download); }, _channelFactory);
        }

        public override void Open()
        {
            _channelFactory.Open();
        }
        public override void Close()
        {
            _channelFactory.Close();
        }

        //Members
        private ChannelFactory<IUpdateStatus> _channelFactory;
    }
}

namespace AutoDL.ServiceContracts
{
    /// <summary>
    /// Defines the contract for publishing updates.
    /// </summary>
    [ServiceContract(SessionMode = SessionMode.Allowed)]
    public interface IUpdateStatus
    {
        [OperationContract(IsOneWay = true)]
        void PublishStatusUpdate(DownloadStatus status);

        [OperationContract(IsOneWay = true)]
        void PublishNextDownload(Download download);
    }
}
