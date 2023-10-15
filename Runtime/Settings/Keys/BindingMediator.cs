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
    public sealed class BindingMediator : MonoBehaviour
    {
        struct ButtonState
        {
            public Func<BindingButton> Getter;
            public Action<BindingButton> Setter;
        }

        struct AxisState
        {
            public Func<BindingAxis> Getter;
            public Action<BindingAxis> Setter;
        }

        ButtonState[] _triggers = new ButtonState[128];
        int _triggersCount;
        AxisState[] _axis = new AxisState[128];
        int _axisCount;

        int _joysticsCount = 0;

        readonly Dictionary<string, bool> _excludeAxis = new Dictionary<string, bool>();

        internal void ListenKey(FieldInfo fieldInfo)
        {
            try
            {
                var field = Expression.Field(null, fieldInfo);
                var value = Expression.Parameter(fieldInfo.FieldType);
                var getter = Expression
                    .Lambda<Func<BindingButton>>(field)
                    .Compile();
                var setter = Expression
                    .Lambda<Action<BindingButton>>(
                        Expression.Assign(field, value),
                        value)
                    .Compile();

                var state = new ButtonState
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

        internal void ListenAxis(FieldInfo fieldInfo)
        {
            try
            {
                var field = Expression.Field(null, fieldInfo);
                var value = Expression.Parameter(fieldInfo.FieldType);
                var getter = Expression
                    .Lambda<Func<BindingAxis>>(field)
                    .Compile();
                var setter = Expression
                    .Lambda<Action<BindingAxis>>(
                        Expression.Assign(field, value),
                        value)
                    .Compile();

                var state = new AxisState
                {
                    Getter = getter,
                    Setter = setter
                };
                Append(ref _axisCount, ref _axis, in state);
            }
            catch (Exception exc)
            {
                Debug.LogError($"Error when create listener of {fieldInfo.Name}");
                Debug.LogException(exc);
            }
        }

        void Append<T>(ref int count, ref T[] states, in T state)
        {
            if (count == states.Length)
            {
                var newStates = new T[states.Length * 2];
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
                for (var ti = 0; ti < b._triggers.Length; ti++)
                {
                    if (GetPressed(ref b._triggers[ti]))
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
        bool GetPressed(ref ButtonTrigger t)
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

        public bool ReadAnyButton(BindingFilterFlags filter, out ButtonTrigger value)
        {
            if (filter.HasFlag(BindingFilterFlags.Keyboard)
                && ReadKeyCodes(SettingsController.DefaultKeyboardKeyCodes, out value))
            {
                return true;
            }

            if (filter.HasFlag(BindingFilterFlags.Joystick)
                && ReadKeyCodes(SettingsController.DefaultJoystickKeyCodes, out value))
            {
                return true;
            }

            if (filter.HasFlag(BindingFilterFlags.MouseKeys)
                && ReadKeyCodes(SettingsController.DefaultMouseKeyCodes, out value))
            {
                return true;
            }

            if (filter.HasFlag(BindingFilterFlags.MouseAxis)
                && ReadAxis(SettingsController.DefaultMouseAxis, out value))
            {
                return true;
            }

            if (filter.HasFlag(BindingFilterFlags.Joystick)
                && ReadAxis(SettingsController.DefaultJoystickAxis, out value))
            {
                return true;
            }

            value = default;
            return false;
        }

        public bool ReadAxis(BindingFilterFlags filter, out ButtonTrigger value)
        {
            if (filter.HasFlag(BindingFilterFlags.Keyboard)
                && ReadKeyCodes(SettingsController.DefaultKeyboardKeyCodes, out value))
            {
                return true;
            }

            if (filter.HasFlag(BindingFilterFlags.Joystick)
                && ReadKeyCodes(SettingsController.DefaultJoystickKeyCodes, out value))
            {
                return true;
            }

            if (filter.HasFlag(BindingFilterFlags.MouseKeys)
                && ReadKeyCodes(SettingsController.DefaultMouseKeyCodes, out value))
            {
                return true;
            }

            if (filter.HasFlag(BindingFilterFlags.MouseAxis)
                && ReadAxis(SettingsController.DefaultMouseAxis, out value))
            {
                return true;
            }

            if (filter.HasFlag(BindingFilterFlags.Joystick)
                && ReadAxis(SettingsController.DefaultJoystickAxis, out value))
            {
                return true;
            }

            value = default;
            return false;
        }

        private bool ReadKeyCodes(KeyCode[] codes, out ButtonTrigger value)
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

        private bool ReadAxis(string[] axis, out ButtonTrigger value)
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

        public bool ReadAnyAxis(BindingFilterFlags filter, out AxisTrigger value)
        {
            if (filter.HasFlag(BindingFilterFlags.MouseAxis)
                && ReadAxis(SettingsController.DefaultMouseAxis, out value))
            {
                return true;
            }
            if (filter.HasFlag(BindingFilterFlags.Joystick)
                && ReadAxis(SettingsController.DefaultJoystickAxis, out value))
            {
                return true;
            }
            value = default;
            return false;
        }

        private bool ReadAxis(string[] axis, out AxisTrigger value)
        {
            foreach (var a in axis)
            {
                if (GetAxis(a) != 0)
                {
                    value = a;
                    return true;
                }
            }
            value = default;
            return false;
        }
    }
}
