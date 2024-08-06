using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPRestaurent.BackEndCore.Domain.Models
{
    public class CustomerSavedCoupon
    {
        [Key] 
        public Guid CustomerSavedCouponId { get; set; }
        public bool IsUsedOrExpired { get; set; }
        public Guid CustomerInfoId { get; set; }
        [ForeignKey(nameof(CustomerInfoId))]
        public CustomerInfo? CustomerInfo { get; set; }
        public Guid CouponId { get; set; }
        [ForeignKey(nameof(CouponId))]
        public Coupon? Coupon { get; set; }

    }
}
