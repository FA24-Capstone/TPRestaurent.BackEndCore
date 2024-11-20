using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPRestaurent.BackEndCore.Common.DTO.Request
{
    public class CouponProgramDto
    {
        public string Code { get; set; } = null!;
        public int DiscountPercent { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public double MinimumAmount { get; set; }
        public int Quantity { get; set; }
        public IFormFile File { get; set; } = null!;
        public string AccountId { get; set; }
    }
}
