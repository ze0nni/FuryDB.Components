using FDB.Components.Text;
using Fury.Strings;
using System.Collections;
using UnityEngine;

namespace FDB.Components
{
    public abstract class TextComponentBase<TDB, TConfig, TTextResolver> : MonoBehaviour
        where TConfig : class
        where TTextResolver : struct, ITextResolver<TConfig>
    {
        [SerializeField] TextValueBase<TDB, TConfig, TTextResolver> _text;
        Args _args;

        public Args Args
        {
            get
            {
                if (_args == null)
                {
                    _args = new Args(SetDirty);
                }
                _args.Clear();
                return _args;
            }
        }

        VariableProcessorDelegate _variableProcessor;
        public void SetVariableProcessor(VariableProcessorDelegate value)
        {
            _variableProcessor = value;
            SetDirty();
        }

        public bool Translate => _text.Translate;

        public void SetText(string text, bool translate = false)
        {
            _text.Translate = translate;
            _text.Value = text;
            SetDirty();
        }

        public void SetText(Kind<TConfig> kind)
        {
            _text.Translate = true;
            _text.Value = kind.Value;
            SetDirty();
        }

        protected virtual void OnEnable()
        {
            StartRender();
        }

#if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            _isDirty = false;
            SetDirty();
        }
#endif
        protected virtual ITextProcessor GetProcessor() => null;

        protected abstract void Render(string text);

        private bool _isDirty = false;
        private IEnumerator _render;
        protected void SetDirty()
        {
            if (_isDirty)
            {
                return;
            }
            StartRender();
        }

        void StartRender()
        {
            _isDirty = true;
            if (!gameObject.activeInHierarchy)
            {
                return;
            }
            if (_render == null)
            {
                _render = new RenderTask(this);
            }
            _render.Reset();

            StartCoroutine(_render);
        }

        void Render()
        {
            _isDirty = false;

            string format;
            if (_text.Translate)
            {
                format = _text.Text;
            }
            else
            {
                format = _text.Value;
            }

            string text = "!";
            var processor = GetProcessor();
            if (processor == null)
            {
                text = _args != null && _args.Length > 0
                    ? string.Format(format, _args)
                    : format;
            } else
            {
                text = processor.Execute(format, _args, _variableProcessor);
            }

            Render(text);
        }

        private class RenderTask : IEnumerator
        {
            readonly TextComponentBase<TDB, TConfig, TTextResolver> _text;

            public RenderTask(TextComponentBase<TDB, TConfig, TTextResolver> text)
            {
                _text = text;
            }

            int _index;

            public object Current => null;

            public bool MoveNext()
            {
                switch (_index++)
                {
                    case 0:
                        return true;
                    case 1:
                        _text.Render();
                        return false;
                    default:
                        return false;
                }
            }

            public void Reset()
            {
                _index = 0;
            }
        }
    }

    public interface ITextResolver<TConfig>
        where TConfig : class
    {
        Index<TConfig> Index { get; }
        string GetText(TConfig config);
    }
}
