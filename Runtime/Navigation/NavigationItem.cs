using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace FDB.Components.Navigation
{
    [RequireComponent(typeof(RectTransform))]
    public sealed class NavigationItem : MonoBehaviour
    {
        [SerializeField] GameObject[] _activateOnSelect;
        [SerializeField] Button _button;

        [SerializeField] public ItemSelectedChangedEvent OnSelectedChanged;
        [SerializeField] public ItemSuccessEvent OnSuccess;

        [Serializable]
        public class ItemSelectedChangedEvent : UnityEvent
        {
        }

        [Serializable]
        public class ItemSuccessEvent : UnityEvent
        {
        }

        bool _selected;
        public bool Selected => _selected;

        NavigationGroup _group;

        private void OnEnable()
        {
            _group = null;
            var go = gameObject;
            while (go != null && _group == null)
            {
                _group = go.GetComponent<NavigationGroup>();
                go = go.transform.parent?.gameObject;
            }

            if (_group == null)
            {
                Debug.LogWarning($"{nameof(NavigationItem)} component must be child of {nameof(NavigationGroup)}");
                return;
            }
            _group.RegisterItem(this);
        }

        private void OnDisable()
        {
            _group?.RemoveItem(this);
            _group = null;
        }

        internal void SetSelected(bool value)
        {
            _selected = value;
            foreach (var go in _activateOnSelect)
            {
                go.SetActive(value);
            }
            OnSelectedChanged?.Invoke();
        }

        internal void Success()
        {
            _button?.onClick?.Invoke();
            OnSuccess?.Invoke();
        }
    }
}