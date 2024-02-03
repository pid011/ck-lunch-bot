namespace CKLunchBot;

public interface IBotConfig
{
    TimeSpan BriefingTime { get; }
    TimeSpan BreakfastTime { get; }
    TimeSpan LunchTime { get; }
    TimeSpan DinnerTime { get; }

    IReadOnlyList<string> BriefingMessage { get; }
    IReadOnlyList<string> BreakfastMessage { get; }
    IReadOnlyList<string> LunchMessage { get; }
    IReadOnlyList<string> DinnerMessage { get; }

    IReadOnlyList<string> Emoji { get; }
}

public sealed class BotConfig : IBotConfig
{
    public TimeSpan BriefingTime { get; set; }
    public TimeSpan BreakfastTime { get; set; }
    public TimeSpan LunchTime { get; set; }
    public TimeSpan DinnerTime { get; set; }

    public IReadOnlyList<string> BriefingMessage { get; set; } = [];
    public IReadOnlyList<string> BreakfastMessage { get; set; } = [];
    public IReadOnlyList<string> LunchMessage { get; set; } = [];
    public IReadOnlyList<string> DinnerMessage { get; set; } = [];

    public IReadOnlyList<string> Emoji { get; set; } = [];
}
