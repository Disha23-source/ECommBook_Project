using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;
using Twilio.TwiML;
using Twilio.TwiML.Voice;

namespace EComm_Project_Utility
{
    public class SmsSender
    {
        private readonly TwilioSettings _twilio;

        public SmsSender(IOptions<TwilioSettings> twilio)
        {
            _twilio = twilio.Value;
        }

        // SEND SMS
        public void SendOrderMessage(string phoneNumber, string name, int orderId, double total)
        {
            TwilioClient.Init(_twilio.AccountSid, _twilio.AuthToken);

            MessageResource.Create(
                body: $"Dear {name}, your order #{orderId} is placed successfully. Total: ${total}. Thank you!",
                from: new PhoneNumber(_twilio.FromPhone),
                to: new PhoneNumber(phoneNumber)
            );
        }
       
    }
}
