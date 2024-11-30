using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPRestaurent.BackEndCore.Domain.Enums;

namespace TPRestaurent.BackEndCore.Common.DTO.Request
{
    public class DuplicatedPaidOrderRefundRequest
    {
        public Guid OrderId { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
    }
}
