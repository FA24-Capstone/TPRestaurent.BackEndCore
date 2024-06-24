﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPRestaurent.BackEndCore.Domain.Models
{
    public class Token
    {
        [Key]
        public Guid TokenId { get; set; }
        public string AccessTokenValue { get; set; } = null!;
        public DateTime CreateDateAccessToken { get; set; }
        public DateTime ExpiryTimeAccessToken { get; set; }
        public string RefreshTokenValue { get; set; } = null!;
        public DateTime CreateRefreshToken { get; set; }
        public DateTime ExpiryTimeRefreshToken { get; set; }
        public string AccountId { get; set; } = null!;
        [ForeignKey(nameof(AccountId))]
        public Account? Account { get; set; }
    }
}
