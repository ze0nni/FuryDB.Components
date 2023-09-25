using System;
using UnityEngine;

namespace FDB.Components
{
    [Serializable]
    public abstract class ColorValueBase<TDB, TConfig, TColorResolver> 
        where TConfig : class 
        where TColorResolver : struct
    {
        [SerializeField] bool _raw;
        [SerializeField] Color _rawValue = Color.white;
        [SerializeField] string _value;

        public Color Color
        {
            get
            {
                return default;
            }
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
