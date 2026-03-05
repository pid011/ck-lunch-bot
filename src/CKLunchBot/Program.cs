using CKLunchBot;
using CKLunchBot.Menu;
using CKLunchBot.Post;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MenuWebService = CKLunchBot.Menu.MenuWebService;
using XPostService = CKLunchBot.Post.XPostService;

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
    .AddSingleton<BotService>();

builder.Build().Run();
