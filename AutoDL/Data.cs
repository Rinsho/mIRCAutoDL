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
    /// Interface for use with data-persisting (save/load/etc) visitor
    /// </summary>
    internal interface IPersistData
    {
        void Accept(IVisitAndPersistData stateHandler);
    }

    /// <summary>
    /// Handles data and actions related to download settings.
    /// </summary>
    /// <remarks>
    /// Implements <see cref="IEnumerable{T}"/>
    /// </remarks>
    internal class SettingsData : IPersistData, IEnumerable<string>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="filePath">Path to the configuration file.</param>
        public SettingsData()
        {
            Data = new Dictionary<string, string>() { { SettingsData.RETRY, null }, { SettingsData.DELAY, null } };
            this.DefaultAll();
        }

        /// <summary>
        /// Indexer
        /// </summary>
        /// <returns>Setting's value.</returns>
        public string this[string setting]
        {
            get
            {
                if (!String.IsNullOrEmpty(setting))
                {
                    return Data[setting];
                }
                return default(string);
            }

            private set
            {
                switch (setting)
                {
                    case RETRY:
                        if (value == "True" || value == "False")
                        {
                            Data[setting] = value;
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
                            Data[setting] = value;
                        }
                        else
                        {
                            throw new ArgumentException("Settings: Invalid DownloadDelay value (Integer > 0)", value);
                        }
                        break;
                    default:
                        throw new ArgumentException("Settings: Invalid Setting", setting);
                }
            }
        }

        //Methods

        /// <exception cref="System.ArgumentException">Throws when <c>setting</c> or <c>value</c> are invalid.</exception>
        public void Update(string setting, string value)
        {
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
        public void Accept(IVisitAndPersistData stateHandler)
        {
            stateHandler.Visit(this);
        }
        public Dictionary<string, string> GetData()
        {
            Dictionary<string, string> copy = new Dictionary<string, string>();
            foreach (KeyValuePair<string, string> setting in Data)
            {
                copy.Add(String.Copy(setting.Key), String.Copy(setting.Value));
            }
            return copy;
        }

        //Members
        public const string RETRY = "RetryFailedDownload";
        public const string DELAY = "DownloadDelay";
        private Dictionary<string, string> Data;

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
    internal class AliasData : IPersistData, IEnumerable<string>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="filePath">Path to the configuration file.</param>
        public AliasData()
        {
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
                return default(string);
            }

            private set
            {
                if (!String.IsNullOrEmpty(alias) && !String.IsNullOrEmpty(value))
                {
                    Data[alias] = value;
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
        public void Accept(IVisitAndPersistData stateHandler)
        {
            stateHandler.Visit(this);
        }
        public Dictionary<string, string> GetData()
        {
            Dictionary<string, string> copy = new Dictionary<string, string>();
            foreach (KeyValuePair<string, string> alias in Data)
            {
                copy.Add(String.Copy(alias.Key), String.Copy(alias.Value));
            }
            return copy;
        }

        //Members
        private Dictionary<string, string> Data;

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
    internal class DownloadData : IPersistData, IEnumerable<string>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="filePath">Path to the configuration file.</param>
        public DownloadData()
        {
            Data = new OrderedDictionary();
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
                if (!String.IsNullOrEmpty(key))
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
        public void Accept(IVisitAndPersistData stateHandler)
        {
            stateHandler.Visit(this);
        }
        public OrderedDictionary GetData()
        {
            OrderedDictionary copy = new OrderedDictionary();
            foreach (DictionaryEntry entry in Data)
            {
                List<int> packets = new List<int>(entry.Value as List<int>);
                copy.Add(String.Copy(entry.Key as string), packets);
            }
            return copy;
        }

        /// <summary>
        /// Used to retrieve a download from the queue.
        /// </summary>
        /// <param name="retry">True to send current download again, false to send next.</param>
        /// <returns>Returns valid <see cref="Download"/> or one with invalid values if no download exists.</returns>
        public Download NextDownload(bool retry)
        {
            Download nextDownload = new Download() { Name = "", Packet = 0 };

            if (Data.Count == 0)
            {
                IsDownloading = false;
                return nextDownload;
            }
          
            DictionaryEntry entry = Data.Cast<DictionaryEntry>().ElementAt(0);
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
                            entry = Data.Cast<DictionaryEntry>().ElementAt(0);
                            name = entry.Key as string;
                            packets = entry.Value as List<int>;
                        }
                    }
                }
                IsDownloading = false;
                return nextDownload;
            }     
        }

        //Members
        private OrderedDictionary Data;
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
