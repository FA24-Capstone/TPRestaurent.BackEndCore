using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPRestaurent.BackEndCore.Common.DTO.Request
{
    public class ReservationDishDto
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
