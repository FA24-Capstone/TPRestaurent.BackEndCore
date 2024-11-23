namespace TPRestaurent.BackEndCore.Common.DTO.Response
{
    public class CalculateOrderResponse
    {
        public double Amount { get; set; }
        public double PaidDeposit { get; set; }
        public double CouponDiscount { get; set; }
        public double LoyalPointUsed { get; set; }
        public double LoyalPointAdded { get; set; }
        public double FinalPrice { get; set; }
    }
}