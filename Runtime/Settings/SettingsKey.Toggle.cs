using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
            public SettingsKey<TKeyData> Produce<TKeyData>(
                KeyContext context,
                SettingsGroup<TKeyData> group,
                FieldInfo keyField) where TKeyData : ISettingsKeyData
            {
                if (keyField.FieldType != typeof(bool))
                {
                    return null;
                }
                var groupAttr = keyField.GetCustomAttribute<ToggleGroupAttribute>();
                var toggleGroup = groupAttr?.GroupName == null
                    ? null
                    : context.Local.GetOrCreate<ToggleGroup<TKeyData>>(
                        groupAttr.GroupName, 
                        () => new ToggleGroup<TKeyData>(groupAttr?.GroupName));

                return new ToggleKey<TKeyData>(group, keyField, toggleGroup);
            }
        }

        public sealed class ToggleGroup<TKeyData>
            where TKeyData : ISettingsKeyData
        {
            public readonly string Name;
            internal ToggleGroup(string name)
            {
                Name = name;
            }

            readonly List<ToggleKey<TKeyData>> _list = new List<ToggleKey<TKeyData>>();
            public IReadOnlyCollection<ToggleKey<TKeyData>> List => _list;

            internal void Add(ToggleKey<TKeyData> toggle)
            {
                _list.Add(toggle);
            }
        }

        public sealed class ToggleKey<TKeyData> : SettingsKey<bool, TKeyData>
            where TKeyData : ISettingsKeyData
        {
            public readonly ToggleGroup<TKeyData> ToggleGroup;

            public ToggleKey(
                SettingsGroup<TKeyData> group,
                FieldInfo keyField,
                ToggleGroup<TKeyData> toggleGroup
                ) : base(group, keyField)
            {
                ToggleGroup = toggleGroup;
                if (toggleGroup != null)
                {
                    toggleGroup.Add(this);
                }
            }

            protected override void NotifyKeyChanged()
            {
                base.NotifyKeyChanged();

                if (ToggleGroup == null || !Value)
                {
                    return;
                }
                foreach (var toggle in ToggleGroup.List)
                {
                    if (toggle != this)
                    {
                        toggle.Value = false;
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

            protected override bool ValueFromJson(JsonTextReader reader)
            {
                switch (reader.TokenType)
                {
                    case JsonToken.Boolean:
                    case JsonToken.String:
                        if (bool.TryParse(reader.Value.ToString(), out var b))
                            return b;
                        break;
                    default:
                        reader.Skip();
                        break;
                }
                Debug.LogWarning($"Return default value for key {Id}");
                return (bool)SettingsController.DefaultKeys.Read(this);
            }

            protected override void ValueToJson(JsonTextWriter writer, bool value)
            {
                writer.WriteValue(value);
            }

            protected internal override void OnFieldLayout(float containerWidth)
            {
                Value = GUILayout.Toggle(Value, "");
            }
        }
    }
}