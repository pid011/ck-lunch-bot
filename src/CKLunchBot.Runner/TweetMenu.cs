using System.Text;
using CKLunchBot.Core;
using Serilog;
using Tweetinvi;

namespace CKLunchBot.Runner;

public static class TweetMenu
{
    public static async Task RunAsync(ILogger log, DateOnly date, MenuType menuType)
    {
        log.Information("Requesting menus from web...");

        var weekMenu = await WeekMenu.LoadAsync();
        var todayMenu = weekMenu.Find(date);
        if (todayMenu is null)
        {
            log.Warning($"Cannot found menu of {date}");
            return;
        }

        var menu = todayMenu[menuType];
        if (menu is null || menu.IsEmpty())
        {
            log.Warning($"Cannot found menu of type {menuType}");
            return;
        }

        log.Information(menu.ToString());

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

        var tweetText = GenerateContentFromMenu(date, menuType, menu);
        log.Information($"\n- CONTENT -\n{tweetText}\n- CONTENT -");

#if RELEASE
        var tweet = await Twitter.PublishTweetAsync(twitterClient, tweetText);
        log.Information($"Done! {tweet.Url}");
#else
        log.Information("Did not tweet because it's debug mode.");
#endif
    }

    private static string GenerateContentFromMenu(DateOnly date, MenuType type, Menu menu)
    {
        return new StringBuilder()
            .AppendLine($"[{date.GetFormattedKoreanString()}]")
            .AppendLine($"🥪 오늘의 청강대 {type switch
            {
                MenuType.Breakfast => "아침",
                MenuType.Lunch => "점심",
                MenuType.Dinner => "저녁",
                _ => string.Empty
            }} 메뉴는")
            .AppendLine()
            .AppendJoin(", ", menu.Menus)
            .ToString();
    }
}
