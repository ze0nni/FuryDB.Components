using FDB.Components.Settings;
using System;
using UnityEngine;

namespace FDB.Components.Settings
{
    public sealed partial class BindingButtonKey<TKeyData> : SettingsKey<BindingButton, TKeyData>
            where TKeyData : ISettingsKeyData
    {
        protected internal override void OnFieldGUI(ISettingsGUIState state, float containerWidth)
        {
            var newValue = DirectValue;
            if (newValue.Triggers == null || newValue.Triggers.Count == 0)
            {
                if (GUILayout.Button("Add"))
                {
                    Value = Value.Append(default);
                }
                return;
            }

            var i = 0;
            foreach (var t in newValue.Triggers)
            {
                var index = i++;
                if (GUILayout.Button(t.ToString()))
                {
                    if (Event.current.button == 0)
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
                                if (GUILayout.Button("Cancel"))
                                {
                                    handle.Dispose();
                                }
                            });
                    } else if (Event.current.button == 1)
                    {
                        state.ShowDropdownWindow(
                                GuiRects.field,
                                () =>
                                {
                                    GUILayout.Label(Value.Triggers[index].ToString(), GUI.skin.box, GUILayout.ExpandWidth(true));

                                    if (index < DefaultValue.Triggers.Count && GUILayout.Button("Default"))
                                    {
                                        Value = Value.Update(index, DefaultValue.Triggers[index]);
                                        state.CloseWindow();
                                    }
                                    if (GUILayout.Button("Set null"))
                                    {
                                        Value = Value.Update(index, default);
                                        state.CloseWindow();
                                    }
                                    if (index >= DefaultValue.Triggers.Count && GUILayout.Button("Delete"))
                                    {
                                        Value = Value.Delete(index);
                                        state.CloseWindow();
                                    }
                                    if (GUILayout.Button("Add"))
                                    {
                                        Value = Value.Append(default);
                                        state.CloseWindow();
                                    }
                                });
                    }
                }
            }
        }

        public ReadBindingButtonHandle ReadTrigger(ISettingsGUIState state, int triggerIndex, Action close)
        {
            var mediator = Group.Page.Controller.Registry.Get<BindingMediator>();
            return new ReadBindingButtonHandle(mediator, state, FilterFlags, close, (ButtonTrigger t) =>
            {
                Value = Value.Update(triggerIndex, t);
            });
        }
    }
}

public class ReadBindingButtonHandle : IDisposable
{
    readonly BindingMediator _mediator;
    readonly ISettingsGUIState _state;
    readonly BindingFilterFlags _filter;
    Action _close;
    readonly Action<ButtonTrigger> _triggerCallback;

    private KeyCode? _firstKey;
    public KeyCode? First => _firstKey;

    internal ReadBindingButtonHandle(
        BindingMediator mediator,
        ISettingsGUIState state,
        BindingFilterFlags filter,
        Action close,
        Action<ButtonTrigger> callback)
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

    ButtonTrigger _lastReadedTrigger;

    void OnUpdate()
    {
        if (_state.Mode == GUIMode.Screen)
        {
            if (_mediator.ReadAnyButton(_filter, out var t))
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

    void Perform(ButtonTrigger t)
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