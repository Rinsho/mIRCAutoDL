using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using System.Runtime.InteropServices;

namespace AutoDL
{
    public abstract class CustomConfigManager
    {
        public CustomConfigManager()
        {
            CheckForValidFile();
        }

        //Methods

        //File validity check is necessary but implementation-dependent
        protected abstract void CheckForValidFile();

        protected Configuration OpenConfigFile()
        {
            ExeConfigurationFileMap configFile = new ExeConfigurationFileMap() { ExeConfigFilename = SettingsPath };
            Configuration config = ConfigurationManager.OpenMappedExeConfiguration(configFile, ConfigurationUserLevel.None, false);
            return config;
        }

        //Allow custom overriding of Save
        protected virtual void SaveConfigFile(Configuration config, string section = null)
        {
            config.Save(ConfigurationSaveMode.Modified);
            if (section != null)
            {
                ConfigurationManager.RefreshSection(section);
            }
        }

        //Members
        private readonly string SettingsPath = AppDomain.CurrentDomain.BaseDirectory + @"\AutoDL.dll.config";
    }

    public class SettingsConfig : CustomConfigManager
    {
        public SettingsConfig() : base() {}

        //Methods
        protected override void CheckForValidFile()
        {
            Configuration config = OpenConfigFile();
            bool notify = false, retry = false, delay = false;
            
            foreach (string key in config.AppSettings.Settings.AllKeys)
            {
                switch (key)
                {
                    case "Notifications":
                        notify = true;
                        break;
                    case "RetryFailedDownload":
                        retry = true;
                        break;
                    case "DownloadDelay":
                        delay = true;
                        break;
                }
            }
            if (!notify)
            {
                config.AppSettings.Settings.Add("Notifications", "True");
            }
            if (!retry)
            {
                config.AppSettings.Settings.Add("RetryFailedDownload", "False");
            }
            if (!delay)
            {
                config.AppSettings.Settings.Add("DownloadDelay", "5");
            }

            SaveConfigFile(config, "appSettings");
        }

        public void SaveSettings(Settings newSettings)
        {
            Configuration config = OpenConfigFile();

            foreach (string key in config.AppSettings.Settings.AllKeys)
            {
                switch (key)
                {
                    case "Notifications":
                        config.AppSettings.Settings[key].Value = newSettings.Notifications.ToString();
                        break;
                    case "RetryFailedDownload":
                        config.AppSettings.Settings[key].Value = newSettings.RetryFailedDownload.ToString();
                        break;
                    case "DownloadDelay":
                        if (newSettings.DownloadDelay > 0)
                        {
                            config.AppSettings.Settings[key].Value = newSettings.DownloadDelay.ToString();
                        }
                        break;
                }
            }

            SaveConfigFile(config, "appSettings");
        }

        public Settings LoadSettings()
        {
            Settings loadedSettings;
            Configuration config = OpenConfigFile();

            loadedSettings.Notifications = Convert.ToBoolean(config.AppSettings.Settings["Notifications"].Value);
            loadedSettings.RetryFailedDownload = Convert.ToBoolean(config.AppSettings.Settings["RetryFailedDownload"].Value);
            loadedSettings.DownloadDelay = Convert.ToInt32(config.AppSettings.Settings["DownloadDelay"].Value);

            return loadedSettings;
        }
    }

    public class NicknameConfig : CustomConfigManager
    {
        public NicknameConfig() : base() {}

        protected override void CheckForValidFile()
        {
            Configuration config = OpenConfigFile();

            if (config.Sections.Get(BotNickSection.SECTION_NAME) == null)
            {
                config.Sections.Add(BotNickSection.SECTION_NAME, new BotNickSection());
                SaveConfigFile(config);
            }
        }

        public void AddNick(string name, string nick)
        {
            Configuration config = OpenConfigFile();
            BotNickSection nickSection = config.GetSection(BotNickSection.SECTION_NAME) as BotNickSection;
            BotNickElement newNickname = new BotNickElement();
            newNickname.Name = name;
            newNickname.Nickname = nick;
            nickSection.Nicknames.Add(newNickname);
            SaveConfigFile(config, BotNickSection.SECTION_NAME);
        }

        public List<string> GetAllNamesAndNicks()
        {
            Configuration config = OpenConfigFile();
            BotNickSection nickSection = config.GetSection(BotNickSection.SECTION_NAME) as BotNickSection;
            List<string> loaded = new List<string>();
            foreach (BotNickElement nickElement in nickSection.Nicknames)
            {
                loaded.Add(nickElement.Name);
                loaded.Add(nickElement.Nickname);
            }
            return loaded;
        }

        public string GetName(string nick)
        {
            Configuration config = OpenConfigFile();
            BotNickSection nickSection = config.GetSection(BotNickSection.SECTION_NAME) as BotNickSection;
            BotNickElement nickElement = nickSection.Nicknames[nick];
            if (nickElement == null)
            {
                return "null";
            }
            else
            {
                return nickElement.Name;
            }
        }

        public bool RemoveNick(string nick)
        {
            bool success;
            Configuration config = OpenConfigFile();
            BotNickSection nickSection = config.GetSection(BotNickSection.SECTION_NAME) as BotNickSection;
            success = nickSection.Nicknames.Remove(nick);
            SaveConfigFile(config, BotNickSection.SECTION_NAME);
            return success;
        }

        public void ClearNicks()
        {
            Configuration config = OpenConfigFile();
            BotNickSection nickSection = config.GetSection(BotNickSection.SECTION_NAME) as BotNickSection;
            nickSection.Nicknames.Clear();
            SaveConfigFile(config, BotNickSection.SECTION_NAME);
        }
    }

    public class QueueConfig : CustomConfigManager
    {
        public QueueConfig() : base() {}

        protected override void CheckForValidFile()
        {
            Configuration config = OpenConfigFile();

            if (config.Sections.Get(DLQueueSection.SECTION_NAME) == null)
            {
                config.Sections.Add(DLQueueSection.SECTION_NAME, new DLQueueSection());
                SaveConfigFile(config);
            }
        }

        public void SaveQueue(IQueueDownloads queue)
        {
            Download nextItem = queue.NextItem();
            if (nextItem.Bot != "null")
            {
                Configuration config = OpenConfigFile();
                DLQueueSection queueSection = config.GetSection(DLQueueSection.SECTION_NAME) as DLQueueSection;
                DLQueueCollection queueCollection = queueSection.Queue;

                while (nextItem.Bot != "null")
                {
                    DLQueueItemElement queueItem = new DLQueueItemElement();
                    PacketCollection packetsCollection = queueItem.PacketList;
                    queueItem.BotName = nextItem.Bot;

                    do
                    {
                        PacketElement packetItem = new PacketElement();
                        packetItem.Packet = nextItem.Packet;
                        packetsCollection.Add(packetItem);
                        nextItem = queue.NextItem();
                    } while (queueItem.BotName == nextItem.Bot);

                    queueCollection.Add(queueItem);
                }

                SaveConfigFile(config, DLQueueSection.SECTION_NAME);
            }
        }

        public List<string> LoadQueue(IQueueDownloads queue)
        {
            Configuration config = OpenConfigFile();

            DLQueueSection queueSection = config.GetSection(DLQueueSection.SECTION_NAME) as DLQueueSection;
            string botname;
            List<int> packets = new List<int>();
            List<string> loaded = new List<string>();

            foreach (DLQueueItemElement queueItem in queueSection.Queue)
            {
                packets = new List<int>();
                botname = queueItem.BotName;
                loaded.Add(botname);
                foreach (PacketElement packet in queueItem.PacketList)
                {
                    packets.Add(packet.Packet);
                    loaded.Add(packet.Packet.ToString());
                }

                queue.Add(botname, packets);
            }

            queueSection.Queue.Clear();
            SaveConfigFile(config, DLQueueSection.SECTION_NAME);
            return loaded;
        }

        public void ClearSavedQueue()
        {
            Configuration config = OpenConfigFile();
            DLQueueSection queueSection = config.GetSection(DLQueueSection.SECTION_NAME) as DLQueueSection;
            queueSection.Queue.Clear();
            SaveConfigFile(config, DLQueueSection.SECTION_NAME);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Settings
    {
        //[MarshalAs(UnmanagedType.Bool)]
        public bool Notifications;
        //[MarshalAs(UnmanagedType.Bool)]
        public bool RetryFailedDownload;
        public int DownloadDelay;
    }
}
