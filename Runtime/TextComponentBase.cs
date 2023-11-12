using FDB.Components.Text;
using Fury.Strings;
using System;
using System.Collections;
using UnityEngine;

namespace FDB.Components
{
    public interface ITextComponent
    {
        Transform transform { get; }
        void Render();
    }

    public abstract partial class TextComponentBase<TDB, TTextConfig, TColorConfig, TResolver> : MonoBehaviour, ITextComponent
        where TTextConfig : class
        where TColorConfig : class
        where TResolver : struct, ITextResolver<TTextConfig>, IColorResolver<TColorConfig>
    {
        [SerializeField] TextValueBase<TDB, TTextConfig, TResolver> _text;
        [SerializeField] ColorValueBase<TDB, TColorConfig, TResolver> _color;

        Args _args;
        Action _clearArgs;
        string _currentString;
        Color _currentColor;

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

        public void SetText(Kind<TTextConfig> kind)
        {
            _text.Translate = true;
            _text.Value = kind.Value;
            SetDirty();
        }

        public void SetColor(Color color)
        {
            _color.SetColor(color);
            SetDirty();
        }

        public void SetColor(Kind<TColorConfig> color)
        {
            _color.SetColor(color);
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
        protected virtual ITextProcessor GetProcessor() => GetDefaultProcessor();

        protected abstract void Render(string text, Color color);

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

            var newColor = _color.Color;
            if (ReferenceEquals(_currentString, newString) && _currentColor == newColor)
            {
                return;
            }
            _currentString = newString;
            _currentColor = newColor;

            Render(_currentString, newColor);
        }
    }

    public interface ITextResolver<TConfig>
        where TConfig : class
    {
        Index<TConfig> TextIndex { get; }
        string GetText(TConfig config);
    }
}
