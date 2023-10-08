using System;
using System.Collections.Generic;
using System.Linq;

namespace FDB.Components.Settings
{
    public abstract class SettingsGroup
    {
        public readonly string Name;
        public readonly SettingsPage Page;
        public readonly Type GroupType;
        public IReadOnlyList<SettingsKey> Keys { get; private set; }

        public bool IsChanged { get; private set; }

        internal SettingsGroup(SettingsPage page, Type groupType)
        {
            Name = groupType.Name;
            Page = page;
            GroupType = groupType;
        }

        protected void SetKeys(IEnumerable<SettingsKey> keys)
        {
            Keys = keys.ToArray();
        }

        internal void OnKeyChanged(SettingsKey key)
        {
            IsChanged = true;
            Page.OnKeyChanged(key);
        }

        internal void Load(ISettingsReader reader)
        {
            foreach (var key in Keys)
            {
                if (reader.Read(key, out var str))
                {
                    key.Load(str);
                } else
                {
                    key.Reset();
                }
            }
        }

        internal void Save(ISettingsWriter writer)
        {
            foreach (var key in Keys)
            {
                writer.Write(key);
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

            var keys = new List<SettingsKey<TKeyData>>();
            foreach (var field in GroupType.GetFields())
            {
                var key = Page.Controller.CreateKey(this, field);
                if (field != null)
                {
                    keys.Add(key);
                }
            }
            foreach (var key in keys)
            {
                key.Setup();
            }
            Keys = keys;
            SetKeys(keys.Cast<SettingsKey>());
        }
    }
}
