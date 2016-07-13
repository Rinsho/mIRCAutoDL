//Contracts for the AutoDL service.

using System;
using System.Collections.Generic;
using System.ServiceModel;

namespace AutoDL.ServiceContracts
{
    /// <summary>
    /// Defines the contract for using the download queue.
    /// </summary>
    [ServiceContract(SessionMode = SessionMode.Allowed)]
    public interface IDownload
    {
        //Add: Adds downloads to the queue.
        [OperationContract(IsOneWay = true)]
        void Add(Download[] downloads);

        //Remove: Removes downloads from the queue.
        [OperationContract(IsOneWay = true)]
        void Remove(Download[] downloads);

        //Clear: Removes all bots and packets from the queue
        [OperationContract(IsOneWay = true)]
        void Clear();

        //StartDownload: Starts the download queue.
        [OperationContract(IsOneWay = true)]
        void StartDownload();

        //Save: Saves queue to the config file
        [OperationContract]
        [FaultContract(typeof(ConfigurationFault))]
        void Save();

        //Load: Loads queue from the config file
        [OperationContract]
        [FaultContract(typeof(ConfigurationFault))]
        Download[] Load();

        //ClearSaved: Removes queue from the config file
        [OperationContract]
        [FaultContract(typeof(ConfigurationFault))]
        void ClearSaved();      
    }

    /// <summary>
    /// Defines the contract for managing download queue settings.
    /// </summary>
    [ServiceContract(SessionMode = SessionMode.Allowed)]
    public interface ISettings
    {
        //Update: Updates setting
        [OperationContract(IsOneWay = true)]
        void Update(Setting setting);

        //Default: Resets a setting to its default value
        [OperationContract(IsOneWay = true)]
        void Default(SettingName name);

        //DefaultAll: Resets all settings to their default values
        [OperationContract(IsOneWay = true)]
        void DefaultAll();

        //Save: Saves settings to the config file
        [OperationContract]
        [FaultContract(typeof(ConfigurationFault))]
        void Save();

        //Load: Loads settings from the config file
        [OperationContract]
        [FaultContract(typeof(ConfigurationFault))]
        Setting[] Load();
    }

    /// <summary>
    /// Defines the contract for managing aliases.
    /// </summary>
    [ServiceContract(SessionMode = SessionMode.Allowed)]
    public interface IAlias
    {
        //Add: Adds alias(es) 
        [OperationContract(IsOneWay = true)]
        void Add(Alias alias);

        //Remove: Removes an alias
        [OperationContract(IsOneWay = true)]
        void Remove(string alias);

        //Clear: Removes all aliases
        [OperationContract(IsOneWay = true)]
        void Clear();

        //Save: Saves aliases to the config file
        [OperationContract]
        [FaultContract(typeof(ConfigurationFault))]
        void Save();

        //Load: Loads aliases from the config file
        [OperationContract]
        [FaultContract(typeof(ConfigurationFault))]
        Alias[] Load();

        //ClearSaved: Removes all aliases from the config file
        [OperationContract]
        [FaultContract(typeof(ConfigurationFault))]
        void ClearSaved();
    }
}