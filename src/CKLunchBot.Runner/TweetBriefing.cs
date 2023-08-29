using System.Text;

using CKLunchBot.Core;

using Serilog;

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
            log.Warning($"Cannot found menu of {date}!");
            return;
        }
        if (todayMenu.Breakfast.IsEmpty() && todayMenu.Lunch.IsEmpty() && todayMenu.Dinner.IsEmpty())
        {
            log.Warning($"All Menu of date {todayMenu.Date} is empty!");
            return;
        }

        log.Information(todayMenu.ToString());
        log.Information("Getting TwitterClient using enviroment keys...");
        Twitter.Credentials credentials;
        try
        {
            credentials = Twitter.GetCredentialsFromEnv();
        }
        catch (EnvNotFoundException e)
        {
            log.Error($"Faild to get API key '{e.EnvKey}' from enviroment values!");
            return;
        }
        log.Information("Successfully get TwitterClient using enviroment.");

        var tweetText = GenerateContentFromMenu(todayMenu);
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
        }
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
            .Append(!menu.Breakfast.IsEmpty() ? $"🥗 아침\n{string.Join(", ", menu.Breakfast.Menus)}\n\n" : string.Empty)
            .Append(!menu.Lunch.IsEmpty() ? $"🍱 점심\n{string.Join(", ", menu.Lunch.Menus)}\n\n" : string.Empty)
            .Append(!menu.Dinner.IsEmpty() ? $"🌭 저녁\n{string.Join(", ", menu.Dinner.Menus)}" : string.Empty)
            .ToString();
    }
}
