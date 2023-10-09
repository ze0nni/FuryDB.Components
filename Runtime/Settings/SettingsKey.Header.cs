using System.Reflection;
using UnityEngine;

namespace FDB.Components.Settings
{
    public partial class SettingsKey
    {
        public sealed class Header<TKeyData> : SettingsKey<TKeyData>
            where TKeyData : ISettingsKeyData
        {
            public Header(SettingsGroup<TKeyData> group, HeaderAttribute header, ICustomAttributeProvider keyAttributesProvide) : base(group, header, keyAttributesProvide)
            {
            }

            protected override void ApplyValue()
            {
                
            }

            protected override void ResetValue()
            {
                
            }

            internal override void Load(string str)
            {
                
            }

            internal override void LoadDefault()
            {
                
            }
        }
    }
}