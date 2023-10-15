using System;
using UnityEngine;

namespace FDB.Components.Settings
{
    public sealed partial class BindingAxisKey<TKeyData> : SettingsKey<BindingAxis, TKeyData>
        where TKeyData : ISettingsKeyData
    {
        protected internal override void OnFieldGUI(ISettingsGUIState state, float containerWidth)
        {
            var value = DirectValue;
            if (value.Triggers != null && value.Triggers.Count > 0)
            {
                var i = 0;
                foreach (var t in value.Triggers)
                {
                    var index = i++;
                    if (GUILayout.Button(t.ToString()))
                    {
                        if (Event.current.button == 0)
                        {
                            var handle = ReadTrigger(state, index, state.CloseWindow);

                            state.OpenWindow(
                                Data.Name,
                                state.RowHeight * 15, state.RowHeight * 4,
                                true,
                                () =>
                                {
                                    GUILayout.FlexibleSpace();
                                    using (new GUILayout.HorizontalScope())
                                    {
                                        GUILayout.FlexibleSpace();
                                        if (handle.NegKey == null)
                                        {
                                            GUILayout.Label("Read input");
                                        }
                                        else
                                        {
                                            GUILayout.Label($"{handle.NegKey} - ...");
                                        }
                                        GUILayout.FlexibleSpace();
                                    }
                                    GUILayout.FlexibleSpace();
                                    if (GUILayout.Button("Cancel"))
                                    {
                                        handle.Dispose();
                                    }
                                });
                        }
                        else if (Event.current.button == 1)
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
                                        Value = Value.Append();
                                        state.CloseWindow();
                                    }
                                });
                        }
                    }
                }
            }
            else
            {
                if (GUILayout.Button("Add"))
                {
                    Value = Value.Append();
                }
            }
        }

        public ReadBindingAxisHandle ReadTrigger(ISettingsGUIState state, int triggerIndex, Action close)
        {
            var mediator = Group.Page.Controller.Registry.Get<BindingMediator>();
            return new ReadBindingAxisHandle(mediator, state, FilterFlags, close, (AxisTrigger t) =>
            {
                Value = Value.Update(triggerIndex, t);
            });
        }
    }

    public class ReadBindingAxisHandle : IDisposable
    {
        readonly BindingMediator _mediator;
        readonly ISettingsGUIState _state;
        readonly BindingFilterFlags _filter;
        Action _close;
        readonly Action<AxisTrigger> _triggerCallback;

        private KeyCode? _negKey;
        public KeyCode? NegKey => _negKey;

        internal ReadBindingAxisHandle(
            BindingMediator mediator,
            ISettingsGUIState state,
            BindingFilterFlags filter,
            Action close,
            Action<AxisTrigger> callback)
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

        void OnUpdate()
        {
            if (_state.Mode == GUIMode.Screen)
            {
                if (_mediator.ReadAnyAxis(_filter, out var axisTrigger))
                    Perform(axisTrigger);
                if (_mediator.ReadAnyButton(_filter, out var buttonTrigger) && buttonTrigger.Key != KeyCode.None)
                    Perform(buttonTrigger.Key);
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

        void Perform(KeyCode c)
        {
            if (_negKey == null)
            {
                _negKey = c;
            }
            else if (_negKey != c)
            {
                Dispose();
                _triggerCallback((_negKey.Value, c));
            }
        }

        void Perform(AxisTrigger t)
        {
            if (_negKey == null)
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