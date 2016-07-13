using System;
using System.Collections.Generic;
using System.ServiceModel;

using AutoDL.ServiceContracts;

namespace AutoDL.ServiceClients
{
    /// <summary>
    /// Base class for Publisher clients.
    /// </summary>
    public abstract class ClientBase
    {
        /// <summary>
        /// Wrapper for service calls that handles exceptions.
        /// </summary>
        /// <param name="serviceFunction">Service function to call.</param>
        /// <param name="channel">Valid channel to use.</param>
        /// <example>
        /// ServiceCall(() => { channel.ServiceFunction(); }, channel );
        /// </example>
        /// <remarks>Does not call IClient.Close() since contract has IsTerminating operation.</remarks>
        protected virtual void ServiceCall(Action serviceFunction, IReceiveUpdates channel)
        {
            try
            {
                serviceFunction();
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

        public abstract void OpenClient();
        public abstract void CloseClient();

        //Members
        protected string _endpointBase = "net.pipe://localhost/AutoDL/";
        protected NetNamedPipeBinding _binding = new NetNamedPipeBinding();
    }

    public class UpdateSubscriberClient : ClientBase, IReceiveUpdates
    {
        public UpdateSubscriberClient(InstanceContext context, string serviceExtension)
        {
            base._binding.ReceiveTimeout = new TimeSpan(0, 30, 0);
            _channelFactory = new DuplexChannelFactory<IReceiveUpdates>(
                context,
                base._binding,
                new EndpointAddress(base._endpointBase + serviceExtension + "/Update/Subscribe"));
            _channelFactory.Faulted += (s, e) => { OnFaulted(); };
        }

        public void Subscribe()
        {
            _channel = _channelFactory.CreateChannel(); ;          
            ServiceCall(() => { _channel.Subscribe(); }, _channel);
        }
        public void Unsubscribe()
        {
            ServiceCall(() => { _channel.Unsubscribe(); }, _channel);
            (_channel as IClientChannel).Close();
        }

        public override void OpenClient()
        {
            _channelFactory.Open();
        }
        public override void CloseClient()
        {
            _channelFactory.Close();
        }
        private void OnFaulted()
        {
            (_channel as IClientChannel).Abort();
            Subscribe();
        }

        //Members
        private DuplexChannelFactory<IReceiveUpdates> _channelFactory;
        private IReceiveUpdates _channel;
    }
}
