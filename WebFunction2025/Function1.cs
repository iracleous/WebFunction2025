using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
 
using Newtonsoft.Json;

using WebFunction2025.Models;
using Azure.Data.Tables;
using Azure;
using System.Net;

namespace WebFunction2025;

public class Function1
{
    private readonly ILogger<Function1> _logger;
    private const string TableName = "ItemsTable";
    private const string PartitionKey = "DefaultPartition";

    public Function1(ILogger<Function1> logger)
    {
        _logger = logger;
    }



    private   TableClient GetTableClient()
    {
        string? connectionString = @"DefaultEndpointsProtocol=https;EndpointSuffix=core.windows.net;AccountName=sia23vati2025;AccountKey=B7TcMQYKYFvH6+MwFwV+wK2m0MH0n3NrHZmanQLjlsdyeHj8BEeGdWEWg3tBBJ3l/y2cM4b1Ya13+ASt/M8x4Q==;BlobEndpoint=https://sia23vati2025.blob.core.windows.net/;FileEndpoint=https://sia23vati2025.file.core.windows.net/;QueueEndpoint=https://sia23vati2025.queue.core.windows.net/;TableEndpoint=https://sia23vati2025.table.core.windows.net/";
        //Environment.GetEnvironmentVariable("AzureWebJobsStorage");
        var client = new TableClient(connectionString, TableName);
        client.CreateIfNotExists();
        return client;
    }

    [Function("CreateItem")]
    public   async Task<IActionResult> CreateItem(
          [HttpTrigger(AuthorizationLevel.Function, "post", Route = "items")] HttpRequest req,
          ILogger log)
    {
        var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        var data = JsonConvert.DeserializeObject<Item>(requestBody);

        if (data == null || string.IsNullOrEmpty(data.Id))
            return new BadRequestObjectResult("Invalid payload");

        var client = GetTableClient();
        var entity = new TableEntity(PartitionKey, data.Id)
        {
            ["Value"] = data.Value
        };

        await client.AddEntityAsync(entity);
        return new OkObjectResult(new { message = "Created", data });
    }

    [Function("GetItem")]
    public   async Task<IActionResult> GetItem(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "items/{id}")] HttpRequest req,
            string id,
            ILogger log)
    {
        var client = GetTableClient();

        try
        {
            var entity = await client.GetEntityAsync<TableEntity>(PartitionKey, id);
            return new OkObjectResult(new { id = entity.Value.RowKey, value = entity.Value["Value"] });
        }
        catch (RequestFailedException ex) when (ex.Status == (int)HttpStatusCode.NotFound)
        {
            return new NotFoundObjectResult("Item not found");
        }
    }
    [Function("GetItem3")]
    public      IActionResult  GetItem3(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "items2")] HttpRequest req,
            
            ILogger log)
    {
         
           
            return new OkObjectResult(new { id = 2, value = "gpt iy" });
         
    }




    [Function("GetItem2")]
    public   IActionResult Ping(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = "ping")] HttpRequest req,
           ILogger log)
    {
        return new OkObjectResult(new { message = "Pang" });
    }

    [Function("GetItem5")]
    public IActionResult GetItem5(
      [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "items21")] HttpRequest req,
      ILogger log)
    {
        log.LogDebug("Logging message template: {Message}", "fff");
        return new OkObjectResult(new { id = 2, value = "gpt iy" });
    }




}
