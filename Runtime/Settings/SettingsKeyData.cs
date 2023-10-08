using System.Reflection;

namespace FDB.Components.Settings
{
    public interface ISettingsKeyData
    {
        string Name { get; }
        void Setup(SettingsKey key);
        void Setup(FieldInfo field);
    }

    public class VoidSettingsKeyData : ISettingsKeyData
    {
        public string Name { get; private set; }

        void ISettingsKeyData.Setup(SettingsKey key)
        {
            Name = key.KeyField.Name;
        }

        void ISettingsKeyData.Setup(FieldInfo field)
        {
            Name = field.Name;
        }
    }
}