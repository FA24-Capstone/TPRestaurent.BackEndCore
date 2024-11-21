using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPRestaurent.BackEndCore.Domain.Enums;
using TPRestaurent.BackEndCore.Domain.Models.EnumModels;

namespace TPRestaurent.BackEndCore.Common.DTO.Request
{
    public class CouponProgramDto
    {
        public string Code { get; set; } = null!;
        public string Title { get; set;} = null!;
        public Domain.Enums.CouponProgramType CouponProgramType { get; set; }    
        public Domain.Enums.UserRank UserRank { get; set; }     
        public int DiscountPercent { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public double MinimumAmount { get; set; }
        public int Quantity { get; set; }
        public string File { get; set; } = null!;
        public string AccountId { get; set; }
    }
}
