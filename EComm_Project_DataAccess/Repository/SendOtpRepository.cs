using EComm_Project_DataAccess.Repository.IRepository;
using EComm_Project_Utility;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace EComm_Project_DataAccess.Repository
{
    public class SendOtpRepository : ISendOtpRepository
    {
        private readonly TwilioSettings _twilio;

        public SendOtpRepository(IOptions<TwilioSettings> twilio)
        {
            _twilio = twilio.Value;
        }
        public async Task SendSmsAsync(string number, string message)
        {
            TwilioClient.Init(_twilio.AccountSid, _twilio.AuthToken);
            if (!number.StartsWith("+"))
            {
                number = "+91" + number;
            }
            await MessageResource.CreateAsync(to: new PhoneNumber(number), from: new PhoneNumber(_twilio.FromPhone), body: message);
        }
    }
    }
