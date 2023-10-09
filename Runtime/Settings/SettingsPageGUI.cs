using System;
using System.Linq;
using UnityEngine;

namespace FDB.Components.Settings
{
    public sealed class SettingsPageGUI<TKeysData>
        where TKeysData : ISettingsKeyData
    {
        private readonly SettingsPage<TKeysData> _page;
        private readonly GUIContent[] _groupNames;

        public event Action OnCloseClick;

        public SettingsPageGUI(
            SettingsController controler)
        {
            _page = controler.CreatePage<TKeysData>();
            _groupNames = _page.Groups.Select(x => new GUIContent(x.Name)).ToArray();
        }

        int _selectedGroup;
        Vector2 _scrollPosition;
        SettingsGroup<TKeysData> SelectedGroup => _selectedGroup < 0 || _selectedGroup >= _groupNames.Length
            ? null
            : _page.Groups[_selectedGroup];

        public void OnScreenGUI()
        {
            var width = Screen.width;
            var height = Screen.height;

            GUI.Box(new Rect(0, 0, width, height), "");

            using (new GUILayout.AreaScope(
                new Rect(
                    width / 4,
                    height / 8,
                    width / 2,
                    height - (height / 4))))
            {
                OnGroupsGUILayout();
                OnGroupKeysGUILayout(SelectedGroup, width / 2);
                OnPageActionsGUILayout();
            }
        }

        public void OnInspectorGUILayout()
        {
            OnGroupsGUILayout();
            OnGroupKeysGUILayout(SelectedGroup, Screen.width);
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

        private void OnGroupKeysGUILayout(SettingsGroup<TKeysData> group, float width)
        {
            using (var scrollView = new GUILayout.ScrollViewScope(_scrollPosition, GUILayout.ExpandHeight(true)))
            {
                if (group == null)
                {
                    return;
                }

                foreach (var key in group.Keys)
                {
                    if (!key.Visible)
                    {
                        continue;
                    }
                    GUI.enabled = key.Enabled;
                    if (key is SettingsKey.Header<TKeysData> headerkey)
                    {
                        GUILayout.Box(headerkey.Data.Name, GUILayout.ExpandWidth(true));
                    }
                    else
                    {
                        using (new GUILayout.HorizontalScope())
                        {
                            key.OnGUILayout(width);
                        }
                    }
                }
                GUI.enabled = true;
                _scrollPosition = scrollView.scrollPosition;
            }
        }

        private void OnPageActionsGUILayout()
        {
            using (new GUILayout.HorizontalScope())
            {
                GUI.enabled = _page.IsChanged;
                {
                    if (GUILayout.Button("Apply"))
                    {
                        _page.Apply(true);
                    }
                    if (GUILayout.Button("Reset"))
                    {
                        _page.Reset();
                    }
                }
                GUI.enabled = true;
                if (GUILayout.Button("Default"))
                {
                    _page.ResetDefault();
                }
                if (GUILayout.Button("Close"))
                {
                    OnCloseClick?.Invoke();
                }
            }
        }
    }
}
