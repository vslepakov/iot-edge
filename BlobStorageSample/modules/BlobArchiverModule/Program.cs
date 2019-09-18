using System;
using System.Collections.Generic;
using System.Runtime.Loader;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Client.Transport.Mqtt;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;

namespace BlobArchiverModule
{
    class Program
    {
        private const string LOCAL_BLOB_CONTAINER_NAME = "toupload";
        private static int counter;
        private static readonly List<string> buffer = new List<string>();
        private static CloudBlobContainer cloudBlobContainer;

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

            var storageConnectionString = Environment.GetEnvironmentVariable("BLOB_STORAGE_CONNECTION_STR");
            if (CloudStorageAccount.TryParse(storageConnectionString, out CloudStorageAccount storageAccount))
            {
                var cloudBlobClient = storageAccount.CreateCloudBlobClient();
                cloudBlobContainer = cloudBlobClient.GetContainerReference(LOCAL_BLOB_CONTAINER_NAME);
                await cloudBlobContainer.CreateIfNotExistsAsync();
            }
            else
            {
                // Otherwise, let the user know that they need to define the environment variable.
                Console.WriteLine(
                    "A connection string has not been defined in the system environment variables. " +
                    "Add an environment variable named 'BLOB_STORAGE_CONNECTION_STR' with your storage " +
                    "connection string as a value.");
            }
        }

        /// <summary>
        /// This method is called whenever the module is sent a message from the EdgeHub. 
        /// It just pipe the messages without any change.
        /// It prints all the incoming messages.
        /// </summary>
        static async Task<MessageResponse> PipeMessage(Message message, object userContext)
        {
            int counterValue = Interlocked.Increment(ref counter);

            var messageBytes = message.GetBytes();
            var messageString = Encoding.UTF8.GetString(messageBytes);
            Console.WriteLine($"Received message: {counterValue}, Body: [{messageString}]");

            buffer.Add(messageString);

            if (buffer.Count >= 50)
            {
                var cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference($"uploaded_{DateTime.UtcNow.ToString("o")}.txt");
                await cloudBlockBlob.UploadTextAsync(string.Join(Environment.NewLine, buffer.ToArray()));

                buffer.Clear();
                Console.WriteLine("Stored messages in Blob Storage");
            }

            return MessageResponse.Completed;
        }
    }
}
