using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPRestaurent.BackEndCore.Domain.Models;

namespace TPRestaurent.BackEndCore.Common.DTO.Response
{
    public class OrderSessionResponse
    {
        public OrderSession OrderSession { get; set; } = null!;
        public Order Order { get; set; } = null!;
        public List<OrderDetailResponse> OrderDetails { get; set; } = new List<OrderDetailResponse>();
    }
}
