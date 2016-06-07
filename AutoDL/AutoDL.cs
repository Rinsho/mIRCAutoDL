/*
 * TO-DO:  Fix up initialization after I finish the Service classes.
 * 
 */

using System;
using System.Collections.Generic;
using System.ServiceModel;

namespace AutoDL
{
    /* Class: AutoDLMain
     * Description: Entry-point for the DLL.  Initializes and maintains the main class DownloadManager.
     */
    public class AutoDLMain : IDisposable
    {
        public AutoDLMain(Action<Data.Download> callbackFunc, string serviceExtension, string fileSettingsName = @"\AutoDL.dll.config")
        {
            FileSettingsPath += fileSettingsName;
            queue = new Services.DownloadService(FileSettingsPath, callbackFunc);

            //Service Setup
            host = new ServiceHost(queue,
                new Uri("net.pipe://localhost/AutoDL/" + serviceExtension));
            NetNamedPipeBinding binding = new NetNamedPipeBinding();
            host.AddServiceEndpoint(typeof(ServiceContracts.IDownload),
                binding, "Download");
            host.AddServiceEndpoint(typeof(ServiceContracts.ISettings),
                binding, "Settings"); 
            host.AddServiceEndpoint(typeof(ServiceContracts.IAlias),
                 binding, "Alias");
            host.Open();
        }
        
        //Wrapper Methods
        
        void DownloadStatusUpdate(bool success)
        {
            //Implement
        }

        //Members       
        private string FileSettingsPath = AppDomain.CurrentDomain.BaseDirectory;
        ServiceHost host;

        //Properties
        private Services.DownloadService queue;
        public ServiceContracts.IDownload Queue
        {
            get
            {
                return queue as ServiceContracts.IDownload;
            }
        }
        public ServiceContracts.ISettings Settings
        {
            get
            {
                return queue.SettingsMgr as ServiceContracts.ISettings;
            }
        }
        public ServiceContracts.IAlias Aliases
        {
            get
            {
                return queue.AliasMgr as ServiceContracts.IAlias;
            }
        }

        //Dispose implementation
        private bool disposed = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposed) { return; }
            if (disposing)
            {
                if (host.State != CommunicationState.Closing || host.State != CommunicationState.Closed)
                {
                    host.Close();
                }
            }
            disposed = true;
        }
    }
}
