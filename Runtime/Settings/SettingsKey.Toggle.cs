using System.Reflection;
using UnityEngine;

namespace FDB.Components.Settings
{
    public partial class SettingsKey
    {
        internal class ToggleFactory : ISettingsKeyFactory
        {
            public SettingsKey<TKeyData> Produce<TKeyData>(SettingsGroup<TKeyData> group, FieldInfo keyField) where TKeyData : ISettingsKeyData
            {
                if (keyField.FieldType == typeof(bool))
                {
                    return new ToggleKey<TKeyData>(group, keyField);
                }
                return null;
            }
        }

        public sealed class ToggleKey<TKeyData> : SettingsKey<bool, TKeyData>
            where TKeyData : ISettingsKeyData
        {
            public ToggleKey(SettingsGroup<TKeyData> group, FieldInfo keyField) : base(group, keyField)
            {
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

            protected internal override void OnFieldLayout()
            {
                Value = GUILayout.Toggle(Value, "");
            }
        }
    }
}