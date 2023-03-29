using System;
using System.CommandLine;
using System.Threading.Tasks;
using CKLunchBot.Core;
using CKLunchBot.Runner;
using Serilog;

namespace CKLunchBot.Actions;

internal class Program
{
    private static async Task<int> Main(string[] args)
    {
        var menuTypeArg = new Argument<MenuType>("menuType");

        var rootCommand = new RootCommand();
        rootCommand.AddArgument(menuTypeArg);
        rootCommand.SetHandler(RunAsync, menuTypeArg);

        try
        {
            return await rootCommand.InvokeAsync(args).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            Log.Error(e.ToString());
            return 1;
        }
    }

    private static async Task RunAsync(MenuType menuType)
    {
        var log = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .CreateLogger();

        var today = KST.Now;
        await TweetMenu.RunAsync(log, today.ToDateOnly(), menuType);
    }
}
