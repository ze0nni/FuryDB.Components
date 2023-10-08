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

            public IDisposable Read(string userId, out ISettingsReader reader)
            {
                reader = new Reader(GetPath(userId));
                return (Reader)reader;
            }

            public IDisposable Write(string userId, out ISettingsWriter writer)
            {
                writer = new Writer(GetPath(userId));
                return (Writer)writer;
            }

            internal sealed class Reader: ISettingsReader, IDisposable
            {
                private readonly string _path;
                private readonly Dictionary<string, string> _keyValue = new Dictionary<string, string>();

                public Reader(string path)
                {
                    _path = path;
                    if (!File.Exists(_path))
                    {
                        return;
                    }
                    using (var textReader = new StreamReader(_path))
                    {
                        using (var reader = new JsonTextReader(textReader))
                        {
                            while (reader.Read())
                            {
                                switch (reader.TokenType)
                                {
                                    case JsonToken.Comment:
                                        break;
                                    case JsonToken.StartObject:
                                        ReadSettingsBody(reader);
                                        break;
                                    default:
                                        throw new ArgumentException($"Unexcepted token {reader.TokenType}");
                                }
                            }
                        }
                    }
                }

                private void ReadSettingsBody(JsonTextReader reader)
                {
                    while (reader.Read())
                    {
                        switch (reader.TokenType)
                        {
                            case JsonToken.EndObject:
                                return;
                            case JsonToken.Comment:
                                break;
                            case JsonToken.PropertyName:
                                var key = (string)reader.Value;
                                reader.Read();
                                var value = (string)reader.Value;
                                _keyValue[key] = value;
                                break;
                            default:
                                throw new ArgumentException($"Unexcepted token {reader.TokenType}");
                        }
                    }
                }

                public bool Read(SettingsKey key, out string value)
                {
                    return _keyValue.TryGetValue(key.Id, out value);
                }
                public void Dispose()
                {
                    
                }
            }

            internal sealed class Writer : ISettingsWriter, IDisposable
            {
                private readonly string _path;
                private readonly TextWriter _textwriter;
                private readonly JsonTextWriter _jsonWriter;

                public Writer(string path)
                {
                    _path = path;
                    Directory.CreateDirectory(Path.GetDirectoryName(_path));

                    _textwriter = new StreamWriter(_path);
                    _jsonWriter = new JsonTextWriter(_textwriter);
                    _jsonWriter.Formatting = Formatting.Indented;

                    _jsonWriter.WriteStartObject();
                }

                public void Write(SettingsKey key)
                {
                    _jsonWriter.WritePropertyName(key.Id);
                    _jsonWriter.WriteValue(key.StringValue);
                }

                public void Dispose()
                {
                    _jsonWriter.WriteEndObject();

                    _jsonWriter.Close();
                    _textwriter.Close();
                    _textwriter.Dispose();
                }
            }
        }
    }
}