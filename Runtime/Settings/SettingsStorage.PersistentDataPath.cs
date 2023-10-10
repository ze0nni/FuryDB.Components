using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace FDB.Components.Settings
{
    internal static partial class SettingsStorage
    {
        internal sealed class PersistentDataPath : ISettingsStorage
        {
            private readonly string _fileName;

            public PersistentDataPath(Type settingsType)
            {
                _fileName = SettingsStorageAttribute.ResolveFileName(settingsType);
            }

            private string GetPath(string userId)
            {
                return Path.Combine(
                    Application.persistentDataPath,
                    string.Format(_fileName, userId));
            }

            public void Load(string userId, IReadOnlyDictionary<string, SettingsKey> keys)
            {
                var path = GetPath(userId);
                if (!File.Exists(path))
                {
                    return;
                }
                using (var streamReader = new StreamReader(path))
                {
                    using (var reader = new JsonTextReader(streamReader))
                    {
                        while (reader.Read())
                        {
                            switch (reader.TokenType)
                            {
                                case JsonToken.Comment:
                                    continue;
                                case JsonToken.StartObject:
                                    LoadKeys(reader);
                                    break;
                                default:
                                    throw new ArgumentException($"Unexcepted token type {reader.TokenType}");
                            }
                        }
                    }
                }

                void LoadKeys(JsonTextReader reader)
                {
                    while (reader.Read())
                    {
                        switch (reader.TokenType)
                        {
                            case JsonToken.Comment:
                                continue;
                            case JsonToken.PropertyName:
                                var id = (string)reader.Value;
                                reader.Read();
                                LoadKey(id, reader);
                                break;
                            case JsonToken.EndObject:
                                return;
                            default:
                                throw new ArgumentException($"Unexcepted token type {reader.TokenType}");
                        }
                    }
                }

                void LoadKey(string id, JsonTextReader reader)
                {
                    if (!keys.TryGetValue(id, out var key))
                    {
                        reader.Skip();
                    } else
                    {
                        var startDepth = reader.Depth;
                        try
                        {
                            key.Load(reader);
                        } catch (Exception exc)
                        {
                            Debug.LogError($"Exception when read key {key.Id}");
                            Debug.LogException(exc);
                        }
                        switch (reader.TokenType)
                        {
                            case JsonToken.StartArray:
                            case JsonToken.StartObject:
                            case JsonToken.StartConstructor:
                                reader.Read();
                                break;
                        }
                        while(reader.Depth > startDepth)
                        {
                            reader.Read();
                        }
                    }
                }
            }

            public void Save(string userId, IReadOnlyList<SettingsKey> keys)
            {
                using (var streamWriter = new StreamWriter(GetPath(userId)))
                {
                    using (var writer = new JsonTextWriter(streamWriter))
                    {
                        writer.Formatting = Formatting.Indented;

                        writer.WriteStartObject();
                        foreach (var key in keys)
                        {
                            writer.WritePropertyName(key.Id);
                            key.Save(writer);
                        }
                        writer.WriteEndObject();
                    }
                }
            }
        }
    }
}