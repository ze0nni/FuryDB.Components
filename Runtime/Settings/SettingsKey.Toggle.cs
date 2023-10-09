using System;
using System.Reflection;
using UnityEngine;

namespace FDB.Components.Settings
{
    public partial class SettingsKey
    {
        [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
        public class ToggleGroupAttribute : Attribute
        {
            public readonly string GroupName;
            public ToggleGroupAttribute(string groupName)
            {
                GroupName = groupName;
            }
        }

        internal class ToggleFactory : ISettingsKeyFactory
        {
            public SettingsKey<TKeyData> Produce<TKeyData>(SettingsGroup<TKeyData> group, FieldInfo keyField) where TKeyData : ISettingsKeyData
            {
                if (keyField.FieldType != typeof(bool))
                {
                    return null;
                }
                var groupAttr = keyField.GetCustomAttribute<ToggleGroupAttribute>();
                return new ToggleKey<TKeyData>(group, keyField, groupAttr?.GroupName);
            }
        }

        public sealed class ToggleKey<TKeyData> : SettingsKey<bool, TKeyData>
            where TKeyData : ISettingsKeyData
        {
            public readonly string ToggleGroup;

            public ToggleKey(
                SettingsGroup<TKeyData> group,
                FieldInfo keyField,
                string toggleGroup
                ) : base(group, keyField)
            {
                ToggleGroup = toggleGroup;
            }

            protected override void NotifyKeyChanged()
            {
                base.NotifyKeyChanged();

                if (ToggleGroup == null || !Value)
                {
                    return;
                }
                foreach (var key in Group.Keys)
                {
                    if (key != this
                        && key is ToggleKey<TKeyData> toggleKey
                        && toggleKey.ToggleGroup == ToggleGroup)
                    {
                        toggleKey.Value = false;
                    }
                }
            }

            protected override bool ValidateValue(ref bool value)
            {
                return true;
            }

            protected override bool ReadValue(object value)
            {
                switch (value)
                {
                    case bool b: return b;
                }
                return false;
            }

            protected override object WriteValue(bool value)
            {
                return value;
            }

            protected override bool ValueFromString(string value)
            {
                bool.TryParse(value, out var b);
                return b;
            }

            protected override string ValueToString(bool value)
            {
                return value.ToString();
            }

            protected internal override void OnFieldLayout()
            {
                Value = GUILayout.Toggle(Value, "");
            }
        }
    }
}