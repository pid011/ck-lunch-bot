namespace CKLunchBot;

public interface IBotConfig
{
    IReadOnlyList<string> BriefingMessage { get; }
    IReadOnlyList<string> BreakfastMessage { get; }
    IReadOnlyList<string> LunchMessage { get; }
    IReadOnlyList<string> DinnerMessage { get; }

    IReadOnlyList<string> Emoji { get; }
}

public sealed class BotConfig : IBotConfig
{
    public IReadOnlyList<string> BriefingMessage { get; set; } = [];
    public IReadOnlyList<string> BreakfastMessage { get; set; } = [];
    public IReadOnlyList<string> LunchMessage { get; set; } = [];
    public IReadOnlyList<string> DinnerMessage { get; set; } = [];

    public IReadOnlyList<string> Emoji { get; set; } = [];
}
