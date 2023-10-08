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
        public string HashedUserId { get; private set; } = "";
        private ISettingsKeyFactory[] _factories;
        private Action _onChangedCallback;
        private ISettingsStorage _storage;
        private IUserIdHash _hash;
        private string _hashSalt;
        private SettingsPage<VoidSettingsKeyData> _innserPage;

        internal void OnApplyChanges() => _onChangedCallback?.Invoke();

        public event Action OnUserChanged;

        public SettingsController(
            string userId,
            Type settingsType,
            params ISettingsKeyFactory[] userFactories)
        {
            Init(userId, settingsType, SettingsStorageAttribute.ResolveStorage(settingsType), userFactories);
        }

        public SettingsController(
            string userId,
            Type settingsType,
            StorageType storageType,
            params ISettingsKeyFactory[] userFactories)
        {
            Init(userId, settingsType, storageType.Resolve(settingsType), userFactories);
        }

        public SettingsController(
            string userId,
            Type settingsType,
            ISettingsStorage storage,
            params ISettingsKeyFactory[] userFactories)
        {
            Init(userId, settingsType, storage, userFactories);
        }

        private void Init(
            string userId,
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
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
            _hash = SettingsUserIdHashAttribute.Resolve(settingsType, out _hashSalt);
            _innserPage = CreatePage<VoidSettingsKeyData>();
            if (userId != null)
            {
                SetUsetId(userId);
            }
        }

        public SettingsPage<TKeyData> CreatePage<TKeyData>()
            where TKeyData : ISettingsKeyData
        {
            if (UserId == null)
            {
                throw new ArgumentNullException($"Call {nameof(SettingsController)}.{nameof(SetUsetId)}() before use settings");
            }
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
            UserId = userId ?? throw new ArgumentNullException(nameof(userId));
            HashedUserId = _hash == null ? userId : _hash.Hash(_hashSalt + userId);
            _innserPage.LoadDefault();
            _innserPage.Apply(false);
            Load();
            OnUserChanged?.Invoke();
        }

        public void Load()
        {
            _innserPage.Reset();
            using (_storage.Read(HashedUserId, out var reader))
            {
                _innserPage.Load(reader);
            }
            _innserPage.Apply(false);
        }

        public void LoadDefault()
        {
            _innserPage.LoadDefault();
            _innserPage.Apply(false);
        }

        public void Save()
        {
            _innserPage.Reset();
            Save(_innserPage);
        }

        internal void Save(SettingsPage page)
        {
            using (_storage.Write(HashedUserId, out var writer))
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