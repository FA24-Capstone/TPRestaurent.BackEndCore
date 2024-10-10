using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPRestaurent.BackEndCore.Domain.Enums;
using TPRestaurent.BackEndCore.Domain.Models;

namespace TPRestaurent.BackEndCore.Common.DTO.Response
{
    public class KitchenGroupedDishResponse
    {
        public List<KitchenGroupedDishItemResponse> MutualOrderDishes { get; set; } = new List<KitchenGroupedDishItemResponse> ();
        public List<KitchenGroupedDishItemResponse> SingleOrderDishes { get; set; } = new List<KitchenGroupedDishItemResponse> ();
    }

    public class KitchenGroupedDishItemResponse
    {
        public List<QuantityBySize> Total { get; set; } = new List<QuantityBySize> ();
        public Dish Dish { get; set; }
        public List<DishFromTableOrder> DishFromTableOrders { get; set; } = new List<DishFromTableOrder>();
    }

    public class DishFromTableOrder
    {
        public Table? Table { get; set; }
        public Order Order { get; set; }
        public OrderSession OrderSession { get; set; }
        public QuantityBySize Quantity { get; set; }
    }

    public class QuantityBySize
    {
        public Domain.Models.EnumModels.DishSize DishSize { get; set; }
        public int Quantity { get; set; }
    }
}
