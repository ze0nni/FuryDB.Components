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

    internal delegate void GetGUISeize(out Matrix4x4 matrix, out Vector2 screenSize, out Rect pageSize);

    public interface ISettingsGUIState
    {
        GUIMode Mode { get; }
        float ScreenWidth { get; }
        float ScreenHeight { get; }
        float X { get; }
        float Y { get; }
        float Width { get; }
        float Height { get; }
        float RowHeight { get; }
        void OpenWindow(GuiWindow window);
        void CloseWindow();
        void Repaint();
        event Action<Event> OnGUIEvent;
        event Action OnUpdate;
    }

    internal sealed partial class SettingsGUI<TKeysData> : ISettingsGUIState
        where TKeysData : ISettingsKeyData
    {
        private readonly SettingsPage<TKeysData> _page;
        private readonly GUIContent[] _groupNames;

        readonly GUIMode _mode;
        readonly GetGUISeize _getGuiSize;
        readonly Action _onClose;
        readonly Action _repaint;

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
        float _rowHeight;
        float ISettingsGUIState.RowHeight => _rowHeight;

        public event Action<Event> OnGUIEvent;
        public event Action OnUpdate;

        void ISettingsGUIState.Repaint()
        {
            _repaint?.Invoke();
        }

        public SettingsGUI(
            GUIMode mode,
            SettingsController controler,
            GetGUISeize getGuiSize,
            Action onClose,
            Action repaint)
        {
            _mode = mode;
            _repaint = repaint;
            _onClose = onClose;
            _getGuiSize = getGuiSize;
            _page = controler.NewPage<TKeysData>();
            _groupNames = _page.Groups.Select(x => new GUIContent(x.Name)).ToArray();
        }

        public void Update()
        {
            OnUpdate?.Invoke();
        }

        int _selectedGroup;
        Vector2 _scrollPosition;
        SettingsGroup<TKeysData> SelectedGroup => _selectedGroup < 0 || _selectedGroup >= _groupNames.Length
            ? null
            : _page.Groups[_selectedGroup];

        public void OnGUI()
        {
            OnGUIEvent?.Invoke(Event.current);
            _getGuiSize.Invoke(out var matrix, out var screenSize, out var pageRect);
            GUI.matrix = matrix;
            _screenWidth = screenSize.x;
            _screenHeight = screenSize.y;
            _x = pageRect.x;
            _y = pageRect.y;
            _width = pageRect.width;
            _height = pageRect.height;
            _rowHeight = GUI.skin.button.CalcHeight(GUIContent.none, 8);

            OnWindowBefore();
            using (new GUILayout.AreaScope(pageRect))
            {
                OnGroupsGUILayout();
                OnGroupKeysGUILayout(this, SelectedGroup, _width);
                OnPageActionsGUILayout();
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
                    if (key is HeaderKey<TKeysData> headerkey)
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
                if (_onClose != null && GUILayout.Button("Close"))
                {
                    _onClose.Invoke();
                }
            }
            GUI.enabled = enabledRev;
        }
    }
}
