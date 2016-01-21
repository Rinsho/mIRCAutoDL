using System;
using System.Collections.Generic;

namespace AutoDL
{
    public static class Queue
    {
        //Methods
        public static void Add(string name, string[] packets)
        {
            DLData.Add(name, new List<string>(packets));
        }

        //Download.Bot will be null and Packet will be 0 on empty or invalid queue
        public static Download Next() 
        {
            return DLData.NextItem();
        }

        public static void Pause() 
        {
            Config.SData.SaveQueue(DLData);
        }

        public static void Resume() 
        {
            Config.SData.LoadQueue(DLData);
        }

        public static void Remove(string name, int packet = 0)
        {
            if (packet <= 0)
            {
                DLData.Remove(name);
            }
            else
            {
                DLData.Remove(name, packet);
            }
        }

        public static void Clear()
        {
            DLData.RemoveAll();
        }

        //Members
        internal static DownloadQueue DLData = new DownloadQueue();
        
    }

    public static class Config
    {
        public static void Save(Settings newSettings)
        {
            SData.SaveSettings(newSettings);
        }

        public static Settings Load()
        {
            return SData.LoadSettings();
        }

        internal static SettingsData SData = new SettingsData();
    }
}
