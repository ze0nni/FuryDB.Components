using System;
using UnityEngine;

namespace FDB.Components
{
    [Serializable]
    public class TextValueBase<TDB, TConfig, TTextResolver>
        where TConfig : class
        where TTextResolver : struct, ITextResolver<TConfig>
    {
        public bool Translate = true;
        public string Value = "Text";

        public string Text
        {
            get
            {
                TryGetText(out var text);
                return text;
            }
        }

        public bool TryGetText(out string text)
        {
            Index<TConfig> index;
#if UNITY_EDITOR
            index = Application.isPlaying
                ? default(TTextResolver).Index
                : (Index<TConfig>)FDB.Editor.EditorDB<TDB>.Resolver.GetIndex(typeof(TConfig));
#else
                index = default(TColorResolver).Index;
#endif
            if (index == null)
            {
                Debug.LogWarning($"Index<{typeof(TConfig).Name}> not found in {typeof(TDB).Name}");
                text = Value ?? "Empty";
                return false;
            }
            if (Value == null || !index.TryGet(new Kind<TConfig>(Value), out var config))
            {
                text = Value ?? "Empty";
                return false;
            }
            text = default(TTextResolver).GetText(config);
            return true;
        }
    }
}
