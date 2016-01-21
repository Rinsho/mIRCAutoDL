using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Configuration;
using System.Runtime.InteropServices;

namespace AutoDL
{
    public class SettingsData
    {
        public SettingsData()
        {
            CheckForValidSettingsFile();
        }

        //Methods
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

            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
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

                config.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection(DLQueueSection.SECTION_NAME);
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
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection(DLQueueSection.SECTION_NAME);
            return loaded;
        }

        public void ClearSavedQueue()
        {
            Configuration config = OpenConfigFile();
            DLQueueSection queueSection = config.GetSection(DLQueueSection.SECTION_NAME) as DLQueueSection;
            queueSection.Queue.Clear();
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection(DLQueueSection.SECTION_NAME);
        }

        public void AddNick(string name, string nick)
        {
            Configuration config = OpenConfigFile();
            BotNickSection nickSection = config.GetSection(BotNickSection.SECTION_NAME) as BotNickSection;
            BotNickElement newNickname = new BotNickElement();
            newNickname.Name = name;
            newNickname.Nickname = nick;
            nickSection.Nicknames.Add(newNickname);
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection(BotNickSection.SECTION_NAME);
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
            return nickElement.Name;
        }

        public bool RemoveNick(string nick)
        {
            bool success;
            Configuration config = OpenConfigFile();
            BotNickSection nickSection = config.GetSection(BotNickSection.SECTION_NAME) as BotNickSection;
            success = nickSection.Nicknames.Remove(nick);
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection(BotNickSection.SECTION_NAME);
            return success;
        }

        public void ClearNicks()
        {
            Configuration config = OpenConfigFile();
            BotNickSection nickSection = config.GetSection(BotNickSection.SECTION_NAME) as BotNickSection;
            nickSection.Nicknames.Clear();
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection(BotNickSection.SECTION_NAME);
        }

        private void CheckForValidSettingsFile()
        {
            Configuration config = OpenConfigFile();

            if (config.HasFile == false)
            {
                config.AppSettings.Settings.Add("Notifications", "True");
                config.AppSettings.Settings.Add("RetryFailedDownload", "False");
                config.AppSettings.Settings.Add("DownloadDelay", "5");
                config.Sections.Add(DLQueueSection.SECTION_NAME, new DLQueueSection());
                config.Sections.Add(BotNickSection.SECTION_NAME, new BotNickSection());
                config.Save(ConfigurationSaveMode.Minimal);
            }
        }

        private Configuration OpenConfigFile()
        {
            ExeConfigurationFileMap configFile = new ExeConfigurationFileMap() { ExeConfigFilename = SettingsPath };
            Configuration config = ConfigurationManager.OpenMappedExeConfiguration(configFile, ConfigurationUserLevel.None, false);
            return config;
        }

        //Members
        private readonly string SettingsPath = AppDomain.CurrentDomain.BaseDirectory + @"\AutoDL.dll.config";
    }

    public class DownloadQueue : IQueueDownloads
    {
        //Constructor
        public DownloadQueue()
        {
            DLInfo = new Dictionary<string, List<int>>();
            KeyList = new List<string>();
        }

        //Methods
        public void Add(string name, List<int> packets)
        {
            if (DLInfo.ContainsKey(name))
            {
                DLInfo[name].AddRange(packets);
            }
            else
            {
                DLInfo.Add(name, packets);
                KeyList.Add(name);
            }
        }

        public bool Remove(string name)
        {
            if (DLInfo.ContainsKey(name))
            {
                DLInfo.Remove(name);
                KeyList.Remove(name);
                if (name == Key)
                {
                    Key = null;
                }
                return true;
            }
            return false;
        }

        public bool Remove(string name, int packet)
        {
            bool success = false;
            if (DLInfo.ContainsKey(name))
            {
                success = DLInfo[name].Remove(packet);
            }
            return success;
        }

        public void RemoveAll()
        {
            DLInfo.Clear();
            KeyList.Clear();
            Key = null;
        }

        public Download NextItem()
        {
            Download nextDL;
            nextDL.Bot = "null";
            nextDL.Packet = 0;

            while (DLInfo.Count > 0)
            {
                if (Key == null)
                {
                    Key = KeyList[0];
                }
                if (DLInfo[Key].Count > 0)
                {
                    nextDL.Bot = Key;
                    nextDL.Packet = (DLInfo[Key])[0];
                    if (DLInfo[Key].Count > 1)
                    {
                        DLInfo[Key].RemoveAt(0);
                    }
                    else
                    {
                        DLInfo.Remove(Key);
                        KeyList.Remove(Key);
                        Key = null;
                    }
                    break;
                }
                else
                {
                    DLInfo.Remove(Key);
                    KeyList.Remove(Key);
                    Key = null;
                }
            }
            return nextDL;
        }

        //Parses file ranges of the form "#-#"
        [Obsolete]
        private List<int> ParsePacketNumbers(List<string> packets)
        {
            List<int> NewPacketList = new List<int>();
            foreach (string packet in packets)
            {
                if (!String.IsNullOrWhiteSpace(packet))
                {
                    if (packet.Contains("-") == true)
                    {
                        string[] temp = packet.Split('-');
                        int value1, value2;
                        if (Int32.TryParse(temp[0], out value1))
                        {
                            if (Int32.TryParse(temp[1], out value2))
                            {
                                for (int i = value1; i <= value2; i++)
                                {
                                    NewPacketList.Add(i);
                                }
                            }
                        }
                    }
                    else
                    {
                        int value;
                        if (Int32.TryParse(packet, out value))
                        {
                            NewPacketList.Add(value);
                        }
                    }
                }
            }
            return NewPacketList;
        }

        //Members
        private Dictionary<string, List<int>> DLInfo;
        private List<string> KeyList;
        private string Key;
    }

    public interface IQueueDownloads
    {
        void Add(string name, List<int> packets);
        Download NextItem();
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Download
    {
        //[MarshalAs(UnmanagedType.LPWStr)]
        public string Bot;
        public int Packet;
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