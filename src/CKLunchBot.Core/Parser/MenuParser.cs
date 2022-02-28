﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace CKLunchBot.Core.Parser;

internal class MenuParser
{
    private static readonly string[] s_selfCornerTexts = { "샐프바", "셀프바", "샐프코너", "셀프코너" };

    private static readonly Dictionary<string, bool> s_checks;

    static MenuParser()
    {
        // 미리 딕셔너리로 캐싱해서 검색속도 향상
        s_checks = new Dictionary<string, bool>(s_selfCornerTexts.ToDictionary(s => s, s => true));
    }

    public static IReadOnlyCollection<TodayMenu> ParseAllDayMenu(HtmlDocument html)
    {
        try
        {
            var table = html.DocumentNode.SelectSingleNode(@"//table[@id='user-table']");

            var dates = table.SelectNodes(@"./tr/th").Skip(1).GetEnumerator();
            var menus = table.SelectNodes(@"./tbody/tr");
            var breakfasts = menus[0].Elements("td").GetEnumerator();
            var lunchs = menus[1].Elements("td").GetEnumerator();
            var dinners = menus[2].Elements("td").GetEnumerator();

            var weekMenu = new List<TodayMenu>();
            while (dates.MoveNext())
            {
                var breakfast = breakfasts.MoveNext();
                var lunch = lunchs.MoveNext();
                var dinner = dinners.MoveNext();

                var date = ParseDateText(dates.Current.InnerText);
                var todayMenu = new TodayMenu(date)
                {
                    Breakfast = breakfast ? ParseMenu(MenuType.Breakfast, breakfasts.Current) : new Menu(MenuType.Lunch),
                    Lunch = lunch ? ParseMenu(MenuType.Lunch, lunchs.Current) : new Menu(MenuType.Lunch),
                    Dinner = dinner ? ParseMenu(MenuType.Dinner, dinners.Current) : new Menu(MenuType.Dinner),
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
        return new Menu(type)
        {
            Menus = ParseMenuText(node),
            SelfCorner = ParseSelfBarText(node),
        };

        static IReadOnlyCollection<string> ParseMenuText(HtmlNode node) => node.ChildNodes
            .Where(node => node.Name is "#text")
            .Select(node => node.InnerText)
            .ToArray();

        static IReadOnlyCollection<string> ParseSelfBarText(HtmlNode node) => node.ChildNodes
            .FirstOrDefault(node => s_checks.ContainsKey(node.Name))?
            .ChildNodes
            .Where(node => node.Name is "#text")
            .Select(node => node.InnerText)
            .ToArray()
            ?? Array.Empty<string>();
    }

    private static DateOnly ParseDateText(string dateText)
    {
        var matched = Regex.Matches(dateText, @"[0-9]{1,2}");
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
