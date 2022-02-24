using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CKLunchBot.Core.Menu;
using HtmlAgilityPack;

namespace CKLunchBot.Core.Parser
{
    internal class MenuParser
    {
        public static IList<TodayMenu> Parse(HtmlDocument html)
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

                    var todayMenu = new TodayMenu(ParseDateText(dates.Current.InnerText))
                    {
                        Breakfast = breakfast ? new MenuItem
                        {
                            Menus = ParseMenuText(breakfasts.Current),
                            SelfBar = ParseSelfBarText(breakfasts.Current)
                        } : null,

                        Lunch = lunch ? new MenuItem
                        {
                            Menus = ParseMenuText(lunchs.Current),
                            SelfBar = ParseSelfBarText(lunchs.Current)
                        } : null,

                        Dinner = dinner ? new MenuItem
                        {
                            Menus = ParseMenuText(dinners.Current),
                            SelfBar = ParseSelfBarText(dinners.Current)
                        } : null,
                    };
                    weekMenu.Add(todayMenu);
                }

                return weekMenu;
            }
            catch (Exception e)
            {
                throw new MenuParseException("Faild to parse menu html", e);
            }

            static DateTime ParseDateText(string dateText)
            {
                var matched = Regex.Matches(dateText, @"[0-9]{1,2}");
                if (matched.Count is not 2
                    || !int.TryParse(matched[0].Value, out var month)
                    || !int.TryParse(matched[1].Value, out var day))
                {
                    throw new MenuParseException($"Faild to parse date text [{dateText}]");
                }

                var now = KST.Now;

                // parsed_month == now_month (01/02 :: 01/04)
                if (now.Month == month)
                {
                    return now.AddDays(day - now.Day);
                }

                // parsed_month < now_month (02/29 :: 03/01)
                // parsed_month > now_month (03/01 :: 02/29)
                var plusDay = month > now.Month ? 1.0 : -1.0;
                while (now.Day != day)
                {
                    now = now.AddDays(plusDay);
                }
                return now;
            }

            static IReadOnlyList<string> ParseMenuText(HtmlNode node) =>
                node.ChildNodes
                .Where(node => node.Name is "#text")
                .Select(node => node.InnerText)
                .ToList();

            static IReadOnlyList<string> ParseSelfBarText(HtmlNode node) => node.ChildNodes
                .FirstOrDefault(node => node.Name is "샐프바")?
                .ChildNodes
                .Where(node => node.Name is "#text")
                .Select(node => node.InnerText)
                .ToList()
                ?? new List<string>();
        }
    }
}
