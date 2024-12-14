using TPRestaurent.BackEndCore.Domain.Enums;

namespace TPRestaurent.BackEndCore.Common.DTO.Request
{
    public class OrderPaymentRequestDto
    {
        public Guid OrderId { get; set; }
        public string? AccountId { get; set; }
        public double? CashReceived { get; set; }
        public double? ChangeReturned { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public List<Guid>? CouponIds { get; set; } = new List<Guid> { };
        public int? LoyalPointsToUse { get; set; }
        public bool? ChooseCashRefund { get; set; } = false;
        public string? returnUrl { get; set; } = "https://thienphurestaurant.vercel.app/payment";
    }
}