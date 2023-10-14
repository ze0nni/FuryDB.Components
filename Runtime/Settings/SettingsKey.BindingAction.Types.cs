using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Linq;
using UnityEngine;

namespace FDB.Components.Settings
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum BindingActionType
    {
        Trigger,
        Axis
    }

    public struct ActionTrigger
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public KeyCode Key;
        public string Axis;
        public bool AxisPositive;

        public ActionTrigger(KeyCode key)
        {
            Key = key;
            Axis = null;
            AxisPositive = false;
        }

        public ActionTrigger(string axis, bool isPositive)
        {
            Key = KeyCode.None;
            Axis = axis;
            AxisPositive = isPositive;
        }

        public static implicit operator ActionTrigger(KeyCode key) =>
            new ActionTrigger(key);

        public static implicit operator ActionTrigger(string axis)
        {
            if (axis.StartsWith('+'))
                return (axis.Substring(1), +1);
            if (axis.StartsWith('-'))
                return (axis.Substring(1), -1);
            return (axis.Substring(1), +1);
        }

        public static implicit operator ActionTrigger((string axis, int value) pair)
        {
            if (pair.value > 0)
            {
                return new ActionTrigger(pair.axis, true);
            }
            if (pair.value < 0)
            {
                return new ActionTrigger(pair.axis, false);
            }
            Debug.LogWarning($"Excepted positive or negative value for axis {pair.axis}");
            return default;
        }

        public static bool operator ==(ActionTrigger a, ActionTrigger b)
        {
            return a.Key == b.Key && a.Axis == b.Axis && a.AxisPositive == b.AxisPositive;
        }

        public static bool operator !=(ActionTrigger a, ActionTrigger b)
        {
            return !(a == b);
        }

        public override string ToString()
        {
            if (Key != KeyCode.None)
                return $"Key({Key})";
            if (Axis != null)
                return $"Axis({(AxisPositive ? "+" : "-") }{Axis})";
            return "None";
        }
    }

    public struct BindingAction : IEquatable<BindingAction>, ICloneable
    {
        public static BindingAction Of(params ActionTrigger[] triggers)
        {
            var b = new BindingAction();
            b.Type = BindingActionType.Trigger;
            b.Triggers = triggers;
            return b;
        }

        public BindingActionType Type;

        public ActionTrigger[] Triggers;
        public string[] Axis;

        internal bool _presset;
        internal bool _justPressed;
        internal bool _justReleased;
        [JsonIgnore] public bool Pressed => _presset;
        [JsonIgnore] public bool JustPressed => _justPressed;
        [JsonIgnore] public bool JustReleased => _justReleased;

        float _value;
        [JsonIgnore] public float Value => _value;

        public bool Equals(BindingAction other)
        {
            return Type == other.Type
                && (object.ReferenceEquals(Triggers, other.Triggers) || Enumerable.SequenceEqual(Triggers, other.Triggers))
                && (object.ReferenceEquals(Axis, other.Axis) || Enumerable.SequenceEqual(Axis, other.Axis));

        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Type, Triggers, Axis);
        }

        public static BindingAction FromTriggers(params ActionTrigger[] triggers)
        {
            var b = new BindingAction();
            b.Type = BindingActionType.Trigger;
            b.Triggers = triggers;
            return b;
        }

        public static implicit operator BindingAction(ActionTrigger trigger)
        {
            return FromTriggers(trigger);
        }

        public static implicit operator BindingAction((ActionTrigger t0, ActionTrigger t1) triggers)
        {
            return FromTriggers(triggers.t0, triggers.t1);
        }

        public static BindingAction operator +(BindingAction def, BindingAction curr) {
            if (def.Type != curr.Type)
            {
                return (BindingAction)def.Clone();
            }
            curr = (BindingAction)curr.Clone();
            if (def.Type == BindingActionType.Trigger)
            {
                Merge(ref curr.Triggers, ref def.Triggers, t =>
                    t.Key == KeyCode.None && string.IsNullOrEmpty(t.Axis));
            } else
            {
                Merge(ref curr.Axis, ref def.Axis,
                    string.IsNullOrEmpty);
            }
            return curr;

            void Merge<T>(ref T[] currA, ref T[] defA, Predicate<T> isNull)
            {
                if (ReferenceEquals(currA, defA))
                {
                    return;
                }
                if (currA != null && defA == null)
                {
                    return;
                }
                if (currA == null)
                {
                    currA = defA.ToArray();
                }
                else if (currA.Length < defA.Length)
                {
                    var newCurr = new T[defA.Length];
                    Array.Copy(currA, newCurr, currA.Length);
                    currA = newCurr;
                }
                for (var i = 0; i < defA.Length; i++)
                {
                    if (isNull(currA[i]))
                    {
                        currA[i] = defA[i];
                    }
                }
            }
        }

        public override string ToString()
        {
            var args = Type == BindingActionType.Trigger
                ? (Triggers == null ? "null" : string.Join(", ", Triggers))
                : (Axis == null ? "null" : string.Join(", ", Axis));
            return $"BindingAction({Type}, { args })";
        }

        public object Clone()
        {
            var b = new BindingAction();
            b.Type = Type;
            b.Triggers = Triggers?.ToArray();
            b.Axis = Axis?.ToArray();
            return b;
        }
    }
}