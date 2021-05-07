using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace OpenBots.Server.Business
{
    public class CredentialHasher
    {
        public static string GenerateSaltedHash(string plainText, string salt)
        {
            byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            byte[] saltBytes = Encoding.UTF8.GetBytes(salt);

            HashAlgorithm algorithm = new SHA256Managed();

            byte[] plainTextWithSaltBytes =
              new byte[plainText.Length + salt.Length];

            for (int i = 0; i < plainText.Length; i++)
            {
                plainTextWithSaltBytes[i] = plainTextBytes[i];
            }
            for (int i = 0; i < salt.Length; i++)
            {
                plainTextWithSaltBytes[plainText.Length + i] = saltBytes[i];
            }

            byte[] hash = algorithm.ComputeHash(plainTextWithSaltBytes);
            return Convert.ToBase64String(hash);
        }

        public static bool CompareByteArrays(string array1, string array2)
        {
            byte[] array1Bytes = Encoding.UTF8.GetBytes(array1);
            byte[] array2Byes = Encoding.UTF8.GetBytes(array2);

            if (array1Bytes.Length != array2Byes.Length)
            {
                return false;
            }

            for (int i = 0; i < array1Bytes.Length; i++)
            {
                if (array1Bytes[i] != array2Byes[i])
                {
                    return false;
                }
            }

            return true;
        }

        public static string CreateSalt(int size)
        {
            //Generate a cryptographic random number.
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            byte[] buff = new byte[size];
            rng.GetBytes(buff);

            // Return a Base64 string representation of the random number.
            return Convert.ToBase64String(buff);
        }
    }
}
