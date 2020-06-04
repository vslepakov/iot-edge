using Dapr.Actors;
using System;
using System.Threading.Tasks;

namespace SensorActor.Interfaces
{
    public interface ISensorActor : IActor
    {
        Task SetDataAsync(SensorData data);
    }

    public class SensorData
    {
        public string SensorId { get; set; }

        public DateTime Timestamp { get; set; }

        public double Temperature { get; set; }

        public override string ToString()
        {
            return $"{Timestamp:o} Temperature from sensor {SensorId}: ${Temperature}";
        }
    }
}
