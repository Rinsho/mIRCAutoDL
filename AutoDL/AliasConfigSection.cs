using System;
using System.Configuration;

namespace AutoDL
{
    public class AliasSection : ConfigurationSection
    {
        public static string SECTION_NAME = "Aliases";

        [ConfigurationProperty("", IsDefaultCollection=true, IsRequired=false)]
        public AliasCollection Aliases
        {
            get
            {
                return this[""] as AliasCollection;
            }
        }
    }

    [ConfigurationCollection(typeof(AliasElement))]
    public class AliasCollection : ConfigurationElementCollection
    {
        public new AliasElement this[string alias]
        {
            get
            {
                return BaseGet(alias) as AliasElement;
            }
        }

        public void Add(AliasElement item)
        {
            try
            {
                BaseAdd(item, true);
            }
            catch (Exception)
            {
                BaseRemove(item.Alias);
                BaseAdd(item);
            }
        }

        public bool Remove(AliasElement item)
        {
            bool success = true;
            try
            {
                BaseRemove(item.Alias);
            }
            catch (Exception)
            {
                success = false;
            }
            return success;
        }

        public bool Remove(string alias)
        {
            bool success = true;
            try
            {
                BaseRemove(alias);
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
            return new AliasElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return (element as AliasElement).Alias;
        }

    }

    public class AliasElement : ConfigurationElement
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

        [ConfigurationProperty("alias", IsRequired=true, IsKey=true)]
        public string Alias
        {
            get
            {
                return this["alias"] as string;
            }

            set
            {
                this["alias"] = value;
            }
        }
    }
}