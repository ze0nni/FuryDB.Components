using FDB.Components.Settings;
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

        readonly List<NavigationGroup> _groups = new List<NavigationGroup>();
        NavigationGroup _active;

        public delegate ref BindingButton BindingButtonDelegate();
        BindingButtonDelegate _upBinding;
        BindingButtonDelegate _downBinding;
        BindingButtonDelegate _leftBinding;
        BindingButtonDelegate _rightBinding;
        BindingButtonDelegate _successBinding;
        BindingButtonDelegate _cancelBinding;

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

        public void Bind(
            BindingButtonDelegate up,
            BindingButtonDelegate down,
            BindingButtonDelegate left,
            BindingButtonDelegate right,
            BindingButtonDelegate success,
            BindingButtonDelegate cancel
            )
        {
            _upBinding = up;
            _downBinding = down;
            _leftBinding = left;
            _rightBinding = right;
            _successBinding = success;
            _cancelBinding = cancel;
        }

        private void Update()
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
                if (_leftBinding!= null && _leftBinding().CaptureJustPressed())
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
