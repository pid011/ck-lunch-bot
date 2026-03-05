using CKLunchBot;
using CKLunchBot.Core;
using CKLunchBot.Functions;
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
    .Configure<XCredentials>(builder.Configuration.GetSection("Credentials"))
    .Configure<BotConfig>(builder.Configuration.GetSection("BotConfig"))
    .AddSingleton<IMenuParser, MenuParser>()
    .AddHttpClient<IMenuService, MenuWebService>()
    .Services
    .AddSingleton<IPostService, XPostService>()
    .AddSingleton<PostingService>();

builder.Build().Run();
