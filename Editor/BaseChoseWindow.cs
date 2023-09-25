using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace FDB.Components.Editor
{
    public abstract class BaseChoseWindow : PopupWindowContent
    {
        string _value;
        readonly string[] _kinds;
        public IReadOnlyList<string> Kinds => _kinds;
        readonly float _width;
        public float Width => _width;
        readonly Action<string> _callback;

        int _ticksForUpdate = 1;
        string _searchText;
        TextField _searchField;
        ListView _listView;
        protected ListView ListView => _listView;

        public BaseChoseWindow(
            string value,
            IEnumerable<string> kinds,
            float width,
            Action<string> callback)
        {
            _value = value;
            _kinds = kinds.ToArray();
            _width = width;
            _callback = callback;
        }

        public override Vector2 GetWindowSize()
        {
            var size = base.GetWindowSize();
            size.x = _width;
            return size;
        }

        public override void OnOpen()
        {
            _searchField = new TextField();
            _searchField.RegisterValueChangedCallback(e =>
            {
                _listView.selectedIndex = 0;
                _listView.ScrollToItem(0);
                Filter(e.newValue);
            });
            editorWindow.rootVisualElement.Add(_searchField); ;

            _listView = new ListView();
            Filter("");
            _listView.selectedIndex = Array.IndexOf(_kinds.ToArray(), _value);
            _listView.RegisterCallback<MouseDownEvent>(e =>
            {
                if (e.button == 0 && e.clickCount == 2)
                {
                    var kind = SelectedKind;
                    if (kind != null)
                    {
                        _callback(kind);
                        editorWindow.Close();
                    }
                }
            });
            editorWindow.rootVisualElement.Add(_listView);

            editorWindow.rootVisualElement.RegisterCallback<KeyDownEvent>(e =>
            {
                switch (e.keyCode)
                {
                    case KeyCode.UpArrow:
                        SelectIndex(_listView.selectedIndex - 1);
                        break;
                    case KeyCode.DownArrow:
                        SelectIndex(_listView.selectedIndex + 1);
                        break;
                    case KeyCode.Return:
                        var kind = SelectedKind;
                        if (kind != null)
                            _callback(kind);
                        editorWindow.Close();
                        break;
                    case KeyCode.Escape:
                        Filter("");
                        _ticksForUpdate = 2;
                        break;
                }
            }, TrickleDown.TrickleDown);
        }

        public override void OnClose()
        {
            editorWindow.rootVisualElement.Clear();
            base.OnClose();
        }

        public string SelectedKind
        {
            get
            {
                var index = _listView.selectedIndex;
                if (index == -1)
                    return null;
                return (string)_listView.itemsSource[index];
            }
        }

        void Filter(string text)
        {
            _listView.itemsSource = _kinds
                .Where(kind => text == null || text.Length == 0 || kind.Contains(text))
                .ToArray();
            _listView.Rebuild();
        }

        void SelectIndex(int index)
        {
            index = Mathf.Clamp(index, 0, _listView.itemsSource.Count - 1);
            _listView.selectedIndex = index;
            _listView.ScrollToItem(index);
        }

        public override void OnGUI(Rect rect)
        {
            if (_ticksForUpdate == 0)
                return;
            if (--_ticksForUpdate != 0)
                return;

            _searchField.Focus();
            _listView.ScrollToItem(_listView.selectedIndex);
        }
    }
}