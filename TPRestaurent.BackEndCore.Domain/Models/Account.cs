using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace TPRestaurent.BackEndCore.Domain.Models
{
    public class Account : IdentityUser
    {
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public DateTime? DOB { get; set; }
        public bool? Gender { get; set; }
        public string? Address { get; set; } = null!;
        public bool IsVerified { get; set; }
        public bool IsDeleted { get; set; } = false;
        public int LoyaltyPoint { get; set; }
        public string? Avatar { get; set; }
        public bool IsManuallyCreated { get; set; }
        public bool IsDelivering { get; set; }
        public double StoreCreditAmount { get; set; }
        public DateTime ExpiredDate { get; set; }
        public DateTime RegisteredDate { get; set; }
        public bool IsBanned { get; set; }
        public Enums.UserRank? UserRankId { get; set; }

        [ForeignKey(nameof(UserRankId))]
        public EnumModels.UserRank? UserRank { get; set; }
    }
}