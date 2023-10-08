using System;
using System.Security.Cryptography;
using System.Text;

namespace FDB.Components.Settings
{
    public enum HashType
    {
        MD5 = 1
    }

    public interface IUserIdHash
    {
        string Hash(string userId);
    }

    internal static partial class SettingsStorage
    {
        internal static IUserIdHash Resolve(this HashType type)
        {
            switch (type)
            {
                case HashType.MD5:
                    return new UserIdHash(new MD5CryptoServiceProvider());
                default:
                    throw new ArgumentOutOfRangeException(type.ToString());
            }
        }

        internal class UserIdHash : IUserIdHash
        {
            readonly HashAlgorithm _algorithm;

            public UserIdHash(HashAlgorithm algorithm) => _algorithm = algorithm;

            public string Hash(string userId)
            {
                var bytes = Encoding.ASCII.GetBytes(userId);
                var hash = _algorithm.ComputeHash(bytes);
                return BitConverter.ToString(hash);
            }
        }
    }
}