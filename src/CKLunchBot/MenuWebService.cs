using System.Text.RegularExpressions;
using CKLunchBot.Core;
using HtmlAgilityPack;

namespace CKLunchBot;

public interface IMenuService
{
    Task<IReadOnlyCollection<MenuTable>> GetWeekMenuAsync(CancellationToken cancellationToken);
}

public sealed class MenuWebService(ILogger<MenuWebService> logger) : IMenuService, IDisposable
{
    public static readonly string RequestUri = @"https://www.ck.ac.kr/univ-life/menu";

    private readonly HttpClient _client = new();
    private readonly ILogger<MenuWebService> _logger = logger;

    public void Dispose()
    {
        _logger.LogDebug("Disposing MenuWebService...");
        _client.Dispose();
        GC.SuppressFinalize(this);
    }

    public async Task<IReadOnlyCollection<MenuTable>> GetWeekMenuAsync(CancellationToken cancellationToken)
    {
        HtmlDocument html;
        try
        {
            html = await GetHtmlAsync(cancellationToken);
        }
        catch (Exception e)
        {
            throw new HtmlLoadException($"Failed to get html from {RequestUri}.", e);
        }

        try
        {
            return ParseAllDayMenu(html);
        }
        catch (MenuParseException)
        {
            throw;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to parse html.");
            throw;
        }
    }

    private async Task<HtmlDocument> GetHtmlAsync(CancellationToken cancelToken = default)
    {
        using var stream = await _client.GetStreamAsync(RequestUri, cancelToken);

        var html = new HtmlDocument();
        html.Load(stream);

        return html;
    }

    public static IReadOnlyCollection<MenuTable> ParseAllDayMenu(HtmlDocument html)
    {
        try
        {
            var table = html.DocumentNode.SelectSingleNode(@"//table[@id='user-table']/tbody[1]");
            var weekMenuRaw = table.SelectNodes(@"./tr");
            var weekMenu = new List<MenuTable>();
            foreach (var todayMenuRaw in weekMenuRaw)
            {
                var dateString = todayMenuRaw.SelectSingleNode(@"./th").InnerText;
                var menus = todayMenuRaw.SelectNodes(@"./td");
                // Need refactoring
                var breakfast = menus[0];
                var lunch = menus[1];
                var dinner = menus[2];

                var date = ParseDateText(dateString);
                var todayMenu = new MenuTable(date)
                {
                    Breakfast = breakfast != null ? ParseMenu(breakfast) : Menu.Empty,
                    Lunch = lunch != null ? ParseMenu(lunch) : Menu.Empty,
                    Dinner = dinner != null ? ParseMenu(dinner) : Menu.Empty,
                };
                weekMenu.Add(todayMenu);
            }
            return weekMenu;
        }
        catch (Exception e)
        {
            throw new MenuParseException("Faild to parse menu html", e);
        }

        static Menu ParseMenu(HtmlNode node)
        {
            var menus = node.ChildNodes
                .Where(node => node.Name is "#text")
                .Select(node => RegexParser.MenuTextRegex().Replace(node.InnerText, " ").TrimStart().TrimEnd())
                .ToArray();
            return new Menu(menus);
        }
    }

    private static DateOnly ParseDateText(string dateText)
    {
        var matched = RegexParser.DateTextRegex().Matches(dateText);
        if (matched.Count is not 2
            || !int.TryParse(matched[0].Value, out var month)
            || !int.TryParse(matched[1].Value, out var day))
        {
            throw new MenuParseException($"Faild to parse date text [{dateText}]");
        }

        var now = KST.Now;
        var date = new DateOnly(now.Year, now.Month, now.Day);

        // parsed_month == now_month (01/02 :: 01/04)
        if (date.Month == month)
        {
            return date.AddDays(day - date.Day);
        }

        // parsed_month < now_month (02/29 :: 03/01)
        // parsed_month > now_month (03/01 :: 02/29)
        var plusDay = month > date.Month ? 1 : -1;
        while (date.Day != day)
        {
            date = date.AddDays(plusDay);
        }
        return date;
    }
}

internal partial class RegexParser
{
    [GeneratedRegex(@"[0-9]{1,2}")]
    public static partial Regex DateTextRegex();

    [GeneratedRegex(@"\s{2,}")]
    public static partial Regex MenuTextRegex();
}
