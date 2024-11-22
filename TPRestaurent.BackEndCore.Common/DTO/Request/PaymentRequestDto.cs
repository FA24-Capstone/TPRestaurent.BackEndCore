namespace TPRestaurent.BackEndCore.Common.DTO.Request
{
    public class PaymentRequestDto
    {
        public Guid? OrderId { get; set; }
        public string? AccountId { get; set; }
        public double? StoreCreditAmount { get; set; }
        public Domain.Enums.PaymentMethod PaymentMethod { get; set; }
    }
}