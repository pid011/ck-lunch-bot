// Copyright (c) Sepi. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Bullseye;
using CKLunchBot.Actions;
using CKLunchBot.Core.ImageProcess;
using CKLunchBot.Core.Menu;
using CKLunchBot.Core.Utils;
using Serilog;
using Tweetinvi;

#if RELEASE
using Tweetinvi.Models;
#endif

using static Bullseye.Targets;
using static CKLunchBot.Actions.BotService;

const string ConsumerApiKeyName = "TWITTER_CONSUMER_API_KEY";
const string ConsumerSecretKeyName = "TWITTER_CONSUMER_SECRET_KEY";
const string AccessTokenName = "TWITTER_ACCESS_TOKEN";
const string AccessTokenSecretName = "TWITTER_ACCESS_TOKEN_SECRET";

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .CreateLogger();

var command = new RootCommand
{
    new Argument<string>("targets") { Arity = ArgumentArity.ZeroOrMore },
    new Option<string>(new string[]{"--type"}, "Menu type") { IsRequired = true },
    new Option<int>(new string[]{"--day"}, () => 0, "Day to get menu")
};

foreach (var option in Options.Definitions)
{
    command.Add(
        new Option(
            new[] { option.ShortName, option.LongName }.Where(n => !string.IsNullOrWhiteSpace(n)).ToArray(),
            option.Description));
}

command.Handler = CommandHandler.Create(async () =>
{
    var commandline = command.Parse(args);
    var targets = commandline.CommandResult.Tokens.Select(token => token.Value);
    var options = new Bullseye.Options(Options.Definitions.Select(o => (o.LongName, commandline.ValueForOption<bool>(o.LongName))));

    var typeText = commandline.ValueForOption<string>("--type");
    var day = commandline.ValueForOption<int>("--day");

    MenuType flag;
    switch (typeText)
    {
        case "breakfast":
            flag = MenuType.Breakfast;
            break;

        case "lunch":
            flag = MenuType.Lunch;
            break;

        case "dinner":
            flag = MenuType.Dinner;
            break;

        default:
            Log.Information("Invalid flag name.");
            return;
    }

    AssemblyName botInfo = Assembly.GetExecutingAssembly().GetName();
    Log.Information($"[{botInfo.Name} v{botInfo.Version.ToString(3)}]");

    string coreDllPath = Path.Combine(AppContext.BaseDirectory, "CKLunchBot.Core.dll");
    AssemblyName coreInfo = Assembly.LoadFrom(coreDllPath).GetName();
    Log.Information($"[{coreInfo.Name} v{coreInfo.Version.ToString(3)}]");

    Log.Information($"\nAction started time: {DateTime.UtcNow.AddHours(9)} KST\n");

    BotConfig config = null;
    Target("get-keys", () =>
    {
        var consumerApiKey = Environment.GetEnvironmentVariable(ConsumerApiKeyName, EnvironmentVariableTarget.Process)
            ?? throw new TargetFailedException($"Faild to get enviroment value name of {ConsumerApiKeyName}");

        var consumerSecretKey = Environment.GetEnvironmentVariable(ConsumerSecretKeyName, EnvironmentVariableTarget.Process)
            ?? throw new TargetFailedException($"Faild to get enviroment value name of {ConsumerSecretKeyName}");

        var accessToken = Environment.GetEnvironmentVariable(AccessTokenName, EnvironmentVariableTarget.Process)
            ?? throw new TargetFailedException($"Faild to get enviroment value name of {AccessTokenName}");

        var accessTokenSecret = Environment.GetEnvironmentVariable(AccessTokenSecretName, EnvironmentVariableTarget.Process)
            ?? throw new TargetFailedException($"Faild to get enviroment value name of {AccessTokenSecretName}");

        var twitterKeys = new TwitterApiKeys(consumerApiKey, consumerSecretKey, accessToken, accessTokenSecret);

        config = new BotConfig(twitterKeys);
    });

    WeekMenu weekMenu = null;
    Target("request-menu", DependsOn("get-keys"), async () =>
    {
        Log.Information("Requesting menu list...");
        weekMenu = await GetWeekMenuAsync();

        Log.Debug($"Responsed menu list:\n{weekMenu}");
    });

    TodayMenu todayMenu = null;
    Target("select-menu", DependsOn("request-menu"), () =>
    {
        try
        {
            if (day is 0)
            {
                Log.Information("Selecting todays menu...");
                todayMenu = SelectTodayMenu(weekMenu);
                Log.Debug($"Selected today menu:\n{todayMenu}");
            }
            else
            {
                Log.Information($"Selecting menu for {day}");
                todayMenu = SelectMenu(weekMenu, day);
                Log.Debug($"Selected menu for {day}:\n{todayMenu}");
            }
        }
        catch (NoProvidedMenuException e)
        {
            throw new TargetFailedException("There is no provided menu.", e);
        }
    });

    byte[] image = null;
    Target("generate-image", DependsOn("select-menu"), async () =>
    {
        Log.Information("Generating menu image...");

        try
        {
            image = await todayMenu.MakeImageAsync(flag);
        }
        catch (MenuImageGenerateException e)
        {
            throw new TargetFailedException("Faild to generate image.", e);
        }
    });

    Target("publish-tweet", DependsOn("get-keys", "generate-image"), async () =>
    {
        Log.Information("Publishing tweet...");
        var tweetText = new StringBuilder()
            .Append(TimeUtils.GetFormattedKoreaTime(todayMenu.Date))
            .Append(" 오늘의 청강대 ")
            .Append(flag switch
            {
                MenuType.Breakfast => "아침",
                MenuType.Lunch => "점심",
                MenuType.Dinner => "저녁",
                _ => throw new TargetFailedException()
            })
            .Append(" 메뉴는!")
            .ToString();
        Log.Debug($"tweetText: {tweetText}");
        TwitterClient twitter = await ConnectToTwitterAsync(config.TwitterConfig);

#if DEBUG
        Log.Information("The bot didn't tweet because it's Debug mode.");
#endif

#if RELEASE
        if (image is null)
        {
            throw new TargetFailedException("Tweet image is null");
        }
        ITweet tweet = await PublishTweetAsync(twitter, tweetText.ToString(), image);
        Log.Information($"tweet: {tweet}");
        Log.Information("Tweet publish completed.");
#endif
    });

    Target("default", DependsOn("publish-tweet"));

    await RunTargetsAndExitAsync(targets, options);
});

return await command.InvokeAsync(args);
