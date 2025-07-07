using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using WebFunction2025.Services;

namespace WebFunction2025.Functions;

public class FunctionToStorage
{
    private readonly ILogger<FunctionToStorage> _logger;
 //   private readonly IStorageService _storageService;

    public FunctionToStorage(ILogger<FunctionToStorage> logger)//, IStorageService storageService)
    {
        _logger = logger;
      //  _storageService = storageService;
    }

    [Function("FunctionToStorage")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "storage")] HttpRequest req)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request.");


        string fileContent = "Hello, Azure Blob Storage!\nThis is a test file uploaded from C#.";

            await Task.Delay(1); // Simulate some processing delay
        string? _connectionString = "";

        try { 
_connectionString = Environment.GetEnvironmentVariable("MyStorageConnectionString");
            await Task.Delay(1); // Simulate some processing delay
                                    // Save the file content to Azure Blob Storage
              //      await _storageService.SaveStorage("filename _"+DateTime.Now.ToString("u") + ".txt", fileContent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while saving to storage.");
            return new OkObjectResult("Welcome to Status500InternalServerError!");
            //StatusCodeResult(StatusCodes.Status500InternalServerError);
        }
       


        return new OkObjectResult("Welcome to Azure Functions! "+ _connectionString);
    }
}