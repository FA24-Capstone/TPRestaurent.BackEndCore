using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPRestaurent.BackEndCore.Domain.Models.BaseModel;

namespace TPRestaurent.BackEndCore.Domain.Models
{
    public class CouponProgram : BaseEntity
    {
        [Key]
        public Guid CouponProgramId { get; set; }
        public string Code { get; set; } = null!;
        public int DiscountPercent { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public bool IsDeleted { get; set; }
        public double MinimumAmount { get; set; }
        public int Quantity { get; set; }
        public string? Img { get; set; }
        public Enums.UserRange? UserRangeId { get; set; }
        [ForeignKey(nameof(UserRangeId))]
        public EnumModels.UserRange? UserRange { get; set; }
        public Enums.CouponProgramType CouponProgramTypeId { get; set; }
        [ForeignKey(nameof(CouponProgramTypeId))]
        public EnumModels.CouponProgramType? CouponProgramType { get; set; }
    }
}
