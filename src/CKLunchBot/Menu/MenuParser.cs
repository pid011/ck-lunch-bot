using CKLunchBot.Extensions;
using HtmlAgilityPack;

namespace CKLunchBot.Menu;

public interface IMenuParser
{
    IReadOnlyCollection<MenuTable> Parse(HtmlDocument html);
}

public sealed class MenuParser : IMenuParser
{
    public IReadOnlyCollection<MenuTable> Parse(HtmlDocument html)
    {
        try
        {
            var table = html.DocumentNode.SelectSingleNode(@"//table[@id='user-table']/tbody[1]");
            var rows = table.SelectNodes(@"./tr");
            var weekMenu = new List<MenuTable>();

            foreach (var row in rows)
            {
                var dateString = row.SelectSingleNode(@"./th").InnerText;
                var cells = row.SelectNodes(@"./td");

                var date = ParseDateText(dateString);
                var todayMenu = new MenuTable(date)
                {
                    Breakfast = GetMenuAt(cells, 0),
                    Lunch = GetMenuAt(cells, 1),
                    Dinner = GetMenuAt(cells, 2),
                };
                weekMenu.Add(todayMenu);
            }

            return weekMenu;
        }
        catch (Exception e)
        {
            throw new MenuParseException("Failed to parse menu html", e);
        }
    }

    private static Menu GetMenuAt(HtmlNodeCollection cells, int index)
    {
        if (cells is null || index >= cells.Count || cells[index] is not { } node)
        {
            return Menu.Empty;
        }
        return ParseMenu(node);
    }

    private static Menu ParseMenu(HtmlNode node)
    {
        var menus = node.ChildNodes
            .Where(n => n.Name is "#text")
            .Select(n => RegexParser.MenuTextRegex().Replace(n.InnerText, " ").Trim())
            .Where(text => text.Length > 0)
            .ToArray();
        return new Menu(menus);
    }

    public static DateOnly ParseDateText(string dateText)
    {
        var matched = RegexParser.DateTextRegex().Matches(dateText);
        if (matched.Count is not 2
            || !int.TryParse(matched[0].Value, out var month)
            || !int.TryParse(matched[1].Value, out var day))
        {
            throw new MenuParseException($"Failed to parse date text [{dateText}]");
        }

        var now = KST.Now;
        var year = now.Year;

        // 연도 경계 처리: 12월에 1월 메뉴가 보이거나, 1월에 12월 메뉴가 보이는 경우
        if (now.Month == 12 && month == 1)
        {
            year++;
        }
        else if (now.Month == 1 && month == 12)
        {
            year--;
        }

        return new DateOnly(year, month, day);
    }
}
