using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Profiling;

namespace FDB.Components
{
    internal class TextComponentRenderer : MonoBehaviour
    {
        static TextComponentRenderer _instance;
        static TextComponentRenderer Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject($"[{nameof(TextComponentRenderer)}]");
                    _instance = go.AddComponent<TextComponentRenderer>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        internal static void Render(ITextComponent text)
        {
            if (!Application.isPlaying)
            {
                text.Render();
                return;
            }
            Instance._render.Add(text);
        }

        readonly HashSet<ITextComponent> _render = new HashSet<ITextComponent>();

        private void LateUpdate()
        {
            if (_render.Count == 0)
            {
                return;
            }

            Profiler.BeginSample("ITextComponent.Render()");
            try
            {
                foreach (var text in _render)
                {
#if UNITY_EDITOR
                    Profiler.BeginSample("#if UNITY_EDITOR");
                    var path = GetPath(text.transform);
                    Profiler.EndSample();
                    Profiler.BeginSample(path);
                    try
#endif
                    {
                        text.Render();
                    }
#if UNITY_EDITOR
                    finally
                    {
                        Profiler.EndSample();
                    }
#endif
                }
            }
            catch (Exception exc)
            {
                Debug.LogException(exc);
            }
            finally
            {
                Profiler.EndSample();
                _render.Clear();
            }
        }

        StringBuilder _sb;
        string GetPath(Transform transform)
        {
            if (_sb == null)
            {
                _sb = new StringBuilder();
            }
            _sb.Clear();

            void It(StringBuilder sb, Transform t)
            {
                if (t == null)
                {
                    return;
                }
                It(sb, t.parent);
                if (sb.Length > 0)
                {
                    sb.Append(".");
                }
                sb.Append(t.name);
            }
            It(_sb, transform);

            return _sb.ToString();
        }
    }
}
