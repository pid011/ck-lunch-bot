using System.Text;
using CKLunchBot.Core;
using Serilog;
using Tweetinvi;

namespace CKLunchBot.Runner;

public static class TweetBriefing
{
    public static async Task RunAsync(ILogger log, DateOnly date)
    {
        log.Information("Requesting menus from web...");

        var weekMenu = await WeekMenu.LoadAsync();
        var todayMenu = weekMenu.Find(date);
        if (todayMenu is null)
        {
            log.Warning($"Cannot found menu of {date}");
            return;
        }
        log.Information(todayMenu.ToString());
        log.Information("Getting TwitterClient using enviroment keys...");
        TwitterClient twitterClient;
        try
        {
            twitterClient = Twitter.GetClientUsingKeys();
        }
        catch (EnvNotFoundException e)
        {
            log.Error($"Faild to get API key '{e.EnvKey}' from enviroment values!");
            return;
        }
        log.Information("Successfully get TwitterClient using enviroment.");

        log.Information("Checking bot account...");
        var authenticatedUser = await twitterClient.Users.GetAuthenticatedUserAsync();
        log.Information($"Bot information: {authenticatedUser.Name}@{authenticatedUser.ScreenName}");

        var tweetText = GenerateContentFromMenu(todayMenu);
        log.Information($"\n- CONTENT -\n{tweetText}\n- CONTENT -");

#if RELEASE
        var tweet = await Twitter.PublishTweetAsync(twitterClient, tweetText);
        log.Information($"Done! {tweet.Url}");
#else
        log.Information("Did not tweet because it's debug mode.");
#endif
    }

    private static string GenerateContentFromMenu(TodayMenu menu)
    {
        return new StringBuilder()
            .AppendLine($"[{menu.Date.GetFormattedKoreanString()}]")
            .AppendLine("금일 청강대 학식메뉴 브리핑입니다!")
            .AppendLine()
            .Append(!menu.Breakfast.IsEmpty() ? $"🥗 아침\n{string.Join(", ", menu.Breakfast.Menus)}\n" : string.Empty)
            .Append(!menu.Lunch.IsEmpty() ? $"🍱 점심\n{string.Join(", ", menu.Lunch.Menus)}\n" : string.Empty)
            .Append(!menu.Dinner.IsEmpty() ? $"🌭 저녁\n{string.Join(", ", menu.Dinner.Menus)}" : string.Empty)
            .ToString();
    }
}
