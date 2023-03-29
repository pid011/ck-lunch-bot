using CKLunchBot.Core;
using CKLunchBot.Runner;
using Microsoft.Azure.Functions.Worker;
using Serilog;

namespace CKLunchBot;

public class TweetMenuFunction
{
    // UTC
    private const string BreakfastCron = "0 00 22 * * 0-4"; // 07:00
    private const string LunchCron = "0 0 1 * * 1-5"; // 10:00
    private const string DinnerCron = "0 0 6 * * 1-5"; // 15:00

    // Test
    // private const string LunchCron = "0 */2 * * * *";

    [Function("TweetBreakfast")]
    public static async Task TweetBreakfastAsync([TimerTrigger(BreakfastCron)] TimerInfo timer)
    {
        await RunAsync(timer, MenuType.Breakfast).ConfigureAwait(false);
    }

    [Function("TweetLunch")]
    public static async Task TweetLunchAsync([TimerTrigger(LunchCron)] TimerInfo timer)
    {
        await RunAsync(timer, MenuType.Lunch).ConfigureAwait(false);
    }

    [Function("TweetDinner")]
    public static async Task TweetDinnerAsync([TimerTrigger(DinnerCron)] TimerInfo timer)
    {
        await RunAsync(timer, MenuType.Dinner).ConfigureAwait(false);
    }

    private static async Task RunAsync(TimerInfo timer, MenuType menuType)
    {
        var now = KST.Now;
        Log.Information($"Function started: {now} KST");

        try
        {
            await TweetMenu.RunAsync(Log.Logger, now.ToDateOnly(), menuType);
        }
        catch (Exception e)
        {
            Log.Fatal(e, "Function running faild!");
        }
        finally
        {
            if (timer.ScheduleStatus is not null)
            {
                Log.Information($"Next: {timer.ScheduleStatus.Next}");
            }
        }
    }

    public class TimerInfo
    {
        public ScheduleStatus? ScheduleStatus { get; set; }

        public bool IsPastDue { get; set; }
    }

    public class ScheduleStatus
    {
        public DateTime Last { get; set; }

        public DateTime Next { get; set; }

        public DateTime LastUpdated { get; set; }
    }
}
