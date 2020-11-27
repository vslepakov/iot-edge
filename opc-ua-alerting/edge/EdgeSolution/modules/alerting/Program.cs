namespace alerting
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Loader;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Devices.Client;
    using Microsoft.Azure.Devices.Client.Transport.Mqtt;
    using Microsoft.Azure.Devices.Shared;
    using Newtonsoft.Json;

    class Program
    {
        private static AlertProcessor alertProcessor;

        static async Task Main(string[] args)
        {
            await InitAsync();

            // Wait until the app unloads or is cancelled
            var cts = new CancellationTokenSource();
            AssemblyLoadContext.Default.Unloading += (ctx) => cts.Cancel();
            Console.CancelKeyPress += (sender, cpe) => cts.Cancel();

            await alertProcessor.CheckForAlertsAsync(TimeSpan.FromMinutes(1), cts.Token).ConfigureAwait(false);
        }

        /// <summary>
        /// Initializes the ModuleClient and sets up the callback to receive
        /// messages containing temperature information
        /// </summary>
        static async Task InitAsync()
        {
            MqttTransportSettings mqttSetting = new MqttTransportSettings(TransportType.Mqtt_Tcp_Only);
            ITransportSettings[] settings = { mqttSetting };

            // Open a connection to the Edge runtime
            var ioTHubModuleClient = await ModuleClient.CreateFromEnvironmentAsync(settings);
            await ioTHubModuleClient.OpenAsync().ConfigureAwait(false);

            Logger.LogInfo("IoT Hub module client initialized.");

            alertProcessor = new AlertProcessor(new ModuleClientWrapper(ioTHubModuleClient));

            // Register callback to be called when a message is received by the module
            await ioTHubModuleClient.SetInputMessageHandlerAsync("opc-ua", HandleMessage, ioTHubModuleClient).ConfigureAwait(false);

            var moduleTwin = await ioTHubModuleClient.GetTwinAsync().ConfigureAwait(false);
            await OnDesiredPropertiesUpdate(moduleTwin.Properties.Desired, ioTHubModuleClient).ConfigureAwait(false);

            // Attach a callback for updates to the module twin's desired properties.
            await ioTHubModuleClient.SetDesiredPropertyUpdateCallbackAsync(OnDesiredPropertiesUpdate, null).ConfigureAwait(false);
        }

        private static Task<MessageResponse> HandleMessage(Message message, object userContext)
        {
            try
            {
                var messageBytes = message.GetBytes();
                var messageString = Encoding.UTF8.GetString(messageBytes);

                Logger.LogInfo($"Received new data points! {messageString}");

                var dataPoints = JsonConvert.DeserializeObject<IList<OpcUaDataPoint>>(messageString);

                if (dataPoints != null)
                {
                    alertProcessor.HandleNewValues(dataPoints);
                }

                return Task.FromResult(MessageResponse.Completed);
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error when receiving new data: {ex}");
                throw;
            }
        }

        private static Task OnDesiredPropertiesUpdate(TwinCollection desiredProperties, object userContext)
        {
            try
            {
                Logger.LogInfo("Desired property change:");
                Logger.LogInfo(JsonConvert.SerializeObject(desiredProperties));

                if (desiredProperties["MonitoredItems"] != null)
                {
                    var monitoredItemsProperty = desiredProperties["MonitoredItems"].ToString();
                    var monitoredItems = JsonConvert.DeserializeObject<IList<MonitoredItem>>(monitoredItemsProperty);
                    alertProcessor.SetMonitoredItems(monitoredItems);
                }
            }
            catch (AggregateException ex)
            {
                foreach (Exception exception in ex.InnerExceptions)
                {
                    Logger.LogError($"Error when receiving desired property: {exception}");
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error when receiving desired property: {ex}");
            }
            return Task.CompletedTask;
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
    }
}
