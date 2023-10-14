using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace FDB.Components.Settings
{
    public sealed partial class SettingsController
    {
        public readonly Type SettingsType;
        private readonly ISettingsKeyFactory[] _factories;
        private readonly ISettingsStorage _storage;
        private readonly ISettingsHash _hash;
        private readonly string _hashSalt;
        private readonly Action _onChangedCallback;
        public readonly Registry Registry;
        private readonly SettingsPage<DefaultKeyData> _primaryPage;

        public string UserId { get; private set; } = "";
        public string HashedUserId { get; private set; } = "";

        internal void OnApplyChanges() => _onChangedCallback?.Invoke();

        public event Action OnUserChanged;

        internal SettingsController(
            string userId,
            Type settingsType,
            ISettingsStorage storage,
            ISettingsHash hash,
            string hashSalt,
            IEnumerable<ISettingsKeyFactory> factories)
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
            _factories = factories.ToArray();
            _storage = storage;
            _hash = hash;
            _onChangedCallback = OnApplySettingsAttribute.ResolveCallback(settingsType);
            Registry = new Registry();
            _primaryPage = new SettingsPage<DefaultKeyData>(true, this);
            _primaryPage.Setup();
            if (userId != null)
            {
                SetUsetId(userId);
            }
        }

        public SettingsPage<DefaultKeyData> NewPage()
        {
            return NewPage<DefaultKeyData>();
        }

        public SettingsPage<TKeyData> NewPage<TKeyData>()
            where TKeyData : ISettingsKeyData
        {
            if (UserId == null)
            {
                throw new ArgumentNullException($"Call {nameof(SettingsController)}.{nameof(SetUsetId)}() before use settings");
            }
            var page = new SettingsPage<TKeyData>(false, this);
            page.Setup();
            return page;
        }

        internal SettingsKey<TKeyData> CreateKey<TKeyData>(
            KeyContext context,
            SettingsGroup<TKeyData> group, 
            FieldInfo keyField,
            out SettingsKey<TKeyData> headerKey)
            where TKeyData: ISettingsKeyData
        {
            var result = default(SettingsKey<TKeyData>);
            headerKey = null;
            foreach (var factory in _factories)
            {
                var key = factory.Produce<TKeyData>(context, group, keyField);
                if (key != null)
                {
                    result = key;
                }
            }
            if (result != null)
            {
                var attr = keyField.GetCustomAttribute<HeaderAttribute>();
                if (attr != null)
                {
                    headerKey = new HeaderKey<TKeyData>(group, attr, keyField);
                }
            }

            return result;
        }

        internal TKeyData CreateKeyData<TKeyData>(SettingsKey key)
            where TKeyData : ISettingsKeyData
        {
            var data = Activator.CreateInstance<TKeyData>();
            if (key.HeaderAttr != null)
            {
                data.Setup(key.Group, key.HeaderAttr, key.KeyAttributesProvider);
            }
            else
            {
                data.Setup(key);
            }
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
            _primaryPage.ResetDefault();
            _primaryPage.Apply(false);
            Load();
            OnUserChanged?.Invoke();
        }

        public void Load()
        {
            _primaryPage.ResetDefault();
            _storage.Load(HashedUserId, _primaryPage.KeysToLoad);
            _primaryPage.Apply(false);
        }

        public void LoadDefault()
        {
            _primaryPage.ResetDefault();
            _primaryPage.Apply(false);
        }

        public void Save()
        {
            _primaryPage.Reset();
            Save(_primaryPage);
        }

        internal void Save(SettingsPage page)
        {
            _storage.Save(HashedUserId, page.KeysToSave);
            page.NotifySave();
        }

        public override string ToString()
        {
            return $"{typeof(SettingsController).FullName}({SettingsType.FullName})";
        }
    }
}