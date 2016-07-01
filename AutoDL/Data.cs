//The classes which represent data and actions related to
//settings, aliases, and the queue/downloads.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using AutoDL.FileConfiguration;

namespace AutoDL.Data
{
    /// <summary>
    /// Handles data and actions related to download settings.
    /// </summary>
    /// <remarks>
    /// Implements <see cref="IEnumerable{T}"/>
    /// </remarks>
    internal class SettingsData : IEnumerable<string>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="filePath">Path to the configuration file.</param>
        public SettingsData(string filePath)
        {
            Data = new Dictionary<string, string>() 
            { { SettingsData.RETRY, null }, { SettingsData.DELAY, null } };
            this.DefaultAll();
            this.FilePath = filePath;
        }

        /// <summary>
        /// Indexer
        /// </summary>
        /// <returns>Setting's value.</returns>
        internal string this[string setting]
        {
            get
            {
                return Data[setting];
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
                        else
                        {
                            throw new ArgumentException("Settings: Invalid RetryFailedDownload value (True|False)", value);
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
                        else
                        {
                            throw new ArgumentException("Settings: Invalid DownloadDelay value (Int32 > 0)", value);
                        }
                        break;
                }
            }
        }

        //Methods

        /// <exception cref="System.ArgumentException">Throws when <c>setting</c> or <c>value</c> are invalid.</exception>
        public void Update(string setting, string value)
        {
            if (!Data.ContainsKey(setting))
            {
                throw new ArgumentException("Settings.Update(): Invalid Setting", setting);
            }
            this[setting] = value;
        }

        /// <exception cref="System.ArgumentException">Throws when <c>setting</c> is invalid.</exception>
        public void Default(string setting)
        {
            switch (setting)
            {
                case RETRY:
                    this[setting] = "False";
                    break;
                case DELAY:
                    this[setting] = "5";
                    break;
                default:
                    throw new ArgumentException("Settings.Default(): Invalid Setting", setting);
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
        public const string RETRY = "RetryFailedDownload";
        public const string DELAY = "DownloadDelay";
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

    /// <summary>
    /// Handles data and actions related to aliases.
    /// </summary>
    /// <remarks>
    /// Implements <see cref="IEnumerable{T}"/>
    /// </remarks>
    internal class AliasData : IEnumerable<string>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="filePath">Path to the configuration file.</param>
        public AliasData(string filePath)
        {
            this.FilePath = filePath;
            Data = new Dictionary<string, string>();
        }

        /// <summary>
        /// Indexer
        /// </summary>
        /// <returns>Associated name.</returns>
        public string this[string alias]
        {
            get
            {
                if (!String.IsNullOrEmpty(alias) && Data.ContainsKey(alias))
                {
                    return Data[alias];
                }
                else
                {
                    return default(string);
                }
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
        public void Add(string alias, string name)
        {
            this[alias] = name;
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

    /// <summary>
    /// Handles data and actions related to the queue/downloads.
    /// </summary>
    /// <remarks>
    /// Implements <see cref="IEnumerable{T}"/>
    /// </remarks>
    internal class DownloadData : IEnumerable<string>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="filePath">Path to the configuration file.</param>
        public DownloadData(string filePath)
        {
            Data = new OrderedDictionary();
            this.FilePath = filePath;
            IsDownloading = false;
        }

        /// <summary>
        /// Indexer
        /// </summary>
        /// <param name="key">Bot name.</param>
        /// <returns>Associated packet list.</returns>
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

        /// <exception cref="InvalidPacketException">Throws when packet list is empty.</exception>
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
                throw new InvalidPacketException(name, "Add(): No valid packets.");
            }
        }
        public void Remove(string name, List<int> packets)
        {
            if (packets.Count == 0)
            {
                this.Remove(name);
            }
            else
            {
                if (Data.Contains(name))
                {
                    (Data[name] as List<int>).RemoveAll(x => packets.Contains(x));
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
                (Data[name] as List<int>).Remove((int)packet);
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

        /// <exception cref="InvalidDownloadException">Throws when queue is empty.</exception>
        public Download NextDownload(bool retry)
        {
            if (Data.Count == 0)
            {
                IsDownloading = false;
                throw new InvalidDownloadException();
            }

            Download nextDownload = new Download();
            IEnumerable<DictionaryEntry> RawData = Data.Cast<DictionaryEntry>();
            DictionaryEntry entry = RawData.ElementAt(0);
            string name = entry.Key as string;
            List<int> packets = entry.Value as List<int>;

            if (retry)
            {
                nextDownload.Name = name;
                nextDownload.Packet = packets[0];
                return nextDownload;
            }
            else
            {
                packets.RemoveAt(0);
                while (Data.Count > 0)
                {
                    if (packets.Count > 0)
                    {
                        nextDownload.Name = name;
                        nextDownload.Packet = packets[0];
                        return nextDownload;
                    }
                    else
                    {
                        Data.Remove(name);
                        if (Data.Count > 0)
                        {
                            entry = RawData.ElementAt(0);
                            name = entry.Key as string;
                            packets = entry.Value as List<int>;
                        }
                    }
                }
                IsDownloading = false;
                throw new InvalidDownloadException();
            }     
        }

        //Members
        private OrderedDictionary Data;
        private string FilePath;
        public bool IsDownloading;

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

    /// <summary>
    /// Represents a single download.
    /// </summary>
    public class Download
    {
        public string Name { get; set; }
        public int Packet { get; set; }
    }

    /// <summary>
    /// Represents an invalid download request (i.e. a request when no downloads exist)
    /// </summary>
    public class InvalidDownloadException : Exception 
    {
        public InvalidDownloadException() : base() { }
    }

    /// <summary>
    /// Represents an invalid packet list
    /// </summary>
    public class InvalidPacketException : Exception
    {
        public InvalidPacketException(string name, string message) : base(message)
        {
            this.Name = name;
        }

        //Members
        public string Name;
    }
}
