using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FDB.Components.Settings
{
    public struct ButtonTrigger
    {
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

        internal ButtonTriggerDTO ToDTO()
        {
            return new ButtonTriggerDTO
            {
                Key = Key,
                Axis = Axis,
                AxisPositive = AxisPositive
            };
        }
    }

    public struct BindingButton : IEquatable<BindingButton>
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
            b._triggers = triggers.ToArray();
            return b;
        }

        internal ButtonTrigger[] _triggers;
        public IReadOnlyList<ButtonTrigger> Triggers => _triggers;

        public BindingButton Update(int index, ButtonTrigger trigger)
        {
            var b = Of(_triggers);
            b._triggers[index] = trigger;
            return b;
        }

        public BindingButton Append(ButtonTrigger trigger)
        {
            return Of(_triggers.Append(trigger).ToArray());
        }

        public BindingButton Delete(int index)
        {
            var list = _triggers.ToList();
            list.RemoveAt(index);
            return Of(list.ToArray());
        }

        internal bool _presset;
        internal bool _justPressed;
        internal bool _justReleased;
        public bool Pressed => _presset;
        public bool JustPressed => _justPressed;
        public bool JustReleased => _justReleased;

        public bool Equals(BindingButton other)
        {
            return (ReferenceEquals(Triggers, other.Triggers)
                || Enumerable.SequenceEqual(Triggers, other.Triggers));
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Triggers);
        }

        public static BindingButton FromTriggers(params ButtonTrigger[] triggers)
        {
            var b = new BindingButton();
            b._triggers = triggers.ToArray();
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

        public override string ToString()
        {
            var args = Triggers == null ? "null" : string.Join(", ", Triggers);
            return $"BindingAction({ args })";
        }

        internal BindingButtonDTO ToDTO()
        {
            return new BindingButtonDTO
            {
                Triggers = _triggers == null ? new ButtonTriggerDTO[0] : _triggers.Select(x => x.ToDTO()).ToArray()
            };
        }
    }

    internal struct BindingButtonDTO
    {
        public ButtonTriggerDTO[] Triggers;

        public BindingButton ToBinding()
        {
            return new BindingButton
            {
                _triggers = Triggers == null ? new ButtonTrigger[0] : Triggers.Select(x => x.ToTrigger()).ToArray()
            };
        }
    }

    internal struct ButtonTriggerDTO
    {
        [JsonConverter(typeof(StringEnumConverter))] public KeyCode Key;
        public string Axis;
        public bool AxisPositive;

        public ButtonTrigger ToTrigger()
        {
            return new ButtonTrigger
            {
                Key = Key,
                Axis = Axis,
                AxisPositive = AxisPositive
            };
        }
    }
}