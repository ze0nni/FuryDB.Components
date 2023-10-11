using System;
using System.Linq;
using UnityEngine;

namespace FDB.Components.Settings
{
    public enum GUIMode
    {
        Editor,
        Screen
    }

    public interface ISettingsGUIState
    {
        GUIMode Mode { get; }
        float ScreenWidth { get; }
        float ScreenHeight { get; }
        float X { get; }
        float Y { get; }
        float Width { get; }
        float Height { get; }
        void OpenWindow(Rect? fieldRect, GuiWindow window);
        void CloseWindow();
    }

    public sealed partial class SettingsPageGUI<TKeysData> : ISettingsGUIState
        where TKeysData : ISettingsKeyData
    {
        private readonly SettingsPage<TKeysData> _page;
        private readonly GUIContent[] _groupNames;

        readonly GUIMode _mode;
        GUIMode ISettingsGUIState.Mode => _mode;
        float _screenWidth;
        float ISettingsGUIState.ScreenWidth => _screenWidth;
        float _screenHeight;
        float ISettingsGUIState.ScreenHeight => _screenHeight;
        float _x;
        float ISettingsGUIState.X => _x;
        float _y;
        float ISettingsGUIState.Y => _y;
        float _width;
        float ISettingsGUIState.Width => _width;
        float _height;
        float ISettingsGUIState.Height => _height;

        public event Action OnCloseClick;

        public SettingsPageGUI(
            GUIMode mode,
            SettingsController controler)
        {
            _mode = mode;
            _page = controler.NewPage<TKeysData>();
            _groupNames = _page.Groups.Select(x => new GUIContent(x.Name)).ToArray();
        }

        int _selectedGroup;
        Vector2 _scrollPosition;
        SettingsGroup<TKeysData> SelectedGroup => _selectedGroup < 0 || _selectedGroup >= _groupNames.Length
            ? null
            : _page.Groups[_selectedGroup];

        public void OnGUI()
        {
            switch (_mode)
            {
                case GUIMode.Editor:
                    {
                        _screenWidth = Screen.width;
                        _screenHeight = Screen.height;
                        _x = 0;
                        _y = 0;
                        _width = Screen.width;
                        _height = Screen.height;
                    }
                    break;
                case GUIMode.Screen:
                    {
                        _screenWidth = Screen.width;
                        _screenHeight = Screen.height;
                        _x = Screen.width / 4;
                        _y = Screen.height / 8;
                        _width = Screen.width / 2;
                        _height = Screen.height - (Screen.height / 4);
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(_mode));
            }

            OnWindowBefore();
            if (_mode == GUIMode.Screen)
            {
                GUILayout.BeginArea(new Rect(
                        _x,
                        _y,
                        _width,
                        _height));
            }

            OnGroupsGUILayout();
            OnGroupKeysGUILayout(this, SelectedGroup, _width);
            OnPageActionsGUILayout();

            if (_mode == GUIMode.Screen)
            {
                GUILayout.EndArea();
            }
                OnWindowAfter();
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

        private void OnGroupKeysGUILayout(
            ISettingsGUIState state,
            SettingsGroup<TKeysData> group,
            float width)
        {
            var enabledRev = GUI.enabled;

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
                    if (key is SettingsKey.Header<TKeysData> headerkey)
                    {
                        GUI.enabled = enabledRev;
                        GUILayout.Box(headerkey.Data.Name, GUILayout.ExpandWidth(true));
                    }
                    else
                    {
                        GUI.enabled = enabledRev && key.Enabled;
                        using (new GUILayout.HorizontalScope())
                        {
                            key.OnGUI(state, width);
                        }
                    }
                }
                _scrollPosition = scrollView.scrollPosition;
            }

            GUI.enabled = enabledRev;
        }

        private void OnPageActionsGUILayout()
        {
            var enabledRev = GUI.enabled;
            using (new GUILayout.HorizontalScope())
            {
                GUI.enabled = enabledRev && _page.IsChanged;
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
                GUI.enabled = enabledRev;
                if (GUILayout.Button("Default"))
                {
                    _page.ResetDefault();
                }
                if (GUILayout.Button("Close"))
                {
                    OnCloseClick?.Invoke();
                }
            }
            GUI.enabled = enabledRev;
        }
    }
}
