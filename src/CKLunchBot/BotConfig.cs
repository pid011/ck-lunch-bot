namespace CKLunchBot;

public interface IBotConfig
{
    TimeSpan Briefing { get; }
    TimeSpan Breakfast { get; }
    TimeSpan Lunch { get; }
    TimeSpan Dinner { get; }
}

public sealed class BotConfig : IBotConfig
{
    public TimeSpan Briefing { get; set; }
    public TimeSpan Breakfast { get; set; }
    public TimeSpan Lunch { get; set; }
    public TimeSpan Dinner { get; set; }
}
