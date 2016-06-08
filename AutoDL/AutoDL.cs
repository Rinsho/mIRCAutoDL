/*
 * TO-DO:  Finish up writing Wrapper methods.
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
        public AutoDLMain(Action<Data.Download> callback, string serviceExtension, string fileSettingsName = @"\AutoDL.dll.config")
        {
            FileSettingsPath += fileSettingsName;
            Service = new Services.ServiceManager(FileSettingsPath, callback);

            //Service Setup
            NetNamedPipeBinding binding = new NetNamedPipeBinding();
            host = new ServiceHost(Service,
                new Uri("net.pipe://localhost/AutoDL/" + serviceExtension));           
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
            throw new NotImplementedException();
        }

        //Members       
        private string FileSettingsPath = AppDomain.CurrentDomain.BaseDirectory;
        ServiceHost host;

        //Properties
        private Services.ServiceManager Service;
        public ServiceContracts.IDownload Queue
        {
            get
            {
                return Service as ServiceContracts.IDownload;
            }
        }
        public ServiceContracts.ISettings Settings
        {
            get
            {
                return Service as ServiceContracts.ISettings;
            }
        }
        public ServiceContracts.IAlias Aliases
        {
            get
            {
                return Service as ServiceContracts.IAlias;
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
