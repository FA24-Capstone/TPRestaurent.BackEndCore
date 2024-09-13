using Castle.Core.Resource;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace TPRestaurent.BackEndCore.Domain.Models
{
    public class Account : IdentityUser
    {
        public override string? PhoneNumber { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public bool Gender { get; set; }
        public bool IsDeleted { get; set; } = false;
        public int LoyaltyPoint { get; set; }
        public bool IsVerified { get; set; } = false;
        public string? Avatar {  get; set; }
        public string? VerifyCode { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime RefreshTokenExpiryTime { get; set; }
        public Guid? CustomerId { get; set; }
        [ForeignKey(nameof(CustomerId))]
        public CustomerInfo? Customer { get; set; }
        public bool IsManuallyCreated { get; set; }   
    }
}