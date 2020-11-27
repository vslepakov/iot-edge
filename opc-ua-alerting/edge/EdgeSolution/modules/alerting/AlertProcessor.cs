using Microsoft.Azure.Devices.Client;
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
            _monitoredItems = new List<MonitoredItem>();
        }

        public void SetMonitoredItems(IList<MonitoredItem> monitoredItems)
        {
            if (Validate(monitoredItems))
            {
                _monitoredItems = monitoredItems;
                Logger.LogInfo("Successfully applied new Monitored Items configuration");
            }
            else
            {
                Logger.LogInfo("Failed to apply new Monitored Items configuration");
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

                Logger.LogInfo($"Added Value {dataPoint.Value} for: {dataPoint.Key} and SourceTimestamp {dataPoint.Value.SourceTimestamp:o}");
            }
        }

        public async Task CheckForAlertsAsync(TimeSpan interval, CancellationToken cancellationToken)
        {
            while (true)
            {
                Logger.LogInfo("Starting alert check");

                var alerts = new List<Alert>();

                foreach (var series in _cachedItems)
                {
                    var monitoredItem = _monitoredItems.FirstOrDefault(item => item.Key == series.Key);

                    if(monitoredItem != null)
                    {
                        var avg = series.Value.Average(val => val.Value.Value);

                        if (avg >= (monitoredItem.ThresholdValue + monitoredItem.ToleranceHigh) ||
                           avg <= (monitoredItem.ThresholdValue - monitoredItem.ToleranceLow))
                        {
                            Logger.LogInfo($"Detected alert condition for {series.Key} with average value {avg}");

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

                    Logger.LogInfo("Alert message sent");
                }

                CleanUp();

                Task task = Task.Delay(interval, cancellationToken);

                try
                {
                    await task.ConfigureAwait(false);
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
                Logger.LogError("Check your Monitored Items configuration, you have duplicates!");
                isValid = false;
            }

            return isValid;
        }

        #region Disposable

        private bool _disposed = false;

        ~AlertProcessor() => Dispose(false);

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            // Dispose of managed resources here.
            if (disposing)
            {
                _moduleClientWrapper?.Dispose();
            }

            // Dispose of any unmanaged resources not wrapped in safe handles.

            _disposed = true;
        }

        #endregion
    }
}
