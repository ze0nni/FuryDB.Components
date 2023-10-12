using System;
using System.Collections.Generic;
using UnityEngine;

namespace FDB.Components.Settings
{
    public partial class SettingsKey
    {
        public sealed partial class BindingAction<TKeyData> : SettingsKey<BindingAction, TKeyData>
            where TKeyData : ISettingsKeyData
        {
            protected internal override void OnFieldGUI(ISettingsGUIState state, float containerWidth)
            {
                switch (Value.Type)
                {
                    case BindingActionType.Trigger:
                        OnTriggerGUI(state, Value);
                        break;

                }
            }

            private void OnTriggerGUI(ISettingsGUIState state, BindingAction a)
            {
                if (a.Triggers == null)
                {
                    return;
                }

                var i = 0;
                foreach (var t in a.Triggers)
                {
                    var index = i++;
                    if (GUILayout.Button(t.ToString()))
                    {
                        var handle = ReadTrigger(state, state.CloseWindow, newT =>
                        {
                            a.Triggers[index] = newT;
                            state.CloseWindow();
                        });

                        state.OpenWindow(
                            Data.Name,
                            state.RowHeight * 10, state.RowHeight * 3,
                            true,
                            () =>
                            {
                                GUILayout.FlexibleSpace();
                                using (new GUILayout.HorizontalScope())
                                {
                                    GUILayout.FlexibleSpace();
                                    if (handle.First == null)
                                    {
                                        GUILayout.Label("Read input...");
                                    } else
                                    {
                                        GUILayout.Label($"{handle.First}?");
                                    }
                                    GUILayout.FlexibleSpace();
                                }
                                GUILayout.FlexibleSpace();
                            });
                    }
                }
            }

            public ReadActionHandle ReadTrigger(ISettingsGUIState state, Action close, Action<ActionTrigger> callback)
            {
                return new ReadActionHandle(state, close, callback);
            }

            public ReadActionHandle ReadAxis(ISettingsGUIState state, Action close, Action<string> callback)
            {
                return new ReadActionHandle(state, close, callback);
            }
        }
    }

    public class ReadActionHandle : IDisposable
    {
        public readonly BindingActionType Type;
        readonly ISettingsGUIState _state;
        readonly Action _close;
        readonly Action<ActionTrigger> _triggerCallback;
        readonly Action<string> _axisCallback;

        ActionTrigger? _first;
        public ActionTrigger? First => _first;

        internal ReadActionHandle(ISettingsGUIState state, Action close, Action<ActionTrigger> callback)
        {
            Type = BindingActionType.Trigger;
            _state = state;
            _close = close;
            _triggerCallback = callback;
            Listen();
        }

        internal ReadActionHandle(ISettingsGUIState state, Action close, Action<string> callback)
        {
            Type = BindingActionType.Axis;
            _state = state;
            _close = close;
            _axisCallback = callback;
            Listen();
        }

        void Listen()
        {
            _state.OnUpdate += OnUpdate;
            _state.OnGUIEvent += OnGUIEvent;
        }

        void OnUpdate()
        {

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
            if (_first == null)
            {
                _first = t;
                _state.Repaint();
            } else
            {
                if (_first.Value == t)
                {
                    Dispose();
                    _triggerCallback(t);
                } else
                {
                    _first = null;
                    _state.Repaint();
                }
            }
        }

        public void Dispose()
        {
            _state.OnUpdate -= OnUpdate;
            _state.OnGUIEvent -= OnGUIEvent;
        }
    }
}