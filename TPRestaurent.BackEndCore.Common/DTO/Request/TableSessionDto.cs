namespace TPRestaurent.BackEndCore.Common.DTO.Request
{
    public class TableSessionDto
    {
        public Guid TableId { get; set; }
        public DateTime StartTime { get; set; }
        public Guid? ReservationId { get; set; }
        public List<PrelistOrderItemDto>? PrelistOrderDtos { get; set; } = new List<PrelistOrderItemDto>();
    }

    public class PrelistOrderDto
    {
        public Guid TableSessionId { get; set; }
        public DateTime OrderTime { get; set; }
        public List<PrelistOrderItemDto>? PrelistOrderDtos { get; set; } = new List<PrelistOrderItemDto>();
    }

    public class PrelistOrderItemDto
    {
        public int Quantity { get; set; }
        public Guid? ReservationDishId { get; set; }
        public Guid? DishSizeDetailId { get; set; }
        public ComboOrderDto? Combo { get; set; }
    }
}