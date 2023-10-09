using System;
using System.Collections.Generic;
using System.Linq;

namespace FDB.Components.Settings
{
    public sealed partial class SettingsController
    {
        internal static IReadOnlyList<ISettingsKeyFactory> DefaultKeyFactories = new ISettingsKeyFactory[] {
            new SettingsKey.EnumKeyFactory(),
            new SettingsKey.NumberKeyFactory(),
            new SettingsKey.ToggleFactory(),
        };

        private static readonly List<ISettingsKeyFactory> _userKeyFactories = new List<ISettingsKeyFactory>();
        public static IReadOnlyList<ISettingsKeyFactory> UserKeyFactories => _userKeyFactories;

        public static class DefaultKeys
        {
            private static readonly Dictionary<(Type, string), object> _defaults 
                = new Dictionary<(Type, string), object>();

            internal static void Store(SettingsKey key)
            {
                var dKey = (key.Group.Page.Controller.SettingsType, key.Id);
                _defaults.TryAdd(dKey, key.KeyField.GetValue(null));
            }

            internal static object Read(SettingsKey key)
            {
                var dKey = (key.Group.Page.Controller.SettingsType, key.Id);
                _defaults.TryGetValue(dKey, out var value);
                return value;
            }
        }

        private static Dictionary<Type, IReadOnlyList<ISettingsKeyFactory>> _keyFactories
            = new Dictionary<Type, IReadOnlyList<ISettingsKeyFactory>>();

        internal static IReadOnlyList<ISettingsKeyFactory> GetKeyFactories(Type settingsType)
        {
            if (!_keyFactories.TryGetValue(settingsType, out var list))
            {
                list = SettingsKeyFactoryAttribute
                    .Resolve(settingsType)
                    .Concat(DefaultKeyFactories)
                    .ToArray();
                _keyFactories.Add(settingsType, list);
            }
            return list;
        }

        public static SettingsController New(Type settingsType)
        {
            return new SettingsController(
                "default_user",
                settingsType,
                SettingsStorageAttribute.ResolveStorage(settingsType),
                SettingHashAttribute.Resolve(settingsType, out var hashSalt),
                hashSalt,
                GetKeyFactories(settingsType));
        }

        public static SettingsController New(string userId, Type settingsType)
        {
            return new SettingsController(
                userId,
                settingsType,
                SettingsStorageAttribute.ResolveStorage(settingsType),
                SettingHashAttribute.Resolve(settingsType, out var hashSalt),
                hashSalt,
                GetKeyFactories(settingsType));
        }
    }
}