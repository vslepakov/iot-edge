using System;

namespace alerting
{
    public static class AlertTypes
    {
        public const string ThresholdViolation = "ThresholdViolationAlert";
    }

    public class Alert
    {
        public string AlertType { get; set; }

        public string DisplayName { get; set; }

        public string ApplicationUri { get; set; }

        public double AverageValue { get; set; }

        public DateTime Timestamp { get; set; }
    }
}
