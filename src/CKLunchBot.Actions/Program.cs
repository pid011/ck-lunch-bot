using System;
using System.CommandLine;
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
        var now = KST.Now;
        var date = now.ToDateOnly();
        Log.Information($"Action started time: {now} KST\n");

        var consumerApiKey = GetEnvVariable(ConsumerApiKeyName);
        var consumerSecretKey = GetEnvVariable(ConsumerSecretKeyName);
        var accessToken = GetEnvVariable(AccessTokenName);
        var accessTokenSecret = GetEnvVariable(AccessTokenSecretName);
        var twitterApiKeys = new TwitterApiKeys(consumerApiKey, consumerSecretKey, accessToken, accessTokenSecret);
        Log.Information($"Successfully loaded Twitter API keys.");

        Log.Information("Requesting menu list...");

        var weekMenu = await WeekMenu.LoadAsync();
        var todayMenu = weekMenu.Find(date);
        if (todayMenu is null)
        {
            Log.Warning($"Cannot found menu of day {now.Day}.");
            return;
        }

        var menu = todayMenu[menuType];
        if (menu is null || menu.IsEmpty())
        {
            Log.Warning($"Cannot found menu of type {menuType}.");
            return;
        }

        Log.Information(menu.ToString());

        Log.Information("Generating image...");
        var image = await MenuImageGenerator.GenerateAsync(date, menuType, menu);
        Log.Information($"Image generated. length={image.Length}");

        var twitterClient = GetTwitterClient(twitterApiKeys);
        Log.Information("Tweeting...");
        var tweetMessage = GetMenuTweetText(date, menuType);
        Log.Information($"\n- tweet message -\n{tweetMessage}\n- tweet message -");

#if DEBUG
        Log.Information("Cannot tweet because it's debug mode.");
#else
        var tweet = await PublishTweetAsync(twitterClient, tweetMessage, image);
        Log.Information($"Done! {tweet.Url}");
#endif

        return;
    }

    private static string GetEnvVariable(string key)
    {
        return Environment.GetEnvironmentVariable(key, EnvironmentVariableTarget.Process)
            ?? throw new Exception($"Faild to get enviroment value name of {key}");
    }

    private static TwitterClient GetTwitterClient(TwitterApiKeys keys)
    {
        var credentials = new TwitterCredentials
        {
            ConsumerKey = keys.ConsumerApiKey,
            ConsumerSecret = keys.ConsumerSecretKey,
            AccessToken = keys.AccessToken,
            AccessTokenSecret = keys.AccessTokenSecret
        };

        return new TwitterClient(credentials);
    }

    private static string GetMenuTweetText(DateOnly date, MenuType type)
    {
        var builder = new StringBuilder()
            .AppendLine($"[{date.GetFormattedKoreanString()}]")
            .Append("🥪 오늘의 청강대 ")
            .Append(type switch
            {
                MenuType.Breakfast => "아침",
                MenuType.Lunch => "점심",
                MenuType.Dinner => "저녁",
                _ => string.Empty
            })
            .AppendLine(" 메뉴는")
            .AppendLine()
            .AppendJoin(MenuTextSperator, menu.Menus);

        if (menu.SpecialMenus.Count > 0)
        {
            builder
                .AppendLine()
                .AppendLine()
                .AppendLine($"<{menu.SpecialTitle}>")
                .AppendJoin(MenuTextSperator, menu.SpecialMenus);
        }

        return builder.ToString();
    }

    public static async Task<ITweet> PublishTweetAsync(TwitterClient twitter, string tweetText, byte[]? image = null)
    {
        if (image is null)
        {
            return await twitter.Tweets.PublishTweetAsync(new PublishTweetParameters(tweetText.ToString()));
        }

        IMedia uploadedImage = await twitter.Upload.UploadTweetImageAsync(image);
        return await twitter.Tweets.PublishTweetAsync(new PublishTweetParameters(tweetText.ToString())
        {
            Medias = { uploadedImage }
        });
    }
}
