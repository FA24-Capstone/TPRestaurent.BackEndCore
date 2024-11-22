namespace TPRestaurent.BackEndCore.Common.DTO.Request
{
    public class CalculateOrderRequest
    {
        public double Total { get; set; }
        public Guid? CustomerId { get; set; }
        public Guid? ReservationId { get; set; }
        public Guid? LoyalPointsHistoryId { get; set; }
        public Guid? CouponId { get; set; }
        public int? LoyalPointsToUse { get; set; }
    }
}