﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TPRestaurent.BackEndCore.Domain.Models
{
    public class Coupon
    {
        [Key]
        public Guid CouponId { get; set; }

        public bool IsUsedOrExpired { get; set; }
        public Guid? OrderId { get; set; }

        [ForeignKey(nameof(OrderId))]
        public Order? Order { get; set; }

        public string AccountId { get; set; }

        [ForeignKey(nameof(AccountId))]
        public Account? Account { get; set; } = null;

        public Guid CouponProgramId { get; set; }

        [ForeignKey(nameof(CouponProgramId))]
        public CouponProgram? CouponProgram { get; set; } = null;
    }
}