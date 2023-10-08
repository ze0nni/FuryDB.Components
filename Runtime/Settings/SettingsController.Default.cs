using System;
using System.Collections.Generic;
using System.Linq;

namespace FDB.Components.Settings
{
    public sealed partial class SettingsController
    {
        public static IReadOnlyList<ISettingsKeyFactory> DefaultKeyFactories = new ISettingsKeyFactory[] {
            new SettingsKey.EnumKeyFactory(),
            new SettingsKey.NumberKeyFactory(),
            new SettingsKey.ToggleFactory(),
        };

        private static readonly List<ISettingsKeyFactory> _userKeyFactories = new List<ISettingsKeyFactory>();
        public static IReadOnlyList<ISettingsKeyFactory> UserKeyFactories => _userKeyFactories;

        private static bool _lockRegisterFactories;
        public static void RegisterKeyFactory(ISettingsKeyFactory factory)
        {
            if (_lockRegisterFactories)
            {
                throw new Exception($"You must call {nameof(SettingsController)}.{nameof(RegisterKeyFactory)}() before first time call {nameof(SettingsController)}.{nameof(New)}");
            }
            _userKeyFactories.Add(factory);
        }

        public static IReadOnlyList<ISettingsKeyFactory> KeyFactories
            => DefaultKeyFactories.Concat(_userKeyFactories).ToArray();

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

        public static SettingsController New(Type settingsType)
        {
            _lockRegisterFactories = true;

            return new SettingsController(
                "default_user",
                settingsType,
                SettingsStorageAttribute.ResolveStorage(settingsType),
                SettingHashAttribute.Resolve(settingsType, out var hashSalt),
                hashSalt,
                KeyFactories.ToArray());
        }

        public static SettingsController New(string userId, Type settingsType)
        {
            _lockRegisterFactories = true;

            return new SettingsController(
                userId,
                settingsType,
                SettingsStorageAttribute.ResolveStorage(settingsType),
                SettingHashAttribute.Resolve(settingsType, out var hashSalt),
                hashSalt,
                KeyFactories.ToArray());
        }
    }
}