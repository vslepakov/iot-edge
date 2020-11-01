using Microsoft.Extensions.Logging;
using Polly;
using System;
using System.Threading.Tasks;
using Twilio;
using Twilio.Exceptions;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace EventProcessor
{
    public class TwilioNotificationService : INotificationService
    {
        private readonly ILogger _log;
        private readonly TwilioConfiguration _config;

        public TwilioNotificationService(TwilioConfiguration config, ILogger log)
        {
            _log = log;
            _config = config;
        }

        public Task NotifyAsync(Alert alert)
        {
            if (_config.SendTextMessagesFeatureEnabled)
            {
                try
                {
                    _log.LogInformation($"Sending SMS for new Alert {alert.ApplicationUri}_{alert.DisplayName}");

                    var policy = Policy
                        .Handle<ApiException>(ex => (ex.Status >= 500 && ex.Status <= 599) || ex.Status == 429)
                        .WaitAndRetry(2, retryAttempt =>
                         TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
                    );

                    var message = $"{alert.AlertType} for {alert.DisplayName} on {alert.ApplicationUri}. Average value is {alert.AverageValue}";
                    policy.Execute(() => SendTextMessage(message));
                }
                catch (ApiException ex)
                {
                    _log.LogError($"Failed to send SMS for new Alert {alert.ApplicationUri}_{alert.DisplayName}. Error {ex.Code}_{ex.MoreInfo}");
                    throw;
                }
            }
            else
            {
                _log.LogInformation($"NOT sending SMS for new Alert {alert.ApplicationUri}_{alert.DisplayName}. Feature disabled.");
            }

            return Task.CompletedTask;
        }

        private MessageResource SendTextMessage(string message)
        {
            TwilioClient.Init(_config.AccountSid, _config.AuthToken);

            return MessageResource.Create(
                    new PhoneNumber(_config.ToPhoneNumber),
                    from: new PhoneNumber(_config.FromPhoneNumber),
                    body: message
             );
        }
    }
}
