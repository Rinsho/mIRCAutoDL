using System;
using System.Collections.Generic;
using System.ServiceModel;

namespace AutoDL
{
    /* Class: AutoDLMain
     * Description: Entry-point for the DLL.  Initializes and maintains the main class DownloadManager.
     */
    public class AutoDLMain
    {
        public AutoDLMain(AutoDLCaller callbackFunc, string serviceExtension, string fileSettingsName = @"\AutoDL.dll.config")
        {
            FileSettingsPath += fileSettingsName;
            queue = new DownloadManager(FileSettingsPath, callbackFunc);
        }
        
        //Members       
        private string FileSettingsPath = AppDomain.CurrentDomain.BaseDirectory;

        //Properties
        private DownloadManager queue;
        public IDownload Queue
        {
            get
            {
                return queue as IDownload;
            }
        }
        public ISettings Settings
        {
            get
            {
                return queue.SettingsMgr as ISettings;
            }
        }
        public IAlias Aliases
        {
            get
            {
                return queue.AliasMgr as IAlias;
            }
        }
        public IUpdate Status
        {
            get
            {
                return queue as IUpdate;
            }
        }
    }

    /* Delegate: AutoDLCaller
     * Description: Provides a function pointer for the DLL to callback on to return Download objects.
     */
    public delegate void AutoDLCaller(Download nextDownload);
}

/*Service Setup
             * 
            //GOING TO NEED TO IMPLMEENT THREADING AND RUN SERVICE
            //ON ITS OWN THREAD.
             * 
            //ALSO NEED TO USE INSTANCECONTEXTMODE.SINGLE FOR SERVICEBEHAVIORATTRIBUTE
            //ON CLASSES IN ORDER TO PASS OBJECT INTO SERVICEHOST WHICH MEANS I NEED
            //TO IMPLEMENT THEM AS SINGLETONS WITH LOCKS.
            //THIS ALLOWS ME TO USE THE SAME INSTANCES FOR BOTH PROGRAMMATIC (VIA WRAPPER)
            //AND SERVICE ACCESS WHICH IS THE INTENDED FUNCTIONALITY.
             * 
             * 
            using (ServiceHost host = new ServiceHost(queue,
                new Uri("net.pipe://localhost/AutoDL/" + serviceExtension)))
                {
                    host.AddServiceEndpoint(typeof(IDownload),
                        new NetNamedPipeBinding(),
                        "Connect");

                    host.Open();

                    //STUFF
             
                    host.Close();
                }   
*/
