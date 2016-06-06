using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ServiceModel;

namespace AutoDL.ServiceContracts
{
    /* Interface: IDownload
     * Description: Defines the contract for downloading.  Functions as both
     *              the wrapper and ServiceHost contract.
     */
    [ServiceContract(SessionMode = SessionMode.Required, CallbackContract=typeof(IDownloadCallback))]
    public interface IDownload
    {
        //Add: Adds a bot with packet(s) to the queue
        [OperationContract]
        void Add(string name, List<int> packets);

        //Remove: Removes bot and packet(s) from the queue
        [OperationContract]
        void Remove(Dictionary<string, List<int>> data);

        //Remove: Removes a bot and all associated packets from the queue
        [OperationContract]
        void Remove(string name);

        //Clear: Removes all bots and packets from the queue
        [OperationContract]
        void Clear();

        //Save: Saves queue to the config file
        [OperationContract]
        void Save();

        //Load: Loads queue from the config file
        [OperationContract]
        OrderedDictionary Load();

        //ClearSaved: Removes queue from the config file
        [OperationContract]
        void ClearSaved();
    }

    public interface IDownloadCallback
    {
        //Used to update UI on download status
        [OperationContract]
        void DownloadStatusUpdate(DownloadStatus status);
    }

    public enum DownloadStatus : int {SUCCESS, FAIL, RETRY};

    /* Interface: ISettings
     * Description: Defines the contract for handling settings.  Functions as
     *              both the wrapper and ServiceHost contract.
     */
    [ServiceContract]
    public interface ISettings
    {
        //Update: Updates setting(s)
        [OperationContract]
        void Update(Dictionary<string, string> data);

        //Default: Resets a setting to its default value
        [OperationContract]
        void Default(string setting);

        //DefaultAll: Resets all settings to their default values
        [OperationContract]
        void DefaultAll();

        //Save: Saves settings to the config file
        [OperationContract]
        void Save();

        //Load: Loads settings from the config file
        [OperationContract]
        Dictionary<string, string> Load();
    }

    /* Interface: IAlias
     * Description: Defines the contract for handling aliases.  Functions as
     *              both the wrapper and ServiceHost contract.
     */
    [ServiceContract]
    public interface IAlias
    {
        //Add: Adds alias(es) 
        [OperationContract]
        void Add(Dictionary<string, string> data);

        //Remove: Removes an alias
        [OperationContract]
        void Remove(string alias);

        //Clear: Removes all aliases
        [OperationContract]
        void Clear();

        //Save: Saves aliases to the config file
        [OperationContract]
        void Save();

        //Load: Loads aliases from the config file
        [OperationContract]
        Dictionary<string, string> Load();

        //ClearSaved: Removes all aliases from the config file
        [OperationContract]
        void ClearSaved();
    }
}