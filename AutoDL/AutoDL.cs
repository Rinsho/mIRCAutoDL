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
        public AutoDLMain(AutoDLCaller callbackFunc, string serviceExtension, string fileSettingsName = @"\AutoDL.dll.config")
        {
            FileSettingsPath += fileSettingsName;
            queue = new Services.DownloadService(FileSettingsPath, callbackFunc);

            //Service Setup
            host = new ServiceHost(queue,
                new Uri("net.pipe://localhost/AutoDL/" + serviceExtension));
            host.AddServiceEndpoint(typeof(ServiceContracts.IDownload),
                new NetNamedPipeBinding(),
                "Download");
            host.AddServiceEndpoint(typeof(ServiceContracts.ISettings),
                new NetNamedPipeBinding(),
                "Settings"); 
            host.AddServiceEndpoint(typeof(ServiceContracts.IAlias),
                 new NetNamedPipeBinding(),
                 "Alias");
            host.Open();
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
        public WrapperContracts.IUpdate Status
        {
            get
            {
                return queue as WrapperContracts.IUpdate;
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

    /* Delegate: AutoDLCaller
     * Description: Provides a function pointer for the DLL to callback on to return Download objects.
     */
    public delegate void AutoDLCaller(Download nextDownload);
}
