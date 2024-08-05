using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;


namespace TPRestaurent.BackEndCore.Common.Utils
{
    public static class SmsUtility
    {
        private static string _accountSid;
        private static string _authToken;
        private static string _twilioPhoneNumber;

        static SmsUtility()
        {
            // Initialize Twilio credentials from environment variables or configuration
            _accountSid = Environment.GetEnvironmentVariable("TWILIO_ACCOUNT_SID");
            _authToken = Environment.GetEnvironmentVariable("TWILIO_AUTH_TOKEN");
            _twilioPhoneNumber = Environment.GetEnvironmentVariable("TWILIO_PHONE_NUMBER");

            if (string.IsNullOrEmpty(_accountSid) || string.IsNullOrEmpty(_authToken) || string.IsNullOrEmpty(_twilioPhoneNumber))
            {
                throw new InvalidOperationException("Twilio credentials are not set properly.");
            }

            TwilioClient.Init(_accountSid, _authToken);
        }

        public static void SendSms(string toPhoneNumber, string message)
        {
            var messageOptions = new CreateMessageOptions(new PhoneNumber(toPhoneNumber))
            {
                From = new PhoneNumber(_twilioPhoneNumber),
                Body = message
            };

            var messageResource = MessageResource.Create(messageOptions);

            Console.WriteLine($"Message sent with SID: {messageResource.Sid}");
        }
    }

}
