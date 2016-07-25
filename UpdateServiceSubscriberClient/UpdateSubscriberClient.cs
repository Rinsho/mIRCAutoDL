using System;
using System.Collections.Generic;
using System.ServiceModel;

using AutoDL.ServiceContracts;

namespace AutoDL.ServiceClients
{
    /// <summary>
    /// Base class for Subscriber clients.
    /// </summary>
    public abstract class SubscriberClientBase
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

        public abstract void Open();
        public abstract void Close();

        //Members
        protected string _endpointBase = "net.pipe://localhost/AutoDL/";
        protected NetNamedPipeBinding _binding = new NetNamedPipeBinding();
    }

    /// <summary>
    /// Client used to subscribe to download updates.
    /// </summary>
    public class UpdateSubscriberClient : SubscriberClientBase, IReceiveUpdates
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="context">Context containing class that implements callback contract.</param>
        /// <param name="serviceExtension">Service extension to configure endpoint.</param>
        public UpdateSubscriberClient(InstanceContext context, string serviceExtension)
        {
            _channelFactory = new DuplexChannelFactory<IReceiveUpdates>(
                context,
                base._binding,
                new EndpointAddress(base._endpointBase + serviceExtension + "/Update/Subscribe"));
            _channelFactory.Faulted += (s, e) => { ChannelFault(); };
        }

        public void Subscribe()
        {
            _channel = _channelFactory.CreateChannel();         
            ServiceCall(() => { _channel.Subscribe(); }, _channel);
        }
        public void Unsubscribe()
        {
            ServiceCall(() => { _channel.Unsubscribe(); }, _channel);
            (_channel as IClientChannel).Close();
        }

        /// <summary>
        /// Opens client for use.
        /// </summary>
        public override void Open()
        {
            _channelFactory.Open();
        }

        /// <summary>
        /// Closes client.
        /// </summary>
        public override void Close()
        {
            _channelFactory.Close();
        }

        /// <summary>
        /// Attempts to reconnect when a channel faults.
        /// </summary>
        private void ChannelFault()
        {
            (_channel as IClientChannel).Abort();
            _channel = null;
            Subscribe();
        }

        //Members
        private DuplexChannelFactory<IReceiveUpdates> _channelFactory;
        private IReceiveUpdates _channel;
    }
}
