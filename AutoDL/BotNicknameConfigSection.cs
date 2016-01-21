using System;
using System.Configuration;

namespace AutoDL
{
    public class BotNickSection : ConfigurationSection
    {
        public static string SECTION_NAME = "BotNicknames";

        [ConfigurationProperty("", IsDefaultCollection=true, IsRequired=false)]
        public BotNickCollection Nicknames
        {
            get
            {
                return this[""] as BotNickCollection;
            }
        }
    }

    [ConfigurationCollection(typeof(BotNickElement))]
    public class BotNickCollection : ConfigurationElementCollection
    {
        public new BotNickElement this[string nick]
        {
            get
            {
                return BaseGet(nick) as BotNickElement;
            }
        }

        public void Add(BotNickElement item)
        {
            try
            {
                BaseAdd(item, true);
            }
            catch (Exception)
            {
                BaseRemove(item.Nickname);
                BaseAdd(item);
            }
        }

        public bool Remove(BotNickElement item)
        {
            bool success = true;
            try
            {
                BaseRemove(item.Nickname);
            }
            catch (Exception)
            {
                success = false;
            }
            return success;
        }

        public bool Remove(string nick)
        {
            bool success = true;
            try
            {
                BaseRemove(nick);
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
            return new BotNickElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return (element as BotNickElement).Nickname;
        }

    }

    public class BotNickElement : ConfigurationElement
    {
        [ConfigurationProperty("name", IsRequired=true)]
        public string Name
        {
            get
            {
                return this["name"] as string;
            }

            set
            {
                this["name"] = value;
            }
        }

        [ConfigurationProperty("nickname", IsRequired=true, IsKey=true)]
        public string Nickname
        {
            get
            {
                return this["nickname"] as string;
            }

            set
            {
                this["nickname"] = value;
            }
        }
    }
}