using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Azure.Data.Tables;
using Azure;

namespace Pitaco.Functions
{
    public class DbComment{

    }
    public static class PublicApi
    {
        [FunctionName("AddComment")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            // dynamic data = JsonConvert.DeserializeObject(requestBody);
            // name = name ?? data?.name;

            var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING");
            var _tableClient = new TableClient(connectionString, "comments");

            TableEntity entity = new TableEntity();
            entity.PartitionKey = req.Query["wskey"];
            entity.RowKey = Guid.NewGuid().ToString();

            entity["Url"] = req.Query["url"];
            entity["Text"] = req.Query["text"];
            entity["Author"] = req.Query["author"];

            _tableClient.AddEntity(entity);

            return null;
        }
        
        public class Comment {
            public string wskey {get;set;}
            public string key {get;set;}
            public string url {get;set;}
            public string text {get;set;}
            public string author {get;set;}
        }

        [FunctionName("getComments")]
        public static async Task<IActionResult> RunGet(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING");
            var _tableClient = new TableClient(connectionString, "comments");

            var entities = _tableClient.Query<TableEntity>($"Url eq '{req.Query["url"]}'");
            var result = entities.Select(e => new Comment(){
                wskey = e.PartitionKey, key=e.RowKey, 
                url=e["Url"].ToString(), text=e["Text"].ToString(), author=e["Author"].ToString()
            });

            return new OkObjectResult(result);
        }
    }
}
