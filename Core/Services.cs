//The WCF Service classes.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading;
using System.ServiceModel;

using AutoDL.FileConfiguration;
using AutoDL.ServiceContracts;
using AutoDL.Data;

namespace AutoDL.Services
{
    /// <summary>
    /// Dependency object for <see cref="DownloadService"/>.
    /// </summary>
    internal class DownloadDependencies
    {       
        public IHandleDownloadData DownloadDependency { get; set; }
        public string FilePath { get; set; }
        public Action<Download> Callback { get; set; }
    }

    /// <summary>
    /// Dependency object for <see cref="AliasService"/>.
    /// </summary>
    internal class AliasDependencies
    {
        public IHandleAliasData AliasDependency { get; set; }
        public string FilePath { get; set; }
    }

    /// <summary>
    /// Dependency object for <see cref="SettingsService"/>.
    /// </summary>
    internal class SettingsDependencies
    {
        public IHandleSettingsData SettingsDependency { get; set; }
        public string FilePath { get; set; }
    }

    /// <summary>
    /// Service for settings-related actions.
    /// </summary>
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Single, InstanceContextMode = InstanceContextMode.PerCall)]
    internal class SettingsService : ISettings
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public SettingsService(SettingsDependencies dependency)
        {
            _settings = dependency.SettingsDependency;
            _filePath = dependency.FilePath;
        }

        //Service Methods
        void ISettings.Update(Setting setting)
        {
            _settings.Update(setting);
        }
        void ISettings.Default(SettingName name)
        {
            _settings.Default(name);
        }
        void ISettings.DefaultAll()
        {
            _settings.DefaultAll();
        }

        /// <exception cref="System.ServiceModel.FaultException{T}">Throws when unable to access the configuration file.</exception>
        void ISettings.Save()
        {
            try
            {
                _settings.Accept(new SaveDataVisitor(_filePath));
            }
            catch (System.Configuration.ConfigurationErrorsException ex)
            {
                ConfigurationFault fault = new ConfigurationFault()
                {
                    Description = "Settings.Save(): Error accessing file " + ex.Filename
                };
                throw new FaultException<ConfigurationFault>(fault);
            }
        }

        /// <exception cref="System.ServiceModel.FaultException{T}">Throws when unable to access the configuration file.</exception>
        Setting[] ISettings.Load()
        {
            try
            {
                _settings.Accept(new LoadDataVisitor(_filePath));
                IList<Setting> settings = _settings.GetAllData();
                Setting[] settingsArray = new Setting[settings.Count];
                settings.CopyTo(settingsArray, 0);
                return settingsArray;
                
            }
            catch (System.Configuration.ConfigurationErrorsException ex)
            {
                ConfigurationFault fault = new ConfigurationFault()
                {
                    Description = "Settings.Load(): Error accessing file " + ex.Filename
                };
                throw new FaultException<ConfigurationFault>(fault);
            }
        }

        //Members
        private IHandleSettingsData _settings;
        private string _filePath;
    }

    /// <summary>
    /// Service for alias-related actions.
    /// </summary>
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Single, InstanceContextMode = InstanceContextMode.PerCall)]
    internal class AliasService : IAlias
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public AliasService(AliasDependencies dependency)
        {
            _aliases = dependency.AliasDependency;
            _filePath = dependency.FilePath;
        }

        //Service Methods
        void IAlias.Add(Alias alias)
        {
            _aliases.Add(alias);
        }
        void IAlias.Remove(string alias)
        {
            _aliases.Remove(alias);
        }
        void IAlias.Clear()
        {
            _aliases.Clear();
        }

        /// <exception cref="System.ServiceModel.FaultException{T}">Throws when unable to access the configuration file.</exception>
        void IAlias.Save()
        {
            try
            {
                _aliases.Accept(new SaveDataVisitor(_filePath));
            }
            catch (System.Configuration.ConfigurationErrorsException ex)
            {
                ConfigurationFault fault = new ConfigurationFault()
                {
                    Description = "Alias.Save(): Error accessing file " + ex.Filename
                };
                throw new FaultException<ConfigurationFault>(fault);
            }
        }

        /// <exception cref="System.ServiceModel.FaultException{T}">Throws when unable to access the configuration file.</exception>
        Alias[] IAlias.Load()
        {
            try
            {
                _aliases.Accept(new LoadDataVisitor(_filePath));
                IList<Alias> aliases = _aliases.GetAllData();
                Alias[] aliasesArray = new Alias[aliases.Count];
                aliases.CopyTo(aliasesArray, 0);
                return aliasesArray;
            }
            catch (System.Configuration.ConfigurationErrorsException ex)
            {
                ConfigurationFault fault = new ConfigurationFault()
                {
                    Description = "Alias.Load(): Error accessing file " + ex.Filename
                };
                throw new FaultException<ConfigurationFault>(fault);
            }
        }

        /// <exception cref="System.ServiceModel.FaultException{T}">Throws when unable to access the configuration file.</exception>
        void IAlias.ClearSaved()
        {
            try
            {
                _aliases.Accept(new ClearSavedDataVisitor(_filePath));
            }
            catch (System.Configuration.ConfigurationErrorsException ex)
            {
                ConfigurationFault fault = new ConfigurationFault()
                {
                    Description = "Alias.ClearSaved(): Error accessing file " + ex.Filename
                };
                throw new FaultException<ConfigurationFault>(fault);
            }
        }

        //Members
        private IHandleAliasData _aliases;
        private string _filePath;
    }

    /// <summary>
    /// Service for download-related actions.
    /// </summary>
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Single, InstanceContextMode = InstanceContextMode.PerCall)]
    internal class DownloadService : IDownload
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public DownloadService(DownloadDependencies dependency)
        {
            _downloads = dependency.DownloadDependency;
            _filePath = dependency.FilePath;
            _callback = dependency.Callback;          
        }

        //Service Methods
        void IDownload.Add(Download[] downloads)
        {
            List<Download> downloadList = new List<Download>(downloads);
            _downloads.Add(downloadList);
        }
        void IDownload.Remove(Download[] downloads)
        {
            List<Download> downloadList = new List<Download>(downloads);
            _downloads.Remove(downloadList);
        }
        void IDownload.Clear()
        {
            _downloads.Clear();
        }

        /// <exception cref="System.ServiceModel.FaultException{T}">Throws when unable to access the configuration file.</exception>
        void IDownload.Save()
        {
            try
            {
                _downloads.Accept(new SaveDataVisitor(_filePath));
            }
            catch (System.Configuration.ConfigurationErrorsException ex)
            {
                ConfigurationFault fault = new ConfigurationFault()
                {
                    Description = "Download.Save(): Error accessing file " + ex.Filename
                };
                throw new FaultException<ConfigurationFault>(fault);
            }
        }

        /// <exception cref="System.ServiceModel.FaultException{T}">Throws when unable to access the configuration file.</exception>
        Download[] IDownload.Load()
        {
            try
            {
                _downloads.Accept(new LoadDataVisitor(_filePath));
                IList<Download> downloads = _downloads.GetAllData();
                Download[] downloadArray = new Download[downloads.Count];
                downloads.CopyTo(downloadArray, 0);
                return downloadArray;
            }
            catch (System.Configuration.ConfigurationErrorsException ex)
            {
                ConfigurationFault fault = new ConfigurationFault()
                {
                    Description = "Download.Load(): Error accessing file " + ex.Filename
                };
                throw new FaultException<ConfigurationFault>(fault);
            }
        }

        /// <exception cref="System.ServiceModel.FaultException{T}">Throws when unable to access the configuration file.</exception>
        void IDownload.ClearSaved()
        {
            try
            {
                _downloads.Accept(new ClearSavedDataVisitor(_filePath));
            }
            catch (System.Configuration.ConfigurationErrorsException ex)
            {
                ConfigurationFault fault = new ConfigurationFault()
                {
                    Description = "Download.ClearSaved(): Error accessing file " + ex.Filename
                };
                throw new FaultException<ConfigurationFault>(fault);
            }
        }
        
        /// <summary>
        /// Starts the download queue.
        /// </summary>
        void IDownload.StartDownload()
        {
            if (_downloads.IsDownloading == false)
            {               
                _downloads.IsDownloading = true;
                Download download = _downloads.NextDownload(true);
                if (download != null)
                {
                    _callback(download);
                }
            }
        }

        //Members
        private IHandleDownloadData _downloads;
        private Action<Download> _callback;
        private string _filePath;
    }

    /// <summary>
    /// Service for download queue status updates.
    /// </summary>
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Single, InstanceContextMode = InstanceContextMode.PerSession)]
    internal class UpdateManager : IReceiveUpdates, IUpdateStatus
    {
        public void Subscribe()
        {
            _clientCallback = OperationContext.Current.GetCallbackChannel<IReceiveUpdatesCallback>();
            _statusUpdateHandler = (s, e) =>
                {                  
                    _clientCallback.StatusUpdate(e.Status);
                };
            StatusUpdateEvent += _statusUpdateHandler;

            _nextDownloadHandler = (s, e) =>
                {
                    _clientCallback.DownloadingNext(e.NextDownload);
                };
            NextDownloadEvent += _nextDownloadHandler;

            OperationContext.Current.InstanceContext.Closing += (s, e) => { Unsubscribe(); };
        }

        public void Unsubscribe()
        {
            StatusUpdateEvent -= _statusUpdateHandler;
            NextDownloadEvent -= _nextDownloadHandler;
        }

        public void PublishStatusUpdate(DownloadStatus status)
        {
            EventHandler<StatusEventArgs> handler = StatusUpdateEvent;

            if (handler != null)
            {
                handler(this, new StatusEventArgs(status));
            }
        }

        public void PublishNextDownload(Download download)
        {
            EventHandler<DownloadEventArgs> handler = NextDownloadEvent;

            if (handler != null)
            {
                handler(this, new DownloadEventArgs(download));
            }
        }

        //Members
        public static event EventHandler<StatusEventArgs> StatusUpdateEvent;
        public static event EventHandler<DownloadEventArgs> NextDownloadEvent;
        private EventHandler<StatusEventArgs> _statusUpdateHandler; 
        private EventHandler<DownloadEventArgs> _nextDownloadHandler;
        IReceiveUpdatesCallback _clientCallback;
    }

    /// <summary>
    /// <c>EventArgs</c> for the <c>StatusUpdateEvent</c> in <see cref="UpdateManager"/>.
    /// </summary>
    internal class StatusEventArgs : EventArgs
    {
        public StatusEventArgs(DownloadStatus status)
        {
            this.Status = status;
        }

        public DownloadStatus Status { get; private set; }
    }

    /// <summary>
    /// <c>EventArgs</c> for the <c>NextDownloadEvent</c> in <see cref="UpdateManager"/>.
    /// </summary>
    internal class DownloadEventArgs : EventArgs
    {
        public DownloadEventArgs(Download download)
        {
            NextDownload = download;
        }

        public Download NextDownload { get; private set; }
    }
}
