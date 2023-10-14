using System.Reflection;
using Newtonsoft.Json;
using UnityEngine;

namespace FDB.Components.Settings
{
    public partial class SettingsKey
    {
        internal class BindingActionFactory : ISettingsKeyFactory
        {
            public SettingsKey<TKeyData> Produce<TKeyData>(
                KeyContext context,
                SettingsGroup<TKeyData> group,
                FieldInfo keyField)
                where TKeyData : ISettingsKeyData
            {
                if (keyField.FieldType != typeof(BindingAction))
                {
                    return null;
                }
                return new BindingAction<TKeyData>(group, keyField);
            }
        }

        public sealed partial class BindingAction<TKeyData> : SettingsKey<BindingAction, TKeyData>
            where TKeyData : ISettingsKeyData
        {
            internal BindingAction(
                SettingsGroup<TKeyData> group,
                FieldInfo keyField
                ) : base(group, keyField)
            {
                if (group.Page.PrimaryGameObject != null)
                {
                    if (!group.Page.PrimaryGameObject.TryGetComponent<BindingActionMediator>(out var mediator))
                    {
                        mediator = group.Page.PrimaryGameObject.AddComponent<BindingActionMediator>();
                    }
                    mediator.Listen(keyField);
                }
            }

            protected override BindingAction ReadValue(object value)
            {
                var def = (BindingAction)SettingsController.DefaultKeys.Read(this);
                var curr = (BindingAction)value;

                curr = def + curr;

                return curr;
            }

            protected override bool ValidateValue(ref BindingAction value)
            {
                return true;
            }

            protected override BindingAction ValueFromJson(JsonTextReader reader)
            {
                var s = new JsonSerializer();
                var v = s.Deserialize<BindingAction>(reader);
                return v;
            }

            protected override void ValueToJson(JsonTextWriter writer, BindingAction value)
            {
                var s = new JsonSerializer();
                s.DefaultValueHandling = DefaultValueHandling.Ignore;
                s.Serialize(writer, value);
            }

            protected override object WriteValue(BindingAction value)
            {
                return value;
            }
        }
    }
}