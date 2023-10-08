using System;
using System.Reflection;
using UnityEngine;

namespace FDB.Components.Settings
{
    public partial class SettingsKey
    {
        internal class NumberKeyFactory : ISettingsKeyFactory
        {
            public SettingsKey<TKeyData> Produce<TKeyData>(SettingsGroup<TKeyData> group, FieldInfo keyField)
                where TKeyData : ISettingsKeyData
            {
                if (keyField.FieldType == typeof(int)
                    || keyField.FieldType == typeof(float))
                {
                    return new NumberKey<TKeyData>(group, keyField);
                }
                return null;
            }
        }

        public class NumberKey<TKeyData> : SettingsKey<float, TKeyData>
            where TKeyData : ISettingsKeyData
        {
            public enum NumberType
            {
                Int,
                Float
            }

            public readonly NumberType Type;
            public readonly float? Min;
            public readonly float? Max;

            public NumberKey(SettingsGroup<TKeyData> group, FieldInfo keyField) : base(group, keyField)
            {
                if (KeyType == typeof(int))
                {
                    Type = NumberType.Int;
                }
                else if (KeyType == typeof(float))
                {
                    Type = NumberType.Float;
                }
                else
                {
                    throw new ArgumentException("Excepted float or int");
                }
            }

            protected override bool ValidateValue(ref float value)
            {
                if (Min != null)
                {
                    value = Math.Max(Min.Value, value);
                }
                if (Max != null)
                {
                    value = Math.Min(Max.Value, value);
                }
                if (Type == NumberType.Int)
                {
                    value = (int)value;
                }
                return true;
            }

            protected override float ReadValue(object value)
            {
                switch (value)
                {
                    case int i: return i;
                    case float f: return f;
                }
                return 0;
            }

            protected override object WriteValue(float value)
            {
                switch (Type)
                {
                    case NumberType.Int:
                        return (int)value;
                    case NumberType.Float:
                        return value;
                    default:
                        throw new ArgumentNullException(Type.ToString());
                }
            }

            protected override float ValueFromString(string value)
            {
                float.TryParse(value, out var n);
                return n;
            }

            protected override string ValueToString(float value)
            {
                return value.ToString();
            }

            protected internal override void OnFieldLayout()
            {
                
            }
        }
    }
}