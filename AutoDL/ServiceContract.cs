using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ServiceModel;

namespace AutoDL
{
    public interface IUpdate
    {
        void DownloadUpdate(bool success);
    }

    [ServiceContract]
    public interface IDownload
    {
        [OperationContract]
        bool Add(string name, List<int> packets);

        [OperationContract]
        void Remove(Dictionary<string, List<int>> data);

        [OperationContract]
        void Remove(string name);

        [OperationContract]
        void Clear();

        [OperationContract]
        void Save();

        [OperationContract]
        OrderedDictionary Load();

        [OperationContract]
        void ClearSaved();
    }

    [ServiceContract]
    public interface ISettings
    {
        [OperationContract]
        void Update(Dictionary<string, string> data);

        [OperationContract]
        void Default(string setting);

        [OperationContract]
        void DefaultAll();

        [OperationContract]
        void Save();

        [OperationContract]
        Dictionary<string, string> Load();
    }

    [ServiceContract]
    public interface IAlias
    {
        [OperationContract]
        void Add(Dictionary<string, string> data);

        [OperationContract]
        void Remove(string alias);

        [OperationContract]
        void Clear();

        [OperationContract]
        void Save();

        [OperationContract]
        Dictionary<string, string> Load();
    }
}