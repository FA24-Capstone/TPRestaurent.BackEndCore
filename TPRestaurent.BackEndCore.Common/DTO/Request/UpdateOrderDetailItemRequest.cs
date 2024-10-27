using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPRestaurent.BackEndCore.Common.DTO.Request
{
    public class UpdateOrderDetailItemRequest
    {
        public Guid OrderDetailId { get; set; }
        public Guid? DishId { get; set; }
    }
}
