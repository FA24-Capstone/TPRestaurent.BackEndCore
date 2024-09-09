using TPRestaurent.BackEndCore.Domain.Enums;

namespace TPRestaurent.BackEndCore.Common.DTO.Payment.PaymentRequest
{
    public class PaymentInformationRequest
    {
        public string? OrderID { get; set; }
        public string? StoreCreditID { get; set; }
        public string? TransactionID {  get; set; }
        public double Amount { get; set; }
        public string? AccountID { get; set; }
        public string CustomerName { get; set; }
        public Domain.Enums.PaymentMethod PaymentMethod { get; set; }
    }
}