using System.Reflection;
using Newtonsoft.Json;

namespace FDB.Components.Settings
{
    internal class BindingButtonFactory : ISettingsKeyFactory
    {
        public SettingsKey<TKeyData> Produce<TKeyData>(
            KeyContext context,
            SettingsGroup<TKeyData> group,
            FieldInfo keyField)
            where TKeyData : ISettingsKeyData
        {
            if (keyField.FieldType != typeof(BindingButton))
            {
                return null;
            }
            return new BindingButtonKey<TKeyData>(context, group, keyField);
        }
    }

    public sealed partial class BindingButtonKey<TKeyData> : SettingsKey<BindingButton, TKeyData>
        where TKeyData : ISettingsKeyData
    {
        public readonly BindingFilterFlags FilterFlags;

        internal BindingButtonKey(
            KeyContext context,
            SettingsGroup<TKeyData> group,
            FieldInfo keyField
            ) : base(group, keyField)
        {
            FilterFlags = BindingFilterAttributeAttribute.Resolve(keyField);

            if (group.Page.PrimaryGameObject != null)
            {
                var mediator = context.PrimaryRegistry.GetOrCreate(
                    null,
                    () => group.Page.PrimaryGameObject.AddComponent<BindingMediator>());
                mediator.ListenKey(keyField);
            }
        }

        protected override BindingButton ReadValue(object value)
        {
            var curr = (BindingButton)value;

            return curr;
        }

        protected override bool ValidateValue(ref BindingButton value)
        {
            return true;
        }

        protected override BindingButton ValueFromJson(JsonTextReader reader)
        {
            var s = new JsonSerializer();
            var dto = s.Deserialize<BindingButtonDTO>(reader);
            return dto.ToBinding();
        }

        protected override void ValueToJson(JsonTextWriter writer, BindingButton value)
        {
            var s = new JsonSerializer();
            s.DefaultValueHandling = DefaultValueHandling.Ignore;
            s.Serialize(writer, value.ToDTO());
        }

        protected override object WriteValue(BindingButton value)
        {
            return value;
        }
    }
}