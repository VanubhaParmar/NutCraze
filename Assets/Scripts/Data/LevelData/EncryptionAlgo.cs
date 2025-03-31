using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Tag.NutSort
{
    public static class EncryptionAlgo
    {
        private static IEncryptionAlgo iEncryptionAlgo;
        private static IEncryptionAlgo IEncryptionAlgo
        {
            get
            {
                if (iEncryptionAlgo == null)
                    iEncryptionAlgo = new AesEncryptionAlgo();
                return iEncryptionAlgo;
            }
        }

        public static byte[] Encrypt(byte[] data)
        {
            return IEncryptionAlgo.Encrypt(data);
        }

        public static byte[] Decrypt(byte[] data)
        {
            return IEncryptionAlgo.Decrypt(data);
        }
    }

    public interface IEncryptionAlgo
    {
        byte[] Encrypt(byte[] data);
        byte[] Decrypt(byte[] data);
    }


    public class AesEncryptionAlgo : IEncryptionAlgo
    {
        private static readonly byte[] _encryptionKey = new byte[32] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32 };

        public byte[] Encrypt(byte[] data)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = _encryptionKey;
                aes.GenerateIV(); // Random IV!

                using (MemoryStream outputStream = new MemoryStream())
                {
                    // Write IV first
                    outputStream.Write(aes.IV, 0, aes.IV.Length);

                    using (CryptoStream cryptoStream = new CryptoStream(outputStream, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cryptoStream.Write(data, 0, data.Length);
                        cryptoStream.FlushFinalBlock();

                        return outputStream.ToArray(); // Contains IV + Encrypted Data
                    }
                }
            }
        }

        public byte[] Decrypt(byte[] data)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = _encryptionKey;

                // Extract IV from the beginning of data
                byte[] iv = new byte[16];
                Array.Copy(data, 0, iv, 0, iv.Length);
                aes.IV = iv;

                using (MemoryStream inputStream = new MemoryStream(data, iv.Length, data.Length - iv.Length))
                using (MemoryStream outputStream = new MemoryStream())
                using (CryptoStream cryptoStream = new CryptoStream(outputStream, aes.CreateDecryptor(), CryptoStreamMode.Write))
                {
                    cryptoStream.Write(inputStream.ToArray(), 0, (int)inputStream.Length);
                    cryptoStream.FlushFinalBlock();

                    return outputStream.ToArray(); // Decrypted data
                }
            }
        }
    }

    public class TripleDesEncryptionAlgo : IEncryptionAlgo
    {
        private static readonly string _encryptionKey = "TheAppGuruz@123"; // Don't Change this key plz

        public byte[] Encrypt(byte[] data)
        {
            using (MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider())
            {
                byte[] keyHash = md5.ComputeHash(Encoding.UTF8.GetBytes(_encryptionKey));

                using (TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider())
                {
                    tdes.Key = keyHash;
                    tdes.Mode = CipherMode.ECB; // You might want to consider CBC mode with a proper IV for better security

                    ICryptoTransform encryptor = tdes.CreateEncryptor();

                    return encryptor.TransformFinalBlock(data, 0, data.Length);
                }
            }
        }

        public byte[] Decrypt(byte[] data)
        {
            using (MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider())
            {
                byte[] keyHash = md5.ComputeHash(Encoding.UTF8.GetBytes(_encryptionKey));

                using (TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider())
                {
                    tdes.Key = keyHash;
                    tdes.Mode = CipherMode.ECB; // Match the mode used in encryption

                    ICryptoTransform decryptor = tdes.CreateDecryptor();

                    return decryptor.TransformFinalBlock(data, 0, data.Length);
                }
            }
        }
    }
}
