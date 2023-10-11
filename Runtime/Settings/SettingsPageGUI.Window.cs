using System;
using UnityEngine;

namespace FDB.Components.Settings
{
    public struct GuiWindow
    {
        internal Rect FieldScreenRect;

        public bool Sticky;
        public Action WndFunc;
        public Action OnClose;
        public Func<Vector2> GetSize;
    }

    public sealed partial class SettingsPageGUI<TKeysData>
    {
        GuiWindow? _window;

        void ISettingsGUIState.OpenWindow(
            Rect? fieldRect,
            GuiWindow window)
        {
            if (fieldRect != null)
            {
                window.FieldScreenRect = GUIUtility.GUIToScreenRect(fieldRect.Value);
            }
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

            var popupRect = CalcPopupRect(fieldRect, wnd.GetSize());

            using (new GUILayout.AreaScope(popupRect, GUIContent.none, GUIStyle.none))
            {
                wnd.WndFunc();
            }

            var e = Event.current;
            if (!wnd.Sticky && e.type == EventType.MouseDown && !popupRect.Contains(e.mousePosition))
            {
                wnd.OnClose?.Invoke();
                _window = null;
                GUI.changed = true;
            }
        }

        Rect CalcPopupRect(Rect field, Vector2 size)
        {
            var screen = new Rect(0, 0, _screenWidth, _screenHeight);

                var wnd = new Rect(0, 0, size.x, size.y);
                if (wnd.height > _screenHeight)
                {
                    wnd.height = _screenHeight;
                }
            if (wnd.x > _screenWidth)
            {
                wnd.x = _screenWidth;
            }
            else
            {
                if (field.size != default)
                {
                    if (field.width >= wnd.width)
                    {
                        wnd.x = field.x;
                    }
                    else
                    {
                        wnd.x = field.xMax - wnd.width;
                    }

                    wnd.y = field.yMax;
                    if (Inside(screen, wnd))
                    {
                        return wnd;
                    }
                    wnd.y = field.y - wnd.height;
                    if (Inside(screen, wnd))
                    {
                        return wnd;
                    }
                    wnd.x = _screenWidth - wnd.width;
                    if (Inside(screen, wnd))
                    {
                        return wnd;
                    }
                }
            }

            wnd.x = (_screenWidth - wnd.width) / 2;
            wnd.y = (_screenHeight - wnd.height) / 2;

            return wnd;
        }

        public static bool Inside(Rect screen, Rect wnd) {
            return wnd.x >= screen.x
                && wnd.y >= screen.y
                && wnd.xMax <= screen.xMax
                && wnd.yMax <= screen.yMax;
        }

    }
}