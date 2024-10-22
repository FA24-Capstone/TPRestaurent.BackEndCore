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
        public List<Guid> OrderDetailIds { get; set; } = new List<Guid> ();
    }

    public class KitchenGroupedDishItemResponse
    {
        public List<QuantityBySize> Total { get; set; } = new List<QuantityBySize> ();
        public Dish Dish { get; set; }
        public List<DishFromTableOrder> UncheckedDishFromTableOrders { get; set; } = new List<DishFromTableOrder>();
        public List<DishFromTableOrder> ProcessingDishFromTableOrders { get; set; } = new List<DishFromTableOrder>();
    }

    public class DishFromTableOrder
    {
        public OrderDetail OrderDetail { get; set; }
        public Table? Table { get; set; }
        public Order Order { get; set; }
        public OrderSession OrderSession { get; set; }
        public OrderDetailQuantityBySize Quantity { get; set; }
    }

    public class QuantityBySize
    {
        public Domain.Models.EnumModels.DishSize DishSize { get; set; }
        public OrderDetailStatus OrderDetailStatus { get; set; }
        public int UncheckedQuantity { get; set; }
        public int ProcessingQuantity { get; set; }
    }

    public class OrderDetailQuantityBySize
    {
        public Domain.Models.EnumModels.DishSize DishSize { get; set; }
        public OrderDetailStatus OrderDetailStatus { get; set; }
        public int Quantity { get; set; }
    }
}
