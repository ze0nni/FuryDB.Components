using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Linq;
using UnityEngine;

namespace FDB.Components.Settings
{
    public struct ButtonTrigger
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public KeyCode Key;
        public string Axis;
        public bool AxisPositive;

        public ButtonTrigger(KeyCode key)
        {
            Key = key;
            Axis = null;
            AxisPositive = false;
        }

        public ButtonTrigger(string axis, bool isPositive)
        {
            Key = KeyCode.None;
            Axis = axis;
            AxisPositive = isPositive;
        }

        public static implicit operator ButtonTrigger(KeyCode key) =>
            new ButtonTrigger(key);

        public static implicit operator ButtonTrigger(string axis)
        {
            if (axis.StartsWith('+'))
                return (axis.Substring(1), +1);
            if (axis.StartsWith('-'))
                return (axis.Substring(1), -1);
            return (axis.Substring(1), +1);
        }

        public static implicit operator ButtonTrigger((string axis, int value) pair)
        {
            if (pair.value > 0)
            {
                return new ButtonTrigger(pair.axis, true);
            }
            if (pair.value < 0)
            {
                return new ButtonTrigger(pair.axis, false);
            }
            Debug.LogWarning($"Excepted positive or negative value for axis {pair.axis}");
            return default;
        }

        public static bool operator ==(ButtonTrigger a, ButtonTrigger b)
        {
            return a.Key == b.Key && a.Axis == b.Axis && a.AxisPositive == b.AxisPositive;
        }

        public static bool operator !=(ButtonTrigger a, ButtonTrigger b)
        {
            return !(a == b);
        }

        public bool IsNull => Key == KeyCode.None && Axis == null;

        public override string ToString()
        {
            if (Key != KeyCode.None)
                return $"Key({Key})";
            if (Axis != null)
                return $"Axis({(AxisPositive ? "+" : "-") }{Axis})";
            return "None";
        }
    }

    public struct BindingButton : IEquatable<BindingButton>, ICloneable
    {
        public static BindingButton KeyMoveUp => Of(KeyCode.W, "-JoyY");
        public static BindingButton KeyMoveDown => Of(KeyCode.S, "+JoyY");
        public static BindingButton KeyMoveLeft => Of(KeyCode.A, "-JoyX");
        public static BindingButton KeyMoveRight => Of(KeyCode.D, "+JoyX");
        public static BindingButton KeyFire => Of(KeyCode.Mouse0, KeyCode.Joystick1Button0);
        public static BindingButton KeyJump => Of(KeyCode.Space, KeyCode.Joystick1Button1);

        public static BindingButton Of(params ButtonTrigger[] triggers)
        {
            var b = new BindingButton();
            b.Triggers = triggers;
            return b;
        }

        public ButtonTrigger[] Triggers;

        internal bool _presset;
        internal bool _justPressed;
        internal bool _justReleased;
        [JsonIgnore] public bool Pressed => _presset;
        [JsonIgnore] public bool JustPressed => _justPressed;
        [JsonIgnore] public bool JustReleased => _justReleased;

        public bool Equals(BindingButton other)
        {
            return (ReferenceEquals(Triggers, other.Triggers)
                || !(Triggers == null ^ other.Triggers == null)
                || Enumerable.SequenceEqual(Triggers, other.Triggers));
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Triggers);
        }

        public static BindingButton FromTriggers(params ButtonTrigger[] triggers)
        {
            var b = new BindingButton();
            b.Triggers = triggers;
            return b;
        }

        public static implicit operator BindingButton(ButtonTrigger trigger)
        {
            return FromTriggers(trigger);
        }

        public static implicit operator BindingButton((ButtonTrigger t0, ButtonTrigger t1) triggers)
        {
            return FromTriggers(triggers.t0, triggers.t1);
        }

        public static BindingButton operator +(BindingButton def, BindingButton curr) {
            curr = (BindingButton)curr.Clone();
            
            
            Merge(ref curr.Triggers, ref def.Triggers, t => t.IsNull);
            
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
            var args = Triggers == null ? "null" : string.Join(", ", Triggers);
            return $"BindingAction({ args })";
        }

        public object Clone()
        {
            var b = new BindingButton();
            b.Triggers = Triggers?.ToArray();
            return b;
        }
    }
}