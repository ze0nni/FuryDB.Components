using System;
using System.Reflection;
using UnityEngine;

namespace FDB.Components.Settings
{
    [Flags]
    public enum BindingKeyFilterFlags
    {
        Keyboard = 1 << 0,
        Joystick = 1 << 1,
        MouseKeys = 1 << 2,
        MouseAxis = 1 << 3,
        All = Keyboard | Joystick | MouseKeys | MouseAxis
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Field, AllowMultiple = false)]
    public sealed class BindingKeyFilterAttribute : Attribute
    {
        public BindingKeyFilterFlags Flags;
        public BindingKeyFilterAttribute(BindingKeyFilterFlags flags) => Flags = flags;

        public static BindingKeyFilterFlags Resolve(FieldInfo field)
        {
            var resultFlags = BindingKeyFilterFlags.All;

            var fieldAttr = field.GetCustomAttribute<BindingKeyFilterAttribute>();
            if (fieldAttr != null)
            {
                resultFlags = fieldAttr.Flags;
            }

            void FindInParent(Type parent, ref BindingKeyFilterFlags flags)
            {
                if (parent == null)
                {
                    return;
                }
                var attr = parent.GetCustomAttribute<BindingKeyFilterAttribute>();
                if (attr != null)
                {
                    flags = attr.Flags;
                }
                FindInParent(parent.DeclaringType, ref flags);
            }
            FindInParent(field.DeclaringType, ref resultFlags);

            return resultFlags;
        }
    }
}