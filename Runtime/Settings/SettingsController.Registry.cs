using System;
using System.Collections.Generic;

namespace FDB.Components.Settings
{
    public interface IRegistry
    {
        T Get<T>(string name = null) where T : class;
        IEnumerable<T> GetAll<T>() where T : class;

        T Add<T>(T instance) where T : class;
        T Add<T>(string name, T instance) where T : class;
        T GetOrCreate<T>(string name = null) where T : class, new();
        T GetOrCreate<T>(string name, Func<T> factory) where T : class;
    }

    public sealed class Registry : IRegistry
    {
        readonly Dictionary<Type, object>
            _map = new Dictionary<Type, object>();

        readonly Dictionary<Type, Dictionary<string, object>> 
            _namedMap = new Dictionary<Type, Dictionary<string, object>>();

        public T Get<T>(string name = null)
            where T : class
        {
            if (name == null)
            {
                _map.TryGetValue(typeof(T), out var result);
                return (T)result;
            } else
            {
                if (_namedMap.TryGetValue(typeof(T), out var map)
                    && map.TryGetValue(name, out var result))
                    return (T)result;
            }
            return default;
        }

        public IEnumerable<T> GetAll<T>()
            where T : class
        {
            {
                if (_map.TryGetValue(typeof(T), out var result))
                    yield return (T)result;
            }
            {
                if (_namedMap.TryGetValue(typeof(T), out var map)) {
                    foreach (var result in map.Values)
                    {
                        yield return (T)result;
                    }
                }
            }
        }

        T IRegistry.Add<T>(T instance)
        {
            _map.Add(typeof(T), instance ?? throw new ArgumentNullException(nameof(instance)));
            return instance;
        }

        T IRegistry.Add<T>(string name, T instance)
        {
            if (name == null)
            {
                return ((IRegistry)this).Add<T>(instance);
            }
            if (!_namedMap.TryGetValue(typeof(T), out var map))
            {
                map = new Dictionary<string, object>();
                _namedMap.Add(typeof(T), map);
            }
            map.Add(name, instance ?? throw new ArgumentNullException(nameof(instance)));
            return instance;
        }

        T IRegistry.GetOrCreate<T>(string name = null)
        {
            return Get<T>(name) ?? ((IRegistry)this).Add<T>(name, new T());
        }

        T IRegistry.GetOrCreate<T>(string name, Func<T> factory)
        {
            return Get<T>(name) ?? ((IRegistry)this).Add<T>(name, factory());
        }
    }
}