using System;
using UnityEngine;

namespace FDB.Components
{
    [Serializable]
    public abstract class ColorValueBase<TDB, TConfig, TColorResolver> 
        where TConfig : class 
        where TColorResolver : struct, IColorResolver<TConfig>
    {
        [SerializeField] bool _raw;
        [SerializeField] Color _rawValue = Color.white;
        [SerializeField] string _value;

        public Color Color
        {
            get
            {
                if (_raw)
                {
                    return _rawValue;
                }
                Index<TConfig> index;
#if UNITY_EDITOR
                index = Application.isPlaying
                    ? default(TColorResolver).Index
                    : (Index<TConfig>)FDB.Editor.EditorDB<TDB>.Resolver.GetIndex(typeof(TConfig));
#else
                index = default(TColorResolver).Index;
#endif
                if (index == null)
                {
                    Debug.LogWarning($"Index<{typeof(TConfig).Name}> not found in {typeof(TDB).Name}");
                    return Color.magenta;
                }
                if (_value == null || !index.TryGet(new Kind<TConfig>(_value), out var config))
                {
                    return Color.magenta;
                }
                return default(TColorResolver).GetColor(config);
            }
        }

        public void SetColor(Color color)
        {
            _raw = true;
            _rawValue = color;
        }

        public void SetColor(Kind<TConfig> kind)
        {
            _raw = false;
            _value = kind.Value;
        }
    }

    public interface IColorResolver<TConfig> 
        where TConfig : class
    {
        Index<TConfig> Index { get; }
        Color GetColor(TConfig config);
        Color[] GetColors(TConfig config);
    }
}
