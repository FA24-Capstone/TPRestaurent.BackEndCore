namespace TPRestaurent.BackEndCore.Common.DTO.Request
{
    public class CalculateDepositRequest
    {
        public bool IsPrivate { get; set; }
        public int TableSize { get; set; }
        public List<CalculateDepositItemRequest> reservationDishDtos { get; set; } = new List<CalculateDepositItemRequest>();
    }

    public class CalculateDepositItemRequest
    {
        public Guid? DishSizeId { get; set; }
        public Guid? ComboId { get; set; }
        public int Quantity { get; set; }
    }
}