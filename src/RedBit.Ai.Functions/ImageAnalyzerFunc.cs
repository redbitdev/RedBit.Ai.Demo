// Default URL for triggering event grid function in the local environment.
// http://localhost:7071/runtime/webhooks/EventGrid?functionName={functionname}
// https://cba2fa38.ngrok.io/runtime/webhooks/EventGrid?functionName=ImageAnalyzerFunc

using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using RedBit.Ai.Core;
using Newtonsoft.Json;
using System;

namespace RedBit.Ai.Functions
{
    public static class ImageRecoFunc
    {
        [FunctionName("ImageAnalyzerFunc")]
        public static async void Run([EventGridTrigger]EventGridEvent eventGridEvent, ILogger log, ExecutionContext context)
        {
            // get the config
            _config = new ConfigurationBuilder()
                .SetBasePath(context.FunctionAppDirectory)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            var data = eventGridEvent.Data.ToString();

            try
            {
                // deserialize the payload
                var payload = JsonConvert.DeserializeObject<ImageEntity>(data);
                if (!string.IsNullOrEmpty(payload.OriginalImageUrl))
                {
                    // start task to analyze the image
                    var ir = new ImageAnalyzer(payload, BlobManager, TableManager, CognitiveServicesKey)
                    {
                        DetectBrands = true,
                        DetectFaces = true,
                        IncludeDescription = true,
                        DetectObjects = true,
                        IncludeTags = true
                    };
                    var ie = await ir.Analyze();
                    await TableManager.UpdateRecord(ie.RowKey, (entity) => entity.ImageAnalyzerResults = ie.ImageAnalyzerResults);
                }
                else
                {
                    log.LogInformation($"Original Image not available in payload: {data}");
                }
            }
            catch (Exception ex)
            {
                // cannot deserilaize so just ignore
                log.LogInformation($"Could not process image : {ex.Message}");
                throw ex;
            }
        }

        private static IConfiguration _config;
        private static string AzureConnectionString => _config?["AzureWebJobsStorage"];
        private static string CognitiveServicesKey => _config?["CogServicesKey"];
        private static BlobManager _blobManager;
        private static BlobManager BlobManager => _blobManager ?? (_blobManager = new BlobManager(AzureConnectionString));
        private static TableManager _tableManager;
        private static TableManager TableManager => _tableManager ?? (_tableManager = new TableManager(AzureConnectionString));
    }
}
