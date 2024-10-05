using Api;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults((context, builder) =>
    {
        //builder.UseDefaultWorkerMiddleware();
        //builder.UseMiddleware(async (functionContext, func) =>
        //{
        //    var r = func;
        //    await func();
        //});
        builder.UseMiddleware<SomeMiddleware>();
    })
    .ConfigureServices(services =>
    {
        services.AddTransient<SomeMiddleware>();
        services.AddTransient<IFunctionsWorkerMiddleware, SomeMiddleware>();
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
    })
    .Build();


host.Run();