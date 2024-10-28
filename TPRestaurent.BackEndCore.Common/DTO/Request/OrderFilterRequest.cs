using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPRestaurent.BackEndCore.Domain.Enums;

namespace TPRestaurent.BackEndCore.Common.DTO.Request
{
    public class OrderFilterRequest
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public OrderStatus? Status { get; set; }
        public OrderType Type { get; set; }
        public int pageNumber { get; set; } = 0;
        public int pageSize { get; set; } = 0;
    }
}
