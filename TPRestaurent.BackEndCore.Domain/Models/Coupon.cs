using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPRestaurent.BackEndCore.Domain.Models
{
    public class Coupon
    {
        [Key]
        public Guid CouponId { get; set; }
        public string Code { get; set; } = null!;
        public int DiscountPercent { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public double MinimumAmount { get; set; }
        public int Quantity { get; set; }

    }
}
