//Main DLL class.

using System;
using System.Collections.Generic;
using System.ServiceModel;

using AutoDL.ServiceContracts;
using AutoDL.Services;
using AutoDL.Data;
using AutoDL.WCF;

namespace AutoDL
{
    /// <summary>
    /// Main DLL class.  Handles service hosting and wrapper interaction.
    /// </summary>
    /// <remarks>
    /// Implements <c>Dispose pattern</c>.  Call either <c>Close()</c> or <c>Dipose()</c>
    /// when finished with object or use implicit disposal (ex. using(AutoDLMain){...})!
    /// </remarks>
    public class AutoDLMain : IDisposable
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="wrapperCallback">Wrapper callback function for receiving download information.</param>
        /// <param name="serviceExtension">Extension for configuring <c>ServiceHost</c> endpoint.</param>
        public AutoDLMain(Action<Download> wrapperCallback, string serviceExtension)
        {
            _filePath = AppDomain.CurrentDomain.BaseDirectory + "\\AutoDL\\AutoDL.dll.config";         
            _wrapperCallback = wrapperCallback;
            _serviceExtension = serviceExtension;
            AutoUpdate = true;

            //Data Setup
            _downloadData = new DownloadData();
            _aliasData = new AliasData();
            _settingsData = new SettingsData();

            string baseServiceAddress = "net.pipe://localhost/AutoDL/" + _serviceExtension;

            //Download Service Setup
            NetNamedPipeBinding binding = new NetNamedPipeBinding();
            _downloadHost = new DependentServiceHost<DownloadDependencies>(
                typeof(DownloadService),
                new DownloadDependencies() {
                    DownloadDependency = _downloadData,
                    FilePath = _filePath,
                    Callback = SendDownload
                },
                new Uri(baseServiceAddress));           
            _downloadHost.AddServiceEndpoint(typeof(IDownload), binding, "Download");
            
            //Settings Service Setup
            binding = new NetNamedPipeBinding();
            _settingsHost = new DependentServiceHost<SettingsDependencies>(
                typeof(SettingsService),
                new SettingsDependencies()
                {
                    SettingsDependency = _settingsData,
                    FilePath = _filePath,
                },
                new Uri(baseServiceAddress));
            _settingsHost.AddServiceEndpoint(typeof(ISettings), binding, "Settings");
            
            //Alias Service Setup
            binding = new NetNamedPipeBinding();
            _aliasHost = new DependentServiceHost<AliasDependencies>(
                typeof(AliasService),
                new AliasDependencies()
                {
                    AliasDependency = _aliasData,
                    FilePath = _filePath,
                },
                new Uri(baseServiceAddress));
            _aliasHost.AddServiceEndpoint(typeof(IAlias), binding, "Alias");
            
            //Update Service Setup
            binding = new NetNamedPipeBinding();
            binding.ReceiveTimeout = new TimeSpan(1, 0, 0);
            _updateHost = new ServiceHost(typeof(UpdateManager),
                new Uri(baseServiceAddress + "/Update"));
            _updateHost.AddServiceEndpoint(typeof(IReceiveUpdates), binding, "Subscribe");
            _updateHost.AddServiceEndpoint(typeof(IUpdateStatus), binding, "Publish");           
        }

        /// <summary>
        /// Checks if a download is using a registered alias
        /// </summary>
        /// <returns>Download with full name.</returns>
        private Download CheckForAlias(Download download)
        {
            string actualName = _aliasData[download.Name];
            if (actualName != null)
            {
                return new Download(actualName, download.Packet);
            }
            return download;
        }

        /// <summary>
        /// Sends out initial download.  Called from <c>DownloadService.StartDownload()</c>.
        /// </summary>
        internal void SendDownload(Download download)
        {
            if (_autoUpdate)
            {
                _publishClient.PublishNextDownload(download);
            }
            Download checkedDownload = CheckForAlias(download);
            _wrapperCallback(checkedDownload);           
        }

        /// <summary>
        /// Used to request next download based on status of current download.
        /// </summary>
        /// <param name="success">Indicates whether the current download completed.</param>
        public void RequestNextDownload(bool success)
        {
            bool retry = !success && (bool)_settingsData[SettingName.RetryFailedDownload];
            int downloadDelay = (int)_settingsData[SettingName.DownloadDelay];
            Download download = _downloadData.NextDownload(retry);
            if (_autoUpdate)
            {
                if (success)
                {
                    _publishClient.PublishStatusUpdate(DownloadStatus.Success);
                }
                else if (retry)
                {
                    _publishClient.PublishStatusUpdate(DownloadStatus.Retry);
                }
                else
                {
                    _publishClient.PublishStatusUpdate(DownloadStatus.Fail);
                }
            }

            if (download != null)
            {
                _publishClient.PublishNextDownload(download);
                download = CheckForAlias(download);
                _delayTimer = new System.Threading.Timer(new System.Threading.TimerCallback(x =>
                {
                    try
                    {
                        _wrapperCallback(download);
                    }
                    finally
                    {
                        (x as System.Threading.Timer).Dispose();
                    }
                }));
                _delayTimer.Change(downloadDelay * 1000, System.Threading.Timeout.Infinite);
            }           
        }

        /// <summary>
        /// Opens the services for use.
        /// </summary>
        public void Open()
        {
            //Start Services
            _downloadHost.Open();
            _settingsHost.Open();
            _aliasHost.Open();
            _updateHost.Open();

            _autoUpdate = AutoUpdate;
            if (_autoUpdate)
            {
                _publishClient = new ServiceClients.UpdatePublisherClient(_serviceExtension);
                _publishClient.Open();
            }
        }

        /// <summary>
        /// Disposes the services.
        /// </summary>
        public void Close()
        {
            this.Dispose();
        }

        //Members       
        private DependentServiceHost<DownloadDependencies> _downloadHost;
        private DependentServiceHost<AliasDependencies> _aliasHost;
        private DependentServiceHost<SettingsDependencies> _settingsHost;
        private ServiceHost _updateHost;
        private DownloadData _downloadData;
        private AliasData _aliasData;
        private SettingsData _settingsData;
        private ServiceClients.UpdatePublisherClient _publishClient;
        private string _filePath;
        private string _serviceExtension;
        private Action<Download> _wrapperCallback;
        private System.Threading.Timer _delayTimer;

        //Properties
        private bool _autoUpdate;
        public bool AutoUpdate { get; set; }

        //Dispose implementation
        private bool _disposed = false;
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    if (_downloadHost.State != CommunicationState.Closing || _downloadHost.State != CommunicationState.Closed)
                    {
                        _downloadHost.Close();
                        _downloadHost = null;
                    }
                    if (_aliasHost.State != CommunicationState.Closing || _aliasHost.State != CommunicationState.Closed)
                    {
                        _aliasHost.Close();
                        _aliasHost = null;
                    }
                    if (_settingsHost.State != CommunicationState.Closing || _settingsHost.State != CommunicationState.Closed)
                    {
                        _settingsHost.Close();
                        _settingsHost = null;
                    }
                    if (_updateHost.State != CommunicationState.Closing || _updateHost.State != CommunicationState.Closed)
                    {
                        _updateHost.Close();
                        _updateHost = null;
                    }
                    if (_publishClient != null)
                    {
                        _publishClient.Close();
                        _publishClient = null;
                    }
                }
            }
            _disposed = true;
        }
    }
}
