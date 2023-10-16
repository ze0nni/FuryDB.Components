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

        public void Up()
        {
            _active?.Up();
        }

        public void Down()
        {
            _active?.Down();
        }

        public void Left()
        {
            _active?.Left();
        }

        public void Right()
        {
            _active?.Right();
        }

        public void Success()
        {
            _active?.Success();
        }

        public void Cancel()
        {
            _active?.Cancel();
        }
    }
}
