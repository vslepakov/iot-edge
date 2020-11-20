using System;

namespace alerting
{
    public class OpcUaDataPoint
    {
        public string NodeId { get; set; }

        public OpcUaValue Value { get; set; }

        public string DisplayName { get; set; }

        public string ApplicationUri { get; set; }

        public string Key => $"{ApplicationUri}_{NodeId}";
    }

    public class OpcUaValue
    {
        public double Value { get; set; }

        public DateTime SourceTimestamp { get; set; }
    }
}
