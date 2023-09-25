using FDB.Editor;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;

namespace FDB.Components.Editor
{
    [CustomPropertyDrawer(typeof(TextValue<,>))]
    internal class TextValueProperyRenderer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight * 2;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (GCache.Error != null)
            {
                EditorGUI.HelpBox(position, GCache.Error, MessageType.Error);
                return;
            }

            var translateProp = property.FindPropertyRelative("Translate");
            var valueProp = property.FindPropertyRelative("Value");

            EditorGUI.BeginProperty(position, label, property);
            {
                position = EditorGUI.PrefixLabel(position, label);

                var translateRect = new Rect(position.x, position.y, position.width, position.height / 2);
                {
                    EditorGUI.BeginChangeCheck();
                    var newTranslate = GUI.Toggle(translateRect, translateProp.boolValue, translateProp.name);
                    if (EditorGUI.EndChangeCheck())
                    {
                        translateProp.boolValue = newTranslate;
                    }
                }

                var valueRect = new Rect(position.x, position.y + position.height / 2, position.width, position.height / 2);
                var browseRect = default(Rect);
                if (translateProp.boolValue)
                {
                    valueRect.width -= valueRect.height * 2;
                    browseRect = new Rect(valueRect.xMax, valueRect.y, valueRect.height * 2, valueRect.height);
                }

                var isValid = !translateProp.boolValue || GCache.ValidateKind(valueProp.stringValue);

                GUI.color = isValid ? Color.white : Color.red;
                var newValue = EditorGUI.TextField(valueRect, valueProp.stringValue);
                if (newValue != valueProp.stringValue)
                {
                    valueProp.stringValue = newValue;
                }
                GUI.color = Color.white;

                if (translateProp.boolValue)
                {
                    if (GUI.Button(browseRect, new GUIContent(null, FDBEditorIcons.LinkIcon)))
                    {
                        var kinds = GCache.GetKinds();
                        PopupWindow.Show(valueRect, new TextKindChooseWindow(valueProp.stringValue, kinds, position.width, newKind =>
                        {
                            valueProp.serializedObject.Update();
                            valueProp.stringValue = newKind;
                            valueProp.serializedObject.ApplyModifiedProperties();
                            GUI.changed = true;
                        }));
                    }
                }
            }
            EditorGUI.EndProperty();
        }

        struct GenericCache
        {
            public string Error;
            public Func<string, bool> ValidateKind;
            public Func<IEnumerable<string>> GetKinds;
        }

        static Dictionary<Type, GenericCache> _genericCache = new Dictionary<Type, GenericCache>();

        private GenericCache GCache
        {
            get => GetGenericCache(this.fieldInfo.FieldType);
        }

        static GenericCache GetGenericCache(Type textValueType)
        {
            if (!_genericCache.TryGetValue(textValueType, out var cache))
            {
                try
                {
                    var types = textValueType.GetGenericArguments();
                    var dbType = types[0];
                    var configType = types[1];
                    var configKind = configType.GetField("Kind");
                    var editorDBType = typeof(EditorDB<>).MakeGenericType(dbType);
                    var get_Resolver = editorDBType.GetMethod("get_Resolver", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);

                    var invokeArgs = new object[] { };
                    var resolver = (DBResolver)get_Resolver.Invoke(null, invokeArgs);
                    if (resolver == null)
                    {
                        throw new NullReferenceException($"Database with type ${dbType} not exists");
                    }

                    var index = resolver.GetIndex(configType);
                    if (index == null)
                    {
                        throw new NullReferenceException($"Type Index<{configType.Name}> not found in {dbType.Name}");
                    }

                    cache = new GenericCache
                    {
                        ValidateKind = k =>
                        {
                            return index.TryGet(k, out _);
                        },
                        GetKinds = () =>
                        {
                            return index.All().Cast<object>().Select(o => ((Kind)configKind.GetValue(o)).Value);
                        }
                    };
                } catch (Exception exc)
                {
                    Debug.LogException(exc);
                    cache = new GenericCache
                    {
                        Error = exc.Message
                    };
                }
                _genericCache.Add(textValueType, cache);
            }
            return cache;
        }
    }
}
