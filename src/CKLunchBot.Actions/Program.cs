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
using CKLunchBot.Core.Menu;
using CKLunchBot.Core.Requester;
using CKLunchBot.Core.Utils;
using Tweetinvi;

#if RELEASE
using Tweetinvi.Models;
#endif

using static System.Console;
using static Bullseye.Targets;
using static CKLunchBot.Actions.BotService;

const string ConsumerApiKeyName = "TWITTER_CONSUMER_API_KEY";
const string ConsumerSecretKeyName = "TWITTER_CONSUMER_SECRET_KEY";
const string AccessTokenName = "TWITTER_ACCESS_TOKEN";
const string AccessTokenSecretName = "TWITTER_ACCESS_TOKEN_SECRET";
const string MealApiKeyName = "MEAL_API_KEY";

var command = new RootCommand
{
    new Option(new[]{"--time"}, "Meal time")
    {
        Argument = new Argument<string>()
    }
};

command.Add(new Argument("targets") { Arity = ArgumentArity.ZeroOrMore });
foreach (var option in Options.Definitions)
{
    command.Add(
        new Option(
            new[] { option.ShortName, option.LongName }.Where(n => !string.IsNullOrWhiteSpace(n)).ToArray(),
            option.Description));
}

command.Handler = CommandHandler.Create<string>(async time =>
{
    var commandline = command.Parse(args);
    var targets = commandline.CommandResult.Tokens.Select(token => token.Value);
    var options = new Bullseye.Options(Options.Definitions.Select(o => (o.LongName, commandline.ValueForOption<bool>(o.LongName))));

    MealTimeFlags flag;
    switch (time)
    {
        case "breakfast":
            flag = MealTimeFlags.Breakfast;
            break;

        case "lunch":
            flag = MealTimeFlags.Lunch;
            break;

        case "dorm_lunch":
            flag = MealTimeFlags.DormLunch;
            break;

        case "dinner":
            flag = MealTimeFlags.Dinner;
            break;

        default:
            WriteLine("Invalid flag name.");
            return;
    }

    AssemblyName botInfo = Assembly.GetExecutingAssembly().GetName();
    WriteLine($"[{botInfo.Name} v{botInfo.Version.ToString(3)}]");

    string coreDllPath = Path.Combine(AppContext.BaseDirectory, "CKLunchBot.Core.dll");
    AssemblyName coreInfo = Assembly.LoadFrom(coreDllPath).GetName();
    WriteLine($"[{coreInfo.Name} v{coreInfo.Version.ToString(3)}]");

    WriteLine($"\nAction started time: {DateTime.UtcNow.AddHours(9)} KST\n");

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

        var mealApiKey = Environment.GetEnvironmentVariable(MealApiKeyName, EnvironmentVariableTarget.Process)
            ?? throw new TargetFailedException($"Faild to get enviroment value name of {MealApiKeyName}");

        config = new BotConfig(mealApiKey, twitterKeys);
    });

    RestaurantsWeekMenu menus = null;
    Target("request-menu", DependsOn("get-keys"), async () =>
    {
        WriteLine("Requesting menu list...");
        var menuRequester = new MenuRequester(config.MealApiKey);
        menus = await GetWeekMenu(menuRequester);

        if (menus.Count == 0)
        {
            throw new TargetFailedException("There is no requested menu.");
        }
    });

    byte[] image = null;
    Target("generate-image", DependsOn("request-menu"), async () =>
    {
        WriteLine("Generating menu image...");

        try
        {
            image = await GenerateImageAsync(flag, menus);
        }
        catch (NoProvidedMenuException e)
        {
            var sb = new StringBuilder();
            sb.Append("No provided menu name: ");
            for (int i = 0; i < e.RestaurantsName.Length; i++)
            {
                sb.Append(e.RestaurantsName[i].ToString());
                if (i < e.RestaurantsName.Length - 1)
                {
                    sb.Append(", ");
                }
            }
            WriteLine(sb.ToString());

            throw new TargetFailedException("There is no provided menu.");
        }
    });

    Target("publish-tweet", DependsOn("get-keys", "generate-image"), async () =>
    {
        WriteLine("Publishing tweet...");
        var tweetText = new StringBuilder()
            .Append(TimeUtils.GetFormattedKoreaTime(DateTime.UtcNow))
            .Append(" 오늘의 청강대 ")
            .Append(flag switch
            {
                MealTimeFlags.Breakfast => "아침",
                MealTimeFlags.Lunch => "점심",
                MealTimeFlags.DormLunch => "기숙사 점심",
                MealTimeFlags.Dinner => "저녁",
                _ => throw new TargetFailedException()
            })
            .Append(" 메뉴는!")
            .ToString();

        TwitterClient twitter = await ConnectToTwitter(config.TwitterConfig);

#if DEBUG
        WriteLine("The bot didn't tweet because it's Debug mode.");
#endif

#if RELEASE
        if (image is null)
        {
            throw new TargetFailedException("Tweet image is null");
        }
        ITweet tweet = await PublishTweet(twitter, tweetText.ToString(), image);
        WriteLine($"tweet: {tweet}");
        WriteLine("Tweet publish completed.");
#endif
    });

    Target("default", DependsOn("publish-tweet"));

    await RunTargetsAndExitAsync(targets, options);
});

return await command.InvokeAsync(args);
