using System;
using System.Collections.Generic;
using System.Text;

namespace CKLunchBot.Core;

public record class Menu(MenuType Type)
{
    public IReadOnlyCollection<string> Menus { get; init; } = Array.Empty<string>();
    public IReadOnlyCollection<string> SpecialMenus { get; init; } = Array.Empty<string>();
    public string SpecialTitle { get; init; } = string.Empty;

    public bool IsEmpty()
    {
        return Menus.Count is 0 && SpecialMenus.Count is 0;
    }

    public override string ToString()
    {
        var builder = new StringBuilder().AppendLine($"[{Type}]");
        var emptyString = "(Empty)";

        MakeString(nameof(Menus), Menus);
        builder.AppendLine();
        MakeString(SpecialTitle, SpecialMenus);

        return builder.ToString();

        void MakeString(string name, IReadOnlyCollection<string> items)
        {
            builder!.Append($"\t<{name}> ");
            if (items.Count > 0)
            {
                builder.AppendJoin(',', items);
            }
            else
            {
                builder.Append(emptyString);
            }
        }
    }
}
