namespace TPRestaurent.BackEndCore.Common.DTO.Payment.PaymentRequest
{
    public class PaymentInformationRequest
    {
        public string? OrderID { get; set; }
        public string? ReservationID { get; set; }
        public string AccountID { get; set; }
        public string CustomerName { get; set; }
        public double Amount { get; set; }
        public Domain.Enums.PaymentMethod PaymentMethod { get; set; }
    }
}