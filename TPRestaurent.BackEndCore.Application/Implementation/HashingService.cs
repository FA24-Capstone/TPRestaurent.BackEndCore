﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using TPRestaurent.BackEndCore.Application.Contract.IServices;
using TPRestaurent.BackEndCore.Common.Utils;

namespace TPRestaurent.BackEndCore.Application.Implementation
{
    public class HashingService : GenericBackendService, IHashingService
    {
        public HashingService(IServiceProvider service): base(service) { }
        public string Hashing(string password, string key)
        {
            using (Aes aes = Aes.Create())
            {
                // Convert the key to bytes and ensure it is 32 bytes long (for AES-256)
                byte[] keyBytes = Encoding.UTF8.GetBytes(key);
                aes.Key = keyBytes.Length > 32 ? keyBytes.Take(32).ToArray() : keyBytes;
                aes.IV = new byte[16]; // Initialize IV with zeros (or use a more secure random IV in real applications)

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (MemoryStream ms = new MemoryStream())
                using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                {
                    using (StreamWriter sw = new StreamWriter(cs))
                    {
                        sw.Write(password);
                    }

                    // Convert the encrypted bytes to a Base64 string
                    byte[] encryptedBytes = ms.ToArray();

                    // Convert the Base64 string to URL-safe Base64
                    return Base64UrlEncode(encryptedBytes);
                }
            }
        }

        private string Base64UrlEncode(byte[] input)
        {
            // Standard Base64 encoding
            string base64 = Convert.ToBase64String(input);

            // Convert to Base64 URL-safe encoding
            return base64.Replace('+', '-').Replace('/', '_').Replace("=", "");
        }

        public string DeHashing(string password, string key)
        {
            using (Aes aes = Aes.Create())
            {
                byte[] keyBytes = Encoding.UTF8.GetBytes(key);
                aes.Key = keyBytes.Length > 32 ? keyBytes.Take(32).ToArray() : keyBytes; // AES-256 requires a 32-byte key
                aes.IV = new byte[16]; // The same IV used during encryption

                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                // Convert URL-safe Base64 to standard Base64
                string base64 = password.Replace('-', '+').Replace('_', '/');
                switch (base64.Length % 4)
                {
                    case 2: base64 += "=="; break;
                    case 3: base64 += "="; break;
                }

                // Convert Base64 string to byte array
                byte[] cipherBytes = Convert.FromBase64String(base64);

                using (MemoryStream ms = new MemoryStream(cipherBytes))
                using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                using (StreamReader sr = new StreamReader(cs))
                {
                    return sr.ReadToEnd(); // Return the decrypted string
                }
            }
        }
    }
}