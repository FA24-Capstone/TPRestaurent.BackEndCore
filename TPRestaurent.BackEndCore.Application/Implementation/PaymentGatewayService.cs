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
        private IHashingService _hashingService;
        public PaymentGatewayService(IConfiguration configuration, IHashingService hashingService)
        {
            _configuration = configuration;
            _hashingService = hashingService;
        }
        public async Task<string> CreatePaymentUrlVnpay(PaymentInformationRequest requestDto)
        {
            IConfiguration config = new ConfigurationBuilder()
                           .SetBasePath(Directory.GetCurrentDirectory())
                           .AddJsonFile("appsettings.json", true, true)
                           .Build();
            string key = config["HashingKeys:PaymentLink"];
            var paymentUrl = "";
            var vnpay = new PaymentInformationRequest
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
                urlCallBack = $"{_configuration["Vnpay:ReturnUrl"]}/{requestDto.TransactionID}";
            }

            pay.AddRequestData("vnp_Version", _configuration["Vnpay:Version"]);
            pay.AddRequestData("vnp_Command", _configuration["Vnpay:Command"]);
            pay.AddRequestData("vnp_TmnCode", _configuration["Vnpay:TmnCode"]);
            pay.AddRequestData("vnp_Amount", ((int)requestDto.Amount * 100).ToString());
            pay.AddRequestData("vnp_CreateDate", timeNow.ToString("yyyyMMddHHmmss"));
            pay.AddRequestData("vnp_CurrCode", _configuration["Vnpay:CurrCode"]);
            pay.AddRequestData("vnp_IpAddr", pay.GenerateRandomIPAddress());
            pay.AddRequestData("vnp_Locale", _configuration["Vnpay:Locale"]);
            pay.AddRequestData("vnp_OrderInfo", requestDto.TransactionID);
            pay.AddRequestData("vnp_OrderType", "other");
            pay.AddRequestData("vnp_ReturnUrl", urlCallBack);
            pay.AddRequestData("vnp_TxnRef", requestDto.TransactionID);
            paymentUrl = pay.CreateRequestUrl(_configuration["Vnpay:BaseUrl"], _configuration["Vnpay:HashSecret"]);

            return paymentUrl;
        }
    }
}
