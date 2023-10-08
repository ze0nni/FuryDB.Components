using System;
using System.Reflection;

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
            public readonly int? Min;
            public readonly int? Max;

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

            protected internal override void OnFieldLayout()
            {
                
            }
        }
    }
}