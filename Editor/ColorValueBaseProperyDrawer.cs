using FDB.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace FDB.Components.Editor
{
    [CustomPropertyDrawer(typeof(ColorValueBase<,,>), true)]
    internal class ColorValueBaseProperyDrawer : PropertyDrawer
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

            var rawProp = property.FindPropertyRelative("_raw");
            var rawValueProp = property.FindPropertyRelative("_rawValue");
            var valueProp = property.FindPropertyRelative("_value");

            EditorGUI.BeginProperty(position, label, property);
            {
                position = EditorGUI.PrefixLabel(position, label);

                var rawToggleRect = new Rect(position.x, position.y, position.width, position.height / 2);
                {
                    EditorGUI.BeginChangeCheck();
                    var newTranslate = GUI.Toggle(rawToggleRect, rawProp.boolValue, "Raw color");
                    if (EditorGUI.EndChangeCheck())
                    {
                        rawProp.boolValue = newTranslate;
                    }
                }
            }

            var valueRect = new Rect(position.x, position.y + position.height / 2, position.width, position.height / 2);
            if (rawProp.boolValue)
            {
                EditorGUI.BeginChangeCheck();
                var newValue = EditorGUI.ColorField(valueRect, rawValueProp.colorValue);
                if (EditorGUI.EndChangeCheck())
                {
                    rawValueProp.colorValue = newValue;
                }
            } else
            {
                valueRect.width -= valueRect.height * 2;
                var browseRect = new Rect(valueRect.xMax, valueRect.y, valueRect.height * 2, valueRect.height);

                var labelRect = new Rect(valueRect.x, valueRect.y, valueRect.width / 2, valueRect.height);
                var colorsRect = new Rect(valueRect.x + valueRect.width / 2, valueRect.y, valueRect.width / 2, valueRect.height);

                GUI.Label(labelRect, valueProp.stringValue);
                {
                    var colors = GCache.GetColors(valueProp.stringValue);

                    if (colors != null && colors.Length > 0) {
                        var colorRect = new Rect(colorsRect.x, colorsRect.y, colorsRect.width / colors.Length, colorsRect.height);
                        foreach (var color in colors)
                        {
                            GUI.color = color;
                            GUI.DrawTexture(colorRect, FDBEditorIcons.Solid);
                            colorRect.x += colorRect.width;
                        }
                        GUI.color = Color.white;
                    }
                }

                if (GUI.Button(browseRect, new GUIContent(null, FDBEditorIcons.LinkIcon)))
                {
                    var kinds = GCache.GetKinds();
                    PopupWindow.Show(valueRect, new ColorKindChooseWindow(
                        valueProp.stringValue,
                        kinds,
                        GCache.GetColors,
                        position.width,
                        newKind =>
                    {
                        valueProp.serializedObject.Update();
                        valueProp.stringValue = newKind;
                        valueProp.serializedObject.ApplyModifiedProperties();
                        GUI.changed = true;
                    }));
                }
            }
        }

        struct GenericCache
        {
            public string Error;
            public Func<string, Index> GetIndex;
            public Func<IEnumerable<string>> GetKinds;
            public Func<string, Color[]> GetColors;
        }

        static Dictionary<Type, GenericCache> _genericCache = new Dictionary<Type, GenericCache>();

        private GenericCache GCache
        {
            get => GetGenericCache(this.fieldInfo.FieldType);
        }

        static GenericCache GetGenericCache(Type colorValueType)
        {
            if (!_genericCache.TryGetValue(colorValueType, out var cache))
            {
                try
                {
                    var types = Utils.GetGenericTypeImplementationOf(colorValueType, typeof(ColorValueBase<,,>));
                    var dbType = types[0];
                    var configType = types[1];
                    var configKind = configType.GetField("Kind");
                    var colorResolverType = types[2];

                    var editorDBType = typeof(EditorDB<>).MakeGenericType(dbType);
                    var get_Resolver = editorDBType.GetMethod("get_Resolver", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);

                    var colorResolver = Activator.CreateInstance(colorResolverType);
                    var getColors = colorResolverType.GetMethod("GetColors");

                    var resolver = (DBResolver)get_Resolver.Invoke(null, new object[] { });
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
                        GetIndex = k =>
                        {
                            return index;
                        },
                        GetKinds = () =>
                        {
                            if (index == null)
                            {
                                return null;
                            }
                            return index.Cast<object>().Select(o => ((Kind)configKind.GetValue(o)).Value);
                        },
                        GetColors = kind =>
                        {
                            var config = resolver.GetConfig(configType, kind);
                            if (config == null)
                            {
                                return null;
                            }
                            return (Color[])getColors.Invoke(colorResolver, new[] { config });
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
                _genericCache.Add(colorValueType, cache);
            }
            return cache;
        }
    }
}