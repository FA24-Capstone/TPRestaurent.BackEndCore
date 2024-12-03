using TPRestaurent.BackEndCore.Application.Contract.IServices;
using TPRestaurent.BackEndCore.Common.ConfigurationModel;
using TPRestaurent.BackEndCore.Common.DTO.Response.BaseDTO;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace TPRestaurent.BackEndCore.Application.Implementation
{
    public class SmsService : GenericBackendService, ISmsService
    {
        private readonly SmsConfiguration _configuration;

        public SmsService(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _configuration = Resolve<SmsConfiguration>()!;
        }

        public async Task<AppActionResult> SendMessage(string message, string phoneNumber)
        {
            AppActionResult result = new AppActionResult();
            try
            {
                string accountSid = _configuration.AccountSid;
                string authToken = _configuration.AuthToken;
                TwilioClient.Init(accountSid, authToken);
                if (phoneNumber[0] == '0') phoneNumber = phoneNumber.Substring(1, phoneNumber.Length - 1);
                var apiResponsse = MessageResource.Create(
                   body: $"{message}",
                   from: new Twilio.Types.PhoneNumber("+1 443 333 1958"),
                   to: new Twilio.Types.PhoneNumber($"+84{phoneNumber}")
               );

                result.Result = apiResponsse.Status;
            }
            catch (Exception ex)
            {
                return BuildAppActionResultError(result, ex.Message);
            }

            return result;
        }
    }
}