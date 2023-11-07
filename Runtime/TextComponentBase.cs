using FDB.Components.Text;
using Fury.Strings;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Profiling;

namespace FDB.Components
{
    public interface ITextComponent
    {
        Transform transform { get; }
        void Render();
    }

    public abstract class TextComponentBase<TDB, TConfig, TTextResolver> : MonoBehaviour, ITextComponent
        where TConfig : class
        where TTextResolver : struct, ITextResolver<TConfig>
    {
        [SerializeField] TextValueBase<TDB, TConfig, TTextResolver> _text;
        Args _args;
        Action _clearArgs;
        string _currentString;

        public Args Args
        {
            get
            {
                if (_args == null)
                {
                    _args = new Args(out _clearArgs, SetDirty);
                }
                _clearArgs();
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
        protected virtual ITextProcessor GetProcessor() => ZeroTextProcessorDefault.Instance;

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
            TextComponentRenderer.Render(this);
        }

        void ITextComponent.Render()
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

            var processor = GetProcessor();
            string newString;
            if (processor == null)
            {
                newString = _args != null && _args.Length > 0
                    ? string.Format(format, _args.ToObjectsArray())
                    : format;
            } else
            {
                newString = processor.Execute(format,
                    currentString: _currentString,
                    args: _args, 
                    variableProcessor: _variableProcessor);
            }

            if (ReferenceEquals(_currentString, newString))
            {
                return;
            }
            _currentString = newString;

            Render(_currentString);
        }
    }

    public interface ITextResolver<TConfig>
        where TConfig : class
    {
        Index<TConfig> Index { get; }
        string GetText(TConfig config);
    }
}
