using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace FDB.Components.Settings
{
    public abstract class SettingsPage
    {
        public readonly string Name;
        public readonly bool Primary;
        readonly Dictionary<Type, SettingsPage> _primaryMap = new Dictionary<Type, SettingsPage>();
        public readonly SettingsController Controller;
        public readonly Registry Registrty = new Registry();
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

        public IReadOnlyDictionary<string, SettingsKey> KeysToLoad
            => Groups
            .SelectMany(g => g.Keys)
            .Where(k => k.Type == KeyType.Key)
            .ToDictionary(k => k.Id);

        public IReadOnlyList<SettingsKey> KeysToSave
            => Groups
            .SelectMany(g => g.Keys)
            .Where(k => k.Type == KeyType.Key).
            ToArray();

        internal SettingsPage(
            bool primary,
            SettingsController controller)
        {
            Name = controller.SettingsType.Name;
            Primary = primary;
            Controller = controller;

            if (primary)
            {
                _primaryMap.Add(GetType(), this);
            }
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

        internal void NotifySave()
        {
            IsChanged = false;
            foreach (var g in Groups)
            {
                g.NotifySave();
            }
        }

        public void ResetDefault()
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
    }

    public sealed class SettingsPage<TKeyData> : SettingsPage
        where TKeyData : ISettingsKeyData
    {
        public new IReadOnlyList<SettingsGroup<TKeyData>> Groups { get; private set; }

        internal SettingsPage(bool primary, SettingsController controller) : base(primary, controller) {
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
