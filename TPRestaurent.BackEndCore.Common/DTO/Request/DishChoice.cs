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
        public List<CartDishItem> Items { get; set; } = new List<CartDishItem> { };
    }

    public class CartDishItem
    {
        public CartSizeDetail Dish { get; set; }
        public CartDishSize Size { get; set; }
        public int Quantity { get; set; }
    }

    public class CartSizeDetail
    {
        public string DishId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Image { get; set; }
        public int DishItemTypeId { get; set; }
        public CartDishItemType DishItemType { get; set; }
        public bool IsAvailable { get; set; }
    }

    public class CartDishItemType
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string VietnameseName { get; set; }
    }

    public class CartDishSize
    {
        public string DishSizeDetailId { get; set; }
        public bool IsAvailable { get; set; }
        public double Price { get; set; }
        public double Discount { get; set; }
        public CartSizeDetail Dish { get; set; }
        public int DishSizeId { get; set; }
        public DishSizeDetails DishSize { get; set; }
    }

    public class DishSizeDetails
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string VietnameseName { get; set; }
    }
}
