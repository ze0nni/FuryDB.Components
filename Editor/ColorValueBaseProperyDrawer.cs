using FDB.Editor;
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
                    var type = this.fieldInfo.FieldType;
                    var colorResolverType = type.BaseType.GetGenericArguments()[2];
                    var colorResolver = System.Activator.CreateInstance(colorResolverType);

                    GUI.color = Color.yellow;
                    GUI.DrawTexture(colorsRect, FDBEditorIcons.Solid);
                    GUI.color = Color.white;
                }

                if (GUI.Button(browseRect, new GUIContent(null, FDBEditorIcons.LinkIcon)))
                {
                    
                }
            }
        }
    }
}