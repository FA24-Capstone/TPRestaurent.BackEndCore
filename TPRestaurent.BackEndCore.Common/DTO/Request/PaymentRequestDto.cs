﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPRestaurent.BackEndCore.Common.DTO.Request
{
    public class PaymentRequestDto
    {
        public Guid? OrderId { get; set; }   
        public Guid? ReservationId { get; set; }
        public Guid? StoreCreditId { get; set; }
        public double? StoreCreditAmount { get; set; }
        public Domain.Enums.PaymentMethod PaymentMethod { get; set; } 

    }
}
