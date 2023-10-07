using FDB.Components.Text;
using System;
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
        string[][] _argsList = new string[8][];

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

        private string[] Args(int size)
        {
            if (size == 0)
            {
                _args = null;
                return null;
            }
            if (size > _argsList.Length)
            {
                var newList = new string[size * 2][];
                Array.Copy(_argsList, newList, _argsList.Length);
                _argsList = newList;
            }
            if (_argsList[size - 1] == null)
            {
                _argsList[size - 1] = new string[size];
            }
            _args = _argsList[size - 1];
            return _args;
        }

        public void SetArgs()
        {
            Args(0);
            SetDirty();
        }

        public void SetArgs<A>(A a)
        {
            var args = Args(1);
            var n = 0;
            args[n++] = a.ToString();

            SetDirty();
        }

        public void SetArgs<A, B>(A a, B b)
        {
            var args = Args(2);
            var n = 0;
            args[n++] = a.ToString();
            args[n++] = b.ToString();

            SetDirty();
        }

        public void SetArgs<A, B, C>(A a, B b, C c)
        {
            var args = Args(3);
            var n = 0;
            args[n++] = a.ToString();
            args[n++] = b.ToString();
            args[n++] = c.ToString();

            SetDirty();
        }

        public void SetArgs<A, B, C, D>(A a, B b, C c, D d)
        {
            var args = Args(4);
            var n = 0;
            args[n++] = a.ToString();
            args[n++] = b.ToString();
            args[n++] = c.ToString();
            args[n++] = d.ToString();

            SetDirty();
        }

        public void SetArgs<A, B, C, D, E>(A a, B b, C c, D d, E e)
        {
            var args = Args(5);
            var n = 0;
            args[n++] = a.ToString();
            args[n++] = b.ToString();
            args[n++] = c.ToString();
            args[n++] = d.ToString();
            args[n++] = e.ToString();

            SetDirty();
        }

        public void SetArgs<A, B, C, D, E, F>(A a, B b, C c, D d, E e, F f)
        {
            var args = Args(6);
            var n = 0;
            args[n++] = a.ToString();
            args[n++] = b.ToString();
            args[n++] = c.ToString();
            args[n++] = d.ToString();
            args[n++] = e.ToString();
            args[n++] = f.ToString();

            SetDirty();
        }

        public void SetArgs<A, B, C, D, E, F, H>(A a, B b, C c, D d, E e, F f, H h)
        {
            var args = Args(7);
            var n = 0;
            args[n++] = a.ToString();
            args[n++] = b.ToString();
            args[n++] = c.ToString();
            args[n++] = d.ToString();
            args[n++] = e.ToString();
            args[n++] = f.ToString();
            args[n++] = h.ToString();

            SetDirty();
        }

        public void SetArgs<A, B, C, D, E, F, H, I>(A a, B b, C c, D d, E e, F f, H h, I i)
        {
            var args = Args(8);
            var n = 0;
            args[n++] = a.ToString();
            args[n++] = b.ToString();
            args[n++] = c.ToString();
            args[n++] = d.ToString();
            args[n++] = e.ToString();
            args[n++] = f.ToString();
            args[n++] = h.ToString();
            args[n++] = i.ToString();

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
