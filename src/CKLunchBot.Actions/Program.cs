#nullable enable

using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Text;
using System.Threading.Tasks;
using CKLunchBot.Core;
using CKLunchBot.Core.Drawing;
using Serilog;
using Tweetinvi;
using Tweetinvi.Models;
using Tweetinvi.Parameters;

namespace CKLunchBot.Actions;

internal record TwitterApiKeys(string ConsumerApiKey, string ConsumerSecretKey, string AccessToken, string AccessTokenSecret);

internal class Program
{
    private const string ConsumerApiKeyName = "TWITTER_CONSUMER_API_KEY";
    private const string ConsumerSecretKeyName = "TWITTER_CONSUMER_SECRET_KEY";
    private const string AccessTokenName = "TWITTER_ACCESS_TOKEN";
    private const string AccessTokenSecretName = "TWITTER_ACCESS_TOKEN_SECRET";

    private static async Task<int> Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .CreateLogger();

        var rootCommand = new RootCommand
        {
            new Argument<MenuType>("menuType"),
            new Option<int>(new string[]{"--day"}, () => 0, "Day to get menu")
        };

        rootCommand.Handler = CommandHandler.Create<MenuType, int>(Run);

        var result = await rootCommand.InvokeAsync(args).ConfigureAwait(false);

        if (result == 0)
        {
            Log.Information("Action done.");
        }
        else
        {
            Log.Warning("Action faild.");
        }

        return result;
    }

    private static async Task<int> Run(MenuType menuType, int day)
    {
        var now = KST.Now;

        Log.Information($"\nAction started time: {now} KST\n");

        TwitterApiKeys? twitterKeys = null;
        try
        {
            var consumerApiKey = GetEnvVariable(ConsumerApiKeyName);
            var consumerSecretKey = GetEnvVariable(ConsumerSecretKeyName);
            var accessToken = GetEnvVariable(AccessTokenName);
            var accessTokenSecret = GetEnvVariable(AccessTokenSecretName);
            twitterKeys = new TwitterApiKeys(consumerApiKey, consumerSecretKey, accessToken, accessTokenSecret);
        }
        catch (Exception e)
        {
            Log.Error(e.Message);
            return 1;
        }

        Log.Information("Requesting menu list...");
        var weekMenu = await WeekMenu.LoadAsync();

        Log.Debug($"Responsed menu list:\n{weekMenu}");

        TodayMenu? todayMenu;

        var date = now.ToDateOnly();
        if (day is 0)
        {
            day = date.Day;
            todayMenu = weekMenu.Find(date);
        }
        else
        {
            todayMenu = weekMenu.Find(date.AddDays(day - date.Day));
        }

        if (todayMenu is null)
        {
            Log.Warning($"Cannot found menu of day {day}");
            return 0;
        }
        Log.Debug($"Target menu:\n{todayMenu}");

        var targetMenu = todayMenu![menuType];
        if (targetMenu is null || targetMenu.IsEmpty())
        {
            Log.Warning($"{menuType}의 메뉴가 존재하지 않습니다.");
            return 0;
        }

        Log.Information("Generating menu image...");

        byte[] image;
        try
        {
            image = await MenuImageGenerator.GenerateAsync(todayMenu.Date, menuType, targetMenu);
        }
        catch (MenuImageGenerateException e)
        {
            Log.Warning(e.Message);
            return 1;
        }

        Log.Information("Publishing tweet...");
        var tweetText = new StringBuilder()
            .Append(todayMenu.Date.GetFormattedKoreanString())
            .Append(" 오늘의 청강대 ")
            .Append(menuType switch
            {
                MenuType.Breakfast => "아침",
                MenuType.Lunch => "점심",
                MenuType.Dinner => "저녁",
                _ => string.Empty
            })
            .Append(" 메뉴는!")
            .ToString();
        Log.Debug($"tweetText: {tweetText}");
        TwitterClient twitter = await ConnectToTwitterAsync(twitterKeys);

#if DEBUG
        Log.Information("The bot didn't tweet because it's Debug mode.");
#endif

#if RELEASE
        if (image is null)
        {
            Log.Error("Tweet image is null");
            return 1;
        }

        try
        {
            ITweet tweet = await PublishTweetAsync(twitter, tweetText.ToString(), image);
            Log.Information($"tweet: {tweet}");
            Log.Information("Tweet publish completed.");
        }
        catch (Exception e)
        {
            Log.Error($"Faild to tweet menu.");
            Log.Error($"{e}");
            return 1;
        }
#endif

        return 0;
    }

    private static string GetEnvVariable(string key)
    {
        return Environment.GetEnvironmentVariable(key, EnvironmentVariableTarget.Process)
            ?? throw new Exception($"Faild to get enviroment value name of {key}");
    }

    public static async Task<TwitterClient> ConnectToTwitterAsync(TwitterApiKeys keys)
    {
        var client = new TwitterClient(keys.ConsumerApiKey, keys.ConsumerSecretKey, keys.AccessToken, keys.AccessTokenSecret);
        IAuthenticatedUser authenticatedUser = await client.Users.GetAuthenticatedUserAsync();

        Log.Debug("------------ Bot information ------------");
        Log.Debug($"Name: {authenticatedUser.Name} @{authenticatedUser.ScreenName}");
        Log.Debug($"ID:   {authenticatedUser.Id}");
        Log.Debug("-----------------------------------------");

        return client;
    }

    public static async Task<ITweet> PublishTweetAsync(TwitterClient twitter, string tweetText, byte[]? image = null)
    {
        ITweet tweet;

        if (image is null)
        {
            tweet = await twitter.Tweets.PublishTweetAsync(new PublishTweetParameters(tweetText.ToString()));
        }
        else
        {
            IMedia uploadedImage = await twitter.Upload.UploadTweetImageAsync(image);
            tweet = await twitter.Tweets.PublishTweetAsync(new PublishTweetParameters(tweetText.ToString())
            {
                Medias = { uploadedImage }
            });
        }

        return tweet;
    }
}
