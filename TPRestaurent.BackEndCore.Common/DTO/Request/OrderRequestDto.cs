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
        public Guid? CustomerId { get; set; }
        public OrderType OrderType { get; set; }        
        public string? Note { get; set; }
        public List<OrderDetailsDto> OrderDetailsDtos { get; set; } = new List<OrderDetailsDto>();
        public ReservationOrderDto? ReservationOrder { get; set; }   
        public DeliveryOrderDto? DeliveryOrder { get; set; }
        public MealWithoutReservation? MealWithoutReservation { get; set; } 

    }

    public class ReservationOrderDto
    {
        public int NumberOfPeople { get; set; }
        public DateTime MealTime { get; set; }
        public DateTime? EndTime { get; set; }
        public bool IsPrivate { get; set; }
        public double? Deposit { get; set; }

    }

    public class DeliveryOrderDto
    {
        public DateTime OrderTime { get; set; }  
        public DateTime DeliveryTime { get; set; }  
        public int LoyalPointToUse { get; set; }    
        public List<Guid>? CouponIds { get; set; }
        public PaymentMethod PaymentMethod { get; set; }    
    }

    public class MealWithoutReservation
    {
        public int NumberOfPeople { get; set; }
        public DateTime? MealTime { get; set; }
        public List<Guid>? TableIds { get; set; }

    }

    public class CustomerInfoRequest 
    {
        public string Name { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string Address { get; set; } = null!;
    }


    public class OrderDetailsDto
    {
        public Guid? DishSizeDetailId { get; set; }
        public ComboOrderDto? Combo { get; set; }
        public int Quantity { get; set; }
        public string? Note { get; set; }    
    }

    public class ComboOrderDto
    {
        public Guid ComboId { get; set; }
        public List<Guid> DishComboIds { get; set; } = new List<Guid>();
    }
}
