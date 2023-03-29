using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(s =>
    {
        Log.Logger = new LoggerConfiguration()
        .WriteTo.Console()
        .CreateLogger();
        s.AddLogging(lb => lb.AddSerilog(Log.Logger));
    })
    .Build();
await host.RunAsync();
