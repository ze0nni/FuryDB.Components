using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace FDB.Components.Settings
{
    [DefaultExecutionOrder(-100)]
    public sealed class BindingActionMediator : MonoBehaviour
    {
        private void OnEnable()
        {
            Debug.Log(string.Join(",", Input.GetJoystickNames()));
        }

        struct State
        {
            public Func<BindingAction> Getter;
            public Action<BindingAction> Setter;
        }

        State[] _triggers = new State[128];
        int _triggersCount;
        State[] _axis = new State[128];
        int _axisCount;

        internal void Listen(FieldInfo fieldInfo)
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

                var type = getter().Type;
                switch (type)
                {
                    case BindingActionType.Trigger:
                        Append(ref _triggersCount, ref _triggers, in state);
                        break;
                    case BindingActionType.Axis:
                        Append(ref _axisCount, ref _axis, in state);
                        break;
                    default:
                        Debug.LogWarning($"Unknown  type {type}");
                        break;
                }
            } catch (Exception exc)
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
            for (var i = 0; i < _triggersCount; i++)
            {
                ref var state = ref _triggers[i];
                var a = state.Getter();
                var changed = false;

                var pressed = false;
                var lastPressed = a._presset;
                for (var ti = 0; ti < a.Triggers.Length; ti++)
                {
                    if (GetPressed(ref a.Triggers[ti]))
                    {
                        pressed = true;
                        break;
                    }
                }
                var justPressed = !lastPressed && pressed;
                var justReleased = lastPressed && !pressed;

                changed = pressed != lastPressed
                    || justPressed != a._justPressed
                    || justReleased != a._justReleased;

                if (changed)
                {
                    a._presset = pressed;
                    a._justPressed = justPressed;
                    a._justReleased = justReleased;
                    state.Setter(a);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        bool GetPressed(ref ActionTrigger t)
        {
            if (t.Key != KeyCode.None)
            {
                return Input.GetKey(t.Key);
            } else if (t.Axis != null && false)
            {
                return Input.GetAxis(t.Axis) * (t.AxisPositive ? 1 : -1) > 0.25f;
            }
            return false;
        }
    }
}