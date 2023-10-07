using FDB.Components.Text;
using System.Collections;
using UnityEngine;

namespace FDB.Components
{
    public abstract class TextComponentBase<TDB, TConfig, TTextResolver> : MonoBehaviour 
        where TConfig : class
        where TTextResolver : struct, ITextResolver<TConfig>
    {
        [SerializeField] TextValueBase<TDB, TConfig, TTextResolver> _text;
        string[] _args;

        string[] _arg1;
        string[] _arg2;
        string[] _arg3;

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

        public void SetArgs()
        {
            _args = null;
            SetDirty();
        }

        public void SetArgs<A>(A a)
        {
            _arg1 = _arg1 ?? new string[1];
            _arg1[0] = a.ToString();

            _args = _arg1;
            SetDirty();
        }

        public void SetArgs<A, B>(A a, B b)
        {
            _arg2 = _arg2 ?? new string[2];
            _arg2[0] = a.ToString();
            _arg2[1] = b.ToString();

            _args = _arg2;
            SetDirty();
        }

        public void SetArgs<A, B, C>(A a, B b, C c)
        {
            _arg3 = _arg3 ?? new string[3];
            _arg3[0] = a.ToString();
            _arg3[1] = b.ToString();
            _arg3[2] = c.ToString();

            _args = _arg3;
            SetDirty();
        }

        public void SetArgs(params string[] args)
        {
            _args = args;
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

            string text;
            var processor = GetProcessor();
            if (processor == null)
            {
                text = _args != null && _args.Length > 0
                    ? string.Format(format, _args)
                    : format;
            } else
            {
                text = processor.Execute(format, _args);
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
