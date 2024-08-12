using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPRestaurent.BackEndCore.Domain.Models
{
    public class ComboOrderDetail
    {
        public Guid ComboOrderDetailId { get; set; }
        public Guid DishComboId { get; set; }
        [ForeignKey(nameof(DishComboId))]
        public DishCombo? DishCombo { get; set; }
        public Guid ReservationDishId { get; set; }
        [ForeignKey(nameof(ReservationDishId))]
        public ReservationDish? ReservationDish { get; set; }
        public Guid OrderDetailId { get; set; }
        [ForeignKey(nameof(OrderDetailId))]
        public OrderDetail? OrderDetail { get; set; }
    }
}
