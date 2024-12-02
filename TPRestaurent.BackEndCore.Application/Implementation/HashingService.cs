using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using System.Text;
using TPRestaurent.BackEndCore.Application.Contract.IServices;
using TPRestaurent.BackEndCore.Common.DTO.Response.BaseDTO;

namespace TPRestaurent.BackEndCore.Application.Implementation
{
    public class HashingService : GenericBackendService, IHashingService
    {
        public HashingService(IServiceProvider service) : base(service) { }
        public AppActionResult Hashing(string accountId, double amount, bool isLoyaltyPoint)
        {
            IConfiguration config = new ConfigurationBuilder()
                           .SetBasePath(Directory.GetCurrentDirectory())
                           .AddJsonFile("appsettings.json", true, true)
                           .Build();
            string key = "";
            if (isLoyaltyPoint)
            {
                key = config["PaymentSecurity:LoyaltyPoint"];
            }
            else
            {
                key = config["PaymentSecurity:StoreCredit"];
            }
            return Hashing($"{accountId}_{amount}", key);
        }

        public AppActionResult UnHashing(string text, bool isLoyaltyPoint)
        {
            IConfiguration config = new ConfigurationBuilder()
                           .SetBasePath(Directory.GetCurrentDirectory())
                           .AddJsonFile("appsettings.json", true, true)
                           .Build();
            string key = "";
            if (isLoyaltyPoint)
            {
                key = config["PaymentSecurity:LoyaltyPoint"];
            }
            else
            {
                key = config["PaymentSecurity:StoreCredit"];
            }
            return DeHashing(text, key);

        }
        public AppActionResult Hashing(string password, string key)
        {
            AppActionResult result = new AppActionResult();
            try
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
                        result.Result = Base64UrlEncode(encryptedBytes);
                    }
                }
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }

        private string Base64UrlEncode(byte[] input)
        {
            // Standard Base64 encoding
            string base64 = Convert.ToBase64String(input);

            // Convert to Base64 URL-safe encoding
            return base64.Replace('+', '-').Replace('/', '_').Replace("=", "");
        }

        public AppActionResult DeHashing(string password, string key)
        {
            AppActionResult result = new AppActionResult();
            try
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
                        result.Result = sr.ReadToEnd(); // Return the decrypted string
                    }
                }
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }
    }
}