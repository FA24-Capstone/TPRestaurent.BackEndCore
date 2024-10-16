using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPRestaurent.BackEndCore.Domain.Enums;
using TPRestaurent.BackEndCore.Domain.Models;

namespace TPRestaurent.BackEndCore.Common.DTO.Response
{
    public class ReservationReponse
    {
        public OrderResponse Order { get; set; }    
        public List<OrderDishDto> OrderDishes { get; set; } = new List<OrderDishDto>();
        public List<TableDetail> OrderTables { get; set; } = new List<TableDetail>();
    }

    public class OrderResponse
    {
        public Guid OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime? DepositPaidDate   { get; set; }
        public DateTime? OrderPaidDate   { get; set; }
        public DateTime? StartDeliveringTime { get; set; }
        public DateTime? DeliveredTime { get; set; }
        public DateTime? ReservationDate { get; set; }
        public DateTime? MealTime { get; set; }
        public DateTime? EndTime { get; set; }
        public double TotalAmount { get; set; }
        public OrderStatus StatusId { get; set; }
        [ForeignKey(nameof(StatusId))]
        public Domain.Models.EnumModels.OrderStatus? Status { get; set; }
        public string? AccountId { get; set; }
        [ForeignKey(nameof(AccountId))]
        public Account? Account { get; set; }
        public Guid? LoyalPointsHistoryId { get; set; }
        [ForeignKey(nameof(LoyalPointsHistoryId))]
        public LoyalPointsHistory? LoyalPointsHistory { get; set; }
        public string? Note { get; set; }
        public Domain.Enums.OrderType OrderTypeId { get; set; }
        [ForeignKey(nameof(OrderTypeId))]
        public Domain.Models.EnumModels.OrderType? OrderType { get; set; }
        public int? NumOfPeople { get; set; }
        public double? Deposit { get; set; }
        public bool? IsPrivate { get; set; }
        public Transaction? Transaction { get; set; }
        public string? ValidatingImg { get; set; }
        public Account? Shipper { get; set; }
        public string? TotalDistance { get; set; }
        public string? TotalDuration { get; set; }

    }

    public class OrderDishDto
    {
        public Guid OrderDetailsId { get; set; }
        public int Quantity { get; set; }   
        public Guid? DishSizeDetailId { get; set; }
        public DishSizeDetail? DishSizeDetail { get; set; }
        public ComboDishDto? ComboDish { get; set; }
        public string Note { get; set; }    
        public DateTime OrderTime { get; set; }   
        public Domain.Enums.OrderDetailStatus StatusId { get; set; }
        public Domain.Models.EnumModels.OrderDetailStatus Status { get; set; }

    }

    public class ComboDishDto
    {
        public Guid? ComboId { get; set; }
        [ForeignKey(nameof(ComboId))]
        public Combo? Combo { get; set; }
        public List<DishCombo> DishCombos { get; set; } = new List<DishCombo> { };
    }
}
