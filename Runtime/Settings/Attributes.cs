using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace FDB.Components.Settings
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class OnApplySettingsAttribute : Attribute
    {
        public static Action ResolveCallback(Type type)
        {
            List<Action> actions = new List<Action>();

            foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
            {
                if (method.GetCustomAttribute<OnApplySettingsAttribute>() == null)
                {
                    continue;
                }
                try
                {
                    var action = (Action)Delegate.CreateDelegate(typeof(Action), null, method);
                    actions.Add(action);
                }
                catch (Exception exc)
                {
                    Debug.LogError($"Incorrect method {type.FullName}:{method.Name}. Except static method without arguments");
                    Debug.LogException(exc);
                }
            }

            if (actions == null || actions.Count == 0)
            {
                return null;
            }

            return () =>
            {
                foreach (var a in actions)
                {
                    try
                    {
                        a.Invoke();
                    }
                    catch (Exception exc)
                    {
                        Debug.LogError("Error when apply settings changes");
                        Debug.LogException(exc);
                    }
                }
            };
        }
    }
}