using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TPRestaurent.BackEndCore.Domain.Models
{
    public class OrderSession
    {
        [Key]
        public Guid OrderSessionId { get; set; }

        public double PreparationTime { get; set; }
        public DateTime OrderSessionTime { get; set; }
        public DateTime? StartProcessingTime { get; set; }
        public DateTime? ReadyToServeTime { get; set; }
        public DateTime? CancelTime { get; set; }
        public int OrderSessionNumber { get; set; }
        public Enums.OrderSessionStatus OrderSessionStatusId { get; set; }

        [ForeignKey(nameof(OrderSessionStatusId))]
        public EnumModels.OrderSessionStatus? OrderSessionStatus { get; set; }
    }
}