﻿using Castle.Core.Resource;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPRestaurent.BackEndCore.Domain.Models.EnumModels;

namespace TPRestaurent.BackEndCore.Domain.Models
{
    public class Transaction
    {
        [Key] 
        public Guid Id { get; set; }
        public double Amount { get; set; }
        public DateTime Date { get; set; }
        public Enums.PaymentMethod PaymentMethodId { get; set; }
        [ForeignKey(nameof(PaymentMethodId))]
        public PaymentMethod? PaymentMethod { get; set; }
        public Enums.TranscationStatus TranscationStatusId { get; set; }
        [ForeignKey(nameof(TranscationStatusId))]
        public EnumModels.TranscationStatus TranscationStatus { get; set; }
        public Guid? ReservationId { get; set; }
        [ForeignKey(nameof(ReservationId))]
        public Reservation? Reservation { get; set; }
        public Guid? OrderId { get; set; }
        [ForeignKey(nameof(OrderId))]
        public Order? Order { get; set; }
        public Guid? StoreCreditHistoryId { get; set; }
        [ForeignKey(nameof(StoreCreditHistoryId))]
        public StoreCreditHistory? StoreCreditHistory { get; set; }

    }
}
