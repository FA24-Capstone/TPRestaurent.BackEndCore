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
        public List<CartItem> items { get; set; } = new List<CartItem>();
        public double total { get; set; } // Assuming total represents the subtotal of the cart item
    }

    public class CartItem
    {
        public string comboId { get; set; }
        public string name { get; set; }
        public double price { get; set; }
        public int quantity { get; set; }
        public Dictionary<string, List<CartSelectedDish>> selectedDishes { get; set; } = new Dictionary<string, List<CartSelectedDish>>();
    }

    public class CartSelectedDish
    {
        public string dishComboId { get; set; }
        public int quantity { get; set; }
        public CartDishSizeDetail dishSizeDetail { get; set; }
        public string comboOptionSetId { get; set; } // Consider using a reference to the ComboOptionSet class if applicable
    }

    public class CartDishSizeDetail
    {
        public string dishSizeDetailId { get; set; }
        public bool isAvailable { get; set; }
        public double price { get; set; }
        public double discount { get; set; }
        public CartDish dish { get; set; }
        public int dishSizeId { get; set; } // Consider using a reference to the DishSize class if applicable
        public DishSize dishSize { get; set; } // Include only if you have separate data for dish sizes
    }

    public class CartDish
    {
        public string dishId { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string image { get; set; }
        public int dishItemTypeId { get; set; } // Consider using a reference to the DishItemType class if applicable
        public string dishItemType { get; set; } // Include only if DishItemType has separate data
        public bool isAvailable { get; set; }
    }

}
