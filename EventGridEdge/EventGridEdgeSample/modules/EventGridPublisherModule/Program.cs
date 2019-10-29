using System;
using System.Net.Http;
using System.Runtime.Loader;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Client.Transport.Mqtt;
using Newtonsoft.Json;

namespace EventGridPublisherModule
{
    class Program
    {
        private const string API_VERSION_QUERY_STRING = "?api-version=2019-01-01-preview";
        private static string topicName;
        private static int counter;
        private static HttpClient httpClient;

        static void Main(string[] args)
        {
            Init().Wait();

            // Wait until the app unloads or is cancelled
            var cts = new CancellationTokenSource();
            AssemblyLoadContext.Default.Unloading += (ctx) => cts.Cancel();
            Console.CancelKeyPress += (sender, cpe) => cts.Cancel();
            WhenCancelled(cts.Token).Wait();
        }

        /// <summary>
        /// Handles cleanup operations when app is cancelled or unloads
        /// </summary>
        public static Task WhenCancelled(CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<bool>();
            cancellationToken.Register(s => ((TaskCompletionSource<bool>)s).SetResult(true), tcs);
            return tcs.Task;
        }

        /// <summary>
        /// Initializes the ModuleClient and sets up the callback to receive
        /// messages containing temperature information
        /// </summary>
        static async Task Init()
        {
            MqttTransportSettings mqttSetting = new MqttTransportSettings(TransportType.Mqtt_Tcp_Only);
            ITransportSettings[] settings = { mqttSetting };

            // Open a connection to the Edge runtime
            ModuleClient ioTHubModuleClient = await ModuleClient.CreateFromEnvironmentAsync(settings);
            await ioTHubModuleClient.OpenAsync();
            Console.WriteLine("IoT Hub module client initialized.");

            // Register callback to be called when a message is received by the module
            await ioTHubModuleClient.SetInputMessageHandlerAsync("input1", PipeMessage, ioTHubModuleClient);

            httpClient = new HttpClient
            {
                BaseAddress = new Uri(Environment.GetEnvironmentVariable("LOCAL_EVENT_GRID_URI"))
            };

            httpClient.DefaultRequestHeaders.Add("aeg-sas-key", Environment.GetEnvironmentVariable("EVENT_GRID_SAS_KEY"));

            topicName = Environment.GetEnvironmentVariable("TOPIC_NAME");
            await CreateEventGridTopicAsync();
        }

        /// <summary>
        /// This method is called whenever the module is sent a message from the EdgeHub. 
        /// It just pipe the messages without any change.
        /// It prints all the incoming messages.
        /// </summary>
        static async Task<MessageResponse> PipeMessage(Message message, object userContext)
        {
            try
            {
                int counterValue = Interlocked.Increment(ref counter);

                byte[] messageBytes = message.GetBytes();
                string messageString = Encoding.UTF8.GetString(messageBytes);
                Console.WriteLine($"Received message: {counterValue}, Body: [{messageString}]");

                if (!string.IsNullOrEmpty(messageString))
                {
                    var @events = new[]
                    {
                        new
                        {
                            id = counter.ToString(),
                            topic = topicName,
                            subject = "me",
                            eventType = "telemetry",
                            eventTime = DateTime.UtcNow.ToString("o"),
                            dataVersion = "1.0.0",
                            metadataVersion = "1",
                            data = messageString
                        }
                    };

                    var content = new StringContent(JsonConvert.SerializeObject(@events), Encoding.UTF8, "application/json");
                    var response = await httpClient.PostAsync($"/topics/{topicName}/events{API_VERSION_QUERY_STRING}", content);

                    Console.WriteLine($"Received message sent to EventGrid");
                    await PrintHttpResponseAsync(response);

                    if (response.IsSuccessStatusCode)
                    {
                        Console.WriteLine($"Succefully send event to Topic {topicName}");
                    }
                }
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex);
            }

            return MessageResponse.Completed;
        }

        static async Task CreateEventGridTopicAsync()
        {
            var requestBody = new
            {
                properties = new
                {
                    inputSchema = "EventGridSchema"
                }
            };

            var createTopicContent = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");
            var response = await httpClient.PutAsync($"/topics/{topicName}/{API_VERSION_QUERY_STRING}", createTopicContent);

            Console.WriteLine($"Sent Create Topic Request");
            await PrintHttpResponseAsync(response);
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Succefully created Topic {topicName}");
            }
        }

        static async Task PrintHttpResponseAsync(HttpResponseMessage response)
        {
            var contents = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"status code {response.StatusCode}");
            Console.WriteLine($"reason phrase {response.ReasonPhrase}");
            Console.WriteLine($"response content {contents}");
        }
    }
}
