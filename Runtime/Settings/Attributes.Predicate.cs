using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace FDB.Components.Settings
{
    public enum Op
    {
        IsTrue,
        IsFalse,
        Eq,
        NotEq,
        Between
    }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public abstract class SettingsPredicateAttribute : Attribute
    {
        public readonly string KeyId;
        public readonly Op Op;
        public readonly string[] KeyValues;

        public SettingsPredicateAttribute(string group, string key, Op op, params string[] values)
        {
            KeyId = $"{group}.{key}";
            Op = op;
            KeyValues = values;
        }

        public SettingsPredicateAttribute(string key, Op op, params string[] values)
        {
            KeyId = key;
            Op = op;
            KeyValues = values;
        }

        public static SettingsKey.DisplayPredecateDelegate Resolve<T>(
            ICustomAttributeProvider provider)
            where T : SettingsPredicateAttribute
        {
            var attr = provider.GetCustomAttributes(typeof(T), false).Cast<T>().FirstOrDefault();
            if (attr == null)
            {
                return null;
            }

            if (!string.IsNullOrEmpty(attr.KeyId))
            {
                try
                {
                    return attr.Op.Resolve(attr.KeyId, attr.KeyValues);
                } catch (Exception exc)
                {
                    Debug.LogError($"Exception when create predicate SettingsPredicate({attr.KeyId}, {attr.Op}, ...)");
                    Debug.LogException(exc);
                }
            }
            else
            {
                Debug.LogWarning($"No predicate found");
            }

            return null;
        }
    }

    internal static class OpHelper
    {
        public static SettingsKey.DisplayPredecateDelegate Resolve(this Op op, string id, string[] values)
        {
            switch (op)
            {
                case Op.IsTrue:
                    return IsTrue(id);
                case Op.IsFalse:
                    return IsFalse(id);
                case Op.Eq:
                    return Eq(id, values);
                case Op.NotEq:
                    return NotEq(id, values);
                case Op.Between:
                    return Between(id, values);
                default:
                    throw new ArgumentOutOfRangeException($"Op = {op}");
            }
        }

        private static SettingsKey.DisplayPredecateDelegate IsTrue(string id)
        {
            return (page) =>
            {
                var value = page.GetKey(id).StringValue;
                return value == "true" || value == "True";
            };
        }

        private static SettingsKey.DisplayPredecateDelegate IsFalse(string id)
        {
            return (page) =>
            {
                var value = page.GetKey(id).StringValue;
                return value != "true" && value != "True";
            };
        }

        private static SettingsKey.DisplayPredecateDelegate Eq(string id, string[] values)
        {
            if (values == null || values.Length == 0)
            {
                throw new ArgumentException("Values lists excepted");
            }
            return (page) =>
            {
                var value = page.GetKey(id).StringValue;
                return Array.IndexOf(values, value) != -1;
            };
        }

        private static SettingsKey.DisplayPredecateDelegate NotEq(string id, string[] values)
        {
            if (values == null || values.Length == 0)
            {
                throw new ArgumentException("Values lists excepted");
            }
            return (page) =>
            {
                var value = page.GetKey(id).StringValue;
                return Array.IndexOf(values, value) == -1;
            };
        }

        private static SettingsKey.DisplayPredecateDelegate Between(string id, string[] values)
        {
            if (values == null 
                || values.Length != 2 
                || !float.TryParse(values[0], out var min)
                || !float.TryParse(values[1], out var max))
            {
                throw new ArgumentException("Excepted two number values");
            }
            return (page) =>
            {
                var value = float.Parse(page.GetKey(id).StringValue);
                return value >= min && value <= max;
            };
        }
    }
}