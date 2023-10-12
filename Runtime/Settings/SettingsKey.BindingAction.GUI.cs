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
                if (a.Triggers != null)
                {
                    foreach (var t in a.Triggers)
                    {
                        if (GUILayout.Button(t.ToString()))
                        {
                            state.OpenWindow(
                                Data.Name,
                                state.RowHeight * 10, state.RowHeight * 3,
                                true,
                                () =>
                                {

                                });
                        }
                    }
                }
            }
        }
    }
}