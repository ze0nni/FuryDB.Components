using FDB.Components.Settings;
using System;
using UnityEngine;

namespace FDB.Components.Settings
{
    public sealed partial class BindingKey<TKeyData> : SettingsKey<BindingAction, TKeyData>
            where TKeyData : ISettingsKeyData
    {
        protected internal override void OnFieldGUI(ISettingsGUIState state, float containerWidth)
        {
            var newValue = OriginValue;
            if (newValue.Triggers == null)
            {
                return;
            }

            var i = 0;
            foreach (var t in newValue.Triggers)
            {
                var index = i++;
                if (GUILayout.Button(t.ToString()))
                {
                    var handle = ReadTrigger(state, index, state.CloseWindow);

                    state.OpenWindow(
                        Data.Name,
                        state.RowHeight * 15, state.RowHeight * 3,
                        true,
                        () =>
                        {
                            GUILayout.FlexibleSpace();
                            using (new GUILayout.HorizontalScope())
                            {
                                GUILayout.FlexibleSpace();
                                if (handle.First == null)
                                {
                                    GUILayout.Label("Read input");
                                }
                                else
                                {
                                    GUILayout.Label($"Press {handle.First} again");
                                }
                                GUILayout.FlexibleSpace();
                            }
                            GUILayout.FlexibleSpace();
                        });
                }
            }
        }

        public ReadBindingKeyHandle ReadTrigger(ISettingsGUIState state, int triggerIndex, Action close)
        {
            var mediator = Group.Page.Controller.Registry.Get<BindingKeyMediator>();
            return new ReadBindingKeyHandle(mediator, state, FilterFlags, close, (ActionTrigger t) =>
            {
                var value = Value;
                value.Triggers[triggerIndex] = t;
                Value = value;
            });
        }
    }
}

public class ReadBindingKeyHandle : IDisposable
{
    readonly BindingKeyMediator _mediator;
    readonly ISettingsGUIState _state;
    readonly BindingKeyFilterFlags _filter;
    Action _close;
    readonly Action<ActionTrigger> _triggerCallback;

    private KeyCode? _firstKey;
    public KeyCode? First => _firstKey;

    internal ReadBindingKeyHandle(
        BindingKeyMediator mediator,
        ISettingsGUIState state,
        BindingKeyFilterFlags filter,
        Action close,
        Action<ActionTrigger> callback)
    {
        _mediator = mediator;
        _state = state;
        _filter = filter;
        _close = close;
        _triggerCallback = callback;
        Listen();
    }

    void Listen()
    {
        _state.OnUpdate += OnUpdate;
        _state.OnGUIEvent += OnGUIEvent;
    }

    ActionTrigger _lastReadedTrigger;

    void OnUpdate()
    {
        if (_state.Mode == GUIMode.Screen)
        {
            if (_mediator.ReadTrigger(_filter, out var t))
            {
                if (_lastReadedTrigger != t)
                {
                    _lastReadedTrigger = t;
                    if (!t.IsNull)
                    {
                        Perform(t);
                    }
                }
            }
            else
            {
                _lastReadedTrigger = default;
            }
        }
    }

    void OnGUIEvent(Event e)
    {
        if (_state.Mode == GUIMode.Editor)
        {
            if (e.type == EventType.KeyDown && e.keyCode != KeyCode.None)
            {
                Perform(e.keyCode);
                e.Use();
            }
        }
    }

    void Perform(ActionTrigger t)
    {
        if (t.Key == KeyCode.Escape && _firstKey == null)
        {
            _firstKey = t.Key;
        }
        else if (_firstKey != null)
        {
            if (t.Key == _firstKey)
            {
                Dispose();
                _triggerCallback(t);
            } else
            {
                _triggerCallback(default);
                Dispose();
            }
        }
        else
        {
            Dispose();
            _triggerCallback(t);
        }
    }

    public void Dispose()
    {
        if (_close == null)
        {
            return;
        }
        _state.OnUpdate -= OnUpdate;
        _state.OnGUIEvent -= OnGUIEvent;
        var c = _close;
        _close = null;
        c.Invoke();
    }
}