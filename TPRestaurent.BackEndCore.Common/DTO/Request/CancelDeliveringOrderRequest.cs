using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPRestaurent.BackEndCore.Common.DTO.Request
{
    public class CancelDeliveringOrderRequest
    {
        public Guid OrderId { get; set; }
        public string? CancelledReasons { get; set; }
        public string? ShipperRequestId { get; set; }  
        public bool? isCancelledByAdmin { get; set; }
    }
}
