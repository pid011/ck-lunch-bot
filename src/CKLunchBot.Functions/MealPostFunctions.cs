using CKLunchBot.Core;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace CKLunchBot;

public class MealPostFunctions(PostingHelper helper, ILogger<MealPostFunctions> logger)
{
    [Function("BriefingPost")]
    public async Task RunBriefing(
        [TimerTrigger("0 0 21 * * *")] TimerInfo timer,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Briefing function triggered at {time} UTC", DateTime.UtcNow);
        await helper.ProcessBriefingAsync(cancellationToken);
    }

    [Function("BreakfastPost")]
    public async Task RunBreakfast(
        [TimerTrigger("0 10 21 * * *")] TimerInfo timer,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Breakfast function triggered at {time} UTC", DateTime.UtcNow);
        await helper.ProcessMealAsync(MenuType.Breakfast, cancellationToken);
    }

    [Function("LunchPost")]
    public async Task RunLunch(
        [TimerTrigger("0 0 2 * * *")] TimerInfo timer,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Lunch function triggered at {time} UTC", DateTime.UtcNow);
        await helper.ProcessMealAsync(MenuType.Lunch, cancellationToken);
    }

    [Function("DinnerPost")]
    public async Task RunDinner(
        [TimerTrigger("0 0 7 * * *")] TimerInfo timer,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Dinner function triggered at {time} UTC", DateTime.UtcNow);
        await helper.ProcessMealAsync(MenuType.Dinner, cancellationToken);
    }
}
