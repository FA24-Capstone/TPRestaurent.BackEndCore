using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPRestaurent.BackEndCore.Domain.Models
{
    public class PrelistOrder
    {
        [Key]
        public Guid PrelistOrderId { get; set; }
        public int Quantity { get; set; }
        public DateTime OrderTime { get; set; }
        public DateTime? ReadyToServeTime { get; set; }
        public Guid? ReservationDishId { get; set; }
        [ForeignKey(nameof(ReservationDishId))]
        public ReservationDish? ReservationDish { get; set; }
        public Guid? DishSizeDetailId { get; set; }
        [ForeignKey(nameof(DishSizeDetailId))]
        public DishSizeDetail? DishSizeDetail { get; set; }
        public Guid? ComboId { get; set; }
        [ForeignKey(nameof(ComboId))]
        public Combo? Combo { get; set; }
        public Guid? TableSessionId { get; set; }
        [ForeignKey(nameof(TableSessionId))]
        public TableSession? TableSession { get; set; } 
    }
}
