using System;
using System.Collections.Generic;
using System.Linq;

namespace FDB.Components.Settings
{
    public abstract class SettingsPage
    {
        public readonly string Name;
        public readonly SettingsController Controller;
        public IReadOnlyList<SettingsGroup> Groups { get; private set; }

        public bool IsChanged { get; private set; }

        internal SettingsPage(SettingsController controller)
        {
            Name = controller.SettingsType.Name;
            Controller = controller;
        }

        protected void SetGroups(IEnumerable<SettingsGroup> groups)
        {
            Groups = groups.ToArray();
        }

        internal void OnKeyChanged(SettingsKey key)
        {
            IsChanged = true;
        }


        public void Apply()
        {
            foreach (var group in Groups)
            {
                group.Apply();
            }
            Controller.OnApplyChanges();
            IsChanged = false;
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
            }
            Groups = groups;
            SetGroups(groups.Cast<SettingsGroup>());
        }
    }

}
