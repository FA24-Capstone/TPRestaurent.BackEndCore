using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPRestaurent.BackEndCore.Common.DTO.Request
{
    public class AssignCouponToRankRequest
    {
        public List<Guid>? BronzeCouponProgramIds { get; set; } = new List<Guid>();
        public List<Guid>? SilverCouponProgramIds { get; set; } = new List<Guid>();
        public List<Guid>? GoldCouponProgramIds { get; set; } = new List<Guid>();
        public List<Guid>? DiamondCouponProgramIds { get; set; } = new List<Guid>();
    }
}
