using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPRestaurent.BackEndCore.Application.Contract.IServices;
using TPRestaurent.BackEndCore.Common.DTO.Payment.PaymentLibrary;
using TPRestaurent.BackEndCore.Common.DTO.Payment.PaymentRequest;

namespace TPRestaurent.BackEndCore.Application.Implementation
{
    public class PaymentGatewayService : IPaymentGatewayService
    {
        private readonly IConfiguration _configuration;
        public PaymentGatewayService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public async Task<string> CreatePaymentUrlVnpay(PaymentInformationRequest requestDto, HttpContext httpContext)
        {
            var paymentUrl = "";
            var momo = new PaymentInformationRequest
            {
                AccountID = requestDto.AccountID,
                Amount = requestDto.Amount,
                CustomerName = requestDto.CustomerName,
                OrderID = requestDto.OrderID
            };
            var timeZoneById = TimeZoneInfo.FindSystemTimeZoneById(_configuration["TimeZoneId"]);
            var timeNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZoneById);
            var pay = new VNPayLibrary();
            string urlCallBack = "";
            if (!string.IsNullOrEmpty(requestDto.OrderID))
            {
                urlCallBack = $"{_configuration["Vnpay:ReturnUrl"]}/{requestDto.OrderID}";
            }
            else
            {
                urlCallBack = $"{_configuration["Vnpay:ReturnUrl"]}/{requestDto.ReservationID}";
            }

            pay.AddRequestData("vnp_Version", _configuration["Vnpay:Version"]);
            pay.AddRequestData("vnp_Command", _configuration["Vnpay:Command"]);
            pay.AddRequestData("vnp_TmnCode", _configuration["Vnpay:TmnCode"]);
            pay.AddRequestData("vnp_Amount", ((int)requestDto.Amount * 100).ToString());
            pay.AddRequestData("vnp_CreateDate", timeNow.ToString("yyyyMMddHHmmss"));
            pay.AddRequestData("vnp_CurrCode", _configuration["Vnpay:CurrCode"]);
            pay.AddRequestData("vnp_IpAddr", pay.GetIpAddress(httpContext));
            pay.AddRequestData("vnp_Locale", _configuration["Vnpay:Locale"]);
            if (!string.IsNullOrEmpty(requestDto.OrderID))
            {
                pay.AddRequestData("vnp_OrderInfo",
                $"OR");
            } else if (!string.IsNullOrEmpty(requestDto.ReservationID))
            {
                pay.AddRequestData("vnp_OrderInfo",
                $"RE");
            } else
            {
                pay.AddRequestData("vnp_OrderInfo",
                $"CR_{requestDto.TransactionID}");
            }
            pay.AddRequestData("vnp_OrderType", "other");
            pay.AddRequestData("vnp_ReturnUrl", urlCallBack);
            if (!string.IsNullOrEmpty(requestDto.OrderID))
            {
                pay.AddRequestData("vnp_TxnRef", requestDto.OrderID);
            }
            else if (!string.IsNullOrEmpty(requestDto.ReservationID))
            {
                pay.AddRequestData("vnp_TxnRef", requestDto.ReservationID);
            }
            else
            {
                pay.AddRequestData("vnp_TxnRef", requestDto.StoreCreditID);
            }
            paymentUrl = pay.CreateRequestUrl(_configuration["Vnpay:BaseUrl"], _configuration["Vnpay:HashSecret"]);

            return paymentUrl;
        }
    }
}
