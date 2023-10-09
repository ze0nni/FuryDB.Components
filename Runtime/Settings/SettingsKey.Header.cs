using System.Reflection;
using Newtonsoft.Json;
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

            internal override void Load(JsonTextReader reader)
            {
                throw new System.NotImplementedException();
            }

            internal override void Save(JsonTextWriter writer)
            {
                throw new System.NotImplementedException();
            }

            internal override void LoadDefault()
            {
                
            }

            internal override string ToJsonString()
            {
                throw new System.NotImplementedException();
            }
        }
    }
}