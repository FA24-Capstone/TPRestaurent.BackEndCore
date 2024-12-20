﻿using Microsoft.AspNetCore.Identity;
using TPRestaurent.BackEndCore.Domain.Models;

namespace TPRestaurent.BackEndCore.Common.DTO.Response
{
    public class AccountResponse
    {
        public string Id { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public bool Gender { get; set; }
        public DateTime DOB { get; set; }
        public bool IsVerified { get; set; }
        public string UserName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? Avatar { get; set; }
        public List<CustomerInfoAddress> Addresses { get; set; } = new List<CustomerInfoAddress>();
        public string LoyalPoint { get; set; }
        public string Amount { get; set; }
        public bool IsDeleted { get; set; } = false;
        public bool IsDelivering { get; set; }
        public DateTime StoreCreditExpireDay { get; set; }
        public bool? IsManuallyCreated { get; set; }
        public List<IdentityRole> Roles { get; set; }
        public string MainRole { get; set; }
        public bool IsBanned { get; set; }
    }
}