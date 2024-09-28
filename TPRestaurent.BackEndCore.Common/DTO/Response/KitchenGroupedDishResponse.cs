using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPRestaurent.BackEndCore.Domain.Models;

namespace TPRestaurent.BackEndCore.Common.DTO.Response
{
    public class KitchenGroupedDishResponse
    {
        public List<KitchenGroupedDishItemResponse> MutualOrderDishes { get; set; } = new List<KitchenGroupedDishItemResponse> { };
        public List<KitchenGroupedDishItemResponse> SingleOrderDishes { get; set; } = new List<KitchenGroupedDishItemResponse> { };
    }

    public class KitchenGroupedDishItemResponse
    {
        public int Total { get; set; }
        public List<OrderDetailResponse> orderDetailResponses = new List<OrderDetailResponse>();
    }
}
