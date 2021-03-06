// Default URL for triggering event grid function in the local environment.
// http://localhost:7071/runtime/webhooks/EventGrid?functionName={functionname}
// https://cba2fa38.ngrok.io/runtime/webhooks/EventGrid?functionName=ResizeImageFunc

using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RedBit.Ai.Core;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using System;

namespace RedBit.Ai.Functions
{
    public static class ResizeImageFunc
    {
        

        [FunctionName("ResizeImageFunc")]
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
                    // start tasks to resize the image to different sizes
                    var ir = new ImageResizer(payload, BlobManager, TableManager, CognitiveServicesKey);
                    await ir.Resize(ImageResizer.ImageSize.ExtraSmall);
                    await ir.Resize(ImageResizer.ImageSize.Medium);
                    var ie = await ir.Resize(ImageResizer.ImageSize.Small);

                    await TableManager.UpdateRecord(ie.RowKey, (entity) =>
                    {
                        entity.ExtraSmallImageUrl = ie.ExtraSmallImageUrl;
                        entity.MediumImageUrl = ie.MediumImageUrl;
                        entity.SmallImageUrl = ie.SmallImageUrl;
                    });
                }
                else
                {
                    log.LogInformation($"Original Image not available in payload: {data}");
                }
            }
            catch(Exception ex)
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
