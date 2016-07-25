//Client for managing the download queue.

using System;
using System.Collections.Generic;
using System.ServiceModel;

using AutoDL.ServiceContracts;

namespace AutoDL.ServiceClients
{
    /// <summary>
    /// Base class for AutoDL service clients.
    /// </summary>
    public abstract class DownloadServiceClientBase
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
        /// ServiceCall{IDownload}( (channel) => { channel.ServiceFunction(); }, factory );
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

        public abstract void Open();
        public abstract void Close();

        //Members
        protected string _endpointBase = "net.pipe://localhost/AutoDL/";
        protected NetNamedPipeBinding _binding = new NetNamedPipeBinding();
    }

    /// <summary>
    /// Client for the download service.
    /// </summary>
    public class DownloadClient : DownloadServiceClientBase, IDownload
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="serviceExtension">Service endpoint extension.</param>
        public DownloadClient(string serviceExtension)
        {
            _downloadFactory = new ChannelFactory<IDownload>(
                _binding,
                new EndpointAddress(base._endpointBase + serviceExtension + "/Download"));
        }

        //Service Methods
        public void Add(Download[] downloads)
        {
            DownloadServiceCall((channel) => { channel.Add(downloads); });
        }
        public void Remove(Download[] downloads)
        {
            DownloadServiceCall((channel) => { channel.Remove(downloads); });
        }
        public void Clear()
        {
            DownloadServiceCall((channel) => { channel.Clear(); });
        }
        public void Save()
        {
            DownloadServiceCall((channel) => { channel.Save(); });
        }
        public Download[] Load()
        {
            return DownloadServiceCall<Download[]>(
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
        /// Opens client for use.
        /// </summary>
        public override void Open()
        {
            _downloadFactory.Open();
        }

        /// <summary>
        /// Closes the client.
        /// </summary>
        public override void Close()
        {
            _downloadFactory.Close();
        }

        /// <summary>
        /// Specializes the <see cref="DownloadServiceClientBase"/> generic function <c>ServiceCall{T}</c>
        /// </summary>
        private void DownloadServiceCall(Action<IDownload> serviceFunction)
        {
            base.ServiceCall<IDownload>(serviceFunction, _downloadFactory);
        }

        /// <summary>
        /// Specializes the <see cref="DownloadServiceClientBase"/> generic function <c>ServiceCall{T, R}</c>
        /// </summary>
        private TResult DownloadServiceCall<TResult>(Func<IDownload, TResult> serviceFunction)
        {
            return base.ServiceCall<IDownload, TResult>(serviceFunction, _downloadFactory);
        }

        //Members
        private ChannelFactory<IDownload> _downloadFactory;     
    }

    /// <summary>
    /// Client for the alias service.
    /// </summary>
    public class AliasClient : DownloadServiceClientBase, IAlias
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="serviceExtension">Service endpoint extension.</param>
        public AliasClient(string serviceExtension)
        {
            _aliasFactory = new ChannelFactory<IAlias>(
                _binding,
                new EndpointAddress(base._endpointBase + serviceExtension + "/Alias"));
        }

        //Service Methods
        public void Add(Alias alias)
        {
            AliasServiceCall((channel) => { channel.Add(alias); });
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
        public Alias[] Load()
        {
            return AliasServiceCall<Alias[]>((channel) => { return channel.Load(); });
        }
        public void ClearSaved()
        {
            AliasServiceCall((channel) => { channel.ClearSaved(); });
        }

        //Class Methods

        /// <summary>
        /// Opens client for use.
        /// </summary>
        public override void Open()
        {
            _aliasFactory.Open();
        }

        /// <summary>
        /// Closes the client.
        /// </summary>
        public override void Close()
        {
            _aliasFactory.Close();
        }

        /// <summary>
        /// Specializes the <see cref="DownloadServiceClientBase"/> generic function <c>ServiceCall{T}</c>
        /// </summary>
        private void AliasServiceCall(Action<IAlias> serviceFunction)
        {
            base.ServiceCall<IAlias>(serviceFunction, _aliasFactory);
        }

        /// <summary>
        /// Specializes the <see cref="DownloadServiceClientBase"/> generic function <c>ServiceCall{T, R}</c>
        /// </summary>
        private TResult AliasServiceCall<TResult>(Func<IAlias, TResult> serviceFunction)
        {
            return base.ServiceCall<IAlias, TResult>(serviceFunction, _aliasFactory);
        }

        //Members
        private ChannelFactory<IAlias> _aliasFactory;
    }

    /// <summary>
    /// Client for the settings service.
    /// </summary>
    public class SettingsClient : DownloadServiceClientBase, ISettings
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="serviceExtension">Service endpoint extension.</param>
        public SettingsClient(string serviceExtension)
        {
            _settingsFactory = new ChannelFactory<ISettings>(
                _binding,
                new EndpointAddress(base._endpointBase + serviceExtension + "/Settings"));
        }

        //Service Methods
        public void Update(Setting setting)
        {
            SettingsServiceCall((channel) => { channel.Update(setting); });
        }
        public void Default(SettingName name)
        {
            SettingsServiceCall((channel) => { channel.Default(name); });
        }
        public void DefaultAll()
        {
            SettingsServiceCall((channel) => { channel.DefaultAll(); });
        }
        public void Save()
        {
            SettingsServiceCall((channel) => { channel.Save(); });
        }
        public Setting[] Load()
        {
            return SettingsServiceCall<Setting[]>((channel) => { return channel.Load(); });
        }

        //Class Methods

        /// <summary>
        /// Opens the client for use.
        /// </summary>
        public override void Open()
        {
            _settingsFactory.Open();
        }

        /// <summary>
        /// Closes the client.
        /// </summary>
        public override void Close()
        {
            _settingsFactory.Close();
        }

        /// <summary>
        /// Specializes the <see cref="DownloadServiceClientBase"/> generic function <c>ServiceCall{T}</c>
        /// </summary>
        private void SettingsServiceCall(Action<ISettings> serviceFunction)
        {
            base.ServiceCall<ISettings>(serviceFunction, _settingsFactory);
        }

        /// <summary>
        /// Specializes the <see cref="DownloadServiceClientBase"/> generic function <c>ServiceCall{T, R}</c>
        /// </summary>
        private TResult SettingsServiceCall<TResult>(Func<ISettings, TResult> serviceFunction)
        {
            return base.ServiceCall<ISettings, TResult>(serviceFunction, _settingsFactory);
        }

        //Members
        private ChannelFactory<ISettings> _settingsFactory;
    }
}
