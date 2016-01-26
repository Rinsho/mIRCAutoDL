using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace AutoDL
{
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
                    //Get next key
                    Key = KeyList[0];
                }
                if (DLInfo[Key].Count > 0)
                {
                    nextDL.Bot = Key;
                    nextDL.Packet = (DLInfo[Key])[0];
                    //if more than just one packet in bot's list
                    if (DLInfo[Key].Count > 1)
                    {
                        DLInfo[Key].RemoveAt(0);
                    }
                    //else remove bot
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
}