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
        public static IList<TodayMenu> ParseMenu(HtmlDocument html)
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
                    breakfasts.MoveNext();
                    lunchs.MoveNext();
                    dinners.MoveNext();

                    var rawDate = dates.Current.InnerText;
                    var rawBreakfast = breakfasts.Current.InnerHtml;
                    var rawLunch = lunchs.Current.InnerHtml;
                    var rawDinner = dinners.Current.InnerHtml;

                    var date = ParseDateText(rawDate);
                    var breakfast = ParseMenuText(rawBreakfast);
                    var lunch = ParseMenuText(rawLunch);
                    var dinner = ParseMenuText(rawDinner);

                    var todayMenu = new TodayMenu(date)
                    {
                        Breakfast = breakfast.ToList(),
                        Lunch = lunch.ToList(),
                        Dinner = dinner.ToList()
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

        private static DateTime ParseDateText(string dateText)
        {
            var matched = Regex.Matches(dateText, @"[0-9]{1,2}");
            if (matched.Count is not 2
                || !int.TryParse(matched[0].Value, out var month)
                || !int.TryParse(matched[1].Value, out var day))
            {
                throw new MenuParseException($"Faild to parse date text [{dateText}]");
            }

            // BUG: DateTime.Now가 2021년인데 메뉴 날짜가 2022년 1월 1일이라서 년도가 다른 경우 잘못된 날짜가 되어 버림
            return new DateTime(DateTime.Now.Year, month, day);
        }

        private static IEnumerable<string> ParseMenuText(string menuText)
        {
            return menuText.Split("<br>").Where(s => !string.IsNullOrWhiteSpace(s));
        }
    }
}
