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
        [SerializeField] Button[] _buttonPerformers;

        [SerializeField] public ItemSelectedChangedEvent OnSelectedChanged;
        [SerializeField] public ItemPerformEvent OnPerform;

        [Serializable]
        public class ItemSelectedChangedEvent : UnityEvent
        {
        }

        [Serializable]
        public class ItemPerformEvent : UnityEvent
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

        internal void Perform()
        {
            foreach (var b in _buttonPerformers)
            {
                b.onClick?.Invoke();
            }
            OnPerform?.Invoke();
        }
    }
}