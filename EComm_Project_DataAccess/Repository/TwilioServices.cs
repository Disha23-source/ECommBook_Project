using EComm_Project_DataAccess.Repository.IRepository;
using EComm_Project_Utility;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using Twilio.Rest.Api.V2010.Account;
using Twilio.TwiML;
using Twilio.TwiML.Voice;
using Twilio.Types;
using Task = System.Threading.Tasks.Task;

namespace EComm_Project_DataAccess.Repository
{
    public class TwilioServices : ITwilioServices
    {
        private readonly TwilioSettings _twilio;

        public TwilioServices(IOptions<TwilioSettings> twilio)
        {
            _twilio = twilio.Value;
        }

        public async Task MakeOrderConfirmationCallAsync(string toPhoneNumber, int orderId, string customerName, IEnumerable<string> productNames)
        {
            if (!toPhoneNumber.StartsWith("+"))
            {
                toPhoneNumber = "+91" + toPhoneNumber;
            }

            string products = string.Join(", ", productNames);

            var response = new VoiceResponse();

            // 🔥 ADD THIS (IMPORTANT)
            var gather = new Gather(
                numDigits: 1,
                timeout: 8
            );

            gather.Say(
                $"Hello {customerName}. " +
                $"Your order number is {orderId}. " +
                $"You have ordered {products}. " +
                $"Press 1 to confirm your order. " +
                $"Press 2 to cancel your order.",
                voice: "Polly.Kajal-Neural",
                language: "en-IN"
            );

            response.Append(gather);

            response.Say(
                "We have saved Your response. thankyou. Goodbye.",
                voice: "Polly.Kajal-Neural",
                language: "en-IN"
            );

            await CallResource.CreateAsync(
                twiml: response.ToString(),
                from: new PhoneNumber(_twilio.FromPhone),
                to: new PhoneNumber(toPhoneNumber)
            );
        }
    }
}
