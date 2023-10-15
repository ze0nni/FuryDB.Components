using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace FDB.Components.Navigation
{
    [RequireComponent(typeof(RectTransform))]
    public sealed class NavigationGroup : MonoBehaviour
    {
        public GroupCancelEvent OnCancel;
        [SerializeField] Button _cancelButton;

        [Serializable]
        public class GroupCancelEvent : UnityEvent
        {
        }

        NavigationItem _selected;

        private void OnEnable()
        {
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
            item.SetSelected(_selected == item);
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

        public void Success()
        {
            _selected?.Success();
        }

        public void Cancel()
        {
            _cancelButton?.onClick?.Invoke();
            OnCancel?.Invoke();
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
                else if (direction.x > 0)
                {
                    selectedRect = new Bounds(new Vector2(int.MinValue, 0), Vector2.zero);
                } else if (direction.x < 0)
                {
                    selectedRect = new Bounds(new Vector2(int.MaxValue, 0), Vector2.zero);
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
                        i.rect.center.x - selectedRect.center.x,
                        i.rect.center.y - selectedRect.center.y)))
                .Select(i => (
                    i.rect,
                    i.item,
                    vec: Mathf.Abs(i.vec.x) > Mathf.Abs(i.vec.y)
                        ? new Vector2(Mathf.Sign(i.vec.x), 0)
                        : new Vector2(0, Mathf.Sign(i.vec.y))));

            var filtered = all
                .Where(i => direction.x == 0 || i.vec.x == direction.x)
                .Where(i => direction.y == 0 || i.vec.y == direction.y);

            var sorted = filtered
                .OrderBy(i => Vector2.Distance(i.rect.center, selectedRect.center));

            var next = sorted
                .Select(i => i.item)
                .FirstOrDefault();

            return next ?? _selected;
        }
    }
}