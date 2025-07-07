using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using WebFunction2025.Services;
using Microsoft.Extensions.Configuration;


//FunctionsApplication.CreateBuilder(args);
var builder = new HostBuilder()
     .ConfigureFunctionsWebApplication()
     
    .ConfigureServices(services =>
    {
        services.AddScoped<IStorageService, StorageService>();
    })
    .Build();

builder.Run();
