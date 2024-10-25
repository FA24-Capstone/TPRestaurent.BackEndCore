using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPRestaurent.BackEndCore.Domain.Enums;

namespace TPRestaurent.BackEndCore.Common.DTO.Request
{
    public class ReservationTableRequest
    {
        public DateTime Date { get; set; }
        public OrderStatus? Status { get; set; }
        public OrderType Type { get; set; }
        public Guid? TableId { get; set; }
    }
}
