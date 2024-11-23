﻿using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TPRestaurent.BackEndCore.Domain.Models
{
    public class Token : IdentityUserToken<string>
    {
        public string AccessTokenValue { get; set; } = null!;
        public string DeviceIP { get; set; } = null!;
        public DateTime CreateDateAccessToken { get; set; }
        public DateTime ExpiryTimeAccessToken { get; set; }
        public string RefreshTokenValue { get; set; } = null!;
        public string? DeviceToken { get; set; }
        public string DeviceName { get; set; } = null!;
        public DateTime CreateRefreshToken { get; set; }
        public DateTime ExpiryTimeRefreshToken { get; set; }
        public DateTime LastLogin { get; set; }
        public bool IsActive { get; set; }
        [ForeignKey(nameof(UserId))]
        public Account? Account { get; set; }
    }
}