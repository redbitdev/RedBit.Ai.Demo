using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RedBit.Ai.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using RedBit.Ai.Core;

namespace RedBit.Ai.Functions
{
    public static class UploadImageFunc
    {
        private static IConfiguration _config;
        [FunctionName("UploadImage")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log,
            ExecutionContext context)
        {
            // get the config
            _config = new ConfigurationBuilder()
                .SetBasePath(context.FunctionAppDirectory)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();
            

            // Get request body
            var body = await new StreamReader(req.Body).ReadToEndAsync();

            if (body == null)
                return new BadRequestObjectResult("No body found");

            // deserialize the payload
            var data = Newtonsoft.Json.JsonConvert.DeserializeObject<ImageUpload>(body);

            if (data.Imageb64 == null)
                return new BadRequestObjectResult("No 'imageb64' property found in body");

            // get the base64 image
            var img = data.Imageb64;

            // convert to byte array
            byte[] buffer = new byte[0];
            try
            {
                buffer = Convert.FromBase64String(img.ToString());
            }
            catch
            {
                return new BadRequestObjectResult("unable to read 'imageb64' data");
            }

            // Upload to blob storage
            if (buffer.Length == 0)
            {
                return new BadRequestObjectResult("unable to upload buffer with length of 0");
            }
            else
            {
                log.LogInformation($"Received image of size {buffer.Length} bytes");
                
                // add the image to blob storage and get the url
                string url = await BlobManager.AddOriginalImage(buffer);
                
                // add the image to table and get the id
                var imageEntity = await TableManager.AddOriginalImage(url);

                // send the image entity to event grid
                await EventGridManager.SendNewImageEvent(imageEntity);

                // return the details to the user
                return new OkObjectResult(new ImageUploadResult { Id = imageEntity.RowKey, Url = url });
            }
        }

        private static string AzureConnectionString => _config?["AzureWebJobsStorage"];
        private static string EventGridKey => _config?["EventGridKey"];
        private static string EventGridTopicEndpoint => _config?["EventGridTopicEndpoint"];
        private static BlobManager _blobManager;
        private static BlobManager BlobManager => _blobManager ?? (_blobManager = new BlobManager(AzureConnectionString));
        private static TableManager _tableManager;
        private static TableManager TableManager => _tableManager ?? (_tableManager = new TableManager(AzureConnectionString));
        private static EventGridManager eventGridManager;
        private static EventGridManager EventGridManager => eventGridManager ?? (eventGridManager = new EventGridManager(EventGridTopicEndpoint, EventGridKey));
    }
}
