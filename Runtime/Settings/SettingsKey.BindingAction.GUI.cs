using System;
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
                switch (OriginValue.Type)
                {
                    case BindingActionType.Trigger:
                        OnTriggerGUI(state, OriginValue);
                        break;

                }
            }

            private void OnTriggerGUI(ISettingsGUIState state, BindingAction newValue)
            {
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

            public ReadActionHandle ReadTrigger(ISettingsGUIState state, int triggerIndex, Action close)
            {
                var mediator = Group.Page.Controller.Registry.Get<BindingActionMediator>();
                return new ReadActionHandle(mediator, state, close, (ActionTrigger t)  =>
                {
                    var value = Value;
                    value.Triggers[triggerIndex] = t;
                    Value = value;
                });
            }

            public ReadActionHandle ReadAxis(ISettingsGUIState state, int axisIndex, Action close)
            {
                var mediator = Group.Page.Controller.Registry.Get<BindingActionMediator>();
                return new ReadActionHandle(mediator, state, close, (string a) =>
                {
                });
            }
        }
    }

    public class ReadActionHandle : IDisposable
    {
        public readonly BindingActionType Type;
        readonly BindingActionMediator _mediator;
        readonly ISettingsGUIState _state;
        Action _close;
        readonly Action<ActionTrigger> _triggerCallback;
        readonly Action<string> _axisCallback;

        private KeyCode? _firstKey;
        public KeyCode? First => _firstKey;

        internal ReadActionHandle(BindingActionMediator mediator, ISettingsGUIState state, Action close, Action<ActionTrigger> callback)
        {
            Type = BindingActionType.Trigger;
            _mediator = mediator;
            _state = state;
            _close = close;
            _triggerCallback = callback;
            Listen();
        }

        internal ReadActionHandle(BindingActionMediator mediator, ISettingsGUIState state, Action close, Action<string> callback)
        {
            Type = BindingActionType.Axis;
            _mediator = mediator;
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
            if (_state.Mode == GUIMode.Screen )
            {
                if (Type == BindingActionType.Trigger && _mediator.ReadTrigger(out var t))
                {
                    Perform(t);
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
            else if (t.Key == KeyCode.Escape && _firstKey == KeyCode.Escape)
            {
                Dispose();
                _triggerCallback(t);
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
}