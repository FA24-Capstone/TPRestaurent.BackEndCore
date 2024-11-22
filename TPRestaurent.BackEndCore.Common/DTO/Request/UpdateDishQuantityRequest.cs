namespace TPRestaurent.BackEndCore.Common.DTO.Request
{
    public class UpdateDishQuantityRequest
    {
        public Guid DishSizeDetailId { get; set; }
        public int? QuantityLeft { get; set; }
        public int? DailyCountdown { get; set; }
    }
}