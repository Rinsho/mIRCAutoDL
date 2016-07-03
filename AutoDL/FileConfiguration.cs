//The classes which handle configuration file actions.

using System;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Configuration;

namespace AutoDL.FileConfiguration
{
    /// <summary>
    /// Interface for data-persisting visitor
    /// </summary>
    internal interface IVisitAndPersistData
    {
        void Visit(Data.DownloadData data);
        void Visit(Data.AliasData data);
        void Visit(Data.SettingsData data);
    }

    /// <summary>
    /// Base class for visitors which handle the configuration file.
    /// </summary>
    internal abstract class ConfigDataVisitor : IVisitAndPersistData
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="filePath">Path to the configuration file.</param>
        public ConfigDataVisitor(string filePath)
        {
            FilePath = filePath;
        }

        /// <summary>
        /// Opens the configuration file with <c>ConfigurationUserLevel.None</c> and <c>preload=false</c>.
        /// </summary>
        /// <returns><see cref="Configuration"/> object.</returns>
        protected Configuration OpenConfigFile()
        {
            ExeConfigurationFileMap configFile = new ExeConfigurationFileMap() { ExeConfigFilename = FilePath };
            Configuration config = ConfigurationManager.OpenMappedExeConfiguration(configFile, ConfigurationUserLevel.None, false);
            return config;
        }

        /// <summary>
        /// Saves the configuration file with <c>ConfigurationSaveMode.Modified</c> option.
        /// </summary>
        /// <param name="config"><see cref="Configuration"/> object.</param>
        /// <param name="section">Optional section to be refreshed after save.</param>
        protected void SaveFile(Configuration config, string section = null)
        {
            config.Save(ConfigurationSaveMode.Modified);
            if (section != null)
            {
                ConfigurationManager.RefreshSection(section);
            }
        }

        protected void CheckForValidAliasSection(Configuration config)
        {
            if (config.Sections.Get(AliasSection.SECTION_NAME) == null)
            {
                config.Sections.Add(AliasSection.SECTION_NAME, new AliasSection());
                SaveFile(config, AliasSection.SECTION_NAME);
            }
        }
        protected void CheckForValidDownloadSection(Configuration config)
        {
            if (config.Sections.Get(DLQueueSection.SECTION_NAME) == null)
            {
                config.Sections.Add(DLQueueSection.SECTION_NAME, new DLQueueSection());
                SaveFile(config, DLQueueSection.SECTION_NAME);
            }
        }

        public abstract void Visit(Data.DownloadData data);
        public abstract void Visit(Data.AliasData data);
        public abstract void Visit(Data.SettingsData data);

        //Members
        protected string FilePath;
    }

    /// <summary>
    /// Visitor that saves data to the configuration file.
    /// </summary>
    internal class SaveDataVisitor : ConfigDataVisitor
    {
        public SaveDataVisitor(string filePath) : base(filePath) { }

        public override void Visit(Data.DownloadData data)
        {
            Data.Download nextItem = data.NextDownload(false);
            if (nextItem.Name != "")
            {
                Configuration config = OpenConfigFile();
                CheckForValidDownloadSection(config);
                DLQueueSection queueSection = config.GetSection(DLQueueSection.SECTION_NAME) as DLQueueSection;
                DLQueueCollection queueCollection = queueSection.Queue;

                while (nextItem.Name != "")
                {
                    DLQueueItemElement queueItem = new DLQueueItemElement();
                    PacketCollection packetsCollection = queueItem.PacketList;
                    queueItem.BotName = nextItem.Name;

                    do
                    {
                        PacketElement packetItem = new PacketElement();
                        packetItem.Packet = nextItem.Packet;
                        packetsCollection.Add(packetItem);
                        data.Remove(nextItem.Name, nextItem.Packet);
                        nextItem = data.NextDownload(false);
                    } while (queueItem.BotName == nextItem.Name);

                    queueCollection.Add(queueItem);
                }
                SaveFile(config, DLQueueSection.SECTION_NAME);
            }
        }

        public override void Visit(Data.AliasData data)
        {
            Configuration config = OpenConfigFile();
            CheckForValidAliasSection(config);
            AliasSection aliasSection = config.GetSection(AliasSection.SECTION_NAME) as AliasSection;
            AliasElement newAlias;

            aliasSection.Aliases.Clear();
            foreach (string alias in data)
            {
                newAlias = new AliasElement();
                newAlias.Name = alias;
                newAlias.Alias = data[alias];
                aliasSection.Aliases.Add(newAlias);
            }
            SaveFile(config, AliasSection.SECTION_NAME);
        }

        public override void Visit(Data.SettingsData data)
        {
            Configuration config = OpenConfigFile();

            foreach (string setting in data)
            {
                if (!String.IsNullOrEmpty(data[setting]))
                {
                    foreach (string key in config.AppSettings.Settings.AllKeys)
                    {
                        if (setting == key)
                        {
                            config.AppSettings.Settings[setting].Value = data[setting];
                        }
                        else
                        {
                            config.AppSettings.Settings.Add(setting, data[setting]);
                        }
                    }
                }
            }
            SaveFile(config, "appSettings");
        }
    }

    /// <summary>
    /// Visitor that loads data from the configuration file.
    /// </summary>
    internal class LoadDataVisitor : ConfigDataVisitor
    {
        public LoadDataVisitor(string filePath) : base(filePath) { }

        public override void Visit(Data.DownloadData data)
        {
            Configuration config = OpenConfigFile();
            CheckForValidDownloadSection(config);
            DLQueueSection queueSection = config.GetSection(DLQueueSection.SECTION_NAME) as DLQueueSection;

            foreach (DLQueueItemElement queueItem in queueSection.Queue)
            {
                List<int> loadedPackets = new List<int>();
                foreach (PacketElement packet in queueItem.PacketList)
                {
                    loadedPackets.Add(packet.Packet);
                }
                data.Add(queueItem.BotName, loadedPackets);
            }

            queueSection.Queue.Clear();
            SaveFile(config, DLQueueSection.SECTION_NAME);
        }

        public override void Visit(Data.AliasData data)
        {
            Configuration config = OpenConfigFile();
            CheckForValidAliasSection(config);
            AliasSection aliasSection = config.GetSection(AliasSection.SECTION_NAME) as AliasSection;

            foreach (AliasElement alias in aliasSection.Aliases)
            {
                data.Add(alias.Alias, alias.Name);
            }
        }

        public override void Visit(Data.SettingsData data)
        {
            Configuration config = OpenConfigFile();
            string value;

            foreach (string setting in config.AppSettings.Settings.AllKeys)
            {
                value = config.AppSettings.Settings[setting].Value;
                data.Update(setting, value);
            }
        }
    }

    /// <summary>
    /// Visitor that clears saved data in the configuration file.
    /// </summary>
    internal class ClearSavedDataVisitor : ConfigDataVisitor
    {
        public ClearSavedDataVisitor(string filePath) : base(filePath) { }

        public override void Visit(Data.DownloadData data)
        {
            Configuration config = OpenConfigFile();
            CheckForValidDownloadSection(config);
            DLQueueSection queueSection = config.GetSection(DLQueueSection.SECTION_NAME) as DLQueueSection;
            queueSection.Queue.Clear();
            SaveFile(config, DLQueueSection.SECTION_NAME);
        }

        public override void Visit(Data.AliasData data)
        {
            Configuration config = OpenConfigFile();
            CheckForValidAliasSection(config);
            AliasSection aliasSection = config.GetSection(AliasSection.SECTION_NAME) as AliasSection;
            aliasSection.Aliases.Clear();
            SaveFile(config, AliasSection.SECTION_NAME);
        }

        public override void Visit(Data.SettingsData data)
        {
            data.DefaultAll();
            data.Accept(new SaveDataVisitor(FilePath));
        }
    }
}
