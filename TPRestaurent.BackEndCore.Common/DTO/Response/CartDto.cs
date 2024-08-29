using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPRestaurent.BackEndCore.Common.DTO.Response
{
    public class CartCombo
    {
        public Guid ComboId { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public Dictionary<int, List<SelectedDish>> SelectedDishes { get; set; }
        public int Quantity { get; set; }
    }

    public class SelectedDish
    {
        public Guid DishComboId { get; set; }
        public int Quantity { get; set; }
        public Guid DishSizeDetailId { get; set; }
        public CartDishSizeDetail DishSizeDetail { get; set; }
        public Guid ComboOptionSetId { get; set; }
        public CartComboOptionSet ComboOptionSet { get; set; }
    }

    public class CartDishSizeDetail
    {
        public Guid DishSizeDetailId { get; set; }
        public bool IsAvailable { get; set; }
        public decimal Price { get; set; }
        public decimal Discount { get; set; }
        public Guid DishId { get; set; }
        public CartDish Dish { get; set; }
        public int DishSizeId { get; set; }
        public string DishSize { get; set; }
    }

    public class CartDish
    {
        public Guid DishId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Image { get; set; }
        public int DishItemTypeId { get; set; }
        public string DishItemType { get; set; }
        public bool IsAvailable { get; set; }
    }

    public class CartComboOptionSet
    {
        public Guid ComboOptionSetId { get; set; }
        public int OptionSetNumber { get; set; }
        public int NumOfChoice { get; set; }
        public int DishItemTypeId { get; set; }
        public string DishItemType { get; set; }
        public Guid ComboId { get; set; }
        public string Combo { get; set; }
    }

    public class CartDto
    {
        public List<CartCombo> Items { get; set; }
        public decimal Total { get; set; }
    }
}
