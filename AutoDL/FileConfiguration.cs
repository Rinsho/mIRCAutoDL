/*
 * TO-DO: Comments.
 */

using System;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Configuration;

namespace AutoDL
{
    internal abstract class CustomConfigManager
    {
        public CustomConfigManager(string filePath)
        {
            SettingsPath = filePath;
            CheckForValidFile();
        }

        //Methods
        protected abstract void CheckForValidFile();
        protected Configuration OpenConfigFile()
        {
            ExeConfigurationFileMap configFile = new ExeConfigurationFileMap() { ExeConfigFilename = SettingsPath };
            Configuration config = ConfigurationManager.OpenMappedExeConfiguration(configFile, ConfigurationUserLevel.None, false);
            return config;
        }
        protected void SaveFile(Configuration config, string section = null)
        {
            config.Save(ConfigurationSaveMode.Modified);
            if (section != null)
            {
                ConfigurationManager.RefreshSection(section);
            }
        }

        //Members
        private readonly string SettingsPath;
    }

    internal class SettingsConfig : CustomConfigManager
    {
        public SettingsConfig(string filePath) : base(filePath) { }

        //Methods
        protected override void CheckForValidFile()
        {
            Configuration config = OpenConfigFile();
            
            foreach (string key in config.AppSettings.Settings.AllKeys)
            {
                if (String.IsNullOrEmpty(config.AppSettings.Settings[key].ToString()))
                {
                    config.AppSettings.Settings.Remove(key);
                }
            }

            SaveFile(config, "appSettings");
        }
        public void Save(Data.SettingsData newSettings)
        {
            Configuration config = OpenConfigFile();

            foreach (string setting in newSettings)
            {
                if (!String.IsNullOrEmpty(newSettings[setting]))
                {
                    foreach (string key in config.AppSettings.Settings.AllKeys)
                    {
                        if (setting == key)
                        {
                            config.AppSettings.Settings[setting].Value = newSettings[setting];
                        }
                        else
                        {
                            config.AppSettings.Settings.Add(setting, newSettings[setting]);
                        }
                    }
                }
            }

            base.SaveFile(config, "appSettings");
        }
        public Dictionary<string, string> Load(Data.SettingsData settings)
        {
            Dictionary<string, string> loadedSettings = new Dictionary<string, string>();
            Configuration config = OpenConfigFile();

            foreach (string setting in config.AppSettings.Settings.AllKeys)
            {
                loadedSettings.Add(setting, config.AppSettings.Settings[setting].Value);
            }

            settings.Update(loadedSettings);
            return loadedSettings;
        }
    }

    internal class AliasConfig : CustomConfigManager
    {
        public AliasConfig(string filePath) : base(filePath) { }

        protected override void CheckForValidFile()
        {
            Configuration config = OpenConfigFile();

            if (config.Sections.Get(AliasSection.SECTION_NAME) == null)
            {
                config.Sections.Add(AliasSection.SECTION_NAME, new AliasSection());
                SaveFile(config);
            }
        }
        public void Save(Data.AliasData aliases)
        {
            Configuration config = OpenConfigFile();
            AliasSection aliasSection = config.GetSection(AliasSection.SECTION_NAME) as AliasSection;
            AliasElement newAlias;

            aliasSection.Aliases.Clear();
            foreach (string alias in aliases)
            {
                newAlias = new AliasElement();
                newAlias.Name = alias;
                newAlias.Alias = aliases[alias];
                aliasSection.Aliases.Add(newAlias);
            }

            base.SaveFile(config, AliasSection.SECTION_NAME);
        }
        public Dictionary<string, string> Load(Data.AliasData data)
        {
            Configuration config = OpenConfigFile();
            AliasSection aliasSection = config.GetSection(AliasSection.SECTION_NAME) as AliasSection;
            Dictionary<string, string> loaded = new Dictionary<string, string>();
            foreach (AliasElement alias in aliasSection.Aliases)
            {
                loaded.Add(alias.Alias, alias.Name);
            }
            data.Add(loaded);
            return loaded;
        }
        public void ClearSaved()
        {
            Configuration config = OpenConfigFile();
            AliasSection aliasSection = config.GetSection(AliasSection.SECTION_NAME) as AliasSection;
            aliasSection.Aliases.Clear();
            SaveFile(config, AliasSection.SECTION_NAME);
        }
    }

    internal class QueueConfig : CustomConfigManager
    {
        public QueueConfig(string filePath) : base(filePath) { }

        protected override void CheckForValidFile()
        {
            Configuration config = OpenConfigFile();

            if (config.Sections.Get(DLQueueSection.SECTION_NAME) == null)
            {
                config.Sections.Add(DLQueueSection.SECTION_NAME, new DLQueueSection());
                SaveFile(config);
            }
        }
        public void Save(Data.DownloadData queue)
        {
            Data.Download nextItem = queue.NextDownload();
            if (nextItem.Name != null)
            {
                Configuration config = OpenConfigFile();
                DLQueueSection queueSection = config.GetSection(DLQueueSection.SECTION_NAME) as DLQueueSection;
                DLQueueCollection queueCollection = queueSection.Queue;

                while (nextItem.Name != null)
                {
                    DLQueueItemElement queueItem = new DLQueueItemElement();
                    PacketCollection packetsCollection = queueItem.PacketList;
                    queueItem.BotName = nextItem.Name;

                    do
                    {
                        PacketElement packetItem = new PacketElement();
                        packetItem.Packet = nextItem.Packet;
                        packetsCollection.Add(packetItem);
                        queue.Remove(nextItem.Name, nextItem.Packet);
                        nextItem = queue.NextDownload();
                    } while (queueItem.BotName == nextItem.Name);

                    queueCollection.Add(queueItem);
                }

                base.SaveFile(config, DLQueueSection.SECTION_NAME);
            }
        }
        public OrderedDictionary Load(Data.DownloadData queue)
        {
            Configuration config = OpenConfigFile();

            DLQueueSection queueSection = config.GetSection(DLQueueSection.SECTION_NAME) as DLQueueSection;
            OrderedDictionary loaded = new OrderedDictionary();

            foreach (DLQueueItemElement queueItem in queueSection.Queue)
            {
                List<int> loadedPackets = new List<int>();
                foreach (PacketElement packet in queueItem.PacketList)
                {
                    loadedPackets.Add(packet.Packet);
                }

                loaded.Add(queueItem.BotName, loadedPackets);
            }

            queue.Add(loaded);
            queueSection.Queue.Clear();
            SaveFile(config, DLQueueSection.SECTION_NAME);
            return loaded;
        }
        public void ClearSaved()
        {
            Configuration config = OpenConfigFile();
            DLQueueSection queueSection = config.GetSection(DLQueueSection.SECTION_NAME) as DLQueueSection;
            queueSection.Queue.Clear();
            SaveFile(config, DLQueueSection.SECTION_NAME);
        }
    }
}
