using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPRestaurent.BackEndCore.Domain.Models
{
    public class OrderAppliedCoupon
    {
        public Guid OrderAppliedCouponId { get; set; }  
        public Guid OrderId { get; set; }
        [ForeignKey(nameof(OrderId))]
        public Order? Order { get; set; }    
        public Guid CouponProgramId { get; set; }
        [ForeignKey(nameof(CouponProgramId))]
        public CouponProgram? CouponProgram { get; set; } = null;
    }
}
