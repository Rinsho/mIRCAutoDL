//WCF Service client for the AutoDL service.

using System;
using System.Collections.Generic;
using System.ServiceModel;
using AutoDL.ServiceContracts;

namespace AutoDL.ServiceClient
{
    /// <summary>
    /// Base class for AutoDL service clients.
    /// </summary>
    public abstract class ClientBase
    {
        //Methods

        /// <summary>
        /// Wrapper for service calls that handles channel creation, disposal,
        /// and common exceptions.
        /// </summary>
        /// <typeparam name="TServiceType">Service contract being used.</typeparam>
        /// <param name="serviceFunction">Service function to call.</param>
        /// <param name="factory">ChannelFactory object for the service contract being used.</param>
        /// <example>
        /// ServiceCall( (channel) => { channel.ServiceFunction(); }, factory );
        /// </example>
        protected virtual void ServiceCall<TServiceType>(
            Action<TServiceType> serviceFunction,
            ChannelFactory<TServiceType> factory)
            where TServiceType: class
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

        /// <summary>
        /// Same as <c>ServiceCall{T}</c> with the addition of a generic return type.
        /// </summary>
        /// <typeparam name="TResult">Type returned by the service function.</typeparam>
        /// <returns>Result from service function call.</returns>
        protected virtual TResult ServiceCall<TServiceType, TResult>(
            Func<TServiceType, TResult> serviceFunction,
            ChannelFactory<TServiceType> factory)
            where TServiceType: class
        {
            TServiceType channel = factory.CreateChannel();
            try
            {
                TResult result = serviceFunction(channel);
                (channel as IClientChannel).Close();
                return result;
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
        protected string EndpointBase = "net.pipe://localhost/AutoDL/";
        protected static NetNamedPipeBinding Binding = new NetNamedPipeBinding();
    }

    /// <summary>
    /// Client for the download service.
    /// </summary>
    public class DownloadClient : ClientBase, IDownload
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="context">Context for the duplex binding (context that implements the callback contract).</param>
        /// <param name="serviceExtension">Service endpoint extension.</param>
        public DownloadClient(InstanceContext context, string serviceExtension)
        {
            DownloadFactory = new DuplexChannelFactory<IDownload>(
                context,
                ClientBase.Binding,
                new EndpointAddress(base.EndpointBase + serviceExtension + "/Download"));
        }

        //Service Methods
        public void Add(string name, List<int> packets)
        {
            DownloadServiceCall((channel) => { channel.Add(name, packets); });
        }
        public void Remove(string name, List<int> packets)
        {
            DownloadServiceCall((channel) => { channel.Remove(name, packets); });
        }
        public void Remove(string name)
        {
            DownloadServiceCall((channel) => { channel.Remove(name); });
        }
        public void Clear()
        {
            DownloadServiceCall((channel) => { channel.Clear(); });
        }
        public void Save()
        {
            DownloadServiceCall((channel) => { channel.Save(); });
        }
        public System.Collections.Specialized.OrderedDictionary Load()
        {
            return DownloadServiceCall<System.Collections.Specialized.OrderedDictionary>(
                (channel) => { return channel.Load(); });
        }
        public void ClearSaved()
        {
            DownloadServiceCall((channel) => { channel.ClearSaved(); });
        }
        public void StartDownload()
        {
            DownloadServiceCall((channel) => { channel.StartDownload(); });
        }
        
        //Class Methods

        /// <summary>
        /// Explicitly opens the <c>DownloadFactory</c> for use.
        /// </summary>
        public override void OpenClient()
        {
            DownloadFactory.Open();
        }

        /// <summary>
        /// Closes the <c>DownloadFactory</c>.
        /// </summary>
        public override void CloseClient()
        {
            DownloadFactory.Close();
        }

        /// <summary>
        /// Specializes the <see cref="ClientBase"/> generic function <c>ServiceCall{T}</c>
        /// </summary>
        private void DownloadServiceCall(Action<IDownload> serviceFunction)
        {
            base.ServiceCall<IDownload>(serviceFunction, DownloadFactory);
        }

        /// <summary>
        /// Specializes the <see cref="ClientBase"/> generic function <c>ServiceCall{T, R}</c>
        /// </summary>
        private TResult DownloadServiceCall<TResult>(Func<IDownload, TResult> serviceFunction)
        {
            return base.ServiceCall<IDownload, TResult>(serviceFunction, DownloadFactory);
        }

        //Members
        private ChannelFactory<IDownload> DownloadFactory;     
    }

    /// <summary>
    /// Client for the alias service.
    /// </summary>
    public class AliasClient : ClientBase, IAlias
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="serviceExtension">Service endpoint extension.</param>
        public AliasClient(string serviceExtension)
        {
            AliasFactory = new ChannelFactory<IAlias>(
                ClientBase.Binding,
                new EndpointAddress(base.EndpointBase + serviceExtension + "/Alias"));
        }

        //Service Methods
        public void Add(string alias, string name)
        {
            AliasServiceCall((channel) => { channel.Add(alias, name); });
        }
        public void Remove(string alias)
        {
            AliasServiceCall((channel) => { channel.Remove(alias); });
        }
        public void Clear()
        {
            AliasServiceCall((channel) => { channel.Clear(); });
        }
        public void Save()
        {
            AliasServiceCall((channel) => { channel.Save(); });
        }
        public Dictionary<string, string> Load()
        {
            return AliasServiceCall<Dictionary<string, string>>((channel) => { return channel.Load(); });
        }
        public void ClearSaved()
        {
            AliasServiceCall((channel) => { channel.ClearSaved(); });
        }

        //Class Methods

        /// <summary>
        /// Explicitly opens the <c>AliasFactory</c> for use.
        /// </summary>
        public override void OpenClient()
        {
            AliasFactory.Open();
        }

        /// <summary>
        /// Closes the <c>AliasFactory</c>.
        /// </summary>
        public override void CloseClient()
        {
            AliasFactory.Close();
        }

        /// <summary>
        /// Specializes the <see cref="ClientBase"/> generic function <c>ServiceCall{T}</c>
        /// </summary>
        private void AliasServiceCall(Action<IAlias> serviceFunction)
        {
            base.ServiceCall<IAlias>(serviceFunction, AliasFactory);
        }

        /// <summary>
        /// Specializes the <see cref="ClientBase"/> generic function <c>ServiceCall{T, R}</c>
        /// </summary>
        private TResult AliasServiceCall<TResult>(Func<IAlias, TResult> serviceFunction)
        {
            return base.ServiceCall<IAlias, TResult>(serviceFunction, AliasFactory);
        }

        //Members
        private ChannelFactory<IAlias> AliasFactory;
    }

    /// <summary>
    /// Client for the settings service.
    /// </summary>
    public class SettingsClient : ClientBase, ISettings
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="serviceExtension">Service endpoint extension.</param>
        public SettingsClient(string serviceExtension)
        {
            SettingsFactory = new ChannelFactory<ISettings>(
                ClientBase.Binding,
                new EndpointAddress(base.EndpointBase + serviceExtension + "/Settings"));
        }

        //Service Methods
        public void Update(string setting, string value)
        {
            SettingsServiceCall((channel) => { channel.Update(setting, value); });
        }
        public void Default(string setting)
        {
            SettingsServiceCall((channel) => { channel.Default(setting); });
        }
        public void DefaultAll()
        {
            SettingsServiceCall((channel) => { channel.DefaultAll(); });
        }
        public void Save()
        {
            SettingsServiceCall((channel) => { channel.Save(); });
        }
        public Dictionary<string, string> Load()
        {
            return SettingsServiceCall<Dictionary<string, string>>((channel) => { return channel.Load(); });
        }

        //Class Methods

        /// <summary>
        /// Explicitly opens the <c>SettingsFactory</c> for use.
        /// </summary>
        public override void OpenClient()
        {
            SettingsFactory.Open();
        }

        /// <summary>
        /// Closes the <c>SettingsFactory</c>.
        /// </summary>
        public override void CloseClient()
        {
            SettingsFactory.Close();
        }

        /// <summary>
        /// Specializes the <see cref="ClientBase"/> generic function <c>ServiceCall{T}</c>
        /// </summary>
        private void SettingsServiceCall(Action<ISettings> serviceFunction)
        {
            base.ServiceCall<ISettings>(serviceFunction, SettingsFactory);
        }

        /// <summary>
        /// Specializes the <see cref="ClientBase"/> generic function <c>ServiceCall{T, R}</c>
        /// </summary>
        private TResult SettingsServiceCall<TResult>(Func<ISettings, TResult> serviceFunction)
        {
            return base.ServiceCall<ISettings, TResult>(serviceFunction, SettingsFactory);
        }

        //Members
        private ChannelFactory<ISettings> SettingsFactory;
    }
}
