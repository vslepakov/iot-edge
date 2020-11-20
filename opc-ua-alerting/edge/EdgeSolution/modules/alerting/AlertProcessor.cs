using Microsoft.Azure.Devices.Client;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace alerting
{
    public class AlertProcessor
    {
        private readonly IModuleClientWrapper _moduleClientWrapper;
        private readonly ConcurrentDictionary<string, IList<OpcUaDataPoint>> _cachedItems;
        IList<MonitoredItem> _monitoredItems;

        public AlertProcessor(IModuleClientWrapper moduleClientWrapper)
        {
            _moduleClientWrapper = moduleClientWrapper;
            _cachedItems = new ConcurrentDictionary<string, IList<OpcUaDataPoint>>(StringComparer.OrdinalIgnoreCase);
        }

        public void SetMonitoredItems(IList<MonitoredItem> monitoredItems)
        {
            if (Validate(monitoredItems))
            {
                _monitoredItems = monitoredItems;
                Console.WriteLine("Successfully applied new Monitored Items configuration");
            }
            else
            {
                Console.WriteLine("Failed to apply new Monitored Items configuration");
            }
        }

        public void HandleNewValues(IList<OpcUaDataPoint> dataPoints)
        {
            foreach (var dataPoint in dataPoints.Where(d => IsMonitored(d.Key)))
            {
                if (_cachedItems.TryGetValue(dataPoint.Key, out IList<OpcUaDataPoint> value))
                {
                    value.Add(dataPoint);
                }
                else
                {
                    _cachedItems.TryAdd(dataPoint.Key, new List<OpcUaDataPoint> { dataPoint });
                }

                Console.WriteLine($"Added Value {dataPoint.Value} for: {dataPoint.Key} and SourceTimestamp {dataPoint.SourceTimestamp:o}");
            }
        }

        public async Task CheckForAlertsAsync(TimeSpan interval, CancellationToken cancellationToken)
        {
            while (true)
            {
                Console.WriteLine("Starting alert check");

                var alerts = new List<Alert>();

                foreach (var series in _cachedItems)
                {
                    var monitoredItem = _monitoredItems.FirstOrDefault(item => item.Key == series.Key);

                    if(monitoredItem != null)
                    {
                        var avg = series.Value.Average(val => val.Value);

                        if (avg >= (monitoredItem.ThresholdValue + monitoredItem.ToleranceHigh) ||
                           avg <= (monitoredItem.ThresholdValue - monitoredItem.ToleranceLow))
                        {
                            Console.WriteLine($"Detected alert condition for {series.Key} with average value {avg}");

                            alerts.Add(new Alert
                            {
                                AlertType = AlertTypes.ThresholdViolation,
                                ApplicationUri = monitoredItem.ApplicationUri,
                                DisplayName = monitoredItem.DisplayName,
                                AverageValue = avg,
                                Timestamp = DateTime.UtcNow
                            });
                        }
                    }
                }

                if (alerts.Any())
                {
                    var alertAsJson = JsonConvert.SerializeObject(alerts);
                    using var message = new Message(Encoding.UTF8.GetBytes(alertAsJson));
                    await _moduleClientWrapper.SendEventAsync("output1", message);

                    Console.WriteLine("Alert message sent");
                }

                CleanUp();

                Task task = Task.Delay(interval, cancellationToken);

                try
                {
                    await task;
                }
                catch (TaskCanceledException)
                {
                    return;
                }
            }
        }

        private void CleanUp()
        {
            foreach(var series in _cachedItems.Where(d => !IsMonitored(d.Key)))
            {
                _cachedItems.TryRemove(series.Key, out IList<OpcUaDataPoint> dataPoints);
            }
        }

        private bool IsMonitored(string key)
        {
            return _monitoredItems.Any(item => item.Key == key);
        }


        private bool Validate(IList<MonitoredItem> monitoredItems)
        {
            bool isValid = true;

            var hasDupes = monitoredItems.GroupBy(item => item.Key).Any(group => group.Count() > 1);

            if (hasDupes)
            {
                Console.WriteLine("Check your Monitored Items configuration, you have duplicates!");
                isValid = false;
            }

            return isValid;
        }
    }
}
