//The classes which govern how downloads are saved to the
//configuration file.

using System;
using System.Configuration;

namespace AutoDL.FileConfiguration
{
    /// <summary>
    /// Represents the download queue section in the configuration file.
    /// </summary>
    internal class DLQueueSection : ConfigurationSection
    {
        public static string SECTION_NAME = "DLQueue";

        [ConfigurationProperty("", IsDefaultCollection=true, IsRequired=false)]
        public DLQueueCollection Queue
        {
            get
            {
                return this[""] as DLQueueCollection;
            }
        }
    }

    /// <summary>
    /// Represents the collection within the <c>DLQueueSection</c> of
    /// bots and packets.
    /// </summary>
    [ConfigurationCollection(typeof(DLQueueItemElement), AddItemName="queueItem")]
    internal class DLQueueCollection : ConfigurationElementCollection
    {      
        public void Add(DLQueueItemElement item)
        {
            try
            {
                BaseAdd(item, true);
            }
            catch (Exception)
            {
                BaseRemove(item.BotName);
                BaseAdd(item);
            }
        }

        public bool Remove(DLQueueItemElement item)
        {
            bool success = true;
            try
            {
                BaseRemove(item.BotName);
            }
            catch (Exception)
            {
                success = false;
            }
            return success;
        }

        public bool Remove(string name)
        {
            bool success = true;
            try
            {
                BaseRemove(name);
            }
            catch (Exception)
            {
                success = false;
            }
            return success;
        }

        public void Clear()
        {
            BaseClear();
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new DLQueueItemElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return (element as DLQueueItemElement).BotName;
        }
    }

    /// <summary>
    /// Represents a bot and its associated packet(s).
    /// </summary>
    internal class DLQueueItemElement : ConfigurationElement
    {
        [ConfigurationProperty("botName", IsRequired=true, IsKey=true)]
        public string BotName
        {
            get
            {
                return this["botName"] as string;
            }

            set
            {
                this["botName"] = value;
            }
        }

        [ConfigurationProperty("packetList", IsRequired=true)]
        public PacketCollection PacketList
        {
            get
            {
                return this["packetList"] as PacketCollection;
            }
        }
    }

    /// <summary>
    /// Represents a collection of packets.
    /// </summary>
    [ConfigurationCollection(typeof(PacketElement), AddItemName="packet")]
    internal class PacketCollection : ConfigurationElementCollection
    {
        public PacketElement this[int index]
        {
            get
            {
                return BaseGet(index) as PacketElement;
            }

            set
            {
                if (BaseGet(index) != null)
                {
                    BaseRemoveAt(index);
                }
                BaseAdd(index, value);
            }
        }

        public void Add(PacketElement item)
        {
            BaseAdd(item);
        }

        public bool Remove(PacketElement item)
        {
            bool success = true;
            try
            {
                BaseRemove(item.Packet);
            }
            catch (Exception)
            {
                success = false;
            }
            return success;
        }

        public bool Remove(int packetNumber)
        {
            bool success = true;
            try
            {
                BaseRemove(packetNumber);
            }
            catch (Exception)
            {
                success = false;
            }
            return success;
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new PacketElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return (element as PacketElement).Packet;
        }
    }

    /// <summary>
    /// Represents a single packet.
    /// </summary>
    internal class PacketElement : ConfigurationElement
    {
        [ConfigurationProperty("packet", IsRequired=true)]
        [IntegerValidator(MinValue=0)]
        public int Packet
        {
            get
            {
                return (int)this["packet"];
            }

            set
            {
                this["packet"] = value;
            }
        }
    }
}
