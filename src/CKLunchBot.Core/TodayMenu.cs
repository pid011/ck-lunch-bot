using System;
using System.Text;

namespace CKLunchBot.Core;

public enum MenuType
{
    Breakfast,
    Lunch,
    Dinner
}

public record class TodayMenu(DateOnly Date)
{
    public Menu Breakfast { get; init; } = new Menu(MenuType.Breakfast);
    public Menu Lunch { get; init; } = new Menu(MenuType.Lunch);
    public Menu Dinner { get; init; } = new Menu(MenuType.Dinner);

    public Menu? this[MenuType type] => type switch
    {
        MenuType.Breakfast => Breakfast,
        MenuType.Lunch => Lunch,
        MenuType.Dinner => Dinner,
        _ => null
    };

    public override string ToString()
    {
        return new StringBuilder()
            .AppendLine($"<{Date.Year}-{Date.Month}-{Date.Day} {Date.DayOfWeek}>")
            .AppendLine($"{Breakfast}")
            .AppendLine($"{Lunch}")
            .Append($"{Dinner}")
            .ToString();
    }
}
