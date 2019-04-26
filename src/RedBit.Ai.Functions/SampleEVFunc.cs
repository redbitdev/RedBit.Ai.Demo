// Default URL for triggering event grid function in the local environment.
// http://localhost:7071/runtime/webhooks/EventGrid?functionName={functionname}
// https://cba2fa38.ngrok.io/runtime/webhooks/EventGrid?functionName=SampleEVFunc

using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Extensions.Logging;

namespace RedBit.Ai.Functions
{
    public static class SampleEVFunc
    {
        [FunctionName("SampleEVFunc")]
        public static void Run([EventGridTrigger]EventGridEvent eventGridEvent, ILogger log)
        {
            log.LogInformation(eventGridEvent.Data.ToString());
        }
    }
}

/*
endpoint=$(az eventgrid topic show --name x-ai -g xamarin-cognitive-demo --query "endpoint" --output tsv)
key=$(az eventgrid topic key list --name x-ai -g xamarin-cognitive-demo --query "key1" --output tsv)
event='[ {"id": "'"$RANDOM"'", "eventType": "recordInserted", "subject": "myapp/vehicles/motorcycles", "eventTime": "'`date +%Y-%m-%dT%H:%M:%S%z`'", "data":{ "make": "Ducati", "model": "Monster"},"dataVersion": "1.0"} ]'
curl -X POST -H "aeg-sas-key: $key" -d "$event" $endpoint
*/