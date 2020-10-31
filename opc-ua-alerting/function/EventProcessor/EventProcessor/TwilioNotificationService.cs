using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace EventProcessor
{
    public class TwilioNotificationService : INotificationService
    {
        private readonly ILogger _log;

        public TwilioNotificationService(ILogger log)
        {
            _log = log;
        }

        public Task NotifyAsync(Alert alert)
        {
            _log.LogInformation($"Sending SMS for new Alert {alert.ApplicationUri}_{alert.DisplayName}");
            return Task.CompletedTask;
        }
    }
}
