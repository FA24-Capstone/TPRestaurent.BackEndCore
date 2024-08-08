using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPRestaurent.BackEndCore.Common.DTO.Request
{
    public class AddDishToOrderRequestDto
    {
        public Guid OrderId { get; set; }
        public List<OrderDetailsDto> OrderDetailsDtos { get; set; } = new List<OrderDetailsDto>();
    }
}
