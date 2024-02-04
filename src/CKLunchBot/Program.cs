using CKLunchBot;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration
    .AddJsonFile("config/appsettings.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables("X_");

builder.Services
    .AddSerilog(config => config
            .ReadFrom.Configuration(builder.Configuration)
            .Enrich.FromLogContext()
            .WriteTo.File("logs/.log", rollingInterval: RollingInterval.Month)
            .WriteTo.Console())
    .Configure<X.Credentials>(builder.Configuration.GetSection("Credentials"))
    .Configure<BotConfig>(builder.Configuration.GetSection("BotConfig"))
    .AddSingleton<IMenuService, MenuWebService>()
    .AddSingleton<IPostService, XPostService>()
    .AddSingleton<IMessageFormatter, MessageFormatter>()
    .AddHostedService<BotService>();

var host = builder.Build();
host.Run();
