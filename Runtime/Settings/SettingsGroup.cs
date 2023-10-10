using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace FDB.Components.Settings
{
    public abstract class SettingsGroup
    {
        public readonly string Name;
        public readonly SettingsPage Page;
        public readonly Type GroupType;
        public readonly Registry Registry = new Registry();

        private bool _isDirtyKeys;
        internal void MarkKeysDirty() => _isDirtyKeys = true;
        private SettingsKey[] _keys;
        public IReadOnlyList<SettingsKey> Keys
        {
            get
            {
                if (_isDirtyKeys)
                {
                    _isDirtyKeys = false;
                    foreach (var k in _keys)
                    {
                        k.UpdateDisplayState(this);
                    }
                }
                return _keys;
            }
        }

        internal readonly Dictionary<string, SettingsKey> _keysMap = new Dictionary<string, SettingsKey>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SettingsKey GetKey(string keyNameOrKeyId)
        {
            if (_keysMap.TryGetValue(keyNameOrKeyId, out var value))
            {
                return value;
            }
            return Page.GetKey(keyNameOrKeyId);
        }

        public T GetKey<T>(string keyNameOrKeyId) 
            where T: SettingsKey
        {
            return (T)GetKey(keyNameOrKeyId);
        }

        public bool IsChanged { get; private set; }
        public event Action<SettingsKey> OnKeyChanged;

        internal SettingsGroup(SettingsPage page, Type groupType)
        {
            Name = groupType.Name;
            Page = page;
            GroupType = groupType;
        }

        protected void SetKeys(IEnumerable<SettingsKey> keys)
        {
            _keys = keys.ToArray();
        }

        internal void NotifyKeyChanged(SettingsKey key)
        {
            IsChanged = true;
            foreach (var k in _keys)
            {
                k.UpdateDisplayState(this);
            }
            OnKeyChanged?.Invoke(key);
            Page.NotifyKeyChanged(key);
        }

        internal void NotifySave()
        {
            IsChanged = false;
            foreach (var key in Keys)
            {
                key.NotifySave();
            }
        }

        internal void LoadDefault()
        {
            foreach (var key in Keys)
            {
                key.LoadDefault();
            }
        }

        internal void Apply()
        {
            foreach (var key in Keys)
            {
                key.Apply();
            }
            IsChanged = false;
        }

        internal void Reset()
        {
            foreach (var key in Keys)
            {
                key.Reset();
            }
            IsChanged = false;
        }
    }

    public sealed class SettingsGroup<TKeyData> : SettingsGroup
        where TKeyData : ISettingsKeyData
    {
        public readonly new SettingsPage<TKeyData> Page;
        public new IReadOnlyList<SettingsKey<TKeyData>> Keys { get; private set; }

        internal SettingsGroup(SettingsPage<TKeyData> page, Type groupType): base(page, groupType)
        {
            Page = page;
        }

        internal void Setup()
        {
            var context = new KeyContext(Page.Registrty, Registry);
            var keys = new List<SettingsKey<TKeyData>>();
            foreach (var field in GroupType.GetFields())
            {
                var key = Page.Controller.CreateKey(context, this, field, out var headerKey);
                if (key == null)
                {
                    continue;
                }
                if (headerKey != null)
                {
                    keys.Add(headerKey);
                }
                keys.Add(key);
            }
            foreach (var key in keys)
            {
                key.Setup();
                if (key.Type == KeyType.Key) {
                    _keysMap[key.KeyName] = key;
                }
            }
            Keys = keys;
            SetKeys(keys.Cast<SettingsKey>());
        }
    }
}
