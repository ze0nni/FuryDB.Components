using System;
using System.Linq;
using UnityEngine;

namespace FDB.Components.Settings
{
    public struct AxisTrigger
    {
        public string Axis;
        public (KeyCode Neg, KeyCode Pos) Keys;

        public static implicit operator AxisTrigger(string axis) => new AxisTrigger { Axis = axis };
        public static implicit operator AxisTrigger((KeyCode neg, KeyCode pos) keys) => new AxisTrigger { Keys = keys };

        public override string ToString()
        {
            if (Axis != null)
            {
                return Axis;
            } else if (Keys != default)
            {
                return $"{Keys.Neg}-{Keys.Pos}";
            }
            return "null";
        }
    }

    public struct BindingAxis : IEquatable<BindingAxis>, ICloneable
    {
        public static BindingAxis MouseX = new BindingAxis("MouseX", (KeyCode.LeftArrow, KeyCode.RightArrow));
        public static BindingAxis MouseY = new BindingAxis("MouseY", (KeyCode.DownArrow, KeyCode.UpArrow));

        public AxisTrigger[] Triggers;

        public BindingAxis(params AxisTrigger[] triggers)
        {
            Triggers = triggers;
            _value = 0;
        }

        internal float _value;
        public float Value => _value;

        public object Clone()
        {
            return new BindingAxis()
            {
                Triggers = Triggers?.ToArray()
            };
        }

        public bool Equals(BindingAxis other)
        {
            return (object.ReferenceEquals(Triggers, other.Triggers)
                || Enumerable.SequenceEqual(Triggers, other.Triggers));
        }
    }
}