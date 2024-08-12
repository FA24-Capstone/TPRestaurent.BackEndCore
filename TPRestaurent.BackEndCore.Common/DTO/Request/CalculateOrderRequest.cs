using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPRestaurent.BackEndCore.Domain.Enums;

namespace TPRestaurent.BackEndCore.Common.DTO.Request
{
    public class CalculateOrderRequest
    {
        public double Total {  get; set; }
        public Guid? CustomerId { get; set; }
        public Guid? ReservationId { get; set; }
        public Guid? LoyalPointsHistoryId { get; set; }
        public Guid? CouponId { get; set; }
        public int? LoyalPointsToUse { get; set; }
    }
}
