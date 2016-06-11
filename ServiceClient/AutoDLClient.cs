using System;
using System.Collections.Generic;
using System.ServiceModel;
using AutoDL.ServiceContracts;

namespace AutoDL.ServiceClient
{
    public partial class AutoDLClient : IDownload
    {
        public AutoDLClient(string serviceExtension)
        {
            NetNamedPipeBinding binding = new NetNamedPipeBinding();
            DownloadFactory = new ChannelFactory<IDownload>(
                binding,
                new EndpointAddress("net.pipe://localhost/AutoDL" + serviceExtension + "/Download"));
            AliasConstructor(serviceExtension, binding);
            SettingsConstructor(serviceExtension, binding);          
        }

        //Service Methods
        void IDownload.Add(string name, List<int> packets)
        {
            DownloadChannel = DownloadFactory.CreateChannel();
            try
            {
                DownloadChannel.Add(name, packets);
                (DownloadChannel as IClientChannel).Close();
            }
            catch (TimeoutException ex)
            {
                (DownloadChannel as IClientChannel).Abort();
                throw ex;
            }
            catch (CommunicationException ex)
            {
                (DownloadChannel as IClientChannel).Abort();
                throw ex;
            }
        }
        void IDownload.Remove(string name, List<int> packets)
        {           
            DownloadChannel = DownloadFactory.CreateChannel();
            try
            {
                DownloadChannel.Remove(name, packets);
                (DownloadChannel as IClientChannel).Close();
            }
            catch (TimeoutException ex)
            {
                (DownloadChannel as IClientChannel).Abort();
                throw ex;
            }
            catch (CommunicationException ex)
            {
                (DownloadChannel as IClientChannel).Abort();
                throw ex;
            }
        }
        void IDownload.Remove(string name)
        {        
            DownloadChannel = DownloadFactory.CreateChannel();
            try
            {
                DownloadChannel.Remove(name);
                (DownloadChannel as IClientChannel).Close();
            }
            catch (TimeoutException ex)
            {
                (DownloadChannel as IClientChannel).Abort();
                throw ex;
            }
            catch (CommunicationException ex)
            {
                (DownloadChannel as IClientChannel).Abort();
                throw ex;
            }
        }
        void IDownload.Clear()
        {         
            DownloadChannel = DownloadFactory.CreateChannel();
            try
            {
                DownloadChannel.Clear();
                (DownloadChannel as IClientChannel).Close();
            }
            catch (TimeoutException ex)
            {
                (DownloadChannel as IClientChannel).Abort();
                throw ex;
            }
            catch (CommunicationException ex)
            {
                (DownloadChannel as IClientChannel).Abort();
                throw ex;
            }
        }
        void IDownload.Save()
        {        
            DownloadChannel = DownloadFactory.CreateChannel();
            try
            {
                DownloadChannel.Save();
                (DownloadChannel as IClientChannel).Close();
            }
            catch (TimeoutException ex)
            {
                (DownloadChannel as IClientChannel).Abort();
                throw ex;
            }
            catch (CommunicationException ex)
            {
                (DownloadChannel as IClientChannel).Abort();
                throw ex;
            }
        }
        System.Collections.Specialized.OrderedDictionary IDownload.Load()
        {         
            DownloadChannel = DownloadFactory.CreateChannel();
            try
            {
                return DownloadChannel.Load();
                (DownloadChannel as IClientChannel).Close();
            }
            catch (TimeoutException ex)
            {
                (DownloadChannel as IClientChannel).Abort();
                throw ex;
            }
            catch (CommunicationException ex)
            {
                (DownloadChannel as IClientChannel).Abort();
                throw ex;
            }
        }
        void IDownload.ClearSaved()
        {           
            DownloadChannel = DownloadFactory.CreateChannel();
            try
            {
                DownloadChannel.ClearSaved();
                (DownloadChannel as IClientChannel).Close();
            }
            catch (TimeoutException ex)
            {
                (DownloadChannel as IClientChannel).Abort();
                throw ex;
            }
            catch (CommunicationException ex)
            {
                (DownloadChannel as IClientChannel).Abort();
                throw ex;
            }
        }
        
        //General Methods
        public void CloseClient()
        {
            DownloadFactory.Close();
            AliasFactory.Close();
            SettingsFactory.Close();
        }

        //Members
        private ChannelFactory<IDownload> DownloadFactory;
        private IDownload DownloadChannel;        
    }

    public partial class AutoDLClient : IAlias
    {
        //Constructor
        private void AliasConstructor(string serviceExtension, NetNamedPipeBinding binding)
        {
            AliasFactory = new ChannelFactory<IAlias>(
                binding,
                new EndpointAddress("net.pipe://localhost/AutoDL" + serviceExtension + "/Alias"));
        }

        //Service Methods
        void IAlias.Add(string alias, string name)
        {
            AliasChannel = AliasFactory.CreateChannel();
            try
            {
                AliasChannel.Add(alias, name);
                (AliasChannel as IClientChannel).Close();
            }
            catch (TimeoutException ex)
            {
                (AliasChannel as IClientChannel).Abort();
                throw ex;
            }
            catch (CommunicationException ex)
            {
                (AliasChannel as IClientChannel).Abort();
                throw ex;
            }
        }
        void IAlias.Remove(string alias)
        {
            AliasChannel = AliasFactory.CreateChannel();
            try
            {
                AliasChannel.Remove(alias);
                (AliasChannel as IClientChannel).Close();
            }
            catch (TimeoutException ex)
            {
                (AliasChannel as IClientChannel).Abort();
                throw ex;
            }
            catch (CommunicationException ex)
            {
                (AliasChannel as IClientChannel).Abort();
                throw ex;
            }
        }
        void IAlias.Clear()
        {
            AliasChannel = AliasFactory.CreateChannel();
            try
            {
                AliasChannel.Clear();
                (AliasChannel as IClientChannel).Close();
            }
            catch (TimeoutException ex)
            {
                (AliasChannel as IClientChannel).Abort();
                throw ex;
            }
            catch (CommunicationException ex)
            {
                (AliasChannel as IClientChannel).Abort();
                throw ex;
            }
        }
        void IAlias.Save()
        {
            AliasChannel = AliasFactory.CreateChannel();
            try
            {
                AliasChannel.Save();
                (AliasChannel as IClientChannel).Close();
            }
            catch (TimeoutException ex)
            {
                (AliasChannel as IClientChannel).Abort();
                throw ex;
            }
            catch (CommunicationException ex)
            {
                (AliasChannel as IClientChannel).Abort();
                throw ex;
            }
        }
        Dictionary<string, string> IAlias.Load()
        {
            AliasChannel = AliasFactory.CreateChannel();
            try
            {
                return AliasChannel.Load();
                (AliasChannel as IClientChannel).Close();
            }
            catch (TimeoutException ex)
            {
                (AliasChannel as IClientChannel).Abort();
                throw ex;
            }
            catch (CommunicationException ex)
            {
                (AliasChannel as IClientChannel).Abort();
                throw ex;
            }
        }
        void IAlias.ClearSaved()
        {
            AliasChannel = AliasFactory.CreateChannel();
            try
            {
                AliasChannel.ClearSaved();
                (AliasChannel as IClientChannel).Close();
            }
            catch (TimeoutException ex)
            {
                (AliasChannel as IClientChannel).Abort();
                throw ex;
            }
            catch (CommunicationException ex)
            {
                (AliasChannel as IClientChannel).Abort();
                throw ex;
            }
        }

        //Members
        private ChannelFactory<IAlias> AliasFactory;
        private IAlias AliasChannel;
    }

    public partial class AutoDLClient : ISettings
    {
        //Constructor
        private void SettingsConstructor(string serviceExtension, NetNamedPipeBinding binding)
        {
            SettingsFactory = new ChannelFactory<ISettings>(
                binding,
                new EndpointAddress("net.pipe://localhost/AutoDL" + serviceExtension + "/Settings"));
        }

        //Service Methods
        void ISettings.Update(string setting, string value)
        {
            SettingsChannel = SettingsFactory.CreateChannel();
            try
            {
                SettingsChannel.Update(setting, value);
                (SettingsChannel as IClientChannel).Close();
            }
            catch (TimeoutException ex)
            {
                (SettingsChannel as IClientChannel).Abort();
                throw ex;
            }
            catch (CommunicationException ex)
            {
                (SettingsChannel as IClientChannel).Abort();
                throw ex;
            }
        }
        void ISettings.Default(string setting)
        {
            SettingsChannel = SettingsFactory.CreateChannel();
            try
            {
                SettingsChannel.Default(setting);
                (SettingsChannel as IClientChannel).Close();
            }
            catch (TimeoutException ex)
            {
                (SettingsChannel as IClientChannel).Abort();
                throw ex;
            }
            catch (CommunicationException ex)
            {
                (SettingsChannel as IClientChannel).Abort();
                throw ex;
            }
        }
        void ISettings.DefaultAll()
        {
            SettingsChannel = SettingsFactory.CreateChannel();
            try
            {
                SettingsChannel.DefaultAll();
                (SettingsChannel as IClientChannel).Close();
            }
            catch (TimeoutException ex)
            {
                (SettingsChannel as IClientChannel).Abort();
                throw ex;
            }
            catch (CommunicationException ex)
            {
                (SettingsChannel as IClientChannel).Abort();
                throw ex;
            }
        }
        void ISettings.Save()
        {
            SettingsChannel = SettingsFactory.CreateChannel();
            try
            {
                SettingsChannel.Save();
                (SettingsChannel as IClientChannel).Close();
            }
            catch (TimeoutException ex)
            {
                (SettingsChannel as IClientChannel).Abort();
                throw ex;
            }
            catch (CommunicationException ex)
            {
                (SettingsChannel as IClientChannel).Abort();
                throw ex;
            }
        }
        Dictionary<string, string> ISettings.Load()
        {
            SettingsChannel = SettingsFactory.CreateChannel();
            try
            {
                return SettingsChannel.Load();
                (SettingsChannel as IClientChannel).Close();
            }
            catch (TimeoutException ex)
            {
                (SettingsChannel as IClientChannel).Abort();
                throw ex;
            }
            catch (CommunicationException ex)
            {
                (SettingsChannel as IClientChannel).Abort();
                throw ex;
            }
        }

        //Members
        private ChannelFactory<ISettings> SettingsFactory;
        private ISettings SettingsChannel;
    }
}
