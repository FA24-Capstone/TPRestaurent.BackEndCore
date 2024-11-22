namespace TPRestaurent.BackEndCore.Common.DTO.Request
{
    public class UpdateOrderDetailItemRequest
    {
        public Guid OrderDetailId { get; set; }
        public Guid? DishId { get; set; }
    }
}