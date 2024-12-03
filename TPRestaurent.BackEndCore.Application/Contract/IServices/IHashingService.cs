using TPRestaurent.BackEndCore.Common.DTO.Response.BaseDTO;
using TPRestaurent.BackEndCore.Domain.Models;

namespace TPRestaurent.BackEndCore.Application.Contract.IServices
{
    public interface IHashingService
    {
        public AppActionResult Hashing(string value, string key);

        public AppActionResult DeHashing(string value, string key);
        public AppActionResult Hashing(string accountId, double amount, bool isLoyaltyPoint);
        public AppActionResult UnHashing(string text, bool isLoyaltyPoint);
        public Account GetDecodedAccount(Account account);
    }
}