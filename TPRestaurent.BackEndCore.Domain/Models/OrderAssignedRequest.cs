using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TPRestaurent.BackEndCore.Domain.Enums;

namespace TPRestaurent.BackEndCore.Domain.Models
{
    public class OrderAssignedRequest
    {
        [Key]
        public Guid OrderAssignedRequestId { get; set; }

        public Guid OrderId { get; set; }

        [ForeignKey(nameof(OrderId))]
        public Order? Order { get; set; }

        public DateTime? RequestTime { get; set; }
        public DateTime? AssignedTime { get; set; }
        public string? ShipperRequestId { get; set; }

        [ForeignKey(nameof(ShipperRequestId))]
        public Account? ShipperRequest { get; set; }

        public string? ShipperAssignedId { get; set; }

        [ForeignKey(nameof(ShipperAssignedId))]
        public Account? ShipperAssigned { get; set; }

        public OrderAssignedStatus StatusId { get; set; }

        [ForeignKey(nameof(StatusId))]
        public EnumModels.OrderAssignedStatus? Status { get; set; }

        public string? Reasons { get; set; }
    }
}