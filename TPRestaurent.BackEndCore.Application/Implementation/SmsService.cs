using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPRestaurent.BackEndCore.Application.Contract.IServices;
using TPRestaurent.BackEndCore.Common.DTO.Response.BaseDTO;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace TPRestaurent.BackEndCore.Application.Implementation
{
    public class SmsService : GenericBackendService, ISmsService
    {
        public SmsService(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public async Task<AppActionResult> SendMessage(string message, string phoneNumber)
        {
            AppActionResult result = new AppActionResult();
            try
            {
                string accountSid = "AC16c01656bd500a1aa1ea9fe9040360d7";
                string authToken = "788dab3dcd68e060682311d3141b7299";
                TwilioClient.Init(accountSid, authToken);

                var apiResponsse = MessageResource.Create(
                   body: $"{message}",
                   from: new Twilio.Types.PhoneNumber("+1 443 333 1958"),
                   to: new Twilio.Types.PhoneNumber($"+84{phoneNumber}")
               );

                result.Result = apiResponsse.Status;
            }
            catch (Exception e)
            {
                result = BuildAppActionResultError(result, e.Message);
            }

            return result;
        }
    }
}
