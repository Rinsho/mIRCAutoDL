/*
 * TO-DO: Fix this jumbled mess.
 * 
 * Also tidy up the classes now that ServiceContracts are finalized.  And re-write some
 * of the bool-returns since I'll be using exceptions now instead.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Runtime.Serialization;

namespace AutoDL.Data
{
    internal interface IHaveSettings
    {
        public string this[string setting] { get; }
    }

    internal interface IHaveAliases
    {
        public string this[string alias] { get; }
    }

    /* Class: SettingsData
     * Description: Handles download settings.
     */
    internal class SettingsData : IHaveSettings, IEnumerable<string>
    {
        public SettingsData(string filePath)
        {
            Data = new Dictionary<string, string>();
            this.FilePath = filePath;
        }

        //Indexer
        public string this[string setting]
        {
            get
            {
                if (Data.ContainsKey(setting))
                {
                    return Data[setting];
                }
                return null;
            }           

            set
            {
                switch (setting)
                {
                    case RETRY:
                        if (value == "True" || value == "False")
                        {
                            if (Data.ContainsKey(setting))
                            {
                                Data[setting] = value;
                            }
                            else
                            {
                                Data.Add(setting, value);
                            }
                        }
                        break;
                    case DELAY:
                        int val;
                        bool valid = Int32.TryParse(value, out val);
                        if (valid && val > 0)
                        {
                            if (Data.ContainsKey(setting))
                            {
                                Data[setting] = value;                               
                            }
                            else
                            {
                                Data.Add(setting, value);
                            }
                        }
                        break;
                }
            }
        }

        //Methods
        public void Update(Dictionary<string, string> settings)
        {
            foreach (KeyValuePair<string, string> setting in settings)
            {
                if (this[setting.Key] == null)
                {
                    throw new ArgumentException("Settings Update: Invalid Key: " + setting.Key);
                }
                this[setting.Key] = setting.Value;
            }
        }
        public void Default(string key)
        {
            switch (key)
            {
                case RETRY:
                    this[key] = "False";
                    break;
                case DELAY:
                    this[key] = "5";
                    break;
                default:
                    throw new ArgumentException("Settings Default: Invalid Key: " + key);
            }
        }
        public void DefaultAll()
        {
            this[RETRY] = "False";
            this[DELAY] = "5";
        }
        public void Save()
        {
            SettingsConfig SettingsFile = new SettingsConfig(FilePath);
            SettingsFile.Save(this);
        }
        public Dictionary<string, string> Load()
        {
            SettingsConfig SettingsFile = new SettingsConfig(FilePath);
            return SettingsFile.Load(this);
        }

        //Members
        private const string RETRY = "RetryFailedDownload";
        private const string DELAY = "DownloadDelay";
        private Dictionary<string, string> Data;
        private string FilePath;

        //IEnumberable Implementation
        public IEnumerator<string> GetEnumerator()
        {
            foreach (string key in Data.Keys)
            {
                yield return key;
            }
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }

    /* Class: AliasData
     * Description: Handles the alias feature.
     */
    internal class AliasData : IHaveAliases, IEnumerable<string>
    {
        public AliasData(string filePath)
        {
            this.FilePath = filePath;
            Data = new Dictionary<string, string>();
        }

        //Indexer
        public string this[string alias]
        {
            get
            {
                if (Data.ContainsKey(alias))
                {
                    return Data[alias];
                }
                return null;
            }

            set
            {
                if (!String.IsNullOrEmpty(alias) && !String.IsNullOrEmpty(value))
                {
                    if (Data.ContainsKey(alias))
                    {
                        Data[alias] = value;
                    }
                    else
                    {
                        Data.Add(alias, value);
                    }
                }
            }
        }

        //Methods
        public void Add(Dictionary<string, string> aliases)
        {
            foreach (KeyValuePair<string, string> alias in aliases)
            {
                this[alias.Key] = alias.Value;
            }
        }
        public void Remove(string alias)
        {
            Data.Remove(alias);
        }
        public void Clear()
        {
            Data.Clear();
        }
        public void Save()
        {
            AliasConfig AliasFile = new AliasConfig(FilePath);
            AliasFile.Save(this);
        }
        public Dictionary<string, string> Load()
        {
            AliasConfig AliasFile = new AliasConfig(FilePath);
            return AliasFile.Load(this);
        }
        public void ClearSaved()
        {
            AliasConfig AliasFile = new AliasConfig(FilePath);
            AliasFile.ClearSaved();
        }

        //Members
        private Dictionary<string, string> Data;
        private string FilePath;

        //IEnumberable Implementation
        public IEnumerator<string> GetEnumerator()
        {
            foreach (string key in Data.Keys)
            {
                yield return key;
            }
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }

    /* Class: DownloadData
     * Description: Handles all functionality related to the
     *              download feature.
     */
    internal class DownloadData : IEnumerable<string>
    {
        public DownloadData(string filePath)
        {
            Data = new OrderedDictionary();
            this.FilePath = filePath;
        }

        //Indexer
        public List<int> this[string key]
        {
            get
            {
                if (Data.Contains(key))
                {
                    return Data[key] as List<int>;
                }
                return default(List<int>);
            }
        }

        //Methods
        public void Add(OrderedDictionary data)
        {
            foreach (DictionaryEntry entry in data)
            {
                this.Add(entry.Key as string, entry.Value as List<int>);
            }
        }
        public void Add(string name, List<int> packets)
        {
            if (packets.Count > 0)
            {
                if (Data.Contains(name))
                {
                    (Data[name] as List<int>).AddRange(packets);
                }
                else
                {
                    Data.Add(name, packets);
                }
            }
            else
            {
                throw new InvalidPacketException();
            }
        }
        public void Remove(Dictionary<string, List<int>> data)
        {
            foreach (KeyValuePair<string, List<int>> pair in data)
            {
                if (pair.Value.Count == 0)
                {
                    this.Remove(pair.Key);
                }
                else
                {
                    if (Data.Contains(pair.Key))
                    {
                        (Data[pair.Key] as List<int>).RemoveAll((x) => pair.Value.Contains(x));
                    }
                }
            }
        }
        public void Remove(string name, int? packet = null)
        {
            if (packet == null)
            {
                Data.Remove(name);
            }
            else
            {
                (Data[name] as List<int>).Remove(Convert.ToInt32(packet));
            }
        }
        public void Clear()
        {
            Data.Clear();
        }
        public void Save()
        {
            QueueConfig QueueFile = new QueueConfig(FilePath);
            QueueFile.Save(this);
        }
        public OrderedDictionary Load()
        {
            QueueConfig QueueFile = new QueueConfig(FilePath);
            return QueueFile.Load(this);
        }
        public void ClearSaved()
        {
            QueueConfig QueueFile = new QueueConfig(FilePath);
            QueueFile.ClearSaved();
        }       

        //CHANGE TO REMOVE/SEARCH IF INVALID BOT IS ENCOUNTERED
        public Download NextDownload()
        {
            Download nextDownload = new Download();
            if (Data.Count > 0)
            {
                DictionaryEntry entry = (DictionaryEntry)Data[0];
                string name = entry.Key as string;
                List<int> packets = entry.Value as List<int>;

                if (packets.Count > 0)
                {
                    nextDownload.Name = name;
                    nextDownload.Packet = packets[0];
                }
                else
                {
                    throw new InvalidDownloadException(name + " is invalid.  Packet count is " + packets.Count + ".");
                }
            }
            else
            {
                nextDownload.Name = null;
            }
            return nextDownload;
        }

        //Members
        private OrderedDictionary Data;
        private string FilePath;

        //IEnumberable Implementation
        public IEnumerator<string> GetEnumerator()
        {
            foreach (string key in Data.Keys)
            {
                yield return key;
            }
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }

    /* Class: Download
     * Description: Represents a single download.
     */
    public class Download
    {
        public string Name { get; set; }
        public int Packet { get; set; }
    }

    [Serializable]
    internal class InvalidDownloadException : Exception 
    {
        public InvalidDownloadException(string message) : base(message) { }
    }

    [Serializable]
    internal class InvalidPacketException : Exception
    {
        public InvalidPacketException() : base() { }
    }
}
