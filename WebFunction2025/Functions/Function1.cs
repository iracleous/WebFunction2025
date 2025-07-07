using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace WebFunction2025.Functions;

public class Function1
{
    private readonly ILogger<Function1> _logger;

    public Function1(ILogger<Function1> logger)
    {
        _logger = logger;
    }

  [Function("GetItem1")]
    public   IActionResult Ping(
           [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "ping")] HttpRequest req )
    {
        _logger.LogInformation("Ping function called at {Time}", DateTime.UtcNow);
        return new OkObjectResult(new { message = "Pang " + DateTime.Now.ToString("u") });
    }

 
    [Function("GetItem2")]
    public      IActionResult  GetItem3(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "items2")] HttpRequest req )
    {
        _logger.LogInformation("GetItem3 function called at {Time}", DateTime.UtcNow);
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
