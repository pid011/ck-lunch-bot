using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using HtmlAgilityPack;

namespace CKLunchBot.Core.Parser;

internal partial class MenuParser
{
    [GeneratedRegex("[0-9]{1,2}")]
    private static partial Regex DateParser();

    private static readonly string[] s_specialMenuTitles =
    {
        "샐프바", "셀프바", "샐프코너", "셀프코너", "단품메뉴", "단품 메뉴"
    };

    private static readonly Dictionary<string, bool> s_checks;

    static MenuParser()
    {
        // 미리 딕셔너리로 캐싱해서 검색속도 향상
        s_checks = new Dictionary<string, bool>(s_specialMenuTitles.ToDictionary(s => s, s => true));
    }

    public static IReadOnlyCollection<TodayMenu> ParseAllDayMenu(HtmlDocument html)
    {
        try
        {
            var table = html.DocumentNode.SelectSingleNode(@"//table[@id='user-table']/tbody[1]");
            //var dates = table.SelectNodes(@"./tr/th").Skip(1).GetEnumerator();
            var weekMenuRaw = table.SelectNodes(@"./tr");
            var weekMenu = new List<TodayMenu>();
            foreach (var todayMenuRaw in weekMenuRaw)
            {
                var dateString = todayMenuRaw.SelectSingleNode(@"./th").InnerText;
                var menus = todayMenuRaw.SelectNodes(@"./td");
                // Need refactoring
                var breakfast = menus[0];
                var lunch = menus[1];
                var dinner = menus[2];

                var date = ParseDateText(dateString);
                var todayMenu = new TodayMenu(date)
                {
                    Breakfast = breakfast != null ? ParseMenu(MenuType.Breakfast, breakfast) : new Menu(MenuType.Breakfast),
                    Lunch = lunch != null ? ParseMenu(MenuType.Lunch, lunch) : new Menu(MenuType.Lunch),
                    Dinner = dinner != null ? ParseMenu(MenuType.Dinner, dinner) : new Menu(MenuType.Dinner),
                };
                weekMenu.Add(todayMenu);
            }
            return weekMenu;
        }
        catch (Exception e)
        {
            throw new MenuParseException("Faild to parse menu html", e);
        }
    }

    private static Menu ParseMenu(MenuType type, HtmlNode node)
    {
        var menus = ParseMenuText(node);
        var (specialMenuName, specialMenus) = ParseSpecialMenuText(node);

        return new Menu(type)
        {
            Menus = menus,
            SpecialTitle = specialMenuName,
            SpecialMenus = specialMenus
        };

        static IReadOnlyCollection<string> ParseMenuText(HtmlNode node) => node.ChildNodes
            .Where(node => node.Name is "#text")
            .Select(node => node.InnerText)
            .ToArray();

        static (string Name, IReadOnlyCollection<string> Texts) ParseSpecialMenuText(HtmlNode node)
        {
            var specialMenuNode = node.ChildNodes.FirstOrDefault(node => s_checks.ContainsKey(node.Name));
            if (specialMenuNode is null)
            {
                return (string.Empty, Array.Empty<string>());
            }

            var name = specialMenuNode.Name;
            var texts = specialMenuNode.ChildNodes
                .Where(node => node.Name is "#text")
                .Select(node => node.InnerText)
                .ToArray();

            return (name, texts);
        }
    }

    private static DateOnly ParseDateText(string dateText)
    {
        var matched = DateParser().Matches(dateText);
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
