using System;
using System.Collections.Generic;
using System.ServiceModel;
using AutoDL.ServiceContracts;

namespace AutoDL.ServiceClient
{
    public class AutoDLClient : IDownload, IAlias, ISettings
    {
        public AutoDLClient(string serviceExtention)
        {
            NetNamedPipeBinding binding = new NetNamedPipeBinding();
            DownloadChannel = new ChannelFactory<IDownload>(
                binding, 
                new EndpointAddress("net.pipe://localhost/AutoDL" + serviceExtention + "/Download")
                ).CreateChannel();
            AliasChannel = new ChannelFactory<IAlias>(
                binding,
                new EndpointAddress("net.pipe://localhost/AutoDL" + serviceExtention + "/Alias")
                ).CreateChannel();
            SettingsChannel = new ChannelFactory<ISettings>(
                binding,
                new EndpointAddress("net.pipe://localhost/AutoDL" + serviceExtention + "/Settings")
                ).CreateChannel();
        }

        //Methods
        void IDownload.Add(string name, List<int> packets)
        {
            DownloadChannel.Add(name, packets);
        }
        void IDownload.Remove(string name, List<int> packets)
        {
            DownloadChannel.Remove(name, packets);
        }
        void IDownload.Remove(string name)
        {
            DownloadChannel.Remove(name);
        }
        void IDownload.Clear()
        {
            DownloadChannel.Clear();
        }
        void IDownload.Save()
        {
            DownloadChannel.Save();
        }
        System.Collections.Specialized.OrderedDictionary IDownload.Load()
        {
            return DownloadChannel.Load();
        }
        void IDownload.ClearSaved()
        {
            DownloadChannel.ClearSaved();
        }
        void IAlias.Add(string alias, string name)
        {
            AliasChannel.Add(alias, name);
        }
        void IAlias.Remove(string alias)
        {
            AliasChannel.Remove(alias);
        }
        void IAlias.Clear()
        {
            AliasChannel.Clear();
        }
        void IAlias.Save()
        {
            AliasChannel.Save();
        }
        Dictionary<string, string> IAlias.Load()
        {
            return AliasChannel.Load();
        }
        void IAlias.ClearSaved()
        {
            AliasChannel.ClearSaved();
        }
        void ISettings.Update(string setting, string value)
        {
            SettingsChannel.Update(setting, value);
        }
        void ISettings.Default(string setting)
        {
            SettingsChannel.Default(setting);
        }
        void ISettings.DefaultAll()
        {
            SettingsChannel.DefaultAll();
        }
        void ISettings.Save()
        {
            SettingsChannel.Save();
        }
        Dictionary<string, string> ISettings.Load()
        {
            return SettingsChannel.Load();
        }

        //Members
        public IDownload DownloadChannel;
        public IAlias AliasChannel;
        public ISettings SettingsChannel; 
    }
}
