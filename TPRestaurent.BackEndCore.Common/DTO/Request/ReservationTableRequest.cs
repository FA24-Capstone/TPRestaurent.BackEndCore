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
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public OrderStatus? Status { get; set; }
    }
}
