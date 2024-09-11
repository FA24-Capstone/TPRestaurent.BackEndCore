using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPRestaurent.BackEndCore.Common.DTO.Response;
using TPRestaurent.BackEndCore.Domain.Models.EnumModels;

namespace TPRestaurent.BackEndCore.Common.DTO.Request
{
    public class ComboChoice
    {
        public List<CartItem> Items { get; set; } = new List<CartItem>();
    }
    public class CartItem
    {
        public string ComboId { get; set; }
        public string Name { get; set; }
        public double Price { get; set; }
        public int Quantity { get; set; }
        public double Total { get; set; } // Assuming Total represents the subtotal of the cart item
        public Dictionary<string, List<CartSelectedDish>> SelectedDishes { get; set; } = new Dictionary<string, List<CartSelectedDish>>();
    }
    public class CartSelectedDish
    {
        public string DishComboId { get; set; }
        public int Quantity { get; set; }

        public CartDishSizeDetail DishSizeDetail { get; set; }
        public string ComboOptionSetId { get; set; } // Consider using a reference to the ComboOptionSet class if applicable
    }
    public class CartDishSizeDetail
    {
        public string DishSizeDetailId { get; set; }
        public bool IsAvailable { get; set; }
        public double Price { get; set; }
        public double Discount { get; set; }
        public CartDish Dish { get; set; }
        public int DishSizeId { get; set; } // Consider using a reference to the DishSize class if applicable
        public DishSize DishSize { get; set; } // Include only if you have separate data for dish sizes
    }
    public class CartDish
    {
        public string DishId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Image { get; set; }
        public int DishItemTypeId { get; set; } // Consider using a reference to the DishItemType class if applicable
        public string DishItemType { get; set; } // Include only if DishItemType has separate data
        public bool IsAvailable { get; set; }
    }

}
