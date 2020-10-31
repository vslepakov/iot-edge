// Default URL for triggering event grid function in the local environment.
// http://localhost:7071/runtime/webhooks/EventGrid?functionName={functionname}
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Documents.Client;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using EventProcessor.Data;
using Newtonsoft.Json.Linq;

namespace EventProcessor
{
    public static class AlertProcessor
    {
        [FunctionName("AlertProcessor")]
        public static async Task Run(
            [EventGridTrigger]EventGridEvent eventGridEvent,
            [CosmosDB(databaseName: "TelemetryDb", collectionName: "alerts", ConnectionStringSetting = "CosmosDBConnection")] DocumentClient client,
            ILogger log)
        {
            var data = eventGridEvent.Data.ToString();

            log.LogInformation(data);

            var jObject = JObject.Parse(data);
            var newAlerts = JsonConvert.DeserializeObject<IList<Alert>>(jObject["body"].ToString());

            var databaseName = Environment.GetEnvironmentVariable("COSMOS_DB_DATABASE_NAME");
            var collectionName = Environment.GetEnvironmentVariable("COSMOS_DB_COLLECTION_NAME");

            var alertsRepository = new AlertsRepository(client, databaseName, collectionName);
            var handler = new AlertHandler(alertsRepository, new TwilioNotificationService(log), log);
            await handler.HandleAsync(newAlerts);
        }
    }
}
