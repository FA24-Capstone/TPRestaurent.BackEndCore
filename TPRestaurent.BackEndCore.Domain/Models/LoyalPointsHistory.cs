using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TPRestaurent.BackEndCore.Domain.Models
{
    public class LoyalPointsHistory
    {
        [Key]
        public Guid LoyalPointsHistoryId { get; set; }
        public DateTime TransactionDate { get; set; }
        public int PointChanged { get; set; }
        public int NewBalance { get; set; }
        public bool IsApplied { get; set; }
        public Guid? OrderId { get; set; }
        [ForeignKey(nameof(OrderId))]
        public Order? Order { get; set; }

    }
}
