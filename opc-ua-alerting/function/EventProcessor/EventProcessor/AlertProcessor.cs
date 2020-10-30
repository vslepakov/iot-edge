// Default URL for triggering event grid function in the local environment.
// http://localhost:7071/runtime/webhooks/EventGrid?functionName={functionname}
using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Documents.Client;

namespace EventProcessor
{
    public static class AlertProcessor
    {
        [FunctionName("AlertProcessor")]
        public static void Run(
            [EventGridTrigger]EventGridEvent eventGridEvent,
            [CosmosDB(databaseName: "TelemetryDb", collectionName: "alarms", ConnectionStringSetting = "CosmosDBConnection")] DocumentClient client,
            ILogger log)
        {
            log.LogInformation(eventGridEvent.Data.ToString());
        }
    }
}
