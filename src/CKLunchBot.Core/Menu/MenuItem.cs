// Copyright (c) Sepi. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace CKLunchBot.Core.Menu
{
    public class MenuItem
    {
        public MenuItem(RawMenuItem raw)
        {
            RestaurantName = StringToRestaurants(raw.RestaurantName);
            SundayMenu = SplitMenu(raw.SundayMenuRawText);
            MondayMenu = SplitMenu(raw.MondayMenuRawText);
            TuesdayMenu = SplitMenu(raw.TuesdayMenuRawText);
            WednesdayMenu = SplitMenu(raw.WednesdayMenuRawText);
            ThursdayMenu = SplitMenu(raw.ThursdayMenuRawText);
            FridayMenu = SplitMenu(raw.FridayMenuRawText);
            SaturdayMenu = SplitMenu(raw.SaturdayMenuRawText);

            RawMenu = raw;
        }

        public string[] this[DayOfWeek dayOfWeek] => dayOfWeek switch
        {
            DayOfWeek.Monday => MondayMenu,
            DayOfWeek.Tuesday => TuesdayMenu,
            DayOfWeek.Wednesday => WednesdayMenu,
            DayOfWeek.Thursday => ThursdayMenu,
            DayOfWeek.Friday => FridayMenu,
            DayOfWeek.Saturday => SaturdayMenu,
            DayOfWeek.Sunday => SundayMenu,
            _ => throw new ArgumentException(null, nameof(dayOfWeek))
        };

        public RawMenuItem RawMenu { get; }

        public Restaurants RestaurantName { get; }

        public string[] SundayMenu { get; }

        public string[] MondayMenu { get; }

        public string[] TuesdayMenu { get; }

        public string[] WednesdayMenu { get; }

        public string[] ThursdayMenu { get; }

        public string[] FridayMenu { get; }

        public string[] SaturdayMenu { get; }

        /// <summary>
        /// Convert raw menu text to string array. If raw menu text is null, return empty string array.
        /// </summary>
        /// <param name="rawMenuText">raw menu text. example: "rise\r\nkimchi"</param>
        /// <returns>splited menus</returns>
        private static string[] SplitMenu(string rawMenuText)
        {
            var regexResult = Regex.Split(rawMenuText ?? string.Empty, "\r\n").ToList();
            regexResult.RemoveAll(x => string.IsNullOrWhiteSpace(x));
            return regexResult.ToArray();
        }

        private static Restaurants StringToRestaurants(string name)
        {
            const string Daban = "다반";
            const string NankatsuNanUdong = "난카츠난우동";
            const string TangAndJjigae = "탕&찌개차림";
            const string YukHaeBab = "육해밥";
            const string DormBreakfast = "아침";
            const string DormLunch = "점심";
            const string DormDinner = "저녁";

            var result = name switch
            {
                Daban => Restaurants.Daban,
                NankatsuNanUdong => Restaurants.NankatsuNanUdong,
                TangAndJjigae => Restaurants.TangAndJjigae,
                YukHaeBab => Restaurants.YukHaeBab,
                DormBreakfast => Restaurants.DormBreakfast,
                DormLunch => Restaurants.DormLunch,
                DormDinner => Restaurants.DormDinner,
                _ => Restaurants.Unknown
            };

            return result;
        }

        public override string ToString()
        {
            var nullStr = "null";
            var nullStrArr = new string[1] { nullStr };

            var menuText = new StringBuilder();
            menuText.AppendLine($"[{RestaurantName}]");
            menuText.AppendLine();

            menuText.Append($"{"Monday",-12}: ");
            menuText.AppendJoin(", ", MondayMenu ?? nullStrArr);
            menuText.AppendLine();

            menuText.Append($"{"Tuesday",-12}: ");
            menuText.AppendJoin(", ", TuesdayMenu ?? nullStrArr);
            menuText.AppendLine();

            menuText.Append($"{"Wednesday",-12}: ");
            menuText.AppendJoin(", ", WednesdayMenu ?? nullStrArr);
            menuText.AppendLine();

            menuText.Append($"{"Thursday",-12}: ");
            menuText.AppendJoin(", ", ThursdayMenu ?? nullStrArr);
            menuText.AppendLine();

            menuText.Append($"{"Friday",-12}: ");
            menuText.AppendJoin(", ", FridayMenu ?? nullStrArr);
            menuText.AppendLine();

            menuText.Append($"{"Saturday",-12}: ");
            menuText.AppendJoin(", ", SaturdayMenu ?? nullStrArr);
            menuText.AppendLine();

            menuText.Append($"{"Sunday",-12}: ");
            menuText.AppendJoin(", ", SundayMenu ?? nullStrArr);
            menuText.AppendLine();

            return menuText.ToString();
        }
    }
}
