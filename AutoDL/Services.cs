//The WCF Service class.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading;
using System.ServiceModel;
using AutoDL.FileConfiguration;

namespace AutoDL.Services
{
    /// <summary>
    /// Implements the <c>ServiceContracts</c> interfaces.
    /// </summary>
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Single, InstanceContextMode = InstanceContextMode.Single)]
    internal partial class ServiceManager : ServiceContracts.ISettings
    {
        /// <summary>
        /// Constructor function for Settings-related variables.
        /// </summary>
        /// <param name="filePath">Path to the configuration file.</param>
        private void SettingsConstructor()
        {
            Settings = new Data.SettingsData();
        }

        //Service Methods
        /// <exception cref="System.ServiceModel.FaultException{T}">Throws when an invalid setting or value is encountered.</exception>
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

        /// <exception cref="System.ServiceModel.FaultException{T}">Throws when an invalid setting is encountered.</exception>
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

        /// <exception cref="System.ServiceModel.FaultException{T}">Throws when unable to access the configuration file.</exception>
        void ServiceContracts.ISettings.Save()
        {
            try
            {
                Settings.Accept(new SaveDataVisitor(FilePath));
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

        /// <exception cref="System.ServiceModel.FaultException{T}">Throws when unable to access the configuration file.</exception>
        Dictionary<string, string> ServiceContracts.ISettings.Load()
        {
            try
            {
                Settings.Accept(new LoadDataVisitor(FilePath));
                return Settings.GetData();
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
        private string FilePath;
    }

    internal partial class ServiceManager : ServiceContracts.IAlias
    {
        /// <summary>
        /// Constructor function for Alias-related variables.
        /// </summary>
        /// <param name="filePath">Path to the configuration file.</param>
        private void AliasConstructor()
        {
            Aliases = new Data.AliasData();
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

        /// <exception cref="System.ServiceModel.FaultException{T}">Throws when unable to access the configuration file.</exception>
        void ServiceContracts.IAlias.Save()
        {
            try
            {
                Aliases.Accept(new SaveDataVisitor(FilePath));
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

        /// <exception cref="System.ServiceModel.FaultException{T}">Throws when unable to access the configuration file.</exception>
        Dictionary<string, string> ServiceContracts.IAlias.Load()
        {
            try
            {
                Aliases.Accept(new LoadDataVisitor(FilePath));
                return Aliases.GetData();
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

        /// <exception cref="System.ServiceModel.FaultException{T}">Throws when unable to access the configuration file.</exception>
        void ServiceContracts.IAlias.ClearSaved()
        {
            try
            {
                Aliases.Accept(new ClearSavedDataVisitor(FilePath));
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
        /// <summary>
        /// Constructor for the ServiceManager class.
        /// </summary>
        /// <param name="filePath">Path to the configuration file.</param>
        /// <param name="wrapperCallback">Callback to send download information to the IRC client wrapper.</param>
        public ServiceManager(string filePath, Action<Data.Download> wrapperCallback)
        {
            this.WrapperCallback = wrapperCallback;
            ClientCallback = OperationContext.Current.GetCallbackChannel<ServiceContracts.IDownloadCallback>();
            Downloads = new Data.DownloadData();
            SettingsConstructor();
            AliasConstructor();
        }

        //Service Methods
        /// <summary>
        /// Adds bot and packet(s) to the queue.  If not currently downloading, calls <c>StartDownload()</c>.
        /// </summary>
        /// <exception cref="System.ServiceModel.FaultException{T}">Throws when there are invalid packets.</exception>
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

        /// <exception cref="System.ServiceModel.FaultException{T}">Throws when unable to access the configuration file.</exception>
        void ServiceContracts.IDownload.Save()
        {
            try
            {
                Downloads.Accept(new SaveDataVisitor(FilePath));
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

        /// <exception cref="System.ServiceModel.FaultException{T}">Throws when unable to access the configuration file.</exception>
        OrderedDictionary ServiceContracts.IDownload.Load()
        {
            try
            {
                Downloads.Accept(new LoadDataVisitor(FilePath));
                return Downloads.GetData();
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

        /// <exception cref="System.ServiceModel.FaultException{T}">Throws when unable to access the configuration file.</exception>
        void ServiceContracts.IDownload.ClearSaved()
        {
            try
            {
                Downloads.Accept(new ClearSavedDataVisitor(FilePath));
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
        
        /// <summary>
        /// Calls the wrapper callback to start downloading and the client callback to update the GUI.
        /// </summary>
        void ServiceContracts.IDownload.StartDownload()
        {
            if (Downloads.IsDownloading == false)
            {
                Downloads.IsDownloading = true;
                Data.Download download = Downloads.NextDownload(true);
                WrapperCallback(download);
                ClientCallback.Downloading(download.Name, download.Packet);
            }
        }

        //Methods
        /// <summary>
        /// Used to request the next download while updating status of previous download.
        /// </summary>
        /// <param name="success">Indicates whether the previous download completed.</param>
        public void SendDownload(bool success)
        {
            bool retry = (!success && Convert.ToBoolean(Settings[Data.SettingsData.RETRY]));
            int downloadDelay = Convert.ToInt32(Settings[Data.SettingsData.DELAY]);
            Data.Download download = Downloads.NextDownload(retry);

            if (download.Name == "")
            {
                ClientCallback.DownloadStatusUpdate(ServiceContracts.DownloadStatus.QueueComplete);
            }
            else
            {
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
                        WrapperCallback(download);
                        ClientCallback.Downloading(download.Name, download.Packet);
                    }
                    finally
                    {
                        (x as Timer).Dispose();
                    }
                }));
                DelayTimer.Change(downloadDelay * 1000, Timeout.Infinite);
            }
        }

        //Members
        private Data.DownloadData Downloads;
        private Action<Data.Download> WrapperCallback;
        private ServiceContracts.IDownloadCallback ClientCallback;
    }
}
