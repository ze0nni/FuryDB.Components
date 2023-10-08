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

        public Type SettingsType { get; private set; }
        public string UserId { get; private set; } = "";
        private ISettingsKeyFactory[] _factories;
        private Action _onChangedCallback;
        private ISettingsStorage _storage;
        private SettingsPage<VoidSettingsKeyData> _innserPage;

        internal void OnApplyChanges() => _onChangedCallback?.Invoke();

        public SettingsController(
            Type settingsType,
            params ISettingsKeyFactory[] userFactories)
        {
            Init(settingsType, new SettingsStorage.Prefs(), userFactories);
        }

        public SettingsController(
            Type settingsType,
            StorageType storageType,
            params ISettingsKeyFactory[] userFactories)
        {
            Init(settingsType, storageType.Resolve(settingsType), userFactories);
        }

        public SettingsController(
            Type settingsType,
            ISettingsStorage storage,
            params ISettingsKeyFactory[] userFactories)
        {
            Init(settingsType, storage, userFactories);
        }

        private void Init(
            Type settingsType,
            ISettingsStorage storage,
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
            _factories = userFactories.Concat(DefaultFactories).ToArray();
            _onChangedCallback = OnApplySettingsAttribute.ResolveCallback(settingsType);
            if (storage == null)
            {
                throw new ArgumentNullException(nameof(storage));
            }
            _storage = storage;
            _innserPage = CreatePage<VoidSettingsKeyData>();
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
            foreach (var factory in _factories)
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

        public void SetUsetId(string userId)
        {
            UserId = userId;
            _innserPage.LoadDefault();
            _innserPage.Apply();
            Load();
        }

        public void Load()
        {
            _innserPage.Reset();
            using (_storage.Read(UserId, out var reader))
            {
                _innserPage.Load(reader);
            }
            _innserPage.Apply();
        }

        public void LoadDefault()
        {
            _innserPage.LoadDefault();
            _innserPage.Apply();
        }

        public void Save()
        {
            _innserPage.Reset();
            Save(_innserPage);
        }

        internal void Save(SettingsPage page)
        {
            using (_storage.Write(UserId, out var writer))
            {
                page.Save(writer);
            }
        }

        public override string ToString()
        {
            return $"{typeof(SettingsController).FullName}({SettingsType.FullName})";
        }
    }
}