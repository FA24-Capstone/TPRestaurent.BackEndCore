using TPRestaurent.BackEndCore.Common.DTO.Response;

namespace TPRestaurent.BackEndCore.Common.DTO.Request
{
    public class ComboChoice
    {
        public List<Item> items { get; set; }
        public double total { get; set; }
    }

    public class Item
    {
        public CartCombo combo { get; set; }
        public List<SelectedDish> selectedDishes { get; set; }
        public string note { get; set; }
        public int quantity { get; set; }
    }

    public class CartCombo
    {
        public Guid ComboId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Image { get; set; }
        public double Price { get; set; }
        public double Discount { get; set; }
        public int CategoryId { get; set; }
        public Category? Category { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsAvailable { get; set; }
        public bool IsDeleted { get; set; }
        public int AverageRating { get; set; }
        public int NumberOfRating { get; set; }
        public int? QuantityLeft { get; set; }
        public int DailyCountdown { get; set; }
    }

    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string VietnameseName { get; set; }
    }

    public class SelectedDish
    {
        public Guid DishComboId { get; set; }
        public int Quantity { get; set; }
        public bool IsAvailable { get; set; }
        public int? QuantityLeft { get; set; }
        public int DailyCountdown { get; set; }
        public Guid DishSizeDetailId { get; set; }
        public CartDishSizeDetail? DishSizeDetail { get; set; }
        public Guid ComboOptionSetId { get; set; }
        public CartComboOptionSet? ComboOptionSet { get; set; } // Adjust type if ComboOptionSet has properties
    }

    public class CartDishSizeDetail
    {
        public Guid DishSizeDetailId { get; set; }
        public bool IsAvailable { get; set; }
        public double Price { get; set; }
        public double Discount { get; set; }
        public Guid DishId { get; set; }
        public CartDish Dish { get; set; }
        public int DishSizeId { get; set; }
        public CartDishSize DishSize { get; set; }
        public int? QuantityLeft { get; set; }
        public int DailyCountdown { get; set; }
    }

    public class CartDish
    {
        public Guid DishId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Image { get; set; }
        public int DishItemTypeId { get; set; }
        public CartDishItemType? DishItemType { get; set; } // Adjust type if DishItemType has properties
        public bool IsAvailable { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsMainItem { get; set; }
        public int? PreparationTime { get; set; }
    }

    public class CartDishSize
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string VietnameseName { get; set; }
    }
}