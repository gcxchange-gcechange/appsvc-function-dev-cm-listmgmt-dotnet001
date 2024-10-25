using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
    })
    .ConfigureWebHostDefaults(builder =>
    {
        builder.ConfigureKestrel(options =>
        {
            options.AllowSynchronousIO = true;
        });
        //builder.UseStartup<Startup>();
    })
    .Build();

host.Run();