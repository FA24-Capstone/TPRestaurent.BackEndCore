using TPRestaurent.BackEndCore.Common.DTO.Payment.PaymentRequest;

namespace TPRestaurent.BackEndCore.Application.Contract.IServices
{
    public interface IPaymentGatewayService
    {
        Task<string> CreatePaymentUrlVnpay(PaymentInformationRequest request);
    }
}