using Newtonsoft.Json;
using System.Reflection;

namespace FDB.Components.Settings
{
    internal class BindingAxisFactory : ISettingsKeyFactory
    {
        public SettingsKey<TKeyData> Produce<TKeyData>(
            KeyContext context,
            SettingsGroup<TKeyData> group,
            FieldInfo keyField)
            where TKeyData : ISettingsKeyData
        {
            if (keyField.FieldType != typeof(BindingAxis))
            {
                return null;
            }
            return new BindingAxisKey<TKeyData>(context, group, keyField);
        }
    }

    public sealed partial class BindingAxisKey<TKeyData> : SettingsKey<BindingAxis, TKeyData>
        where TKeyData : ISettingsKeyData
    {
        public readonly BindingFilterFlags FilterFlags;

        public BindingAxisKey(KeyContext context, SettingsGroup<TKeyData> group, FieldInfo keyField) : base(group, keyField)
        {
            FilterFlags = BindingFilterAttributeAttribute.Resolve(keyField);

            if (group.Page.PrimaryGameObject != null)
            {
                var mediator = context.PrimaryRegistry.GetOrCreate(
                    null,
                    () => group.Page.PrimaryGameObject.AddComponent<BindingMediator>());
                mediator.ListenAxis(keyField);
            }
        }

        protected override BindingAxis ReadValue(object value)
        {
            var def = (BindingAxis)SettingsController.DefaultKeys.Read(this);
            var curr = (BindingAxis)value;

            //TODO: curr = def + curr;

            return curr;
        }

        protected override bool ValidateValue(ref BindingAxis value)
        {
            return true;
        }

        protected override BindingAxis ValueFromJson(JsonTextReader reader)
        {
            var s = new JsonSerializer();
            var v = s.Deserialize<BindingAxis>(reader);
            return v;
        }

        protected override void ValueToJson(JsonTextWriter writer, BindingAxis value)
        {
            var s = new JsonSerializer();
            s.DefaultValueHandling = DefaultValueHandling.Ignore;
            s.Serialize(writer, value);
        }

        protected override object WriteValue(BindingAxis value)
        {
            return value;
        }
    }
}