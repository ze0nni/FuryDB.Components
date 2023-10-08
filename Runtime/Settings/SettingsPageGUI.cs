using System.Linq;
using UnityEngine;

namespace FDB.Components.Settings
{
    public sealed class SettingsPageGUI<TKeysData>
        where TKeysData : ISettingsKeyData
    {
        public readonly SettingsPage<TKeysData> Page;

        private readonly GUIContent[] _groupNames;

        public SettingsPageGUI(SettingsPage<TKeysData> page)
        {
            Page = page;
            _groupNames = Page.Groups.Select(x => new GUIContent(x.Name)).ToArray();
        }

        int _selectedGroup;
        Vector2 _scrollPosition;
        SettingsGroup<TKeysData> SelectedGroup => _selectedGroup < 0 || _selectedGroup >= _groupNames.Length
            ? null
            : Page.Groups[_selectedGroup];

        public void OnGUILayout()
        {
            OnGroupsGUILayout();
            OnGroupKeysGUILayout(SelectedGroup);
            OnPageActionsGUILayout();
        }

        private void OnGroupsGUILayout()
        {
            if (_groupNames.Length == 0)
            {
                return;
            }

            _selectedGroup = Mathf.Clamp(_selectedGroup, 0, _groupNames.Length - 1);
            var newPageIndex = GUILayout.Toolbar(_selectedGroup, _groupNames);
            if (newPageIndex != _selectedGroup)
            {
                _selectedGroup = newPageIndex;
                GUI.changed = true;
            }
        }

        private void OnGroupKeysGUILayout(SettingsGroup<TKeysData> group)
        {
            using (var scrollView = new GUILayout.ScrollViewScope(_scrollPosition, GUILayout.ExpandHeight(true)))
            {
                if (group == null)
                {
                    return;
                }

                foreach (var key in group.Keys)
                {
                    using (new GUILayout.HorizontalScope()) {
                        key.OnGUILayout();
                    }
                }

                _scrollPosition = scrollView.scrollPosition;
            }
        }

        private void OnPageActionsGUILayout()
        {
            using (new GUILayout.HorizontalScope())
            {
                GUI.enabled = Page.IsChanged;
                {
                    if (GUILayout.Button("Apply"))
                    {
                        Page.Apply();
                    }
                    if (GUILayout.Button("Reset"))
                    {
                        Page.Reset();
                    }
                }
                GUI.enabled = true;
                if (GUILayout.Button("Close"))
                {

                }
            }
        }
    }
}
