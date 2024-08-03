using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPRestaurent.BackEndCore.Domain.Enums
{
    public enum DishItemType
    {
        APPETIZER,
        SOUP,
        HOTPOT,
        BBQ,
        HOTPOT_BROTH,           // Nước lẩu
        HOTPOT_MEAT,     // Thịt cho lẩu
        HOTPOT_SEAFOOD,  // Hải sản cho lẩu
        HOTPOT_VEGGIE,   // Rau củ cho lẩu
        BBQ_MEAT,        // Thịt cho BBQ
        BBQ_SEAFOOD,    // Rau củ cho BBQ
        HOTPOT_TOPPING,  // Topping cho lẩu
        BBQ_TOPPING,     // Topping cho BBQ
        SIDEDISH,        // Món ăn kèm
        DRINK,           // Đồ uống
        DESSERT,         // Tráng miệng
        SAUCE            // Nước chấm
    }
}
