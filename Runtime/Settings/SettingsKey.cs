using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace FDB.Components.Settings
{
    public interface ISettingsKeyFactory
    {
        SettingsKey<TKeyData> Produce<TKeyData>(SettingsGroup<TKeyData> group, FieldInfo keyField)
            where TKeyData : ISettingsKeyData;
    }

    public enum KeyType
    {
        Key,
        Header
    }

    public abstract partial class SettingsKey
    {
        public readonly string KeyName;
        public readonly KeyType Type;
        public readonly string Id;
        public readonly SettingsGroup Group;
        public readonly Type KeyType;
        public readonly FieldInfo KeyField;
        public readonly ICustomAttributeProvider KeyAttributesProvider;
        public readonly HeaderAttribute HeaderAttr;
        public readonly IReadOnlyList<Attribute> HeaderAttributes;

        private bool _enabled;
        public bool Enabled => _enabled;
        private bool _visible;
        public bool Visible => _visible;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void UpdateDisplayState(SettingsGroup group)
        {
            var enabled = _enabledPredicate == null ? true : _enabledPredicate(group, this);
            var visible = _visiblePredicate == null ? true : _visiblePredicate(group, this);
            if (_enabled == enabled && _visible == visible)
            {
                return;
            }
            _enabled = enabled;
            _visible = visible;
            OnKeyChanged?.Invoke(this);
        }

        public string StringValue { get; private set; }
        internal void UpdateStringValue(string value) => StringValue = value;

        public bool IsChanged { get; private set; }
        public event Action<SettingsKey> OnKeyChanged;

        public delegate bool DisplayPredecateDelegate(SettingsGroup group, SettingsKey key);

        internal readonly DisplayPredecateDelegate _enabledPredicate;
        internal readonly DisplayPredecateDelegate _visiblePredicate;

        internal SettingsKey(SettingsGroup group, FieldInfo keyField)
        {
            KeyName = keyField.Name;
            Type = Settings.KeyType.Key;
            Id = $"{group.Name}.{KeyName}";
            Group = group;
            KeyType = keyField.FieldType;
            KeyField = keyField;
            KeyAttributesProvider = keyField;
            _enabledPredicate = SettingsPredicateAttribute
                .Resolve<SettingsEnabledAttribute>(group.GroupType, keyField);
            _visiblePredicate = SettingsPredicateAttribute
                .Resolve<SettingsVisibleAttribute>(group.GroupType, keyField);
        }

        internal SettingsKey(SettingsGroup group, HeaderAttribute header, ICustomAttributeProvider keyAttributesProvider)
        {
            KeyName = null;
            Type = Settings.KeyType.Header;
            Id = null;
            Group = group;
            KeyType = typeof(void);
            KeyAttributesProvider = keyAttributesProvider;
            HeaderAttr = header;
            _visiblePredicate = SettingsPredicateAttribute
                .Resolve<SettingsVisibleAttribute>(group.GroupType, keyAttributesProvider);
        }

        protected void NotifyKeyChanged()
        {
            IsChanged = true;
            OnKeyChanged?.Invoke(this);
            Group.NotifyKeyChanged(this);
        }

        internal virtual void Setup()
        {
            UpdateDisplayState(Group);
        }

        internal abstract void LoadDefault();
        internal abstract void Load(string str);

        internal protected void Apply()
        {
            ApplyValue();
            IsChanged = false;
        }

        internal protected void Reset()
        {
            ResetValue();
            IsChanged = false;
        }

        protected abstract void ApplyValue();
        protected abstract void ResetValue();
    }

    public abstract class SettingsKey<TKeyData> : SettingsKey
        where TKeyData : ISettingsKeyData
    {
        public readonly new SettingsGroup<TKeyData> Group;
        public TKeyData Data { get; private set; }

        internal SettingsKey(SettingsGroup<TKeyData> group, FieldInfo keyField) : base(group, keyField)
        {
            Group = group;
        }

        internal SettingsKey(SettingsGroup<TKeyData> group, HeaderAttribute header, ICustomAttributeProvider keyAttributesProvide) : base(group, header, keyAttributesProvide)
        {
            Group = group;
        }

        internal override void Setup()
        {
            base.Setup();
           Data = Group.Page.Controller.CreateKeyData<TKeyData>(this);
        }

        protected internal virtual void OnGUILayout()
        {
            GUILayout.Label(Data.Name);
            OnFieldLayout();
        }

        protected internal virtual void OnFieldLayout()
        {

        }
    }

    public abstract class SettingsKey<TValue, TKeyData> : SettingsKey<TKeyData>
        where TValue : IEquatable<TValue>
        where TKeyData : ISettingsKeyData
    {
        private TValue _value;

        public TValue Value
        {
            get => _value;
            set
            {
                if (object.ReferenceEquals(_value, value))
                {
                    return;
                }
                if (_value.Equals(value))
                {
                    return;
                }
                if (!ValidateValue(ref value))
                {
                    throw new ArgumentException($"Invalid value \"{value}\" for key {Id}");
                }
                _value = value;
                UpdateStringValue(ValueToString(value));
                NotifyKeyChanged();
            }
        }

        public SettingsKey(SettingsGroup<TKeyData> group, FieldInfo keyField): base(group, keyField)
        {
        }

        internal override void Setup()
        {
            base.Setup();
            ResetValue();
        }

        internal sealed override void Load(string str)
        {
            var value = ValueFromString(str);
            ValidateValue(ref value);
            _value = value;
            UpdateStringValue(ValueToString(value));
        }

        internal sealed override void LoadDefault()
        {
            Value = ReadValue(SettingsController.DefaultKeys.Read(this));
        }

        protected sealed override void ApplyValue()
        {
            KeyField.SetValue(null, WriteValue(Value));
        }

        protected sealed override void ResetValue()
        {
            var value = ReadValue(KeyField.GetValue(null));
            ValidateValue(ref value);
            _value = ReadValue(value);
            UpdateStringValue(ValueToString(_value));
        }

        protected abstract bool ValidateValue(ref TValue value);
        protected abstract TValue ReadValue(object value);
        protected abstract object WriteValue(TValue value);
        protected abstract string ValueToString(TValue value);
        protected abstract TValue ValueFromString(string value);
    }
}
