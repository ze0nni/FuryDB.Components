using System.Reflection;
using Newtonsoft.Json;

namespace FDB.Components.Settings
{
    internal class BindingFactory : ISettingsKeyFactory
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
            return new BindingKey<TKeyData>(context, group, keyField);
        }
    }

    public sealed partial class BindingKey<TKeyData> : SettingsKey<BindingAction, TKeyData>
        where TKeyData : ISettingsKeyData
    {
        internal BindingKey(
            KeyContext context,
            SettingsGroup<TKeyData> group,
            FieldInfo keyField
            ) : base(group, keyField)
        {
            if (group.Page.PrimaryGameObject != null)
            {
                var mediator = context.PrimaryRegistry.GetOrCreate(
                    null,
                    () => group.Page.PrimaryGameObject.AddComponent<BindingKeyMediator>());
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