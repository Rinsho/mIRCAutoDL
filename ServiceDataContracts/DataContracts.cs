using System;
using System.Collections.Generic;

using System.Runtime.Serialization;

namespace AutoDL.ServiceContracts
{
    /// <summary>
    /// Represents a single download.  Immutable to avoid
    /// breaking GetHashCode in collections.
    /// </summary>
    [DataContract]
    public class Download : IEquatable<Download>
    {
        public Download(string name, int packet)
        {
            Name = name;
            Packet = packet;
        }

        [DataMember]
        public string Name { get; private set; }

        [DataMember]
        public int Packet { get; private set; }

        //IEquatable implementation
        public bool Equals(Download other)
        {
            return (this.Name == other.Name) && (this.Packet == other.Packet);
        }

        //Overriding Object equality comparers
        public override bool Equals(object obj)
        {
            return this.Equals(obj as Download);
        }
        public override int GetHashCode()
        {
            return Name.GetHashCode() * 17 + Packet;
        }
    }

    /// <summary>
    /// Represents an alias/name pair.  Immutable to avoid
    /// breaking GetHashCode in collections.
    /// </summary>
    /// <remarks>
    /// Equality only compares the Alias property because that
    /// defines uniqueness.
    /// </remarks>
    [DataContract]
    public class Alias : IEquatable<Alias>
    {
        public Alias(string alias, string name)
        {
            AliasName = alias;
            Name = name;
        }

        [DataMember]
        public string AliasName { get; private set; }

        [DataMember]
        public string Name { get; private set; }

        //IEquatable implementation
        public bool Equals(Alias other)
        {
            return (this.AliasName == other.AliasName);
        }

        //Overriding Object equality comparers
        public override bool Equals(object obj)
        {
            return this.Equals(obj as Alias);
        }
        public override int GetHashCode()
        {
            return AliasName.GetHashCode();
        }
    }

    /// <summary>
    /// Represents a setting/value pair.  Immutable to avoid
    /// breaking GetHashCode in collections.
    /// </summary>
    /// <remarks>
    /// Equality only compares the Name property because that
    /// defines uniqueness.
    /// </remarks>
    /// <exception cref="System.FormatException">Throws if value is invalid type for name.</exception>
    [DataContract]
    public class Setting : IEquatable<Setting>
    {
        public Setting(SettingName name, object value)
        {
            Name = name;
            switch (Name)
            {
                case SettingName.RetryFailedDownload:
                    Value = Convert.ToBoolean(value);
                    break;
                case SettingName.DownloadDelay:
                    int temp = Convert.ToInt32(value);
                    Value = (temp > 0) ? temp : 5;
                    break;
            }
        }

        [DataMember]
        public SettingName Name { get; private set; }

        [DataMember]
        public object Value { get; private set; }

        //IEquatable implementation
        public bool Equals(Setting other)
        {
            return (this.Name == other.Name);
        }

        //Overriding Object equality comparers
        public override bool Equals(object obj)
        {
            return this.Equals(obj as Setting);
        }
        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }

    /// <summary>
    /// Represents supported settings.
    /// </summary>
    public enum SettingName : int { RetryFailedDownload = 0, DownloadDelay = 1 };

    /// <summary>
    /// Represents the status of a download.
    /// </summary>
    public enum DownloadStatus : int { Success, Fail, Retry };

    /// <summary>
    /// Class to hold <c>Fault</c> information relating to configuration file errors.
    /// </summary>
    [DataContract]
    public class ConfigurationFault
    {
        [DataMember]
        public string Description { get; set; }
    }
}
