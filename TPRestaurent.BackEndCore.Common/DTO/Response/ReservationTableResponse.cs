using System.ComponentModel.DataAnnotations.Schema;
using TPRestaurent.BackEndCore.Domain.Enums;
using TPRestaurent.BackEndCore.Domain.Models;

namespace TPRestaurent.BackEndCore.Common.DTO.Response
{
    public class ReservationTableResponse
    {
        public DateTime Date { get; set; }
        public List<ReservationTableItemResponse> Reservations { get; set; } = new List<ReservationTableItemResponse>();
    }

    public class ReservationTableItemResponse
    {
        public Guid OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime? ReservationDate { get; set; }
        public DateTime? AssignedTime { get; set; }
        public DateTime? StartDeliveringTime { get; set; }
        public DateTime? DeliveredTime { get; set; }
        public DateTime? MealTime { get; set; }
        public DateTime? EndTime { get; set; }
        public DateTime? CancelledTime { get; set; }
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
        public List<TableDetail> Tables { get; set; }
        public string? CancelDeliveryReason { get; set; }
    }
}