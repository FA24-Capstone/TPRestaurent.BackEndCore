using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPRestaurent.BackEndCore.Domain.Enums;

namespace TPRestaurent.BackEndCore.Common.DTO.Request
{
    public class OrderPaymentRequestDto
    {
        public Guid OrderId { get; set; }
        public PaymentMethod PaymentMethodId { get; set; }
        public Guid? LoyalPointsHistoryId { get; set; }
        public Guid? CouponId { get; set; }
        public int? LoyalPointsToUse { get; set; }

    }
}
