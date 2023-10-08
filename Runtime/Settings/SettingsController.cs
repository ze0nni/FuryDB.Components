using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace FDB.Components.Settings
{
    public sealed class SettingsController
    {
        public static IReadOnlyList<ISettingsKeyFactory> DefaultFactories = new ISettingsKeyFactory[]
        {
            new SettingsKey.EnumKeyFactory(),
            new SettingsKey.NumberKeyFactory(),
            new SettingsKey.ToggleFactory(),
        };

        public readonly Type SettingsType;
        public readonly IReadOnlyList<ISettingsKeyFactory> Factories;
        private Action _onChangedCallback;
        internal void OnApplyChanges() => _onChangedCallback?.Invoke();

        public SettingsController(
            Type settingsType,
            params ISettingsKeyFactory[] userFactories)
        {
            if (!settingsType.IsClass)
            {
                throw new ArgumentException($"Excepted class but {settingsType}");
            }
            if (!settingsType.IsAbstract || !settingsType.IsSealed)
            {
                throw new ArgumentException($"Excepted static class");
            }
            SettingsType = settingsType;
            Factories = userFactories.Concat(DefaultFactories).ToArray();

            _onChangedCallback = OnApplySettingsAttribute.ResolveCallback(settingsType);
        }

        public SettingsPage<TKeyData> CreatePage<TKeyData>()
            where TKeyData : ISettingsKeyData
        {
            var page = new SettingsPage<TKeyData>(this);
            page.Setup();
            return page;
        }

        internal SettingsKey<TKeyData> CreateKey<TKeyData>(SettingsGroup<TKeyData> group, FieldInfo keyField)
            where TKeyData: ISettingsKeyData
        { 
            foreach (var factory in Factories)
            {
                var key = factory.Produce<TKeyData>(group, keyField);
                if (key != null)
                {
                    return key;
                }
            }
            return null;
        }

        internal TKeyData CreateKeyData<TKeyData>(SettingsKey key)
            where TKeyData : ISettingsKeyData
        {
            var data = Activator.CreateInstance<TKeyData>();
            data.Setup(key);
            return data;
        }

        internal TKeyData CreateKeyData<TKeyData>(FieldInfo field)
            where TKeyData : ISettingsKeyData
        {
            var data = Activator.CreateInstance<TKeyData>();
            data.Setup(field);
            return data;
        }

        public override string ToString()
        {
            return $"{typeof(SettingsController).FullName}({SettingsType.FullName})";
        }
    }
}