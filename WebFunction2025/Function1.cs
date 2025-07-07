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

  [Function("GetItem1")]
    public   IActionResult Ping(
           [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "ping")] HttpRequest req )
    {
        return new OkObjectResult(new { message = "Pang" });
    }

 
    [Function("GetItem2")]
    public      IActionResult  GetItem3(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "items2")] HttpRequest req )
    {
         
           
            return new OkObjectResult(new { id = 2, value = "gpt iy" });
         
    }


    [Function("GetItem3")]
    public IActionResult GetItem5(
      [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "items21")] HttpRequest req )
    {
        _logger.LogDebug("Logging message template: {Message}", "fff");
        return new OkObjectResult(new { id = 2, value = "gpt iy" });
    }


}
