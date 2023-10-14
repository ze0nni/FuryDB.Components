using System;
using System.Reflection;

namespace FDB.Components.Settings
{
    [Flags]
    public enum BindingFilterFlags
    {
        Keyboard = 1 << 0,
        Joystick = 1 << 1,
        MouseKeys = 1 << 2,
        MouseAxis = 1 << 3,
        All = Keyboard | Joystick | MouseKeys | MouseAxis
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Field, AllowMultiple = false)]
    public sealed class BindingFilterAttributeAttribute : Attribute
    {
        public BindingFilterFlags Flags;
        public BindingFilterAttributeAttribute(BindingFilterFlags flags) => Flags = flags;

        public static BindingFilterFlags Resolve(FieldInfo field)
        {
            var resultFlags = BindingFilterFlags.All;

            var fieldAttr = field.GetCustomAttribute<BindingFilterAttributeAttribute>();
            if (fieldAttr != null)
            {
                return fieldAttr.Flags;
            }

            void FindInParent(Type parent, ref BindingFilterFlags flags)
            {
                if (parent == null)
                {
                    return;
                }
                var attr = parent.GetCustomAttribute<BindingFilterAttributeAttribute>();
                if (attr != null)
                {
                    flags = attr.Flags;
                }
                else
                {
                    FindInParent(parent.DeclaringType, ref flags);
                }
            }
            FindInParent(field.DeclaringType, ref resultFlags);

            return resultFlags;
        }
    }
}