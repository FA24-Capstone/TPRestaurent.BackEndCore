using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPRestaurent.BackEndCore.Domain.Enums
{
    public enum DishItemType
    {
        APPETIZER = 1,
        SOUP = 2,
        HOTPOT = 3,
        BBQ = 4,
        HOTPOT_BROTH = 5,           // Nước lẩu
        HOTPOT_MEAT = 6,     // Thịt cho lẩu
        HOTPOT_SEAFOOD = 7,  // Hải sản cho lẩu
        HOTPOT_VEGGIE = 8,   // Rau củ cho lẩu
        BBQ_MEAT = 9,        // Thịt cho BBQ
        BBQ_SEAFOOD = 10,    // Rau củ cho BBQ
        HOTPOT_TOPPING = 11,  // Topping cho lẩu
        BBQ_TOPPING = 12,     // Topping cho BBQ
        SIDEDISH = 13,        // Món ăn kèm
        DRINK = 14,           // Đồ uống
        DESSERT = 15,         // Tráng miệng
        SAUCE = 16            // Nước chấm
    }
}
