using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public string? VerifyCode { get; set; }
        public bool IsDeleted { get; set; } = false;
        public int LoyaltyPoint { get; set; }
        public string? Avatar { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime RefreshTokenExpiryTime { get; set; }
        public bool IsManuallyCreated { get; set; }
    }
}
