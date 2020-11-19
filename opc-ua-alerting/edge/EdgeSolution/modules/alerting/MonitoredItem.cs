namespace alerting
{
    public class MonitoredItem
    {
        public string ApplicationUri { get; set; }

        public string NodeId { get; set; }

        public string DisplayName { get; set; }

        public double ThresholdValue { get; set; }

        public double ToleranceHigh { get; set; }

        public double ToleranceLow { get; set; }

        public string Key => $"{ApplicationUri}_{NodeId}";
    }
}
