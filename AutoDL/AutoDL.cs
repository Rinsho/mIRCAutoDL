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
        public AutoDLMain(Action<Data.Download> callback, string serviceExtension)
        {
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
        public void DownloadStatusUpdate(bool success)
        {
            Service.SendDownload(success);
        }

        //Members       
        private string FileSettingsPath = AppDomain.CurrentDomain.BaseDirectory + "AutoDL.dll.config";
        ServiceHost host;
        private Services.ServiceManager Service;

        //Dispose implementation
        public void Close()
        {
            this.Dispose();
        }
        private bool disposed = false;
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    if (host.State != CommunicationState.Closing || host.State != CommunicationState.Closed)
                    {
                        host.Close();
                    }
                }
            }
            disposed = true;
        }
    }
}
