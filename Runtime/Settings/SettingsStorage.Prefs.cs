using Newtonsoft.Json;
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
                foreach (var (_, key) in keys)
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
                }
            }

            public void Save(string userId, IReadOnlyList<SettingsKey> keys)
            {
                foreach (var key in keys)
                {
                    PlayerPrefs.SetString(PrefsKey(userId, key), key.ToJsonString());
                }
            }
        }
    }
}