using CKLunchBot.Core;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace CKLunchBot.Functions;

public partial class MealPostFunctions(PostingService postingService, ILogger<MealPostFunctions> logger)
{
    [Function("BriefingPost")]
    public async Task RunBriefing(
        [TimerTrigger("0 0 21 * * *")] TimerInfo timer,
        CancellationToken cancellationToken)
    {
        LogFunctionTriggered("Briefing");
        await postingService.ProcessBriefingAsync(cancellationToken);
    }

    [Function("BreakfastPost")]
    public async Task RunBreakfast(
        [TimerTrigger("0 10 21 * * *")] TimerInfo timer,
        CancellationToken cancellationToken)
    {
        LogFunctionTriggered("Breakfast");
        await postingService.ProcessMealAsync(MenuType.Breakfast, cancellationToken);
    }

    [Function("LunchPost")]
    public async Task RunLunch(
        [TimerTrigger("0 0 2 * * *")] TimerInfo timer,
        CancellationToken cancellationToken)
    {
        LogFunctionTriggered("Lunch");
        await postingService.ProcessMealAsync(MenuType.Lunch, cancellationToken);
    }

    [Function("DinnerPost")]
    public async Task RunDinner(
        [TimerTrigger("0 0 7 * * *")] TimerInfo timer,
        CancellationToken cancellationToken)
    {
        LogFunctionTriggered("Dinner");
        await postingService.ProcessMealAsync(MenuType.Dinner, cancellationToken);
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "{functionName} function triggered.")]
    private partial void LogFunctionTriggered(string functionName);
}
