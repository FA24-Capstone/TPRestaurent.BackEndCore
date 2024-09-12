using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPRestaurent.BackEndCore.Domain.Enums;

namespace TPRestaurent.BackEndCore.Common.DTO.Request
{
    public class PaymentRequestDto
    {
        public Guid? OrderId { get; set; }   
        public Guid? StoreCreditId { get; set; }
        public double? StoreCreditAmount { get; set; }
        public Domain.Enums.PaymentMethod PaymentMethod { get; set; } 

    }
}
