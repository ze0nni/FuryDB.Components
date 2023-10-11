using System;
using UnityEngine;

namespace FDB.Components.Settings
{
    public static class GuiWindowHeper
    {
        public static void ShowDropdownWindow(
            this ISettingsGUIState state,
            Rect fieldRect,
            Action wndFunc,
            bool sticky = false,
            float maxHeight = 0,
            Action wndBottomFunc = null,
            Action onClose = null)
        {
            var wndWidth = fieldRect.width;
            var scrollContentHeight = 0f;
            var scrollViewVPadding = //Not sure this is right
                GUI.skin.box.padding.vertical;
            var bottomHeight = 0f;

            float GetHeight() => scrollContentHeight + scrollViewVPadding + bottomHeight;

            var scrollPos = new Vector2();

            state.OpenWindow(fieldRect, new GuiWindow
            {
                Sticky = sticky,
                WndFunc = () =>
                {
                    GUI.Box(new Rect(0, 0, wndWidth, GetHeight()), GUIContent.none);

                    using (var scrollView = new GUILayout.ScrollViewScope(scrollPos))
                    {
                        wndFunc();
                        scrollPos = scrollView.scrollPosition;

                        if (Event.current.type == EventType.Repaint)
                        {
                            var lastRect = GUILayoutUtility.GetLastRect();
                            var newHeight = lastRect.yMax;
                            GUI.changed = newHeight != scrollContentHeight;
                            scrollContentHeight = newHeight;
                        }
                    }

                    if (wndBottomFunc != null)
                    {
                        using (new GUILayout.HorizontalScope())
                        {
                            wndBottomFunc();
                        }
                        if (Event.current.type == EventType.Repaint)
                        {
                            var lastRect = GUILayoutUtility.GetLastRect();
                            var newHeight = lastRect.height;
                            GUI.changed = newHeight != bottomHeight;
                            bottomHeight = newHeight;
                        }
                    }
                },
                OnClose = onClose,
                GetSize = () => new Vector2(
                    wndWidth,
                    maxHeight > 0
                    ? Mathf.Min(maxHeight, GetHeight())
                    : GetHeight())
            });
        }

        public static void OpenWindow(
            this ISettingsGUIState state,
            string title,
            float width,
            float height,
            bool sticky,
            Action wndFunc,
            Action onClose = null)
        {
            state.OpenWindow(
                null,
                new GuiWindow
                {
                    Sticky = sticky,
                    WndFunc = () =>
                    {
                        GUI.Box(new Rect(0, 0, width, height), GUIContent.none, GUI.skin.window);
                        GUILayout.Label(title, GUI.skin.box, GUILayout.ExpandWidth(true));
                        wndFunc();
                    },
                    OnClose = onClose,
                    GetSize = () => new Vector2(width, height)
                });
        }
    }
}