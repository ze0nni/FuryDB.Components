using System.Reflection;
using UnityEngine;

namespace FDB.Components.Settings
{
    public interface ISettingsKeyData
    {
        string Name { get; }
        void Setup(SettingsKey key);
        void Setup(FieldInfo field);
        void Setup(SettingsGroup group, HeaderAttribute header, ICustomAttributeProvider attributeProvider);
    }

    public class DefaultKeyData : ISettingsKeyData
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

        void ISettingsKeyData.Setup(SettingsGroup group, HeaderAttribute header, ICustomAttributeProvider attributeProvider)
        {
            Name = header.header;
        }
    }
}