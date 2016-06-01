using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Runtime.Serialization;

namespace AutoDL
{
    internal abstract class KeyValueData<K,V> : IEnumerable<K>
    {
        public KeyValueData()
        {
            Data = new Dictionary<K, V>();
        }

        //Indexer
        public virtual V this[K key]
        {
            get
            {
                if (Data.ContainsKey(key))
                {
                    return Data[key];
                }
                return default(V);
            }

            set { }
        }

        //Members
        protected Dictionary<K, V> Data;

        //IEnumberable Implementation
        public IEnumerator<K> GetEnumerator()
        {
            foreach (K key in Data.Keys)
            {
                yield return key;
            }
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }

    internal class SettingsData : KeyValueData<string, string>
    {
        //Indexer
        public override string this[string setting]
        {
            get
            {
                return base[setting];
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
            }
        }
        public void DefaultAll()
        {
            this[RETRY] = "False";
            this[DELAY] = "5";
        }

        //Members
        private const string RETRY = "RetryFailedDownload";
        private const string DELAY = "DownloadDelay";
    }

    internal class AliasData : KeyValueData<string, string>
    {
        //Indexer
        public override string this[string alias]
        {
            get
            {
                return base[alias];
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
        public bool Remove(string alias)
        {
            return Data.Remove(alias);
        }
        public void Clear()
        {
            Data.Clear();
        }
    }

    internal class DownloadData : IEnumerable<string>
    {
        public DownloadData()
        {
            Data = new OrderedDictionary();
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

    [DataContract]
    public class Download
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
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
