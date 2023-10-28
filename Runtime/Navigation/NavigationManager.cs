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

        internal void RegisterGroup(NavigationGroup group)
        {
            _groups.Add(group);
            _active = group;
        }

        internal void RemoveGroup(NavigationGroup item)
        {
            _groups.Remove(item);
            _active = _groups.LastOrDefault();
        }

        private void Update()
        {
            if (Input.GetMouseButton(0) || Input.GetMouseButton(1))
            {
                ClearSelection();
            }
        }

        public void ClearSelection()
        {
            foreach (var g in _groups)
            {
                g.Select(null);
            }
        }

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
