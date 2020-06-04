namespace SensorActor
{
    using System;
    using System.Text;
    using System.Threading.Tasks;
    using Dapr.Actors.AspNetCore;
    using Dapr.Actors.Runtime;
    using Microsoft.AspNetCore;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Azure.Devices.Client;
    using Microsoft.Azure.Devices.Client.Transport.Mqtt;
    using Microsoft.Extensions.Hosting;

    class Program
    {
        private const int AppChannelHttpPort = 3000;

        static void Main(string[] args)
        {
            var mqttSetting = new MqttTransportSettings(TransportType.Mqtt_Tcp_Only);
            ITransportSettings[] settings = { mqttSetting };

            // Open a connection to the Edge runtime
            var ioTHubModuleClient = ModuleClient.CreateFromEnvironmentAsync(settings).Result;
            ioTHubModuleClient.OpenAsync().Wait();
            Console.WriteLine("IoT Hub module client initialized.");

            CreateWebHostBuilder(args, ioTHubModuleClient).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args, ModuleClient ioTHubModuleClient) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseActors(actorRuntime =>
                {
                    // Register MyActor actor type
                    actorRuntime.RegisterActor<SensorActor>(info =>
                    {
                        return new ActorService(info, (service, id) =>
                        {
                            return new SensorActor(service, id, ioTHubModuleClient);
                        });
                    });
                }
                ).UseUrls($"http://localhost:{AppChannelHttpPort}/");
    }
}
