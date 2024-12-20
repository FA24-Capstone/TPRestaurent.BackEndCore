﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TPRestaurent.BackEndCore.Domain.Models
{
    public class LoyalPointsHistory
    {
        [Key]
        public Guid LoyalPointsHistoryId { get; set; }

        public DateTime TransactionDate { get; set; }
        public string PointChanged { get; set; }
        public string NewBalance { get; set; }
        public bool IsApplied { get; set; }
        public Guid? OrderId { get; set; }

        [ForeignKey(nameof(OrderId))]
        public Order? Order { get; set; }
    }
}