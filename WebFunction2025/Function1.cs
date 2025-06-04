using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;


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
