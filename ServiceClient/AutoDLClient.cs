using System;
using System.Collections.Generic;
using System.ServiceModel;
using AutoDL.ServiceContracts;

namespace AutoDL.ServiceClient
{
    public abstract class ClientBase
    {
        //Methods
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

    public class DownloadClient : ClientBase, IDownload
    {
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
        
        //Class Methods
        public override void OpenClient()
        {
            DownloadFactory.Open();
        }
        public override void CloseClient()
        {
            DownloadFactory.Close();
        }
        private void DownloadServiceCall(Action<IDownload> serviceFunction)
        {
            base.ServiceCall<IDownload>(serviceFunction, DownloadFactory);
        }
        private TResult DownloadServiceCall<TResult>(Func<IDownload, TResult> serviceFunction)
        {
            return base.ServiceCall<IDownload, TResult>(serviceFunction, DownloadFactory);
        }

        //Members
        private ChannelFactory<IDownload> DownloadFactory;     
    }

    public class AliasClient : ClientBase, IAlias
    {
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
        public override void OpenClient()
        {
            AliasFactory.Open();
        }
        public override void CloseClient()
        {
            AliasFactory.Close();
        }
        private void AliasServiceCall(Action<IAlias> serviceFunction)
        {
            base.ServiceCall<IAlias>(serviceFunction, AliasFactory);
        }
        private TResult AliasServiceCall<TResult>(Func<IAlias, TResult> serviceFunction)
        {
            return base.ServiceCall<IAlias, TResult>(serviceFunction, AliasFactory);
        }

        //Members
        private ChannelFactory<IAlias> AliasFactory;
    }

    public class SettingsClient : ClientBase, ISettings
    {
        //Constructor
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
        public override void OpenClient()
        {
            SettingsFactory.Open();
        }
        public override void CloseClient()
        {
            SettingsFactory.Close();
        }
        private void SettingsServiceCall(Action<ISettings> serviceFunction)
        {
            base.ServiceCall<ISettings>(serviceFunction, SettingsFactory);
        }
        private TResult SettingsServiceCall<TResult>(Func<ISettings, TResult> serviceFunction)
        {
            return base.ServiceCall<ISettings, TResult>(serviceFunction, SettingsFactory);
        }

        //Members
        private ChannelFactory<ISettings> SettingsFactory;
    }
}
