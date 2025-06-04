using Azure;
using Azure.Data.Tables;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
 
using Newtonsoft.Json;
using System.Net;
using WebFunction2025.Models;

namespace WebFunction2025;

public class Function1
{
    
    
    [Function("HealthProbe")]
    public     IActionResult  Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "ping")] HttpRequest req)
    {
        
            return new OkObjectResult("Healthy"); // Return 200 OK
    }
 
 
 

}
