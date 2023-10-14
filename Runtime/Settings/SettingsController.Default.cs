using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FDB.Components.Settings
{
    public sealed partial class SettingsController
    {
        internal static readonly string[] DefaultMouseAxis = new string[]
        {
            "MouseX",
            "MouseY"
        };

        internal static readonly string[] DefaultJoystickAxis = new string[]
        {
            "JoyX",
            "JoyY",
            "Joy3",
            "Joy4",
            "Joy5",
            "Joy6",
            "Joy7",
            "Joy8",
            "Joy9",
            "Joy10",
            "Joy11",
            "Joy12",
            "Joy13",
            "Joy14",
            "Joy15",
            "Joy16",
            "Joy17",
            "Joy18",
            "Joy19",
            "Joy20",
            "Joy21",
            "Joy22",
            "Joy23",
            "Joy24",
            "Joy25",
            "Joy26",
            "Joy27",
            "Joy28"
        };

        internal static KeyCode[] DefaultKeyboardKeyCodes = Enum
            .GetValues(typeof(KeyCode))
            .Cast<KeyCode>()
            .Where(c => c != KeyCode.None && !c.ToString().StartsWith("Joystick") && !c.ToString().StartsWith("Mouse"))
            .ToArray();

        internal static KeyCode[] DefaultMouseKeyCodes = Enum
            .GetValues(typeof(KeyCode))
            .Cast<KeyCode>()
            .Where(c => c.ToString().StartsWith("Mouse"))
            .ToArray();

        internal static KeyCode[] DefaultJoystickKeyCodes = Enum
            .GetValues(typeof(KeyCode))
            .Cast<KeyCode>()
            .Where(c => c.ToString().StartsWith("JoystickButton"))
            .ToArray();

        internal static IReadOnlyList<ISettingsKeyFactory> DefaultKeyFactories = new ISettingsKeyFactory[] {
            new EnumKeyFactory(),
            new NumberKeyFactory(),
            new ToggleFactory(),
            new BindingButtonFactory(),
            new BindingAxisFactory(),
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