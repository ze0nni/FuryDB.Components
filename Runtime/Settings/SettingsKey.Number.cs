using Newtonsoft.Json;
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

            public readonly NumberType NumType;
            public readonly float? Min;
            public readonly float? Max;

            public NumberKey(SettingsGroup<TKeyData> group, FieldInfo keyField) : base(group, keyField)
            {
                if (KeyType == typeof(int))
                {
                    NumType = NumberType.Int;
                }
                else if (KeyType == typeof(float))
                {
                    NumType = NumberType.Float;
                }
                else
                {
                    throw new ArgumentException("Excepted float or int");
                }
                var range = keyField.GetCustomAttribute<RangeAttribute>();
                if (range != null)
                {
                    Min = range.min;
                    Max = range.max;
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
                if (NumType == NumberType.Int)
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
                switch (NumType)
                {
                    case NumberType.Int:
                        return (int)value;
                    case NumberType.Float:
                        return value;
                    default:
                        throw new ArgumentNullException(NumType.ToString());
                }
            }

            protected override float ValueFromJson(JsonTextReader reader)
            {
                switch (reader.TokenType)
                {
                    case JsonToken.Integer:
                    case JsonToken.Float:
                    case JsonToken.String:
                        if (float.TryParse(reader.Value.ToString(), out var n))
                            return n;
                        break;
                    default:
                        reader.Skip();
                        break;
                }
                Debug.LogWarning($"Return default value for key {Id}");
                return (float)SettingsController.DefaultKeys.Read(this);
            }

            protected override void ValueToJson(JsonTextWriter writer, float value)
            {
                if (NumType == NumberType.Int)
                {
                    writer.WriteValue((int)value);
                } else
                {
                    writer.WriteValue(value);
                }
            }

            protected internal override void OnFieldLayout(float containerWidth)
            {
                if (Min == null && Max == null)
                {
                    var newValue = GUILayout.TextField(StringValue);
                    if (float.TryParse(newValue, out var n))
                    {
                        if (n != Value)
                        {
                            Value = n;
                        }
                    }
                    else
                    {
                        Value = 0;
                    }
                    if (GUILayout.Button("-", GUILayout.Width(42)))
                    {
                        Value -= 1;
                    }
                    if (GUILayout.Button("+", GUILayout.Width(42)))
                    {
                        Value += 1;
                    }
                } else
                {
                    GUILayout.Label(StringValue, GUILayout.Width(48));
                    var newValue = GUILayout.HorizontalSlider(Value, Min.Value, Max.Value);
                    GUILayout.Label($"{Min.Value}...{Max.Value}", GUILayout.ExpandWidth(false));
                    if (newValue != Value)
                    {
                        Value = newValue;
                    }
                }
            }
        }
    }
}