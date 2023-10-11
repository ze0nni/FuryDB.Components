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
            new SettingsKey.BindingActionFactory(),
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
                var value = key.KeyField.GetValue(null);
                if (value is ICloneable clonable)
                    _defaults.TryAdd(dKey, clonable.Clone());
                else
                    _defaults.TryAdd(dKey, value);
            }

            internal static object Read(SettingsKey key)
            {
                var dKey = (key.Group.Page.Controller.SettingsType, key.Id);
                _defaults.TryGetValue(dKey, out var value);
                if (value is ICloneable clonable)
                    return clonable.Clone();
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

        private static readonly Dictionary<Type, SettingsController> _controllers 
            = new Dictionary<Type, SettingsController>();

        public static SettingsController Get(Type settingsType)
        {
            return Get("default_user", settingsType);
        }

        public static SettingsController Get(string userId, Type settingsType)
        {
            if (!_controllers.TryGetValue(settingsType, out var instance))
            {
                instance = new SettingsController(
                    userId,
                    settingsType,
                    SettingsStorageAttribute.ResolveStorage(settingsType),
                    SettingHashAttribute.Resolve(settingsType, out var hashSalt),
                    hashSalt,
                    GetKeyFactories(settingsType));
                _controllers.Add(settingsType, instance);
            }
            return instance;
        }
    }
}