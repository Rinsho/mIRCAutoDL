//Contracts for the AutoDL service.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ServiceModel;
using System.Runtime.Serialization;

namespace AutoDL.ServiceContracts
{
    /// <summary>
    /// Defines the contract for using the download queue.
    /// </summary>
    [ServiceContract(SessionMode = SessionMode.Required, CallbackContract = typeof(IDownloadCallback))]
    public interface IDownload
    {
        //Add: Adds a bot with packet(s) to the queue
        [OperationContract]
        [FaultContract(typeof(InvalidPacketFault))]
        void Add(string name, List<int> packets);

        //Remove: Removes packet(s) from the queue
        [OperationContract(IsOneWay = true)]
        void Remove(string name, List<int> packets);

        //Remove: Removes a bot and all associated packets from the queue
        [OperationContract(IsOneWay = true)]
        void Remove(string name);

        //Clear: Removes all bots and packets from the queue
        [OperationContract(IsOneWay = true)]
        void Clear();

        //Save: Saves queue to the config file
        [OperationContract]
        [FaultContract(typeof(ConfigurationFault))]
        void Save();

        //Load: Loads queue from the config file
        [OperationContract]
        [FaultContract(typeof(ConfigurationFault))]
        OrderedDictionary Load();

        //ClearSaved: Removes queue from the config file
        [OperationContract]
        [FaultContract(typeof(ConfigurationFault))]
        void ClearSaved();
    }

    /// <summary>
    /// Defines the callback contract for updating the UI on the current download status.
    /// </summary>
    public interface IDownloadCallback
    {
        //Updates UI on current download
        [OperationContract(IsOneWay = true)]
        void DownloadStatusUpdate(DownloadStatus status);

        //Updates UI on next download
        [OperationContract(IsOneWay = true)]
        void Downloading(string name, int packet);
    }

    /// <summary>
    /// Defines the contract for managing download queue settings.
    /// </summary>
    [ServiceContract(SessionMode = SessionMode.Allowed)]
    public interface ISettings
    {
        //Update: Updates setting(s)
        [OperationContract]
        [FaultContract(typeof(InvalidSettingFault))]
        void Update(string setting, string value);

        //Default: Resets a setting to its default value
        [OperationContract]
        [FaultContract(typeof(InvalidSettingFault))]
        void Default(string setting);

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
        Dictionary<string, string> Load();
    }

    /// <summary>
    /// Defines the contract for managing aliases.
    /// </summary>
    [ServiceContract(SessionMode = SessionMode.Allowed)]
    public interface IAlias
    {
        //Add: Adds alias(es) 
        [OperationContract(IsOneWay = true)]
        void Add(string alias, string name);

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
        Dictionary<string, string> Load();

        //ClearSaved: Removes all aliases from the config file
        [OperationContract]
        [FaultContract(typeof(ConfigurationFault))]
        void ClearSaved();
    }

    /// <summary>
    /// Class to hold <c>Fault</c> information relating to packet errors.
    /// </summary>
    [DataContract]
    public class InvalidPacketFault
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Description { get; set; }
    }

    /// <summary>
    /// Class to hold <c>Fault</c> information relating to configuration file errors.
    /// </summary>
    [DataContract]
    public class ConfigurationFault
    {
        [DataMember]
        public string File { get; set; }
        [DataMember]
        public string Description { get; set; }
    }

    /// <summary>
    /// Class to hold <c>Fault</c> information relating to settings errors.
    /// </summary>
    [DataContract]
    public class InvalidSettingFault
    {
        [DataMember]
        public string Value { get; set; }
        [DataMember]
        public string Description { get; set; }
    }

    /// <summary>
    /// Represents the status of a download.
    /// </summary>
    public enum DownloadStatus : int { Success, Fail, Retry, QueueComplete };
}