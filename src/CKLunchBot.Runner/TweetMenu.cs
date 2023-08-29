using System.Text;

using CKLunchBot.Core;

using Serilog;

namespace CKLunchBot.Runner;

public static class TweetMenu
{
    public static async Task<int> RunAsync(ILogger log, DateOnly date, MenuType menuType)
    {
        log.Information("Requesting menus from web...");

        var weekMenu = await WeekMenu.LoadAsync();
        var todayMenu = weekMenu.Find(date);
        if (todayMenu is null)
        {
            log.Warning($"Cannot found menu of {date}");
            return 0;
        }

        var menu = todayMenu[menuType];
        if (menu is null || menu.IsEmpty())
        {
            log.Warning($"Cannot found menu of type {menuType}");
            return 0;
        }

        log.Information(menu.ToString());

        log.Information("Getting TwitterClient using enviroment keys...");
        Twitter.Credentials credentials;
        try
        {
            credentials = Twitter.GetCredentialsFromEnv();
        }
        catch (EnvNotFoundException e)
        {
            log.Error($"Faild to get API key '{e.EnvKey}' from enviroment values!");
            return 1;
        }
        log.Information("Successfully get TwitterClient using enviroment.");

        var tweetText = GenerateContentFromMenu(date, menuType, menu);
        log.Information($"\n- CONTENT -\n{tweetText}\n- CONTENT -");
        log.Information($"Tweet length: {tweetText.Length}");

        log.Information("Check twitter api...");
        var user = await Twitter.GetUserInformationAsync(credentials);
        log.Debug($"Bot information: {user.Name} ({user.Id})");
        log.Information("Successfully checked twitter api.");

#if RELEASE
        try
        {
            var tweet = await Twitter.PublishTweetAsync(credentials, tweetText);
            log.Information($"Done! {tweet.Id}");
        }
        catch (Exception e)
        {
            log.Fatal(e, $"{e.Message}");
            return 1;
        }
#else
        log.Information("Did not tweet because it's debug mode.");
#endif
        return 0;
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
