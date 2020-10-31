using EventProcessor.Data;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EventProcessor
{
    public class AlertHandler
    {
        private readonly IRepository<Alert> _alertsRepository;
        private readonly INotificationService _notificationService;
        private readonly ILogger _log;

        public AlertHandler(IRepository<Alert> alertsRepository, INotificationService notificationService, ILogger log)
        {
            _alertsRepository = alertsRepository;
            _notificationService = notificationService;
            _log = log;
        }

        public async Task HandleAsync(IList<Alert> newAlerts)
        {
            foreach (var newAlert in newAlerts)
            {
                if(newAlert.AlertType == AlertTypes.ThresholdViolation)
                {
                    await HandleThresholdViolationAlert(newAlert);
                }
                else
                {
                    _log.LogInformation($"Cannot handle unknown AlertType: {newAlert.AlertType}");
                }
            }
        }

        private async Task HandleThresholdViolationAlert(Alert newAlert)
        {
            var existingAlert = _alertsRepository.All
                .Where(alert => alert.AlertType == AlertTypes.ThresholdViolation &&
                                alert.PartitionKey == newAlert.PartitionKey)
                .OrderByDescending(alert => alert.Timestamp)
                .AsEnumerable()
                .FirstOrDefault();

            if(existingAlert == null || newAlert.Timestamp.Subtract(existingAlert.Timestamp).TotalMinutes > 45)
            {
                _log.LogInformation($"No existing alert found for {newAlert.ApplicationUri}_{newAlert.DisplayName}! Creating a new one!");
                newAlert.Occurrences = 1;
                await _alertsRepository.AddAsync(newAlert);
                await _notificationService.NotifyAsync(newAlert);
            }
            else
            {
                _log.LogInformation($"Existing alert within 45 minutes window found for {newAlert.ApplicationUri}_{newAlert.DisplayName}! Updating occurrences.");
                existingAlert.Occurrences++;

                await _alertsRepository.UpsertAsync(existingAlert);
            }
        }
    }
}
