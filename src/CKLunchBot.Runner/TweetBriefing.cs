using System.Text;

using CKLunchBot.Core;

using Serilog;

namespace CKLunchBot.Runner;

public static class TweetBriefing
{
    public static async Task<int> RunAsync(ILogger log, DateOnly date)
    {
        log.Information("Requesting menus from web...");

        var weekMenu = await WeekMenu.LoadAsync();
        var todayMenu = weekMenu.Find(date);
        if (todayMenu is null)
        {
            log.Warning($"Cannot found menu of {date}!");
            return 0;
        }
        if (todayMenu.Breakfast.IsEmpty() && todayMenu.Lunch.IsEmpty() && todayMenu.Dinner.IsEmpty())
        {
            log.Warning($"All Menu of date {todayMenu.Date} is empty!");
            return 0;
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
            return 1;
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
            return 1;
        }
#else
        log.Information("Did not tweet because it's debug mode.");
#endif
        return 0;
    }

    private static string GenerateContentFromMenu(TodayMenu menu)
    {
        return new StringBuilder()
            .AppendLine($"{menu.Date.ToString("M월 d일 dddd", new System.Globalization.CultureInfo("ko-KR"))} 오늘의 청강대 학식메뉴 브리핑입니다.")
            .AppendLine()
            .Append(!menu.Breakfast.IsEmpty() ? $"[아침]\n{string.Join(", ", menu.Breakfast.Menus)}\n\n" : string.Empty)
            .Append(!menu.Lunch.IsEmpty() ? $"[점심]\n{string.Join(", ", menu.Lunch.Menus)}\n\n" : string.Empty)
            .Append(!menu.Dinner.IsEmpty() ? $"[저녁]\n{string.Join(", ", menu.Dinner.Menus)}" : string.Empty)
            .ToString();
    }
}
