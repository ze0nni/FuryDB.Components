using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace FDB.Components.Settings
{
    public interface ISettingsKeyFactory
    {
        SettingsKey<TKeyData> Produce<TKeyData>(SettingsGroup<TKeyData> group, FieldInfo keyField)
            where TKeyData : ISettingsKeyData;
    }

    public abstract partial class SettingsKey
    {
        public readonly string Name;
        public readonly string Id;
        public readonly SettingsGroup Group;
        public readonly Type KeyType;
        public readonly FieldInfo KeyField;

        public string StringValue { get; private set; }
        internal void UpdateStringValue(string value) => StringValue = value;

        public bool IsChanged { get; private set; }

        internal SettingsKey(SettingsGroup group, FieldInfo keyField)
        {
            Name = keyField.Name;
            Id = $"{group.Name}.{Name}";
            Group = group;
            KeyType = keyField.FieldType;
            KeyField = keyField;
            SettingsController.DefaultKeys.Store(this);
        }

        protected void OnKeyChanged()
        {
            IsChanged = true;
            Group.OnKeyChanged(this);
        }

        internal virtual void Setup()
        {

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

        internal override void Setup()
        {
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
                OnKeyChanged();
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
