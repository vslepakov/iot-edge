namespace ActorClient
{
    using Dapr.Actors;
    using Dapr.Actors.Client;
    using Newtonsoft.Json;
    using SensorActor.Interfaces;
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Runtime.Loader;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    class Program
    {
        private static readonly List<string> SimulatedSensorIds = new List<string>{ "1", "2", "3"};
        private static readonly HttpClient httpClient = new HttpClient 
        { 
            BaseAddress = new Uri("http://localhost:3500")
        };

        static void Main(string[] args)
        {
            // Wait until the app unloads or is cancelled
            var cts = new CancellationTokenSource();
            AssemblyLoadContext.Default.Unloading += (ctx) => cts.Cancel();
            Console.CancelKeyPress += (sender, cpe) => cts.Cancel();

            StartSimulationAsync(cts.Token).Wait();
        }

        private async static Task StartSimulationAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    foreach (var sensorId in SimulatedSensorIds)
                    {
                        var sensorData = new SensorData
                        {
                            SensorId = sensorId,
                            Temperature = GetRandomNumber(10, 60),
                            Timestamp = DateTime.UtcNow
                        };

                        await InvokeActorMethodWithRemotingAsync(sensorId, sensorData);
                        //await InvokeActorMethodWithoutRemotingAsync(sensorId, sensorData);
                    }
 
                    await Task.Delay(2000);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error invoking Actor method: {ex}");
                    // Wait a little bit in case it is a transient error
                    await Task.Delay(3000);
                }
            }
        }

        private static async Task InvokeActorMethodWithRemotingAsync(string sensorId, SensorData sensorData)
        {
            var proxy = ActorProxy.Create<ISensorActor>(new ActorId(sensorId), "SensorActor");
            await proxy.SetDataAsync(sensorData);

            Console.WriteLine($"Invoked Actor method for Sensor {sensorId} with Temperature {sensorData.Temperature}");
        }

        private static async Task InvokeActorMethodWithoutRemotingAsync(string sensorId, SensorData sensorData)
        {
            var actorId = new ActorId(sensorId);
            var payload = new StringContent(JsonConvert.SerializeObject(sensorData), Encoding.UTF8, "application/json");
            var result = await httpClient.PostAsync($"/v1.0/actors/SensorActor/{actorId}/method/SetDataAsync", payload);

            if (result.IsSuccessStatusCode)
            {
                Console.WriteLine($"Invoked Actor method for Sensor {sensorId} with Temperature {sensorData.Temperature}");
            }
            else
            {
                var content = await result.Content.ReadAsStringAsync();
                Console.WriteLine($"Failed to invoke Actor method for Sensor {sensorId}: {result.StatusCode} / {result.ReasonPhrase} / {content}");
            }
        }

        private static double GetRandomNumber(double minimum, double maximum)
        {
            var random = new Random();
            return random.NextDouble() * (maximum - minimum) + minimum;
        }
    }
}
