using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;

namespace FDB.Components.Settings
{
    [DefaultExecutionOrder(-100)]
    public sealed class BindingKeyMediator : MonoBehaviour
    {
        struct State
        {
            public Func<BindingAction> Getter;
            public Action<BindingAction> Setter;
        }

        State[] _triggers = new State[128];
        int _triggersCount;
        State[] _axis = new State[128];
        int _axisCount;

        int _joysticsCount = 0;

        readonly Dictionary<string, bool> _excludeAxis = new Dictionary<string, bool>();

        internal void ListenBindingKey(FieldInfo fieldInfo)
        {
            try
            {
                var field = Expression.Field(null, fieldInfo);
                var value = Expression.Parameter(fieldInfo.FieldType);
                var getter = Expression
                    .Lambda<Func<BindingAction>>(field)
                    .Compile();
                var setter = Expression
                    .Lambda<Action<BindingAction>>(
                        Expression.Assign(field, value),
                        value)
                    .Compile();

                var state = new State
                {
                    Getter = getter,
                    Setter = setter
                };
                Append(ref _triggersCount, ref _triggers, in state);
            }
            catch (Exception exc)
            {
                Debug.LogError($"Error when create listener of {fieldInfo.Name}");
                Debug.LogException(exc);
            }
        }

        void Append(ref int count, ref State[] states, in State state)
        {
            if (count == states.Length)
            {
                var newStates = new State[states.Length * 2];
                Array.Copy(states, newStates, states.Length);
            }
            states[count++] = state;
        }

        private void Update()
        {
            if (_excludeAxis.Count == 0 || _joysticsCount != Input.GetJoystickNames().Length)
            {
                _joysticsCount = Input.GetJoystickNames().Length;
                ExcludeJoystickAxis();
            }

            for (var i = 0; i < _triggersCount; i++)
            {
                ref var state = ref _triggers[i];
                var b = state.Getter();
                var changed = false;

                var pressed = false;
                var lastPressed = b._presset;
                for (var ti = 0; ti < b.Triggers.Length; ti++)
                {
                    if (GetPressed(ref b.Triggers[ti]))
                    {
                        pressed = true;
                        break;
                    }
                }
                var justPressed = !lastPressed && pressed;
                var justReleased = lastPressed && !pressed;

                changed = pressed != lastPressed
                    || justPressed != b._justPressed
                    || justReleased != b._justReleased;

                if (changed)
                {
                    b._presset = pressed;
                    b._justPressed = justPressed;
                    b._justReleased = justReleased;
                    state.Setter(b);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        bool GetPressed(ref ActionTrigger t)
        {
            if (t.Key != KeyCode.None)
            {
                return Input.GetKey(t.Key);
            }
            else if (t.Axis != null)
            {
                return GetAxis(t.Axis) * (t.AxisPositive ? 1 : -1) > 0.25f;
            }
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        float GetAxis(string name)
        {
            if (_excludeAxis.TryGetValue(name, out var e) && e)
            {
                return 0;
            }
            return Input.GetAxis(name);
        }

        void ExcludeJoystickAxis()
        {
            foreach (var a in SettingsController.DefaultJoystickAxis)
            {
                _excludeAxis[a] = Input.GetAxis(a) != 0;
            }
        }

        public bool ReadTrigger(BindingKeyFilterFlags filter, out ActionTrigger value)
        {
            if (filter.HasFlag(BindingKeyFilterFlags.Keyboard) 
                && ReadKeyCodes(SettingsController.DefaultKeyboardKeyCodes, out value))
            {
                return true;
            }

            if (filter.HasFlag(BindingKeyFilterFlags.Joystick)
                && ReadKeyCodes(SettingsController.DefaultJoystickKeyCodes, out value))
            {
                return true;
            }

            if (filter.HasFlag(BindingKeyFilterFlags.MouseKeys)
                && ReadKeyCodes(SettingsController.DefaultMouseKeyCodes, out value))
            {
                return true;
            }

            if (filter.HasFlag(BindingKeyFilterFlags.MouseAxis)
                && ReadAxis(SettingsController.DefaultMouseAxis, out value))
            {
                return true;
            }

            if (filter.HasFlag(BindingKeyFilterFlags.Joystick)
                && ReadAxis(SettingsController.DefaultJoystickAxis, out value))
            {
                return true;
            }

            value = default;
            return false;
        }

        private bool ReadKeyCodes(KeyCode[] codes, out ActionTrigger value)
        {
            foreach (var c in codes)
            {
                if (Input.GetKey(c))
                {
                    value = c;
                    return true;
                }
            }
            value = default;
            return false;
        }

        private bool ReadAxis(string[] axis, out ActionTrigger value)
        {
            foreach (var a in axis)
            {
                var v = GetAxis(a);
                if (v > 0)
                {
                    value = $"+{a}";
                    return true;
                }
                if (v < 0)
                {
                    value = $"-{a}";
                    return true;
                }
            }
            value = default;
            return false;
        }
    }
}
