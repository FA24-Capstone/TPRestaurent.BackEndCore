﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPRestaurent.BackEndCore.Domain.Enums;
using TPRestaurent.BackEndCore.Domain.Models;

namespace TPRestaurent.BackEndCore.Common.DTO.Response
{
    public class OrderWithPaymentHistory
    {
        public Guid OrderId { get; set; }

        public DateTime OrderDate { get; set; }
        public DateTime? AssignedTime { get; set; }
        public DateTime? StartDeliveringTime { get; set; }
        public DateTime? DeliveredTime { get; set; }
        public DateTime? ReservationDate { get; set; }
        public DateTime? MealTime { get; set; }
        public DateTime? EndTime { get; set; }
        public DateTime? CancelledTime { get; set; }
        public double TotalAmount { get; set; }
        public double? CashReceived { get; set; }
        public double? ChangeReturned { get; set; }
        public OrderStatus StatusId { get; set; }

        public Domain.Models.EnumModels.OrderStatus? Status { get; set; }

        public string? AccountId { get; set; }

        public Account? Account { get; set; }

        public Guid? AddressId { get; set; }

        public CustomerInfoAddress? CustomerInfoAddress { get; set; }

        public Guid? LoyalPointsHistoryId { get; set; }

        public LoyalPointsHistory? LoyalPointsHistory { get; set; }

        public string? Note { get; set; }
        public Domain.Enums.OrderType OrderTypeId { get; set; }

        public Domain.Models.EnumModels.OrderType? OrderType { get; set; }

        public int? NumOfPeople { get; set; }
        public double? Deposit { get; set; }
        public bool? IsPrivate { get; set; }
        public string? ValidatingImg { get; set; }
        public string? ShipperId { get; set; }

        public Account? Shipper { get; set; }

        public string? TotalDistance { get; set; }
        public string? TotalDuration { get; set; }
        public string? CancelDeliveryReason { get; set; }
        public List<Transaction> PaymentHistories { get; set; } = new List<Transaction>();
    }
}
