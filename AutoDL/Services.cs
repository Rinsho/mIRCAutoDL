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
        //Constructor
        private void SettingsConstructor(string filePath)
        {
            Settings = new Data.SettingsData(filePath);
        }

        //Service Methods
        void ServiceContracts.ISettings.Update(string setting, string value)
        {
            try
            {
                Settings.Update(setting, value);
            }
            catch (ArgumentException ex)
            {
                ServiceContracts.InvalidSettingFault fault = new ServiceContracts.InvalidSettingFault()
                {
                    Value = ex.ParamName,
                    Description = ex.Message
                };
                throw new FaultException<ServiceContracts.InvalidSettingFault>(fault);
            }
        }
        void ServiceContracts.ISettings.Default(string setting)
        {
            try
            {
                Settings.Default(setting);
            }
            catch (ArgumentException ex)
            {
                ServiceContracts.InvalidSettingFault fault = new ServiceContracts.InvalidSettingFault()
                {
                    Value = ex.ParamName,
                    Description = ex.Message
                };
                throw new FaultException<ServiceContracts.InvalidSettingFault>(fault);
            }
        }
        void ServiceContracts.ISettings.DefaultAll()
        {
            Settings.DefaultAll();
        }
        void ServiceContracts.ISettings.Save()
        {
            try
            {
                Settings.Save();
            }
            catch (System.Configuration.ConfigurationErrorsException ex)
            {
                ServiceContracts.ConfigurationFault fault = new ServiceContracts.ConfigurationFault()
                {
                    File = ex.Filename,
                    Description = "Settings.Save(): Error accessing file."
                };
                throw new FaultException<ServiceContracts.ConfigurationFault>(fault);
            }
        }
        Dictionary<string, string> ServiceContracts.ISettings.Load()
        {
            try
            {
                return Settings.Load();
            }
            catch (System.Configuration.ConfigurationErrorsException ex)
            {
                ServiceContracts.ConfigurationFault fault = new ServiceContracts.ConfigurationFault()
                {
                    File = ex.Filename,
                    Description = "Settings.Load(): Error accessing file."
                };
                throw new FaultException<ServiceContracts.ConfigurationFault>(fault);
            }
        }

        //Members
        private Data.SettingsData Settings;
    }

    internal partial class ServiceManager : ServiceContracts.IAlias
    {
        private void AliasConstructor(string filePath)
        {
            Aliases = new Data.AliasData(filePath);
        }

        //Service Methods
        void ServiceContracts.IAlias.Add(string alias, string name)
        {
            Aliases.Add(alias, name);
        }
        void ServiceContracts.IAlias.Remove(string alias)
        {
            Aliases.Remove(alias);
        }
        void ServiceContracts.IAlias.Clear()
        {
            Aliases.Clear();
        }
        void ServiceContracts.IAlias.Save()
        {
            try
            {
                Aliases.Save();
            }
            catch (System.Configuration.ConfigurationErrorsException ex)
            {
                ServiceContracts.ConfigurationFault fault = new ServiceContracts.ConfigurationFault()
                {
                    File = ex.Filename,
                    Description = "Alias.Save(): Error accessing file."
                };
                throw new FaultException<ServiceContracts.ConfigurationFault>(fault);
            }
        }
        Dictionary<string, string> ServiceContracts.IAlias.Load()
        {
            try
            {
                return Aliases.Load();
            }
            catch (System.Configuration.ConfigurationErrorsException ex)
            {
                ServiceContracts.ConfigurationFault fault = new ServiceContracts.ConfigurationFault()
                {
                    File = ex.Filename,
                    Description = "Alias.Load(): Error accessing file."
                };
                throw new FaultException<ServiceContracts.ConfigurationFault>(fault);
            }
        }
        void ServiceContracts.IAlias.ClearSaved()
        {
            try
            {
                Aliases.ClearSaved();
            }
            catch (System.Configuration.ConfigurationErrorsException ex)
            {
                ServiceContracts.ConfigurationFault fault = new ServiceContracts.ConfigurationFault()
                {
                    File = ex.Filename,
                    Description = "Alias.ClearSaved(): Error accessing file."
                };
                throw new FaultException<ServiceContracts.ConfigurationFault>(fault);
            }
        }

        //Members
        private Data.AliasData Aliases;
    }

    internal partial class ServiceManager : ServiceContracts.IDownload
    {
        internal ServiceManager(string filePath, Action<Data.Download> wrapperCallback)
        {
            this.WrapperCallback = wrapperCallback;
            ClientCallback = OperationContext.Current.GetCallbackChannel<ServiceContracts.IDownloadCallback>();
            Downloads = new Data.DownloadData(filePath);
            SettingsConstructor(filePath);
            AliasConstructor(filePath);
        }

        //Service Methods
        void ServiceContracts.IDownload.Add(string name, List<int> packets)
        {
            try
            {
                foreach (string alias in Aliases)
                {
                    if (name == alias)
                    {
                        name = Aliases[alias];
                    }
                }
                Downloads.Add(name, packets);
                if (Downloads.IsDownloading == false)
                {
                    this.StartDownload();
                }
            }
            catch (Data.InvalidPacketException ex)
            {
                ServiceContracts.InvalidPacketFault fault = new ServiceContracts.InvalidPacketFault()
                {
                    Name = ex.Name,
                    Description = ex.Message
                };
                throw new FaultException<ServiceContracts.InvalidPacketFault>(fault);
            }
        }
        void ServiceContracts.IDownload.Remove(string name, List<int> packets)
        {
            foreach (string alias in Aliases)
            {
                if (name == alias)
                {
                    name = Aliases[alias];
                }
            }
            Downloads.Remove(name, packets);
        }
        void ServiceContracts.IDownload.Remove(string name)
        {
            foreach (string alias in Aliases)
            {
                if (name == alias)
                {
                    name = Aliases[alias];
                }
            }
            Downloads.Remove(name);
        }
        void ServiceContracts.IDownload.Clear()
        {
            Downloads.Clear();
        }
        void ServiceContracts.IDownload.Save()
        {
            try
            {
                Downloads.Save();
            }
            catch (System.Configuration.ConfigurationErrorsException ex)
            {
                ServiceContracts.ConfigurationFault fault = new ServiceContracts.ConfigurationFault()
                {
                    File = ex.Filename,
                    Description = "Download.Save(): Error accessing file."
                };
                throw new FaultException<ServiceContracts.ConfigurationFault>(fault);
            }
        }
        OrderedDictionary ServiceContracts.IDownload.Load()
        {
            try
            {
                return Downloads.Load();
            }
            catch (System.Configuration.ConfigurationErrorsException ex)
            {
                ServiceContracts.ConfigurationFault fault = new ServiceContracts.ConfigurationFault()
                {
                    File = ex.Filename,
                    Description = "Download.Load(): Error accessing file."
                };
                throw new FaultException<ServiceContracts.ConfigurationFault>(fault);
            }
        }
        void ServiceContracts.IDownload.ClearSaved()
        {
            try
            {
                Downloads.ClearSaved();
            }
            catch (System.Configuration.ConfigurationErrorsException ex)
            {
                ServiceContracts.ConfigurationFault fault = new ServiceContracts.ConfigurationFault()
                {
                    File = ex.Filename,
                    Description = "Download.ClearSaved(): Error accessing file."
                };
                throw new FaultException<ServiceContracts.ConfigurationFault>(fault);
            }
        }

        //Methods
        public void StartDownload()
        {
            Downloads.IsDownloading = true;
            Data.Download download = Downloads.NextDownload(true);
            WrapperCallback(download);
            ClientCallback.Downloading(download.Name, download.Packet);
        }
        public void SendDownload(bool success)
        {
            bool retry = (!success && Convert.ToBoolean(Settings[Data.SettingsData.RETRY]));
            int downloadDelay = Convert.ToInt32(Settings[Data.SettingsData.DELAY]);
            if (success)
            {
                ClientCallback.DownloadStatusUpdate(ServiceContracts.DownloadStatus.Success);
            }
            else if (retry)
            {
                ClientCallback.DownloadStatusUpdate(ServiceContracts.DownloadStatus.Retry);
            }
            else
            {
                ClientCallback.DownloadStatusUpdate(ServiceContracts.DownloadStatus.Fail);
            }

            Timer DelayTimer = new Timer(new TimerCallback(x =>
            {
                try
                {             
                    Data.Download download = Downloads.NextDownload(retry);
                    WrapperCallback(download);
                    ClientCallback.Downloading(download.Name, download.Packet);
                }
                catch (Data.InvalidDownloadException)
                {
                    ClientCallback.DownloadStatusUpdate(ServiceContracts.DownloadStatus.QueueComplete);
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
        private ServiceContracts.IDownloadCallback ClientCallback;
    }
}
