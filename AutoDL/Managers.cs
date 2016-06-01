﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading;

namespace AutoDL
{
    internal class SettingsManager : ISettings
    {
        public SettingsManager(string filePath)
        {
            FilePath = filePath;
            Settings = new SettingsData();
        }

        //Methods
        public void Update(Dictionary<string, string> settings)
        {
            Settings.Update(settings);
        }
        public void Default(string setting)
        {
            Settings.Default(setting);
        }
        public void DefaultAll()
        {
            Settings.DefaultAll();
        }
        public void Save()
        {
            SettingsConfig SettingsFile = new SettingsConfig(FilePath);
            SettingsFile.Save(Settings);
        }
        public Dictionary<string, string> Load()
        {
            SettingsConfig SettingsFile = new SettingsConfig(FilePath);
            return SettingsFile.Load(Settings);
        }
        internal string GetSettingValue(string setting)
        {
            return Settings[setting];
        }

        //Members
        private SettingsData Settings;
        private string FilePath;
    }

    internal class AliasManager : IAlias
    {
        public AliasManager(string filePath)
        {
            FilePath = filePath;
            Aliases = new AliasData();
        }

        //Methods
        public void Add(Dictionary<string, string> aliases)
        {
            Aliases.Add(aliases);
        }
        public void Remove(string alias)
        {
            Aliases.Remove(alias);

        }
        public void Clear()
        {
            Aliases.Clear();
        }
        public void Save()
        {
            AliasConfig AliasFile = new AliasConfig(FilePath);
            AliasFile.Save(Aliases);
        }
        public Dictionary<string, string> Load()
        {
            AliasConfig AliasFile = new AliasConfig(FilePath);
            return AliasFile.Load(Aliases);
        }
        public void ClearSaved()
        {
            AliasConfig AliasFile = new AliasConfig(FilePath);
            AliasFile.ClearSaved();
        }
        internal string GetName(string alias)
        {
            return Aliases[alias];
        }

        //Members
        private AliasData Aliases;
        private string FilePath;
    }

    internal class DownloadManager : IDownload, IUpdate
    {
        public DownloadManager(string filePath, AutoDLCaller callback)
        {
            FilePath = filePath;
            DLCallback = callback;
            Queue = new DownloadData();
            AliasMgr = new AliasManager(FilePath);
            SettingsMgr = new SettingsManager(FilePath);
        }

        //Methods
        public bool Add(string name, List<int> packets)
        {
            try
            {
                string fullName = AliasMgr.GetName(name);
                if (String.IsNullOrEmpty(fullName))
                {
                    fullName = name;
                }
                Queue.Add(fullName, packets);
                return true;
            }
            catch (InvalidPacketException)
            {
                return false;
            }
        }
        public void Remove(Dictionary<string, List<int>> data)
        {
            foreach (string name in data.Keys)
            {
                string fullName = AliasMgr.GetName(name);
                if (!String.IsNullOrEmpty(fullName))
                {
                    data.Add(fullName, data[name]);
                    data.Remove(name);
                }
            }

            Queue.Remove(data);
        }
        public void Remove(string name)
        {
            string fullName = AliasMgr.GetName(name);
            if (String.IsNullOrEmpty(fullName))
            {
                fullName = name;
            }
            Queue.Remove(fullName);
        }
        public void Clear()
        {
            Queue.Clear();
        }
        public void Save()
        {
            QueueConfig QueueFile = new QueueConfig(FilePath);
            QueueFile.Save(Queue);
        }
        public OrderedDictionary Load()
        {
            QueueConfig QueueFile = new QueueConfig(FilePath);
            return QueueFile.Load(Queue);
        }
        public void ClearSaved()
        {
            QueueConfig QueueFile = new QueueConfig(FilePath);
            QueueFile.ClearSaved();
        }
        public void DownloadUpdate(bool success)
        {
            if (!success && Convert.ToBoolean(SettingsMgr.GetSettingValue("RetryFailedDownload")))
            {
                //SEND DOWNLOAD AGAIN
                //UPDATE UI
            }
            else
            {
                this.SendNextDownload();
            }
        }
        private void SendNextDownload()
        {
            //REMOVE MOST RECENT DOWNLOAD

            int downloadDelay = Convert.ToInt32(SettingsMgr.GetSettingValue("DownloadDelay"));
            Timer DelayTimer = new Timer(new TimerCallback(x =>
            {
                DLCallback(Queue.NextDownload());
                (x as Timer).Dispose();
            }));
            DelayTimer.Change(downloadDelay * 1000, Timeout.Infinite);

            //UPDATE UI
        }

        //Members
        private DownloadData Queue;
        private string FilePath;
        private AutoDLCaller DLCallback;

        //Download-related Managers
        internal AliasManager AliasMgr;
        internal SettingsManager SettingsMgr;
    }
}