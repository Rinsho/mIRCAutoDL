/*
 * TO-DO: Fix up this jumbled mess. /facepalm
 * 
 * Think about using async and await Task.Delay instead of Timer?
 * 
 * Consider implementing a ServiceManager class that holds each
 * Service class.  This would decouple using the settings/alias
 * services from the download service.  Seems sloppy right now.
 */


using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading;
using System.ServiceModel;

namespace AutoDL.Services
{
    internal class DownloadManager
    {     
        public void DownloadStatusUpdate(ServiceContracts.DownloadStatus status)
        {
            //SPLIT BETWEEN AUTODL UPDATE AND THIS
            if (!(status == DownloadStatus.Success) && Convert.ToBoolean(SettingsMgr.GetSettingValue("RetryFailedDownload")))
            {
                //SEND DOWNLOAD AGAIN
                //UPDATE UI
            }
            else
            {
                this.SendNextDownload();
            }
        }

        //SendNextDownload: Uses callback to send next download to wrapper
        //                  after "DownloadDelay" seconds.
        private void SendNextDownload()
        {
            //REMOVE MOST RECENT DOWNLOAD

            int downloadDelay = Convert.ToInt32(SettingsMgr.GetSettingValue("DownloadDelay"));
            Timer DelayTimer = new Timer(new TimerCallback(x =>
            {
                try
                {
                    DLCallback(Queue.NextDownload());
                }
                finally
                {
                    (x as Timer).Dispose();
                }
            }));
            DelayTimer.Change(downloadDelay * 1000, Timeout.Infinite);

            //UPDATE UI
        }

    }

    /* MEDIATOR
    internal interface IServiceMediator
    {
        public string RequestSetting(string setting);
        public string RequestName(string alias);
    }

    internal class ServiceMediator : IServiceMediator
    {
        public ServiceMediator(SettingsService settings, AliasService aliases)
        {
            Settings = settings;
            Aliases = aliases;
        }

        //Members
        public string RequestSetting(string setting)
        {
            return Settings.GetSettingValue(setting);
        }
        public string RequestName(string alias)
        {
            return Aliases.GetName(alias);
        }

        //Properties
        public SettingsService Settings { private get; set; }
        public AliasService Aliases { private get; set; }
    }
    */

    //STILL NEED TO IMPLEMENT FAULTS FOR WCF
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
        public ServiceManager(string filePath)
        {
            Downloads = new Data.DownloadData(filePath);
        }

        //Service Methods
        void ServiceContracts.IDownload.Add(string name, List<int> packets)
        {
            try
            {
                Downloads.Add(name, packets);
            }
            catch (ArgumentException ex)
            {
                //re-throw as Fault
            }
        }
        void ServiceContracts.IDownload.Remove(Dictionary<string, List<int>> data)
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
        void ServiceContracts.IDownload.Remove(string name)
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
        void ServiceContracts.IDownload.Clear()
        {
            Downloads.Clear();
        }
        void ServiceContracts.IDownload.Save()
        {
            Downloads.Save();
        }
        OrderedDictionary ServiceContracts.IDownload.Load()
        {
            return Downloads.Load();
        }
        void ServiceContracts.IDownload.ClearSaved()
        {
            Downloads.ClearSaved();
        }

        //Members
        private Data.DownloadData Downloads;
    }

    internal partial class ServiceManager : ServiceContracts.IDownloadCallback
    {
        void ServiceContracts.IDownloadCallback.DownloadStatusUpdate(ServiceContracts.DownloadStatus status)
        {
            throw new NotImplementedException();
        }
    }
}
