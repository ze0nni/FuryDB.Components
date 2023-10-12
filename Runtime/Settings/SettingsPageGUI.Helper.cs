using System;
using UnityEngine;

namespace FDB.Components.Settings
{
    public static class GuiWindowHeper
    {
        public static void ShowDropdownWindow(
            this ISettingsGUIState state,
            Rect fieldRectRaw,
            Action wndFunc,
            bool sticky = false,
            float maxHeight = 0,
            Action wndBottomFunc = null,
            Action onClose = null)
        {
            var fieldScreenRect = GUIUtility.GUIToScreenRect(fieldRectRaw);

            var scrollContentHeight = 0f;
            var scrollViewVPadding = //Not sure this is right
                GUI.skin.box.padding.vertical;
            var bottomHeight = 0f;

            float GetContentHeight() => scrollContentHeight + scrollViewVPadding + bottomHeight;

            var scrollPos = new Vector2();

            state.OpenWindow(new GuiWindow
            {
                Sticky = sticky,
                WndFunc = (size) =>
                {
                    GUI.Box(new Rect(0, 0, size.x, size.y), GUIContent.none);

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
                GetSize = () =>
                {
                    var fieldRect = GUIUtility.ScreenToGUIRect(fieldScreenRect);
                    var w = fieldRect.width;
                    var h = maxHeight == 0 ? GetContentHeight() : MathF.Min(maxHeight, GetContentHeight());
                    var rect = new Rect(fieldRect.x, fieldRect.yMax, w, h);
                    if (rect.yMax <= state.ScreenHeight)
                    {
                        return rect;
                    }
                    rect.y = fieldRect.y - h;
                    return rect;
                }
            });
        }

        public static void OpenWindow(
            this ISettingsGUIState state,
            string title,
            float requestWidth,
            float requestHeight,
            bool sticky,
            Action wndFunc,
            Action onClose = null)
        {
            state.OpenWindow(
                new GuiWindow
                {
                    Sticky = sticky,
                    WndFunc = (size) =>
                    {
                        GUI.Box(new Rect(0, 0, size.x, size.y), GUIContent.none, GUI.skin.window);
                        GUILayout.Label(title, GUI.skin.box, GUILayout.ExpandWidth(true));
                        wndFunc();
                    },
                    OnClose = onClose,
                    GetSize = () => new Rect(
                        (state.ScreenWidth - requestWidth) / 2,
                        (state.ScreenHeight - requestHeight) / 2,
                        requestWidth, 
                        requestHeight + state.RowHeight)
                });
        }
    }
}