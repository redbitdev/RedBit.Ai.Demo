using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using System.Linq;
using RedBit.Ai.Core;

namespace RedBit.Ai.Functions
{
    public static class ImageProcessingStatusFunc
    {
        [FunctionName("ImageProcessingStatusFunc")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log,
            ExecutionContext context)
        {
            // get the config
            _config = new ConfigurationBuilder()
                .SetBasePath(context.FunctionAppDirectory)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            // parse query parameter
            string id = req.GetQueryParameterDictionary()
                .FirstOrDefault(q => string.Compare(q.Key, "id", true) == 0)
                .Value;

            // make sure we have a id
            if (id == null)
                return new BadRequestObjectResult("id not in query string");

            // get the status
            var status = await TableManager.GetStatus(id);

            // if we get no status then something is wrong
            if (status == null)
                return new BadRequestObjectResult($"Not able to retreive record with id {id}");

            // return the status
            return new OkObjectResult(status);
        }

        private static IConfiguration _config;
        private static string AzureConnectionString => _config?["AzureWebJobsStorage"];
        private static TableManager _tableManager;
        private static TableManager TableManager => _tableManager ?? (_tableManager = new TableManager(AzureConnectionString));
    }
}
