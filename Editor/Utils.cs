using System;

namespace FDB.Components.Editor
{
    internal static class Utils
    {
        public static Type[] GetGenericTypeImplementationOf(Type inputType, Type genericType)
        {
            if (!genericType.IsGenericType)
            {
                throw new ArgumentException(
                    $"Type {genericType.FullName} is not generic",
                    nameof(genericType));
            }

            var current = inputType;
            while (current != null)
            {
                if (current.IsGenericType && current.GetGenericTypeDefinition() == genericType)
                {
                    return current.GetGenericArguments();
                }
                current = current.BaseType;
            }

            throw new ArgumentException(
                $"Type {inputType.FullName} not implements {genericType.FullName}",
                nameof(inputType));
        }
    }
}