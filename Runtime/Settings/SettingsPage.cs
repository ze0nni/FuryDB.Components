using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace FDB.Components.Settings
{
    public abstract class SettingsPage
    {
        public readonly string Name;
        public readonly SettingsController Controller;
        public IReadOnlyList<SettingsGroup> Groups { get; private set; }

        internal readonly Dictionary<string, SettingsKey> _keysMap = new Dictionary<string, SettingsKey>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SettingsKey GetKey(string id) {
            _keysMap.TryGetValue(id, out var key);
            return key;
        }
        public T GetKey<T>(string id) 
            where T : SettingsKey
        {
            return (T)GetKey(id);
        }

        public bool IsChanged { get; private set; }
        public event Action<SettingsKey> OnKeyChanged;

        internal SettingsPage(SettingsController controller)
        {
            Name = controller.SettingsType.Name;
            Controller = controller;
        }

        protected void SetGroups(IEnumerable<SettingsGroup> groups)
        {
            Groups = groups.ToArray();
        }

        internal void NotifyKeyChanged(SettingsKey key)
        {
            IsChanged = true;
            foreach (var g in Groups)
            {
                g.MarkKeysDirty();
            }
            OnKeyChanged?.Invoke(key);
    }

    public void LoadDefault()
        {
            foreach (var group in Groups)
            {
                group.LoadDefault();
            }
        }

        public void Apply(bool save)
        {
            foreach (var group in Groups)
            {
                group.Apply();
            }
            Controller.OnApplyChanges();
            IsChanged = false;
            if (save)
            {
                Controller.Save(this);
            }
        }

        public void Reset()
        {
            foreach (var group in Groups)
            {
                group.Reset();
            }
            IsChanged = false;
        }

        internal void Load(ISettingsReader reader)
        {
            foreach (var group in Groups)
            {
                group.Load(reader);
            }
        }

        internal void Save(ISettingsWriter writer)
        {
            foreach (var group in Groups)
            {
                group.Save(writer);
            }
        }
    }

    public sealed class SettingsPage<TKeyData> : SettingsPage
        where TKeyData : ISettingsKeyData
    {
        public new IReadOnlyList<SettingsGroup<TKeyData>> Groups { get; private set; }

        internal SettingsPage(SettingsController controller) : base(controller) {
        }

        internal void Setup()
        {
            var groups = new List<SettingsGroup<TKeyData>>();
            foreach (var type in Controller.SettingsType.GetNestedTypes())
            {
                if (type.IsClass && type.IsAbstract && type.IsSealed)
                {
                    groups.Add(new SettingsGroup<TKeyData>(this, type));
                }
            }
            foreach (var group in groups)
            {
                group.Setup();
                foreach (var key in group.Keys)
                {
                    if (key.Type != KeyType.Key)
                    {
                        continue;
                    }
                    _keysMap[key.Id] = key;
                }
            }
            Groups = groups;
            SetGroups(groups.Cast<SettingsGroup>());
        }
    }

}
