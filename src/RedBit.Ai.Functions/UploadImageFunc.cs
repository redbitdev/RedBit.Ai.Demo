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

namespace RedBit.Ai.Functions
{
    public static class UploadImageFunc
    {
        [FunctionName("UploadImage")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
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
                return new OkObjectResult(new ImageUploadResult { Id = Guid.NewGuid().ToString("N"), Url = "HTTPS://TODO" });
            }
        }
    }
}
