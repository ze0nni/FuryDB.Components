#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace FDB.Components.Navigation.Editor
{
    [CustomEditor(typeof(NavigationManager))]
    public class NavigationManagerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var manager = (NavigationManager)target;

            GUILayout.Label($"Groups count: {manager._groups.Count}");

            GUI.enabled = false;
            using (new GUILayout.HorizontalScope())
            {
                EditorGUILayout.PrefixLabel("Active group");
                EditorGUILayout.ObjectField(manager._active, typeof(NavigationGroup));
            }
            using (new GUILayout.HorizontalScope())
            {
                EditorGUILayout.PrefixLabel("Active item");
                EditorGUILayout.ObjectField(manager._active?._selected, typeof(NavigationItem));
            }
        }
    }
}
#endif