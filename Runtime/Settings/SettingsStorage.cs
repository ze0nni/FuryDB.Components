using System;

namespace FDB.Components.Settings
{
    public enum StorageType
    {
        Null,
        Prefs,
    }

    public interface ISettingsStorage
    {
        IDisposable Write(string userId, out ISettingsWriter writer);
        IDisposable Read(string userId, out ISettingsReader reader);
    }

    public interface ISettingsWriter
    {
        void Write(SettingsKey key);
    }

    public interface ISettingsReader
    {
        bool Read(SettingsKey key, out string value);
    }

    public static partial class SettingsStorage
    {
        public static ISettingsStorage Resolve(this StorageType type, Type settingsType)
        {
            switch (type)
            {
                case StorageType.Null:
                    return new Null();
                case StorageType.Prefs:
                    return new Prefs();
                default:
                    throw new ArgumentOutOfRangeException(type.ToString());
            }
        }

        public static string KeyOf(this SettingsKey key, string userId) => $"{userId}.{key.Id}";
    }
}