using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TPRestaurent.BackEndCore.Domain.Models
{
    public class TableDetail
    {
        [Key]
        public Guid TableDetailId { get; set; }

        public Guid TableId { get; set; }

        [ForeignKey(nameof(TableId))]
        public Table? Table { get; set; }

        public Guid OrderId { get; set; }

        [ForeignKey(nameof(OrderId))]
        public Order? Order { get; set; }

        public DateTime StartTime { get; set; }
        public DateTime? EndDate { get; set; }
    }
}