using System;

namespace alerting
{
    public class OpcUaDataPoint
    {
        public string NodeId { get; set; }

        public double Value { get; set; }

        public string DisplayName { get; set; }

        public string ApplicationUri { get; set; }

        public DateTime SourceTimestamp { get; set; }

        public string Key => $"{ApplicationUri}_{NodeId}";
    }
}
