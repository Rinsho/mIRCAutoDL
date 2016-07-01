//The classes that govern how aliases are saved to the
//configuration file.

using System;
using System.Configuration;

namespace AutoDL.FileConfiguration
{
    /// <summary>
    /// Represents the alias section in the configuration file.
    /// </summary>
    internal class AliasSection : ConfigurationSection
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

    /// <summary>
    /// Represents the collection within the <c>AliasSection</c> of
    /// aliases and names.
    /// </summary>
    [ConfigurationCollection(typeof(AliasElement))]
    internal class AliasCollection : ConfigurationElementCollection
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

    /// <summary>
    /// Represents an alias and its associated name.
    /// </summary>
    internal class AliasElement : ConfigurationElement
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