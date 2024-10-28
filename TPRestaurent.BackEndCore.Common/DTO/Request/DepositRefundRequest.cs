using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPRestaurent.BackEndCore.Domain.Enums;
using TPRestaurent.BackEndCore.Domain.Models;

namespace TPRestaurent.BackEndCore.Common.DTO.Request
{
    public class DepositRefundRequest
    {
        public Guid OrderId { get; set; }
        public Account? Account { get; set; }
        public double RefundAmount { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
    }
}
