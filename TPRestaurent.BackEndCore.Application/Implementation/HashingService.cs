﻿using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using System.Text;
using TPRestaurent.BackEndCore.Application.Contract.IServices;
using TPRestaurent.BackEndCore.Common.DTO.Response.BaseDTO;
using TPRestaurent.BackEndCore.Domain.Models;

namespace TPRestaurent.BackEndCore.Application.Implementation
{
    public class HashingService : GenericBackendService, IHashingService
    {
        private IConfiguration _configuration;

        public HashingService(IServiceProvider service, IConfiguration configuration) : base(service)
        {
            _configuration = configuration;
        }
        public AppActionResult Hashing(string accountId, double amount, bool isLoyaltyPoint)
        {
          
            string key = "";
            if (isLoyaltyPoint)
            {
                key = _configuration["PaymentSecurity:LoyaltyPoint"];
            }
            else
            {
                key = _configuration["PaymentSecurity:StoreCredit"];
            }
            if (string.IsNullOrEmpty(accountId))
            {
                return Hashing($"_{amount}", key);

            }
            return Hashing($"{accountId}_{amount}", key);
        }

        public AppActionResult UnHashing(string text, bool isLoyaltyPoint)
        {
           
            string key = "";
            if (isLoyaltyPoint)
            {
                key = _configuration["PaymentSecurity:LoyaltyPoint"];
            }
            else
            {
                key = _configuration["PaymentSecurity:StoreCredit"];
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

        public Account GetDecodedAccount(Account account)
        {
            try
            {
                if (account == null)
                {
                    return null;
                }
                var storeCreditAmountResult = UnHashing(account.StoreCreditAmount, false).Result;
                if (storeCreditAmountResult != null)
                {
                    account.StoreCreditAmount = storeCreditAmountResult.ToString().Split('_')[1];
                } 

                var loyaltyPointResult = UnHashing(account.LoyaltyPoint, true).Result;
                if (loyaltyPointResult != null)
                {
                    account.LoyaltyPoint = loyaltyPointResult.ToString().Split('_')[1];
                }
            }
            catch (Exception ex)
            {
            }
            return account;
        }

        public Account GetCodedAccount(Account account)
        {
            try
            {
                if (account == null)
                {
                    return null;
                }
                var storeCreditAmountResult = Hashing(account.Id, double.Parse(account.StoreCreditAmount), false).Result;
                if (storeCreditAmountResult != null)
                {
                    account.StoreCreditAmount = storeCreditAmountResult.ToString();
                }

                var loyaltyPointResult = Hashing(account.Id, double.Parse(account.LoyaltyPoint), true).Result;
                if (loyaltyPointResult != null)
                {
                    account.LoyaltyPoint = loyaltyPointResult.ToString();
                }
            }
            catch (Exception ex)
            {
            }
            return account;
        }
    }
}