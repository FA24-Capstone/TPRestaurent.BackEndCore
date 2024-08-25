using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPRestaurent.BackEndCore.Domain.Models;

namespace TPRestaurent.BackEndCore.Common.DTO.Response
{
    public class OrderWithPaymentResponse
    {
        public Order Order { get; set; } = null!;
        public string PaymentLink { get; set; } = null!;
    }
}
