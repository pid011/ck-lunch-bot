using CKLunchBot;
using CKLunchBot.Runner;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateBootstrapLogger();

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration.AddEnvironmentVariables("X_");

builder.Services
    .AddHostedService<BotService>()
    .AddSerilog()
    .Configure<ApiKey>(builder.Configuration.GetSection("X_ApiKey"))
    .Configure<BotConfig>(builder.Configuration.GetSection("BotConfig"));

builder.Build().Run();
