using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
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

        internal AxisTriggerDTO ToDTO()
        {
            return new AxisTriggerDTO
            {
                Axis = Axis,
                NegKey = Keys.Neg,
                PosKey = Keys.Pos
            };
        }
    }

    public struct BindingAxis : IEquatable<BindingAxis>
    {
        public static BindingAxis MouseX = new BindingAxis("MouseX", (KeyCode.LeftArrow, KeyCode.RightArrow));
        public static BindingAxis MouseY = new BindingAxis("MouseY", (KeyCode.DownArrow, KeyCode.UpArrow));

        internal AxisTrigger[] _triggers;
        public IReadOnlyList<AxisTrigger> Triggers => _triggers;
        public BindingAxis Update(int index, AxisTrigger trigger)
        {
            var b = new BindingAxis(_triggers);
            b._triggers[index] = trigger;
            return b;
        }

        public BindingAxis(params AxisTrigger[] triggers)
        {
            _triggers = triggers.ToArray();
            _value = 0;
        }

        internal float _value;
        public float Value => _value;


        public bool Equals(BindingAxis other)
        {
            return (ReferenceEquals(Triggers, other.Triggers)
                || Enumerable.SequenceEqual(Triggers, other.Triggers));
        }

        internal BindingAxisDTO ToDTO()
        {
            return new BindingAxisDTO
            {
                Triggers = _triggers == null ? new AxisTriggerDTO[0] : _triggers.Select(x => x.ToDTO()).ToArray()
            };
        }
    }

    internal struct BindingAxisDTO {
        public AxisTriggerDTO[] Triggers;

        public BindingAxis ToBinding()
        {
            return new BindingAxis
            {
                _triggers = Triggers == null ? new AxisTrigger[0] : Triggers.Select(x => x.ToTrigger()).ToArray()
            };
        }
    }

    internal struct AxisTriggerDTO
    {
        public string Axis;
        [JsonConverter(typeof(StringEnumConverter))] public KeyCode NegKey;
        [JsonConverter(typeof(StringEnumConverter))] public KeyCode PosKey;

        public AxisTrigger ToTrigger()
        {
            return new AxisTrigger
            {
                Axis = Axis,
                Keys = (NegKey, PosKey)
            };
        }
    }
}