using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPRestaurent.BackEndCore.Common.DTO.Request
{
    public class AssignCouponRequestDto
    {
        public Guid CouponProgramId { get; set; }
        public List<string> CustomerIds { get; set; } = new List<string>();
    }
}
