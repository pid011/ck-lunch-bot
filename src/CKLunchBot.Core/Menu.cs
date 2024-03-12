using System;
using System.Collections.Generic;
using System.Text;

namespace CKLunchBot.Core;

public enum MenuType
{
    Breakfast,
    Lunch,
    Dinner
}

public class MenuTable(DateOnly date)
{
    public DateOnly Date { get; } = date;
    public required Menu Breakfast { get; init; }
    public required Menu Lunch { get; init; }
    public required Menu Dinner { get; init; }

    public Menu this[MenuType type] => type switch
    {
        MenuType.Breakfast => Breakfast,
        MenuType.Lunch => Lunch,
        MenuType.Dinner => Dinner,
        _ => throw new NotImplementedException()
    };

    public override string ToString()
    {
        return new StringBuilder()
            .AppendLine($"<{Date:yyyy-MM-dd}>")
            .AppendLine($"Breakfast: {Breakfast}")
            .AppendLine($"Lunch: {Lunch}")
            .Append($"Dinner: {Dinner}")
            .ToString();
    }
}

public class Menu(IReadOnlyCollection<string> menus)
{
    public static Menu Empty { get; } = new([]);
    public IReadOnlyCollection<string> Menus { get; } = menus;

    public bool IsEmpty()
    {
        return Menus.Count is 0;
    }

    public override string ToString()
    {
        return string.Join(", ", Menus);
    }
}
