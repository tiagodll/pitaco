using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Azure.Data.Tables;
using Azure;
using Azure.Storage.Queues; // Namespace for Queue storage types
using Azure.Storage.Queues.Models;

namespace Pitaco.Functions
{
    public class DbComment{

    }
    public static class PublicApi
    {
        [FunctionName("AddComment")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]
            HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request." + req.Query["wskey"]);

            var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING");
            QueueClient queueClient = new QueueClient(connectionString, "events", new QueueClientOptions(){ MessageEncoding = QueueMessageEncoding.Base64});
            
            var content = await new StreamReader(req.Body).ReadToEndAsync();

            var message = new EventMsg()
            {
                EventType = "AddComment", 
                Payload = JsonSerializer.Deserialize<Comment>(content)
            };
            
            log.LogInformation(JsonSerializer.Serialize(message));
            
            await queueClient.SendMessageAsync(JsonSerializer.Serialize(message));

            Console.WriteLine($"Inserted: {message}");
            return null;
        }

        [FunctionName("RunEvent")]
        public static void RunEvent([QueueTrigger("events", Connection = "CONNECTION_STRING")] string item,
            ILogger log)
        {
            log.LogInformation($"C# Queue trigger function processed: {item}");

            dynamic data = JsonSerializer.Deserialize<EventMsg>(item);
            switch (data?.EventType)
            {
                case "AddComment": ProcessAddEvent(data?.Payload); break;
                default: break;
            }
        }
        
        public static void ProcessAddEvent(Comment comment)
        {
            var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING");
            var _tableClient = new TableClient(connectionString, "comments");
            
            TableEntity entity = new TableEntity();
            entity.PartitionKey = comment.wskey;
            entity.RowKey = comment.key;

            entity["Url"] = comment.url;
            entity["Text"] = comment.text;
            entity["Author"] = comment.author;

            _tableClient.AddEntity(entity);
        }

        public class EventMsg
        {
            public string EventType { get; set; }
            public DateTime Timestamp { get; set; }
            public Comment Payload { get; set; }
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
