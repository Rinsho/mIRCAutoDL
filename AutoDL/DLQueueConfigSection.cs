using System;
using System.Configuration;

namespace AutoDL
{
    public class DLQueueSection : ConfigurationSection
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

    [ConfigurationCollection(typeof(DLQueueItemElement), AddItemName="queueItem")]
    public class DLQueueCollection : ConfigurationElementCollection
    {      
        public void Add(DLQueueItemElement item)
        {
            BaseAdd(item);
        }

        public void Remove(DLQueueItemElement item)
        {
            BaseRemove(item.BotName);
        }

        public void Remove(string name)
        {
            BaseRemove(name);
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

    public class DLQueueItemElement : ConfigurationElement
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

    [ConfigurationCollection(typeof(PacketElement), AddItemName="packet")]
    public class PacketCollection : ConfigurationElementCollection
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

        public void Remove(PacketElement item)
        {
            BaseRemove(item.Packet);
        }

        public void Remove(int packetNumber)
        {
            BaseRemove(packetNumber);
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

    public class PacketElement : ConfigurationElement
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
