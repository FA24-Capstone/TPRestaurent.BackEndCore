using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPRestaurent.BackEndCore.Domain.Models
{
    public class AssignedCoupon
    {
        [Key]
        public Guid AssignedCouponId { get; set; }  
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
