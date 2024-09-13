using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPRestaurent.BackEndCore.Domain.Models;

namespace TPRestaurent.BackEndCore.Common.DTO.Request
{
    public class DishChoice
    {
        public List<CartDishItem> items { get; set; } = new List<CartDishItem> { };
    }

    public class CartDishItem
    {
        public CartSizeDetail dish { get; set; }
        public CartDishSize size { get; set; }
        public int quantity { get; set; }
    }

    public class CartSizeDetail
    {
        public string dishId { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string image { get; set; }
        public int dishItemTypeId { get; set; }
        public CartDishItemType dishItemType { get; set; }
        public bool isAvailable { get; set; }
    }

    public class CartDishItemType
    {
        public int id { get; set; }
        public string name { get; set; }
        public string vietnameseName { get; set; }
    }

    public class CartDishSize
    {
        public string dishSizeDetailId { get; set; }
        public bool isAvailable { get; set; }
        public double price { get; set; }
        public double discount { get; set; }
        public CartSizeDetail dish { get; set; }
        public int dishSizeId { get; set; }
        public DishSizeDetails dishSize { get; set; }
    }

    public class DishSizeDetails
    {
        public int id { get; set; }
        public string name { get; set; }
        public string vietnameseName { get; set; }
    }
}
