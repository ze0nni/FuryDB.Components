using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FDB.Components.Navigation
{
    [DefaultExecutionOrder(-90)]
    public sealed class NavigationManager : MonoBehaviour
    {
        public static NavigationManager Instance => _instance.Value;
        static Lazy<NavigationManager> _instance = new Lazy<NavigationManager>(() =>
        {
            var go = new GameObject($"[{nameof(NavigationManager)}]");
            DontDestroyOnLoad(go);
            return go.AddComponent<NavigationManager>();
        });

        internal readonly List<NavigationGroup> _groups = new List<NavigationGroup>();
        internal NavigationGroup _active;
        public NavigationGroup ActiveGroup => _active;

        public event Action<NavigationGroup, NavigationGroup> OnActiveGroupChanged;
        public event Action<NavigationGroup, NavigationItem, NavigationItem> OnSelectedItemChanged;

        internal void RegisterGroup(NavigationGroup group)
        {
            _groups.Add(group);
            UpdateActiveGroup(group);
        }

        internal void RemoveGroup(NavigationGroup item)
        {
            _groups.Remove(item);
            UpdateActiveGroup(_groups.LastOrDefault());
        }

        void UpdateActiveGroup(NavigationGroup newGroup)
        {
            var oldGroup = _active;

            if (oldGroup != null)
            {
                oldGroup.OnSelectedItemChanged -= OnSelectedItemChangedHandler;
            }
            if (newGroup != null)
            {
                newGroup.OnSelectedItemChanged += OnSelectedItemChangedHandler;
            }

            _active = newGroup;
            OnActiveGroupChanged?.Invoke(newGroup, oldGroup);
        }

        void OnSelectedItemChangedHandler(NavigationItem item, NavigationItem oldItem)
        {
            OnSelectedItemChanged?.Invoke(_active, item, oldItem);
        }

        private void Update()
        {
            if (Input.GetMouseButton(0) || Input.GetMouseButton(1))
            {
                ClearSelection();
            }

#if FURY_PERSISTENT_EXISTS
            UpdateBindings();
#endif
        }

        public void ClearSelection()
        {
            foreach (var g in _groups)
            {
                g.Select(null);
            }
        }

#if FURY_PERSISTENT_EXISTS
        public delegate ref Fury.Settings.BindingButton BindingButtonDelegate();
        BindingButtonDelegate _upBinding;
        BindingButtonDelegate _downBinding;
        BindingButtonDelegate _leftBinding;
        BindingButtonDelegate _rightBinding;
        BindingButtonDelegate _successBinding;
        BindingButtonDelegate _cancelBinding;

        public void Bind(
            BindingButtonDelegate up = null,
            BindingButtonDelegate down = null,
            BindingButtonDelegate left = null,
            BindingButtonDelegate right = null,
            BindingButtonDelegate success = null,
            BindingButtonDelegate cancel = null
            )
        {
            _upBinding = up;
            _downBinding = down;
            _leftBinding = left;
            _rightBinding = right;
            _successBinding = success;
            _cancelBinding = cancel;
        }

        public void UpdateBindings()
        {
            if (_active != null)
            {
                if (_upBinding != null && _upBinding().CaptureJustPressed())
                {
                    Up();
                }
                if (_downBinding != null && _downBinding().CaptureJustPressed())
                {
                    Down();
                }
                if (_leftBinding != null && _leftBinding().CaptureJustPressed())
                {
                    Left();
                }
                if (_rightBinding != null && _rightBinding().CaptureJustPressed())
                {
                    Right();
                }
                if (_successBinding != null && _successBinding().CaptureJustPressed())
                {
                    Success();
                }
                if (_cancelBinding != null && _cancelBinding().CaptureJustPressed())
                {
                    Cancel();
                }
            }
        }
#endif

        public bool Up()
        {
            return _active?.Up() ?? false;
        }

        public bool Down()
        {
            return _active?.Down() ?? false;
        }

        public bool Left()
        {
            return _active?.Left() ?? false;
        }

        public bool  Right()
        {
            return _active?.Right() ?? false;
        }

        public bool Success()
        {
            return _active?.Success() ?? false;
        }

        public bool Cancel()
        {
            return _active?.Cancel() ?? false;
        }
    }
}
