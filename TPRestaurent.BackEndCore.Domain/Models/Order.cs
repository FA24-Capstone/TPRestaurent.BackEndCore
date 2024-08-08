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
        public OrderStatus Status { get; set; } 
        public Guid? CustomerId { get; set; } 
        [ForeignKey(nameof(CustomerId))]
        public CustomerInfo? CustomerInfo { get; set; }
        public Enums.PaymentMethod PaymentMethodId { get; set; }
        [ForeignKey(nameof(PaymentMethodId))]
        public EnumModels.PaymentMethod? PaymentMethod { get; set; }
        public Guid? ReservationId { get; set; }
        [ForeignKey(nameof(ReservationId))]
        public Reservation? Reservation { get; set; }
        public Guid? LoyalPointsHistoryId { get; set; }
        [ForeignKey(nameof(LoyalPointsHistoryId))]
        public LoyalPointsHistory? LoyalPointsHistory { get; set; }
        public Guid? CustomerSavedCouponId { get; set; }
        [ForeignKey(nameof(CustomerSavedCouponId))]
        public CustomerSavedCoupon? CustomerSavedCoupon { get; set; }
        public string? Note { get; set; }
        public bool? IsDelivering { get; set; }
        public Guid? TableId { get; set; }
        [ForeignKey(nameof(TableId))]
        public Table? Table { get; set; }       
    }
}
