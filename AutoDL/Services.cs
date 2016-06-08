/*
 * TO-DO: Finish up. SendDownload(), Faults, etc.
 */


using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading;
using System.ServiceModel;

namespace AutoDL.Services
{
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Single, InstanceContextMode = InstanceContextMode.Single)]
    internal partial class ServiceManager : ServiceContracts.ISettings
    {
        public ServiceManager(string filePath)
        {
            Settings = new Data.SettingsData(filePath);
        }

        //Service Methods
        public void ServiceContracts.ISettings.Update(Dictionary<string, string> data)
        {
            try
            {
                Settings.Update(data);
            }
            catch (ArgumentException ex)
            {
                //re-throw as Fault
            }
        }
        public void ServiceContracts.ISettings.Default(string setting)
        {
            try
            {
                Settings.Default(setting);
            }
            catch (ArgumentException ex)
            {
                //re-throw as Fault
            }
        }
        public void ServiceContracts.ISettings.DefaultAll()
        {
            Settings.DefaultAll();
        }
        public void ServiceContracts.ISettings.Save()
        {
            Settings.Save();
        }
        public Dictionary<string, string> ServiceContracts.ISettings.Load()
        {
            return Settings.Load();
        }

        //Members
        private Data.SettingsData Settings;
    }

    internal partial class ServiceManager : ServiceContracts.IAlias
    {
        public ServiceManager(string filePath)
        {
            Aliases = new Data.AliasData(filePath);
        }

        //Service Methods
        public void ServiceContracts.IAlias.Add(Dictionary<string, string> data)
        {
            Aliases.Add(data);
        }
        public void ServiceContracts.IAlias.Remove(string alias)
        {
            Aliases.Remove(alias);
        }
        public void ServiceContracts.IAlias.Clear()
        {
            Aliases.Clear();
        }
        public void ServiceContracts.IAlias.Save()
        {
            Aliases.Save();
        }
        public Dictionary<string, string> ServiceContracts.IAlias.Load()
        {
            return Aliases.Load();
        }
        public void ServiceContracts.IAlias.ClearSaved()
        {
            Aliases.ClearSaved();
        }

        //Members
        private Data.AliasData Aliases;
    }

    internal partial class ServiceManager : ServiceContracts.IDownload
    {
        public ServiceManager(string filePath, Action<Data.Download> wrapperCallback)
        {
            this.WrapperCallback = wrapperCallback;
            Downloads = new Data.DownloadData(filePath);
        }

        //Service Methods
        public void ServiceContracts.IDownload.Add(string name, List<int> packets)
        {
            try
            {
                Downloads.Add(name, packets);
            }
            catch (Data.InvalidPacketException ex)
            {
                //re-throw as Fault
            }
        }
        public void ServiceContracts.IDownload.Remove(Dictionary<string, List<int>> data)
        {
            try
            {
                Downloads.Remove(data);
            }
            catch (ArgumentException ex)
            {
                //re-throw as Fault
            }
        }
        public void ServiceContracts.IDownload.Remove(string name)
        {
            try
            {
                Downloads.Remove(name);
            }
            catch (ArgumentException ex)
            {
                //re-throw as Fault
            }
        }
        public void ServiceContracts.IDownload.Clear()
        {
            Downloads.Clear();
        }
        public void ServiceContracts.IDownload.Save()
        {
            Downloads.Save();
        }
        public OrderedDictionary ServiceContracts.IDownload.Load()
        {
            return Downloads.Load();
        }
        public void ServiceContracts.IDownload.ClearSaved()
        {
            Downloads.ClearSaved();
        }

        //Methods
        public void SendDownload()
        {
            int downloadDelay = Convert.ToInt32(Settings[Data.SettingsData.DELAY]);
            Timer DelayTimer = new Timer(new TimerCallback(x =>
            {
                try
                {
                    WrapperCallback(Downloads.NextDownload());
                }
                catch (Data.InvalidDownloadException ex)
                {
                    //Re-throw as Fault
                }
                finally
                {
                    (x as Timer).Dispose();
                }
            }));
            DelayTimer.Change(downloadDelay * 1000, Timeout.Infinite);
        }

        //Members
        private Data.DownloadData Downloads;
        private Action<Data.Download> WrapperCallback;
    }
}
