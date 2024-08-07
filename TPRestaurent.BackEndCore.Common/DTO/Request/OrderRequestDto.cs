using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPRestaurent.BackEndCore.Domain.Enums;
using TPRestaurent.BackEndCore.Domain.Models;

namespace TPRestaurent.BackEndCore.Common.DTO.Request
{
    public class OrderRequestDto
    {
        public DateTime OrderDate { get; set; }
        public double TotalAmount { get; set; }
        public string? AccountId { get; set; }
        public Account? Account { get; set; } = null!;
        public Guid? CustomerId { get; set; }
        public CustomerInfo? CustomerInfo { get; set; }
        public PaymentMethod PaymentMethodId { get; set; }
        public Guid? ReservationId { get; set; }
        public Guid? LoyalPointsHistoryId { get; set; }
        public Guid? CustomerSavedCouponId { get; set; }
        public string? Note { get; set; }
        public bool? isDelivering { get; set; }  
    }

    public class OrderDetailsDto
    {
        public Guid OrderId { get; set; }
        public Guid? DishSizeDetailId { get; set; }
        public Guid? ComboId { get; set; }
        [ForeignKey(nameof(ComboId))]
        public int Quantity { get; set; }
        public double Price { get; set; }
        public string? Note { get; set; }    
    }
}
