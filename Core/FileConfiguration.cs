//The classes which handle configuration file actions.

using System;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Configuration;

using AutoDL.Data;
using AutoDL.ServiceContracts;

namespace AutoDL.FileConfiguration
{
    /// <summary>
    /// Interface for data-persisting visitor
    /// </summary>
    internal interface IVisitAndPersistData
    {
        void Visit(IHandleDownloadData data);
        void Visit(IHandleAliasData data);
        void Visit(IHandleSettingsData data);
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
            _filePath = filePath;
        }

        /// <summary>
        /// Opens the configuration file with <c>ConfigurationUserLevel.None</c> and <c>preload=false</c>.
        /// </summary>
        /// <returns><see cref="Configuration"/> object.</returns>
        protected Configuration OpenConfigFile()
        {
            ExeConfigurationFileMap configFile = new ExeConfigurationFileMap() { ExeConfigFilename = _filePath };
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

        public abstract void Visit(IHandleDownloadData data);
        public abstract void Visit(IHandleAliasData data);
        public abstract void Visit(IHandleSettingsData data);

        //Members
        protected string _filePath;
    }

    /// <summary>
    /// Visitor that saves data to the configuration file.
    /// </summary>
    internal class SaveDataVisitor : ConfigDataVisitor
    {
        public SaveDataVisitor(string filePath) : base(filePath) { }

        public override void Visit(IHandleDownloadData data)
        {
            Download nextItem = data.NextDownload(false);
            if (nextItem != null)
            {
                Configuration config = OpenConfigFile();
                CheckForValidDownloadSection(config);
                DLQueueSection queueSection = config.GetSection(DLQueueSection.SECTION_NAME) as DLQueueSection;
                DLQueueCollection queueCollection = queueSection.Queue;

                while (nextItem != null)
                {
                    DLQueueItemElement queueItem = new DLQueueItemElement();
                    PacketCollection packetsCollection = queueItem.PacketList;
                    queueItem.BotName = nextItem.Name;

                    do
                    {
                        PacketElement packetItem = new PacketElement();
                        packetItem.Packet = nextItem.Packet;
                        packetsCollection.Add(packetItem);
                        nextItem = data.NextDownload(false);
                    } while (queueItem.BotName == nextItem.Name);

                    queueCollection.Add(queueItem);
                }
                SaveFile(config, DLQueueSection.SECTION_NAME);
            }
        }

        public override void Visit(IHandleAliasData data)
        {
            Configuration config = OpenConfigFile();
            CheckForValidAliasSection(config);
            AliasSection aliasSection = config.GetSection(AliasSection.SECTION_NAME) as AliasSection;
            AliasElement newAlias;

            aliasSection.Aliases.Clear();
            foreach (Alias alias in data.GetAllData())
            {
                newAlias = new AliasElement();
                newAlias.Name = alias.Name;
                newAlias.Alias = alias.AliasName;
                aliasSection.Aliases.Add(newAlias);
            }
            SaveFile(config, AliasSection.SECTION_NAME);
        }

        public override void Visit(IHandleSettingsData data)
        {
            Configuration config = OpenConfigFile();

            foreach (Setting setting in data.GetAllData())
            {
                string settingName = Enum.GetName(typeof(SettingName), setting.Name);
                foreach (string key in config.AppSettings.Settings.AllKeys)
                {
                    if (settingName == key)
                    {
                        config.AppSettings.Settings[settingName].Value = setting.Value as string;
                    }
                    else
                    {
                        config.AppSettings.Settings.Add(settingName, setting.Value as string);
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

        public override void Visit(IHandleDownloadData data)
        {
            Configuration config = OpenConfigFile();
            CheckForValidDownloadSection(config);
            DLQueueSection queueSection = config.GetSection(DLQueueSection.SECTION_NAME) as DLQueueSection;

            List<Download> loadedDownloads = new List<Download>();
            foreach (DLQueueItemElement queueItem in queueSection.Queue)
            {
                foreach (PacketElement packet in queueItem.PacketList)
                {
                    loadedDownloads.Add(new Download(queueItem.BotName, packet.Packet));
                }
            }
            queueSection.Queue.Clear();
            SaveFile(config, DLQueueSection.SECTION_NAME);
            data.Add(loadedDownloads);
        }

        public override void Visit(IHandleAliasData data)
        {
            Configuration config = OpenConfigFile();
            CheckForValidAliasSection(config);
            AliasSection aliasSection = config.GetSection(AliasSection.SECTION_NAME) as AliasSection;

            foreach (AliasElement alias in aliasSection.Aliases)
            {
                data.Add(new Alias(alias.Alias, alias.Name));
            }
        }

        public override void Visit(IHandleSettingsData data)
        {
            Configuration config = OpenConfigFile();
            string value;

            foreach (string setting in config.AppSettings.Settings.AllKeys)
            {
                SettingName settingEnum = (SettingName) Enum.Parse(typeof(SettingName), setting, true);
                value = config.AppSettings.Settings[setting].Value;
                data.Update(new Setting(settingEnum, value));
            }
        }
    }

    /// <summary>
    /// Visitor that clears saved data in the configuration file.
    /// </summary>
    internal class ClearSavedDataVisitor : ConfigDataVisitor
    {
        public ClearSavedDataVisitor(string filePath) : base(filePath) { }

        public override void Visit(IHandleDownloadData data)
        {
            Configuration config = OpenConfigFile();
            CheckForValidDownloadSection(config);
            DLQueueSection queueSection = config.GetSection(DLQueueSection.SECTION_NAME) as DLQueueSection;
            queueSection.Queue.Clear();
            SaveFile(config, DLQueueSection.SECTION_NAME);
        }

        public override void Visit(IHandleAliasData data)
        {
            Configuration config = OpenConfigFile();
            CheckForValidAliasSection(config);
            AliasSection aliasSection = config.GetSection(AliasSection.SECTION_NAME) as AliasSection;
            aliasSection.Aliases.Clear();
            SaveFile(config, AliasSection.SECTION_NAME);
        }

        public override void Visit(IHandleSettingsData data)
        {
            data.DefaultAll();
            data.Accept(new SaveDataVisitor(_filePath));
        }
    }
}
