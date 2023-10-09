using System;
using System.Collections.Generic;

namespace FDB.Components.Settings
{
    public enum StorageType
    {
        Null = 1,
        Prefs,
        PersistentDataPath
    }

    public interface ISettingsStorage
    {
        void Save(string userId, IReadOnlyList<SettingsKey> keys);
        void Load(string userId, IReadOnlyDictionary<string, SettingsKey> keys);
    }

    internal static partial class SettingsStorage
    {
        public static ISettingsStorage Resolve(this StorageType type, Type settingsType)
        {
            switch (type)
            {
                case StorageType.Null:
                    return new Null();
                case StorageType.Prefs:
                    return new Prefs();
                case StorageType.PersistentDataPath:
                    return new PersistentDataPath(settingsType);
                default:
                    throw new ArgumentOutOfRangeException(type.ToString());
            }
        }

        public static string KeyOf(this SettingsKey key, string userId) => $"{userId}.{key.Id}";
    }
}