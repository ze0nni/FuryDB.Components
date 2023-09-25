using FDB.Editor;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace FDB.Components.Editor
{
    public class ColorKindChooseWindow : BaseChoseWindow
    {
        readonly Func<string, Color[]> _getColors;

        public ColorKindChooseWindow(
            string value,
            IEnumerable<string> kinds,
            Func<string, Color[]> getColors,
            float width,
            Action<string> callback)
            : base(value, kinds, width, callback)
        {
            _getColors = getColors;
        }

        public override void OnOpen()
        {
            base.OnOpen();

            ListView.makeItem = () =>
            {
                return new ColorElementItem(Width, ListView.itemHeight);
            };
            ListView.bindItem = (e, i) =>
            {
                var item = (ColorElementItem)e;
                item.Bind(Kinds[i], _getColors(Kinds[i]));
            };
            ListView.unbindItem = (e, i) =>
            {
                var item = (ColorElementItem)e;
            };
        }

        private class ColorElementItem : VisualElement
        {
            readonly Label _label;
            readonly Box _colors = new Box();
            readonly List<Box> _images = new List<Box>();
            readonly float _width;
            readonly float _height;

            public ColorElementItem(float width, float height)
            {
                _width = width;
                _height = height;

                _label = new Label();
                _label.style.position = new StyleEnum<Position>(Position.Absolute);
                _label.style.left = 0;
                _label.style.top = 0;
                _label.style.right = width / 2;
                _label.style.bottom = 0;
                _label.style.height = height;
                Add(_label);

                _colors = new Box();
                _colors.style.position = new StyleEnum<Position>(Position.Absolute);
                _colors.style.left = width / 2;
                _colors.style.top = 0;
                _colors.style.bottom = 0;
                _colors.style.width = width / 2;
                Add(_colors);
            }

            public void Bind(string kind, Color[] colors)
            {
                _label.text = kind;

                while (_images.Count > colors.Length)
                {
                    _colors.Remove(_images[0]);
                    _images.RemoveAt(0);
                }
                while (_images.Count < colors.Length)
                {
                    var image = new Box();
                    _colors.Add(image);
                    _images.Add(image);
                }

                for (var i = 0; i < colors.Length; i++)
                {
                    var image = _images[i];
                    image.style.backgroundColor = colors[i];

                    var imageWidth = (_width / 2) / colors.Length;

                    image.style.position = new StyleEnum<Position>(Position.Absolute);
                    image.style.left = imageWidth * i;
                    image.style.top = 0;
                    image.style.bottom = 0;
                    image.style.width = imageWidth;
                    image.style.height = _height;
                }
            }
        }
    }
}