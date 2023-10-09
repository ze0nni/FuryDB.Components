using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace FDB.Components.Settings
{
    internal static partial class SettingsStorage
    {
        internal sealed class Prefs : ISettingsStorage
        {
            private static string PrefsKey(string userId, SettingsKey key) => $"{userId}.{key.Id}";

            public void Load(string userId, IReadOnlyDictionary<string, SettingsKey> keys)
            {
                List<Exception> _exceptions = null;

                foreach (var (_, key) in keys)
                {
                    try
                    {
                        var prefsKey = PrefsKey(userId, key);
                        if (PlayerPrefs.HasKey(prefsKey))
                        {
                            var rawJson = PlayerPrefs.GetString(prefsKey);
                            var bytes = Encoding.UTF8.GetBytes(rawJson);
                            using (var memoryStream = new MemoryStream(bytes.Length))
                            {
                                memoryStream.Write(bytes);
                                memoryStream.Position = 0;
                                using (var streamReader = new StreamReader(memoryStream))
                                {
                                    using (var reader = new JsonTextReader(streamReader))
                                    {
                                        reader.Read();
                                        key.Load(reader);
                                    }
                                }
                            }
                        }
                    } catch (Exception exc)
                    {
                        if (_exceptions == null)
                        {
                            _exceptions = new List<Exception>();
                        }
                        _exceptions.Add(exc);
                    }
                }

                if (_exceptions != null && _exceptions.Count > 0)
                {
                    throw new AggregateException(_exceptions);
                }
            }

            public void Save(string userId, IReadOnlyList<SettingsKey> keys)
            {
                List<Exception> _exceptions = null;

                foreach (var key in keys)
                {
                    try
                    {
                        PlayerPrefs.SetString(PrefsKey(userId, key), key.ToJsonString());
                    }
                    catch (Exception exc)
                    {
                        if (_exceptions == null)
                        {
                            _exceptions = new List<Exception>();
                        }
                        _exceptions.Add(exc);
                    }

                    if (_exceptions != null && _exceptions.Count > 0)
                    {
                        throw new AggregateException(_exceptions);
                    }
                }
            }
        }
    }
}