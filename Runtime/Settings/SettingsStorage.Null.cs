using System;

namespace FDB.Components.Settings
{
    internal static partial class SettingsStorage
    {
        internal sealed class Null : ISettingsStorage, ISettingsReader, ISettingsWriter, IDisposable
        {
            IDisposable ISettingsStorage.Read(string userId, out ISettingsReader reader)
            {
                reader = this;
                return this;
            }

            IDisposable ISettingsStorage.Write(string userId, out ISettingsWriter writer)
            {
                writer = this;
                return this;
            }

            bool ISettingsReader.Read(SettingsKey key, out string value)
            {
                value = null;
                return false;
            }

            void ISettingsWriter.Write(SettingsKey key)
            {

            }

            void IDisposable.Dispose()
            {
            }
        }
    }
}