using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FDB.Components.Navigation
{
    public sealed class NavigationManager : MonoBehaviour
    {
        public static NavigationManager Instance => _instance.Value;
        static Lazy<NavigationManager> _instance = new Lazy<NavigationManager>(() =>
        {
            var go = new GameObject($"[{nameof(NavigationManager)}]");
            DontDestroyOnLoad(go);
            return go.AddComponent<NavigationManager>();
        });

        readonly List<NavigationGroup> _groups = new List<NavigationGroup>();
        NavigationGroup _active;

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

        public void Perform()
        {
            _active?.Perform();
        }
    }
}
