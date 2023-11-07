using System;
using System.Collections.Generic;
using UnityEngine;

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

            try
            {
                foreach (var text in _render)
                {
                    text.Render();
                }
            }
            catch (Exception exc)
            {
                Debug.LogException(exc);
            }
            finally
            {
                _render.Clear();
            }
        }
    }
}
