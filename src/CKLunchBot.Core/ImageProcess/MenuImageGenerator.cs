using CKLunchBot.Core.Menu;
using CKLunchBot.Core.Utils;

using SixLabors.Fonts;

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace CKLunchBot.Core.ImageProcess
{
    public static class MenuImageGenerator
    {
        private const float titleFontSize = 32.0f;
        private const float contentFontSize = 24.0f;

        private const float contentPosXInterval = 290.0f;
        private const float contentPosXStart = 60.0f;
        private const float contentPosY = 193.0f;
        private const float defaultLineHeight = 40.0f;

        private const int maxLengthOfMenuText = 12;

        private const string noMenuProvidedText = "제공한 메뉴 없음";
        private const string menuPrefix = "::";

        private static readonly string menuTemplateImagePath =
            Path.Combine(Directory.GetCurrentDirectory(), "assets", "images", "menu_template.png");

        private static readonly string dormMenuTemplateImagePath =
            Path.Combine(Directory.GetCurrentDirectory(), "assets", "images", "dorm_menu_template.png");

        public static async Task<byte[]> GenerateTodayLunchMenuImageAsync(List<MenuItem> menus)
        {
            return await Task.Run(() => GenerateTodayLunchMenuImage(menus));
        }

        public static async Task<byte[]> GenerateTodayDormMenuImageAsync(Restaurants mealTime, List<MenuItem> menus)
        {
            return await Task.Run(() => GenerateTodayDormMenuImage(mealTime, menus));
        }

        public static byte[] GenerateTodayLunchMenuImage(List<MenuItem> menus)
        {
            if (menus is null)
            {
                throw new ArgumentNullException(nameof(menus));
            }

            using var generator = new ImageGenerator(menuTemplateImagePath)
                .AddFont("title", FontPath.TitleFontPath, titleFontSize, FontStyle.Regular)
                .AddFont("content", FontPath.ContentFontPath, contentFontSize, FontStyle.Regular);

            (float x, float y) titlePosition = (405.0f, 37.0f);

            string titleText1 = TimeUtils.GetFormattedKoreaTime(DateTime.UtcNow);
            generator.DrawText(titlePosition, generator.Fonts["title"], CKLunchBotColors.White, titleText1, HorizontalAlignment.Right);

            var titleText2 = " 오늘의 점심메뉴는?";
            generator.DrawText(titlePosition, generator.Fonts["title"], CKLunchBotColors.Black, titleText2);

            Dictionary<Restaurants, List<string>> menuTexts = new Dictionary<Restaurants, List<string>>()
            {
                [Restaurants.Daban] = new List<string>(),
                [Restaurants.NankatsuNanUdong] = new List<string>(),
                [Restaurants.TangAndJjigae] = new List<string>(),
                [Restaurants.YukHaeBab] = new List<string>(),
            };

            foreach (var weekMenu in menus)
            {
                if (weekMenu is null || !menuTexts.ContainsKey(weekMenu.RestaurantName))
                {
                    continue;
                }

                string[] todaysMenu = GetTodaysMenu(weekMenu);

                foreach (var item in todaysMenu)
                {
                    if (!string.IsNullOrWhiteSpace(item))
                    {
                        menuTexts[weekMenu.RestaurantName].Add(SplitLine($"{menuPrefix} {item}", maxLengthOfMenuText + menuPrefix.Length + 1));
                    }
                }
                if (menuTexts.Count == 0)
                {
                    menuTexts[weekMenu.RestaurantName].Add(noMenuProvidedText);
                }
            }

            foreach (var textList in menuTexts)
            {
                (float x, float y) drawPosition = (0.0f, contentPosY);

                drawPosition.x = textList.Key switch
                {
                    Restaurants.Daban => GetContentPositionX(0),
                    Restaurants.NankatsuNanUdong => GetContentPositionX(1),
                    Restaurants.TangAndJjigae => GetContentPositionX(2),
                    Restaurants.YukHaeBab => GetContentPositionX(3),
                    _ => 0.0f // Never used because it is filtered out before running this code.
                };

                foreach (var text in textList.Value)
                {
                    generator.DrawText(drawPosition, generator.Fonts["content"], CKLunchBotColors.Black, text);
                    drawPosition.y += defaultLineHeight;
                }
            }

            return generator.ExportAsPng();
        }

        public static byte[] GenerateTodayDormMenuImage(Restaurants mealTime, List<MenuItem> menus)
        {
            string titleText2 = " 오늘의 기숙사 ";
            titleText2 += mealTime switch
            {
                Restaurants.DormBreakfast => "아침메뉴는?",
                Restaurants.DormDinner => "저녁메뉴는?",
                Restaurants.DormLunch => "점심메뉴는?",
                _ => throw new ArgumentException("Argument value is not dormitory meal menu."),
            };

            using var generator = new ImageGenerator(dormMenuTemplateImagePath)
               .AddFont("title", FontPath.TitleFontPath, titleFontSize, FontStyle.Regular)
               .AddFont("content", FontPath.ContentFontPath, contentFontSize, FontStyle.Regular);

            (float x, float y) titlePosition = (405.0f, 37.0f); // change

            string titleText1 = TimeUtils.GetFormattedKoreaTime(DateTime.UtcNow);
            generator.DrawText(titlePosition, generator.Fonts["title"], CKLunchBotColors.White, titleText1, HorizontalAlignment.Right);
            generator.DrawText(titlePosition, generator.Fonts["title"], CKLunchBotColors.Black, titleText2);

            List<string> menuTexts = new List<string>();

            MenuItem weekMenu = menus.Find(menu => menu.RestaurantName == mealTime);
            string[] todaysMenu = GetTodaysMenu(weekMenu);

            foreach (var item in todaysMenu)
            {
                if (!string.IsNullOrWhiteSpace(item))
                {
                    menuTexts.Add(SplitLine($"{menuPrefix} {item}", maxLengthOfMenuText + menuPrefix.Length + 1));
                }
            }
            if (menuTexts.Count == 0)
            {
                menuTexts.Add("(제공한 메뉴가 없음)");
            }

            (float x, float y) drawPosition = (contentPosXStart, contentPosY);

            int i = 0;
            foreach (var text in menuTexts)
            {
                generator.DrawText(drawPosition, generator.Fonts["content"], CKLunchBotColors.Black, text);
                if (i == 3)
                {
                    drawPosition.y += defaultLineHeight * 2;
                    i = 0;
                }
                else
                {
                    i++;
                }
                drawPosition.x = GetContentPositionX(i);
            }

            return generator.ExportAsPng();
        }

        private static string SplitLine(string text, int maxLineLength)
        {
            if (text.Length > maxLineLength)
            {
                return $"{text[..maxLineLength]}\n{SplitLine(text[maxLineLength..], maxLineLength)}";
            }

            return text;
        }

        private static float GetContentPositionX(float n)
        {
            return contentPosXStart + contentPosXInterval * n;
        }

        private static string[] GetTodaysMenu(MenuItem menu)
        {
            if (menu is null)
            {
                throw new ArgumentNullException(nameof(menu));
            }

            string[] todaysMenu = null;

            switch (TimeUtils.GetKoreaNowTime(DateTime.UtcNow).DayOfWeek)
            {
                case DayOfWeek.Friday:
                    todaysMenu = menu.FridayMenu;
                    break;

                case DayOfWeek.Monday:
                    todaysMenu = menu.MondayMenu;
                    break;

                case DayOfWeek.Saturday:
                    todaysMenu = menu.SaturdayMenu;
                    break;

                case DayOfWeek.Sunday:
                    todaysMenu = menu.SundayMenu;
                    break;

                case DayOfWeek.Thursday:
                    todaysMenu = menu.ThursdayMenu;
                    break;

                case DayOfWeek.Tuesday:
                    todaysMenu = menu.TuesdayMenu;
                    break;

                case DayOfWeek.Wednesday:
                    todaysMenu = menu.WednesdayMenu;
                    break;
            }

            return todaysMenu;
        }
    }
}