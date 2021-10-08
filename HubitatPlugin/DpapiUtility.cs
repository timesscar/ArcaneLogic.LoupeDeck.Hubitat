namespace Loupedeck.AudioDevicePlugin
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading.Tasks;

    using Loupedeck.HubitatPlugin;

    public class DpapiUtility
    {
        /// <summary>
        /// The length of the salt.
        /// </summary>
        private const int SaltStringLength = 20;

        /// <summary>
        /// Array of strings with each required character set.
        /// </summary>
        private const string AvailableCharacters =
            "abcdefghijkmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789~@%^&()-_+=[]{}:;<>";


        /// <summary>
        /// The data protection scope.
        /// </summary>
        private DataProtectionScope scope;

        public DpapiUtility(DataProtectionScope scope)
        {
            this.scope = scope;
        }

        /// <summary>
        /// Encrypts a byte array with DPAPI.
        /// </summary>
        /// <param name="data">The raw data.</param>
        /// <returns>A base 64 encoded encrypted password string.</returns>
        public string EncryptData(byte[] data)
        {
            byte[] encryptedData = Array.Empty<byte>();
            try
            {
                var saltString = CreateRandomString(SaltStringLength);

                var salt = Encoding.Unicode.GetBytes(saltString);
                var dataArray = new byte[salt.Length + data.Length];
                salt.CopyTo(dataArray, 0);
                ZeroArray(salt);
                data.CopyTo(dataArray, salt.Length);
                ZeroArray(data);

                // Protect the data with the default entropy
                encryptedData = ProtectedData.Protect(dataArray, null, this.scope);

                return Convert.ToBase64String(encryptedData);
            }
            finally
            {
                ZeroArray(encryptedData);
            }
        }

        /// <summary>
        /// Decryptes a base 64 encoded encrypted password and returns it as a secure string.
        /// </summary>
        /// <param name="base64Encoded">The base 64 encoded encrypted password.</param>
        /// <returns>The secure string.</returns>
        public string DecryptData(string base64Encoded)
        {
            var encryptedBytes = Convert.FromBase64String(base64Encoded);
            byte[] unprotectedBytes = Array.Empty<byte>();

            try
            {
                unprotectedBytes = ProtectedData.Unprotect(encryptedBytes, null, this.scope);

                var securestring = new SecureString();
                // Unicode is 2 bytes per character, plus a string terminator.

                var saltLength = SaltStringLength * 2 + 2;
                return Encoding.Unicode.GetString(unprotectedBytes, saltLength - 2, unprotectedBytes.Length - saltLength + 2);
            }
            finally
            {
                ZeroArray(unprotectedBytes);
            }
        }


        /// <summary>
        /// Zeros an array, i.e. sets each element to the default.
        /// </summary>
        /// <typeparam name="T">the typeof of element.</typeparam>
        /// <param name="array">the array to zero.</param>
        public static void ZeroArray<T>(T[] array)
        {
            if (array == null)
            {
                return;
            }

            for (int i = 0; i < array.Length; i++)
            {
                array[i] = default(T);
            }
        }

        private static string CreateRandomString(
            int secureStringLength)
        {
            var sb = new StringBuilder();

            using (var rngCsp = new RNGCryptoServiceProvider())
            {
                byte[] generatedByte = new byte[2];

                for (int i = 0; i < secureStringLength; i++)
                {
                    rngCsp.GetNonZeroBytes(generatedByte);

                    var randomIndex = generatedByte[0] % AvailableCharacters.Length;

                    sb.Append(AvailableCharacters[randomIndex]);
                }
            }

            return sb.ToString();
        }
    }
}
