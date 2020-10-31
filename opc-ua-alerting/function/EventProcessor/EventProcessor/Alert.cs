using EventProcessor.Data;
using System;

namespace EventProcessor
{
    public static class AlertTypes
    {
        public const string ThresholdViolation = "ThresholdViolationAlert";
    }

    public class Alert : Entity
    {
        public string AlertType { get; set; }

        public string DisplayName { get; set; }

        public string ApplicationUri {get; set;}

        public double AverageValue { get; set; }

        public DateTime Timestamp { get; set; }

        public string PartitionKey => $"{ApplicationUri}_{DisplayName}";

        public int Occurrences { get; set; }
    }
}
