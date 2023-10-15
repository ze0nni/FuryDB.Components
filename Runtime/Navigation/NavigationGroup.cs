using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FDB.Components.Navigation
{
    [RequireComponent(typeof(RectTransform))]
    public sealed class NavigationGroup : MonoBehaviour
    {
        NavigationItem _selected;

        private void OnEnable()
        {
            _selected = null;
            NavigationManager.Instance.RegisterGroup(this);
        }

        private void OnDisable()
        {
            NavigationManager.Instance.RegisterGroup(this);
        }

        readonly List<NavigationItem> _items = new List<NavigationItem>();

        internal void RegisterItem(NavigationItem item)
        {
            _items.Add(item);
            item.SetSelected(false);
        }

        internal void RemoveItem(NavigationItem item)
        {
            _items.Remove(item);
        }

        internal void Select(NavigationItem item)
        {
            _selected = item;
            foreach (var i in _items)
            {
                i.SetSelected(i == item);
            }
        }

        public void Up()
        {
            Select(Find(new Vector2(0, 1)));
        }

        public void Down()
        {
            Select(Find(new Vector2(0, -1)));
        }

        public void Left()
        {
            Select(Find(new Vector2(-1, 0)));
        }

        public void Right()
        {
            Select(Find(new Vector2(1, 0)));
        }

        public void Perform()
        {
            _selected?.Perform();
        }

        private NavigationItem Find(Vector2 direction)
        {
            var groupTransform = GetComponent<RectTransform>();

            Bounds GetBouns(NavigationItem item) {
                return RectTransformUtility
                    .CalculateRelativeRectTransformBounds(groupTransform, item.GetComponent<RectTransform>());
            }

            Bounds selectedRect;
            if (_selected != null)
            {
                selectedRect = GetBouns(_selected);
            } else
            {
                if (direction.y < 0)
                {
                    selectedRect = new Bounds(new Vector2(0, int.MaxValue), Vector2.zero);
                }
                else if (direction.y > 0)
                {
                    selectedRect = new Bounds(new Vector2(0, int.MinValue), Vector2.zero);
                }
                else
                {
                    throw new System.NotImplementedException();
                }
            }
            var all = _items
                .Where(i => i != _selected)
                .Select(i => (rect: GetBouns(i), item: i))
                .Select(i => (
                    rect: i.rect,
                    item: i.item,
                    vec: new Vector2(
                        Mathf.Sign(i.rect.center.x - selectedRect.center.x),
                        Mathf.Sign(i.rect.center.y - selectedRect.center.y))));
            var filtered = all
                .Where(i => direction.x == 0 || i.vec.x == direction.x)
                .Where(i => direction.y == 0 || i.vec.y == direction.y);

            var sorted = filtered
                .OrderBy(i => direction.x != 0 ? i.rect.center.x * direction.x : i.rect.center.y * direction.y);
            var next = sorted
                .Select(i => i.item)
                .FirstOrDefault();

            return next ?? _selected;
        }
    }
}