using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPRestaurent.BackEndCore.Domain.Enums;

namespace TPRestaurent.BackEndCore.Domain.Models
{
    public class Order
    {
        [Key]
        public Guid OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public double TotalAmount { get; set; }
        public  OrderStatus Status { get; set; } 
        public string? AccountId { get; set; }
        [ForeignKey(nameof(AccountId))]
        public Account? Account { get; set; } = null!;
        public Guid? CustomerId { get; set; } 
        [ForeignKey(nameof(CustomerId))]
        public CustomerInfo? CustomerInfo { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public Guid? LoyalPointsHistoryId { get; set; }
        [ForeignKey(nameof(LoyalPointsHistoryId))]
        public LoyalPointsHistory? LoyalPointsHistory { get; set; }
    }
}
