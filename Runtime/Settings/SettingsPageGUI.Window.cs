using System;
using UnityEngine;

namespace FDB.Components.Settings
{
    public struct GuiWindow
    {
        internal Rect FieldScreenRect;

        public bool Sticky;
        public Action<Vector2> WndFunc;
        public Action OnClose;
        public Func<Rect> GetSize;
    }

    public sealed partial class SettingsPageGUI<TKeysData>
    {
        GuiWindow? _window;

        void ISettingsGUIState.OpenWindow(GuiWindow window)
        {
            _window = window;
        }

        void ISettingsGUIState.CloseWindow()
        {
            _window.Value.OnClose?.Invoke();
            _window = null;
            GUI.changed = true;
        }

        void OnWindowBefore()
        {
            if (_window == null)
            {
                return;
            }
            GUI.enabled = false;
        }

        void OnWindowAfter()
        {
            GUI.enabled = true;

            if (_window == null)
            {
                return;
            }
            var wnd = _window.Value;
            var fieldRect = GUIUtility.ScreenToGUIRect(wnd.FieldScreenRect);

            var windowRect = FitToScreen(wnd.GetSize());

            using (new GUILayout.AreaScope(windowRect, GUIContent.none, GUIStyle.none))
            {
                wnd.WndFunc(windowRect.size);
            }

            var e = Event.current;
            if (!wnd.Sticky && e.type == EventType.MouseDown && !windowRect.Contains(e.mousePosition))
            {
                wnd.OnClose?.Invoke();
                _window = null;
                GUI.changed = true;
            }
        }

        Rect FitToScreen(Rect wnd)
        {
            if (wnd.width > _screenWidth) wnd.width = _screenWidth;
            if (wnd.height > _screenHeight) wnd.height = _screenHeight;
            if (wnd.x < 0) wnd.x = 0;
            if (wnd.y < 0) wnd.y = 0;
            if (wnd.xMax > _screenWidth) wnd.x = _screenWidth - wnd.width;
            if (wnd.yMax > _screenHeight) wnd.x = _screenHeight - wnd.height;
            return wnd;
        }

    }
}