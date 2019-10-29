using System;
using System.Net.Http;
using System.Runtime.Loader;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Client.Transport.Mqtt;
using Newtonsoft.Json;

namespace EventGridSubscriberModule
{
    class Program
    {
        private const string API_VERSION_QUERY_STRING = "?api-version=2019-01-01-preview";
        private static string topicName;
        private static HttpClient httpClient;
        private static string enpointUrl;
        private static string edgeHubOutputName;
        private static string cloudEventGridSasKey;
        //private static string cloudEventGridTopicName;
        private static string cloudEventGridEnpointUrl;

        static void Main(string[] args)
        {
            Init().Wait();
            WebHost.CreateDefaultBuilder(args).UseStartup<Startup>().Build().Run();

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
            enpointUrl = Environment.GetEnvironmentVariable("WEBHOOK_URL");
            edgeHubOutputName = Environment.GetEnvironmentVariable("EDGEHUB_OUTPUT_NAME");
            cloudEventGridSasKey = Environment.GetEnvironmentVariable("CLOUD_EVENTGRID_SAS_KEY");
            //cloudEventGridTopicName = Environment.GetEnvironmentVariable("CLOUD_EVENTGRID_TOPIC");
            cloudEventGridEnpointUrl = Environment.GetEnvironmentVariable("CLOUD_EVENTGRID_ENDPOINT");

            await CreateEventGridWebHookSubscriptionAsync();
            await CreateEventGridEdgeHubSubscriptionAsync();
            await CreateCloudEventGridSubscriptionAsync();
        }

        /// <summary>
        /// This method is called whenever the module is sent a message from the EdgeHub. 
        /// It just pipe the messages without any change.
        /// It prints all the incoming messages.
        /// </summary>
        static Task<MessageResponse> PipeMessage(Message message, object userContext)
        {
            return Task.FromResult(MessageResponse.Completed);
        }

        private static async Task CreateCloudEventGridSubscriptionAsync()
        {
            if (string.IsNullOrEmpty(cloudEventGridEnpointUrl) || string.IsNullOrEmpty(cloudEventGridSasKey))
            {
                Console.WriteLine("Skipping configuration of Cloud EventGrid Subscription because settings are missing!");
            }

            var requestBody = new
            {
                properties = new
                {
                    destination = new
                    {
                        endpointType = "EventGrid",
                        properties = new
                        {
                            endpointUrl = cloudEventGridEnpointUrl,
                            sasKey = cloudEventGridSasKey
                            //topicName = cloudEventGridTopicName
                        }
                    }
                }
            };

            await CreateEventGridSubscriptionAsync(requestBody, "cloudEventGrid");
        }

        private static async Task CreateEventGridEdgeHubSubscriptionAsync()
        {
            var defaultEdgeHubOutput = "defaultEdgeHubOutput";

            var requestBody = new
            {
                properties = new
                {
                    destination = new
                    {
                        endpointType = "EdgeHub",
                        properties = new
                        {
                            outputName = edgeHubOutputName ?? defaultEdgeHubOutput
                        }
                    }
                }
            };

            await CreateEventGridSubscriptionAsync(requestBody, edgeHubOutputName ?? defaultEdgeHubOutput);
        }

        private async static Task CreateEventGridWebHookSubscriptionAsync()
        {
            if (string.IsNullOrEmpty(enpointUrl))
            {
                Console.WriteLine($"Skipping configuration of WebHook Subscription because setting {nameof(enpointUrl)} is missing!");
            }

            var requestBody = new
            {
                properties = new
                {
                    inputSchema = "EventGridSchema",
                    destination = new
                    {
                        endpointType = "WebHook",
                        properties = new
                        {
                            endpointUrl = enpointUrl
                        }
                    }
                }
            };

            await CreateEventGridSubscriptionAsync(requestBody, "egsub");
        }

        private async static Task CreateEventGridSubscriptionAsync(object requestBody, string subscriptionName)
        {
            var createTopicContent = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");
            var response = await httpClient.PutAsync($"/topics/{topicName}/eventSubscriptions/{subscriptionName}{API_VERSION_QUERY_STRING}", createTopicContent);

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
