using System;
using UnityEngine;

namespace FDB.Components.Settings
{
    internal static partial class SettingsStorage
    {
        internal sealed class Prefs : ISettingsStorage
        {
            public IDisposable Read(string userId, out ISettingsReader reader)
            {
                reader = new Reader(userId);
                return (Reader)reader;
            }

            public IDisposable Write(string userId, out ISettingsWriter writer)
            {
                writer = new Writer(userId);
                return (Writer)writer;
            }

            private class Reader : ISettingsReader, IDisposable
            {
                readonly string _userId;
                bool _disposed;
                public Reader(string userId) => _userId = userId;

                public bool Read(SettingsKey key, out string value)
                {
                    if (_disposed)
                    {
                        throw new ArgumentNullException($"{GetType().FullName} disposed");
                    }
                    value = null;
                    var k = key.KeyOf(_userId);
                    if (PlayerPrefs.HasKey(k))
                    {
                        value = PlayerPrefs.GetString(k);
                    }
                    return value != null;
                }

                public void Dispose()
                {
                    _disposed = true;
                }
            }

            private class Writer : ISettingsWriter, IDisposable
            {
                readonly string _userId;
                bool _disposed;
                public Writer(string userId) => _userId = userId;

                public void Write(SettingsKey key)
                {
                    if (_disposed)
                    {
                        throw new ArgumentNullException($"{GetType().FullName} disposed");
                    }
                    PlayerPrefs.SetString(key.KeyOf(_userId), key.StringValue);
                }

                public void Dispose()
                {
                    _disposed = true;
                    PlayerPrefs.Save();
                }

            }
        }
    }
}