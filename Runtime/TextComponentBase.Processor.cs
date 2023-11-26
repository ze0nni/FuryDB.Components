using FDB.Components.Text;
using Fury.Strings;
using UnityEngine;

namespace FDB.Components
{
    public abstract partial class TextComponentBase<TDB, TTextConfig, TColorConfig, TResolver>
    {
        static ITextProcessor _defaultProcessor;
        static ITextProcessor GetDefaultProcessor()
        {
            if (_defaultProcessor == null)
            {
                _defaultProcessor = new DefaultProcessor();
            }
            return _defaultProcessor;
        }

        public static void ResetDefaultProcessor()
        {
            _defaultProcessor = null;
        }

        private sealed class DefaultProcessor : ZeroTextProcessorBase<TDB>
        {
            protected override void LoadColors(TDB db, ref ColorsMap map)
            {
                Index<TColorConfig> index;
#if UNITY_EDITOR
                index = Application.isPlaying
                    ? default(TResolver).ColorIndex
                    : (Index < TColorConfig > )FDB.Editor.EditorDB<TDB>.Resolver.GetIndex(typeof(TColorConfig));
#else
                index = default(TResolver).ColorIndex;
#endif

                var kindField = typeof(TColorConfig).GetField("Kind");
                foreach (var config in index)
                {
                    var kind = (Kind)kindField.GetValue(config);
                    var color = default(TResolver).GetColor(config);
                    map.Register(kind.Value, color);
                }
            }

            protected override void LoadTagsAlias(TDB db, ref TagsAliasMap map)
            {
                
            }

            protected override void LoadTagsProcessor(TDB db, ref TagsProcessorMap map)
            {
                Index<TTextConfig> index;
#if UNITY_EDITOR
                index = Application.isPlaying
                    ? default(TResolver).TextIndex
                    : (Index<TTextConfig>)FDB.Editor.EditorDB<TDB>.Resolver.GetIndex(typeof(TTextConfig));
#else
                index = default(TResolver).TextIndex;
#endif

                var textMap = new StringDictionary<TTextConfig>();
                var kindField = typeof(TTextConfig).GetField("Kind");
                foreach (var config in index)
                {
                    var kind = (Kind)kindField.GetValue(config);
                    textMap[kind.Value] = config;
                }
                map.Register("text", (bool slash, StringRef value, FormatBuffer buffer) =>
                {
                    if (textMap.TryGetValue(value, out var config))
                    {
                        buffer.Process(default(TResolver).GetText(config));
                    }
                });
            }

            protected override ZeroFormat.VariableProcessorDelegate LoadVariableProcessor(TDB db)
            {
                return null;
            }

            protected override TDB ResolveDB()
            {
                return default;
            }
        }
    }
}