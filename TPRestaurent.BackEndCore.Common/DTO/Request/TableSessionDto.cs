using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPRestaurent.BackEndCore.Domain.Models;

namespace TPRestaurent.BackEndCore.Common.DTO.Request
{
    public class TableSessionDto
    {
        public Guid TableId { get; set; }
        public DateTime StartTime { get; set; }
        public Guid? ReservationId { get; set; }
        public List<PrelistOrderDto> PrelistOrderDtos { get; set; } = new List<PrelistOrderDto>();
    }

    public class PrelistOrderDto
    {
        public int Quantity { get; set; }
        public DateTime OrderTime { get; set; }
        public Guid? ReservationDishId { get; set; }
        public Guid? DishSizeDetailId { get; set; }
        public Guid? ComboId { get; set; }
        public Guid? TableSessionId { get; set; }
        public List<Guid>? DichComboIds { get; set; } = new List<Guid> { };
    }
}
