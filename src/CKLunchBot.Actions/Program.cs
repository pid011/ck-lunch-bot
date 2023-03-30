using System;
using System.CommandLine;
using System.Threading.Tasks;
using CKLunchBot.Core;
using CKLunchBot.Runner;
using Serilog;

namespace CKLunchBot.Actions;

internal class Program
{
    private static readonly ILogger s_log = new LoggerConfiguration()
        .MinimumLevel.Debug()
        .WriteTo.Console()
        .CreateLogger();

    private static async Task<int> Main(string[] args)
    {
        var briefingCmd = new Command("briefing");
        briefingCmd.SetHandler(RunBriefingAsync);

        var menuCmd = new Command("menu");
        var menuTypeArg = new Argument<MenuType>("menuType");
        menuCmd.AddArgument(menuTypeArg);
        menuCmd.SetHandler(RunTweetMenuAsync, menuTypeArg);

        var rootCommand = new RootCommand();
        rootCommand.AddCommand(briefingCmd);
        rootCommand.AddCommand(menuCmd);

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

    private static async Task RunBriefingAsync()
    {
        await TweetBriefing.RunAsync(s_log, KST.Now.ToDateOnly()).ConfigureAwait(false);
    }

    private static async Task RunTweetMenuAsync(MenuType menuType)
    {
        await TweetMenu.RunAsync(s_log, KST.Now.ToDateOnly(), menuType).ConfigureAwait(false);
    }
}
