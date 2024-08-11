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
        public Guid? CustomerId { get; set; }
        public CustomerInfoRequest? CustomerInfo { get; set; }
        public PaymentMethod PaymentMethodId { get; set; }
        public Guid? ReservationId { get; set; }
        public Guid? LoyalPointsHistoryId { get; set; }
        public Guid? CouponId { get; set; }
        public string? Note { get; set; }
        public bool? isDelivering { get; set; }
        public List<OrderDetailsDto> OrderDetailsDtos { get; set; } = new List<OrderDetailsDto>();
        public int? LoyalPointsToUse { get; set; }
    }

    public class CustomerInfoRequest 
    {
        public string Name { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
    }


    public class OrderDetailsDto
    {
        public Guid? DishSizeDetailId { get; set; }
        public Guid? ComboId { get; set; }
        public int Quantity { get; set; }
        public string? Note { get; set; }    
    }
}
