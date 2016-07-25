//The classes which represent data and actions related to
//settings, aliases, and the queue/downloads.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

using AutoDL.FileConfiguration;
using AutoDL.ServiceContracts;

namespace AutoDL.Data
{
    /// <summary>
    /// Interface for class which uses a data-persisting visitor
    /// </summary>
    internal interface IPersistData
    {
        void Accept(IVisitAndPersistData stateHandler);
    }

    /// <summary>
    /// Interface for class that handles settings data.
    /// </summary>
    internal interface IHandleSettingsData : IPersistData
    {
        object this[SettingName name] { get; }
        void Update(Setting setting);
        void Default(SettingName name);
        void DefaultAll();
        IList<Setting> GetAllData();
    }

    /// <summary>
    /// Interface for class that handles alias data.
    /// </summary>
    internal interface IHandleAliasData : IPersistData
    {
        string this[string alias] { get; }
        void Add(Alias alias);
        void Remove(string alias);
        void Clear();
        IList<Alias> GetAllData();
    }

    /// <summary>
    /// Interface for class that handles download data.
    /// </summary>
    internal interface IHandleDownloadData : IPersistData
    {
        void Add(List<Download> downloads);
        void Remove(List<Download> downloads);
        void Clear();
        IList<Download> GetAllData();
        Download NextDownload(bool retry);
        bool IsDownloading { get; set; }
    }

    /// <summary>
    /// Handles data and actions related to settings.
    /// </summary>
    internal class SettingsData : IHandleSettingsData
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public SettingsData()
        {
            _lock = new object();
            _data = new List<Setting>();
            _data.Add(new Setting(SettingName.RetryFailedDownload, RETRY_DEFAULT));
            _data.Add(new Setting(SettingName.DownloadDelay, DELAY_DEFAULT));
            _data.TrimExcess();
        }

        //Members
        public static readonly bool RETRY_DEFAULT = false;
        public static readonly int DELAY_DEFAULT = 5;
        private List<Setting> _data;
        private object _lock;

        //IHandleSettingsData implementation
        public object this[SettingName name]
        {
            get
            {
                lock (_lock)
                {
                    return _data[(int)name].Value;
                }
            }
        }

        public void Update(Setting setting)
        {
            lock (_lock)
            {
                _data[(int)setting.Name] = setting;
            }
        }
        public void Default(SettingName name)
        {
            object value = null;
            switch (name)
            {
                case SettingName.RetryFailedDownload:
                    value = RETRY_DEFAULT;
                    break;
                case SettingName.DownloadDelay:
                    value = DELAY_DEFAULT;
                    break;
            }
            this.Update(new Setting(name, value));
        }
        public void DefaultAll()
        {
            this.Default(SettingName.RetryFailedDownload);
            this.Default(SettingName.DownloadDelay);
        }
        public IList<Setting> GetAllData()
        {
            lock (_lock)
            {
                return _data.AsReadOnly();
            }
        }

        //IPersistData implementation
        public void Accept(IVisitAndPersistData stateHandler)
        {
            stateHandler.Visit(this);
        }
    }

    /// <summary>
    /// Handles data and actions related to aliases.
    /// </summary>
    internal class AliasData : IHandleAliasData
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public AliasData()
        {
            _lock = new object();
            _data = new Dictionary<string, Alias>();
        }

        //Members
        private Dictionary<string, Alias> _data;
        private object _lock;

        //IHandleAliasData implementation
        public string this[string alias]
        {
            get
            {
                if (_data.ContainsKey(alias))
                {
                    lock (_lock)
                    {
                        if (_data.ContainsKey(alias))
                        {
                            return _data[alias].Name;
                        }
                    }
                }
                return null;
            }
        }

        public void Add(Alias alias)
        {
            lock (_lock)
            {
                _data[alias.AliasName] = alias;
            }
        }
        public void Remove(string alias)
        {
            lock (_lock)
            {
                _data.Remove(alias);
            }
        }
        public void Clear()
        {
            lock (_lock)
            {
                _data.Clear();
            }
        }
        public IList<Alias> GetAllData()
        {
            lock (_lock)
            {
                return _data.Values.ToList<Alias>().AsReadOnly();
            }
        }

        //IPersistData implementation
        public void Accept(IVisitAndPersistData stateHandler)
        {
            stateHandler.Visit(this);
        }
    }

    /// <summary>
    /// Handles data and actions related to the queue/downloads.
    /// </summary>
    internal class DownloadData : IHandleDownloadData
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public DownloadData()
        {
            _lock = new object();
            _data = new List<Download>();
            IsDownloading = false;
        }

        //Members
        private List<Download> _data;
        private object _lock;

        //Properties
        public bool IsDownloading { get; set; }

        //IHandleDownloadData implementation
        public void Add(List<Download> downloads)
        {
            lock (_lock)
            {
                _data.AddRange(downloads);
            }
        }
        public void Remove(List<Download> downloads)
        {
            lock (_lock)
            {
                foreach (Download d in downloads)
                {
                    _data.Remove(d);
                }
            }
        }
        public void Clear()
        {
            lock (_lock)
            {
                _data.Clear();
            }
        }      
        public IList<Download> GetAllData()
        {
            lock (_lock)
            {
                return _data.AsReadOnly();
            }
        }

        /// <summary>
        /// Used to retrieve a download from the queue.
        /// </summary>
        /// <param name="retry">True to send current download again, false to send next.</param>
        /// <returns>Returns valid <see cref="Download"/> or null if no download exists.</returns>
        public Download NextDownload(bool retry)
        {
            if (retry)
            {
                lock (_lock)
                {
                    if (_data.Count != 0)
                    {
                        return _data[0];
                    }
                    else
                    {
                        IsDownloading = false;
                        return null;
                    }
                }
            }
            else
            {
                lock (_lock)
                {
                    if (_data.Count > 0)
                    {
                        _data.RemoveAt(0);
                        if (_data.Count > 0)
                        {
                            return _data[0];
                        }
                        else
                        {
                            IsDownloading = false;
                            return null;
                        }
                    }
                    else
                    {
                        IsDownloading = false;
                        return null;
                    }
                }
            } 
        }

        //IPersistData implementation
        public void Accept(IVisitAndPersistData stateHandler)
        {
            stateHandler.Visit(this);
        }
    }
}
