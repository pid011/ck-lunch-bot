using System;
using System.Text;
using System.Threading.Tasks;

using CKLunchBot.Core;

using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

using Tweetinvi;
using Tweetinvi.Models;

#if RELEASE
using Tweetinvi.Parameters;
#endif

namespace CKLunchBot.Functions;

internal record TwitterApiKeys(string ConsumerApiKey, string ConsumerSecretKey, string AccessToken, string AccessTokenSecret);

public class TweetFunction
{
    // UTC
    // private const string BreakfastCron = "0 00 22 * * 0-4"; // 07:00
    private const string LunchCron = "0 0 1 * * 1-5"; // 10:00
    private const string DinnerCron = "0 0 7 * * 1-5"; // 16:00

    // Test
    // private const string LunchCron = "0 */2 * * * *";

    private const string ConsumerApiKeyName = "TWITTER_CONSUMER_API_KEY";
    private const string ConsumerSecretKeyName = "TWITTER_CONSUMER_SECRET_KEY";
    private const string AccessTokenName = "TWITTER_ACCESS_TOKEN";
    private const string AccessTokenSecretName = "TWITTER_ACCESS_TOKEN_SECRET";

    private const string MenuTextSperator = ", ";

    /*
    [FunctionName("TweetBreakfast")]
    public static async Task TweetBreakfast([TimerTrigger(BreakfastCron)] TimerInfo timer, ILogger log)
    {
        await RunAsync(timer, log, MenuType.Breakfast);
    }
    */

    [FunctionName("TweetLunch")]
    public static async Task TweetLunch([TimerTrigger(LunchCron)] TimerInfo timer, ILogger log)
    {
        await RunAsync(timer, log, MenuType.Lunch);
    }

    [FunctionName("TweetDinner")]
    public static async Task TweetDinner([TimerTrigger(DinnerCron)] TimerInfo timer, ILogger log)
    {
        await RunAsync(timer, log, MenuType.Dinner);
    }

    private static async Task RunAsync(TimerInfo timer, ILogger log, MenuType menuType)
    {
        var now = KST.Now;
        log.LogInformation($"Function started: {now} KST");

        try
        {
            await ProcessAsync(log, now, menuType);
        }
        catch (ProcessFaildException e)
        {
            log.LogWarning(e.Message);
        }
        catch (Exception e)
        {
            log.LogError(e.ToString());
            log.LogTrace(e, "Function running faild!");
        }
        finally
        {
            log.LogInformation($"Next: {timer.Schedule.GetNextOccurrence(DateTime.Now)}");
        }
    }

    private static async Task ProcessAsync(ILogger log, DateTime now, MenuType menuType)
    {
        var date = now.ToDateOnly();

        var consumerApiKey = GetEnvVariable(ConsumerApiKeyName);
        var consumerSecretKey = GetEnvVariable(ConsumerSecretKeyName);
        var accessToken = GetEnvVariable(AccessTokenName);
        var accessTokenSecret = GetEnvVariable(AccessTokenSecretName);
        var twitterApiKeys = new TwitterApiKeys(consumerApiKey, consumerSecretKey, accessToken, accessTokenSecret);

        log.LogInformation($"Successfully loaded Twitter API keys.");
        log.LogInformation($"Menu type = {menuType}");

        log.LogInformation("Requesting menu list...");

        var weekMenu = await WeekMenu.LoadAsync();
        var todayMenu = weekMenu.Find(date);
        if (todayMenu is null)
        {
            throw new ProcessFaildException($"Cannot found menu of day {now.Day}.");
        }

        var menu = todayMenu[menuType];
        if (menu is null || menu.IsEmpty())
        {
            throw new ProcessFaildException($"Cannot found menu of type {menuType}.");
        }

        log.LogInformation(menu.ToString());

        var tweetText = MakeTweetText(date, menuType, menu);

        log.LogInformation("===============================");
        log.LogInformation(tweetText);
        log.LogInformation("===============================");

        var twitterClient = GetTwitterClient(twitterApiKeys);
        log.LogInformation("Tweeting...");

        // Test
        var authenticatedUser = await twitterClient.Users.GetAuthenticatedUserAsync();
        log.LogInformation($"Bot information: {authenticatedUser.Name}@{authenticatedUser.ScreenName}");

#if DEBUG
        log.LogInformation("Cannot tweet because it's debug mode.");
#else
        var tweet = await PublishTweetAsync(twitterClient, tweetText);
        log.LogInformation($"Done! {tweet.Url}");
#endif
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

    private static string MakeTweetText(DateOnly date, MenuType type, Menu menu)
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

#if RELEASE
    private static async Task<ITweet> PublishTweetAsync(TwitterClient twitter, string tweetText, byte[]? image = null)
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
#endif
}
