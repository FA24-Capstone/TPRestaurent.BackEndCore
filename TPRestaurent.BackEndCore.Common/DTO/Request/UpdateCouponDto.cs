using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPRestaurent.BackEndCore.Common.DTO.Request
{
    public class UpdateCouponDto
    {
        public Guid CouponProgramId { get; set; }
        public string? Code { get; set; } 
        public int? DiscountPercent { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public double? MinimumAmount { get; set; }
        public int? Quantity { get; set; }
        public IFormFile? ImageFile { get; set; }
    }
}
