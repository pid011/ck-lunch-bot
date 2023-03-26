using System.Text;
using CKLunchBot.Core;
using Serilog.Core;
using Tweetinvi;

namespace CKLunchBot.Runner;

public static class TweetMenu
{
    // UTC
    public static readonly string BreakfastCron = "0 00 22 * * 0-4"; // 07:00
    public static readonly string LunchCron = "0 0 1 * * 1-5"; // 10:00
    public static readonly string DinnerCron = "0 0 7 * * 1-5"; // 16:00

    private static readonly string s_contentFile = @"menu-tweet.txt";

    public static async Task Run(Logger log, DateOnly date, MenuType menuType)
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

        var tweetText = MakeTweetText(date, menuType, menu);
        log.Information($"\n- CONTENT -\n{tweetText}\n- CONTENT -");

#if RELEASE
        var tweet = await Twitter.PublishTweetAsync(twitterClient, tweetText);
        log.Information($"Done! {tweet.Url}");
#else
        log.Information("Did not tweet because it's debug mode.");
#endif
    }

    private static string MakeTweetText(DateOnly date, MenuType type, Menu menu)
    {
        var content = File.ReadAllText(s_contentFile);

        var menuStr = string.Join(", ", menu.Menus);
        var typeStr = type switch
        {
            MenuType.Breakfast => "아침",
            MenuType.Lunch => "점심",
            MenuType.Dinner => "저녁",
            _ => string.Empty
        };

        return new StringBuilder(content)
            .Replace("{{date}}", date.GetFormattedKoreanString())
            .Replace("{{type}}", typeStr)
            .Replace("{{menu}}", menuStr)
            .ToString();
    }
}
