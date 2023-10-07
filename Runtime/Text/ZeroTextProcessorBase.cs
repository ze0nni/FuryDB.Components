using Fury.Strings;
using UnityEngine;

namespace FDB.Components.Text
{
    public abstract class ZeroTextProcessorBase<TDB> : ITextProcessor
    {
        readonly ZeroFormat _format = new ZeroFormat(1024);
        readonly ZeroFormat.VariableProcessorDelegate _internalVariableProcessor;
        ZeroFormat.VariableProcessorDelegate _defaultVariableProcessor;
        VariableProcessorDelegate _currentVariableProcessor;

        readonly StringDictionary<string> _colorsMap = new StringDictionary<string>();
        readonly StringDictionary<(string, string)> _tagsAlias = new StringDictionary<(string, string)>();
        readonly StringDictionary<ZeroFormat.TagProcessorDelegate> _tagsProcessor = new StringDictionary<ZeroFormat.TagProcessorDelegate>();

        public ZeroTextProcessorBase()
        {
            _internalVariableProcessor = VariableProcessor;
            Reload();
        }

        public string Execute(
            string format,
            string[] args = null, 
            VariableProcessorDelegate variableProcessor = null)
        {
            _currentVariableProcessor = variableProcessor;

            _format.Setup(
                format,
                args,
                variablesProcessor: variableProcessor != null || _defaultVariableProcessor != null 
                    ? _internalVariableProcessor 
                    : null,
                colorsMap: _colorsMap,
                tagsAlias: _tagsAlias,
                tagsProcessor: _tagsProcessor);
            _currentVariableProcessor = null;

            return _format.ToString();
        }

        private void VariableProcessor(StringRef variable, ref FormatBuffer buffer)
        {
            if (_currentVariableProcessor != null && _currentVariableProcessor(variable, ref buffer))
            {
                return;
            }
            if (_defaultVariableProcessor != null)
            {
                _defaultVariableProcessor(variable, ref buffer);
            }
        }

        public void Reload()
        {
#if UNITY_EDITOR
            var db = Application.isPlaying
                ? ResolveDB()
                : FDB.Editor.EditorDB<TDB>.DB;
#else
            var db = ResolveDB();
#endif

            _colorsMap.Clear();
            var colorsMap = new ColorsMap(_colorsMap);
            LoadColors(db, ref colorsMap);

            _tagsAlias.Clear();
            var tagsAlias = new TagsAliasMap(_tagsAlias);
            LoadTagsAlias(db, ref tagsAlias);

            _tagsProcessor.Clear();
            var tagsProcessor = new TagsProcessorMap(_tagsProcessor);
            LoadTagsProcessor(db, ref tagsProcessor);

            _defaultVariableProcessor = LoadVariableProcessor(db);
        }

        protected readonly ref struct ColorsMap
        {
            readonly StringDictionary<string> _colors;
            internal ColorsMap(StringDictionary<string> colors)
            {
                _colors = colors;
            }

            public void Register(string name, Color color)
            {
                _colors.Add(name, $"#{ColorUtility.ToHtmlStringRGB(color)}");
            }

            public void Register(string name, string color)
            {
                _colors.Add(name, color);
            }
        }

        protected readonly ref struct TagsAliasMap
        {
            readonly StringDictionary<(string, string)> _aliases;
            internal TagsAliasMap(StringDictionary<(string, string)> aliases)
            {
                _aliases = aliases;
            }

            public void Register(string tag, string opened, string closed)
            {
                _aliases.Add(tag, (opened, closed));
            }
        }

        protected readonly ref struct TagsProcessorMap
        {
            readonly StringDictionary<ZeroFormat.TagProcessorDelegate> _processors;
            internal TagsProcessorMap(StringDictionary<ZeroFormat.TagProcessorDelegate> processors)
            {
                _processors = processors;
            }

            public void Register(string tag, ZeroFormat.TagProcessorDelegate processor)
            {
                _processors.Add(tag, processor);
            }
        }

        protected abstract TDB ResolveDB();
        protected abstract void LoadColors(TDB db, ref ColorsMap map);
        protected abstract void LoadTagsAlias(TDB db, ref TagsAliasMap map);
        protected abstract void LoadTagsProcessor(TDB db, ref TagsProcessorMap map);
        protected abstract ZeroFormat.VariableProcessorDelegate LoadVariableProcessor(TDB db);
    }
}