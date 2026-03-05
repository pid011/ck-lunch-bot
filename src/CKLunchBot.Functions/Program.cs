using CKLunchBot;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Configuration
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
    .AddEnvironmentVariables();

builder.Services
    .Configure<X.Credentials>(builder.Configuration.GetSection("Credentials"))
    .Configure<BotConfig>(builder.Configuration.GetSection("BotConfig"))
    .AddSingleton<IMenuService, MenuWebService>()
    .AddSingleton<IPostService, XPostService>()
    .AddSingleton<IMessageFormatter, MessageFormatter>()
    .AddSingleton<PostingHelper>();

builder.Build().Run();
