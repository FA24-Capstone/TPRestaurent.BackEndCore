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
        public DateTime? AssignedTime { get; set; }
        public DateTime? StartDeliveringTime { get; set; }
        public DateTime? DeliveredTime { get; set; }
        public DateTime? ReservationDate { get; set; }   
        public DateTime? MealTime { get; set; }     
        public DateTime? EndTime { get; set; }      
        public double TotalAmount { get; set; }
        public OrderStatus StatusId { get; set; }
        [ForeignKey(nameof(StatusId))]
        public EnumModels.OrderStatus? Status { get; set; }  
        public string? AccountId { get; set; } 
        [ForeignKey(nameof(AccountId))]
        public Account? Account { get; set; }
        public Guid? LoyalPointsHistoryId { get; set; }
        [ForeignKey(nameof(LoyalPointsHistoryId))]
        public LoyalPointsHistory? LoyalPointsHistory { get; set; }
        public string? Note { get; set; }
        public Enums.OrderType OrderTypeId { get; set; }
        [ForeignKey(nameof(OrderTypeId))]
        public EnumModels.OrderType? OrderType { get; set; }
        public int? NumOfPeople { get; set; }   
        public double? Deposit { get; set; }
        public bool? IsPrivate { get; set; }
        public string? ValidatingImg { get; set; }
        public string? ShipperId { get; set; }
        [ForeignKey(nameof(ShipperId))]
        public Account? Shipper { get; set; }
    }
}
