using CKLunchBot.Core.Menu;
using CKLunchBot.Core.Utils;

using SixLabors.Fonts;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
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
        private const float defaultLineHeight = 50.0f;

        private const int maxLengthOfMenuText = 12;

        private const string noMenuProvidedText = "(제공한 메뉴 없음)";
        private const string menuPrefix = "::";

        private static readonly string menuTemplateImagePath =
            Path.Combine(Directory.GetCurrentDirectory(), "assets", "images", "menu_template.png");

        private static readonly string dormMenuTemplateImagePath =
            Path.Combine(Directory.GetCurrentDirectory(), "assets", "images", "dorm_menu_template.png");

        public static async Task<byte[]> GenerateTodayLunchMenuImageAsync(RestaurantsWeekMenu menus)
        {
            return await Task.Run(() => GenerateTodayLunchMenuImage(menus));
        }

        public static async Task<byte[]> GenerateTodayDormMenuImageAsync(MenuItem weekMenu)
        {
            return await Task.Run(() => GenerateTodayDormMenuImage(weekMenu));
        }

        /// <summary>
        /// Generate todays lunch menu image
        /// </summary>
        /// <returns>byte image</returns>
        public static byte[] GenerateTodayLunchMenuImage(RestaurantsWeekMenu menus)
        {
            if (menus is null)
            {
                menus = new RestaurantsWeekMenu(null);
            }

            using var generator = new ImageGenerator(menuTemplateImagePath)
                .AddFont("title", FontPath.TitleFontPath, titleFontSize, FontStyle.Regular)
                .AddFont("content", FontPath.ContentFontPath, contentFontSize, FontStyle.Regular);

            (float x, float y) titlePosition = (405.0f, 37.0f);

            string titleText1 = TimeUtils.GetFormattedKoreaTime(DateTime.UtcNow);
            generator.DrawText(titlePosition, generator.Fonts["title"], CKLunchBotColors.White, titleText1, HorizontalAlignment.Right);

            var titleText2 = " 오늘의 점심메뉴는?";
            generator.DrawText(titlePosition, generator.Fonts["title"], CKLunchBotColors.Black, titleText2);

            Dictionary<Restaurants, StringBuilder> menuTexts = new Dictionary<Restaurants, StringBuilder>(new RestautrantsComparer())
            {
                [Restaurants.Daban] = new StringBuilder(),
                [Restaurants.NankatsuNanUdong] = new StringBuilder(),
                [Restaurants.TangAndJjigae] = new StringBuilder(),
                [Restaurants.YukHaeBab] = new StringBuilder()
            };

            foreach (var weekMenu in menus)
            {
                if (!menuTexts.ContainsKey(weekMenu.Key))
                {
                    continue;
                }

                // bot test code
                string[] todaysMenu = weekMenu.Value[DayOfWeek.Thursday];
                //string[] todaysMenu = weekMenu.Value[TimeUtils.GetKoreaNowTime(DateTime.UtcNow).DayOfWeek];

                foreach (var item in todaysMenu)
                {
                    if (!string.IsNullOrWhiteSpace(item))
                    {
                        menuTexts[weekMenu.Key].AppendLine(SplitLine($"{menuPrefix} {item}\n", maxLengthOfMenuText + menuPrefix.Length + 1));
                    }
                }
            }

            bool allMenusWereNotProvided = false;
            foreach (var item in menuTexts)
            {
                if (item.Value.Length != 0)
                {
                    continue;
                }
                allMenusWereNotProvided = true;
            }
            if (allMenusWereNotProvided)
            {
                throw new NoProvidedMenuException(Restaurants.Daban, Restaurants.NankatsuNanUdong,
                                                  Restaurants.TangAndJjigae, Restaurants.YukHaeBab);
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

                if (textList.Value.Length == 0)
                {
                    textList.Value.AppendLine(noMenuProvidedText);
                }

                generator.DrawText(drawPosition, generator.Fonts["content"], CKLunchBotColors.Black, textList.Value.ToString());
            }

            return generator.ExportAsPng();
        }

        public static byte[] GenerateTodayDormMenuImage(MenuItem weekMenu)
        {
            string titleText2 = " 오늘의 기숙사 ";
            titleText2 += weekMenu.RestaurantName switch
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

            // bot test code
            //string[] todaysMenu = weekMenu[DayOfWeek.Thursday];
            string[] todaysMenu = weekMenu[TimeUtils.GetKoreaNowTime(DateTime.UtcNow).DayOfWeek];

            if (todaysMenu.Length == 0)
            {
                throw new NoProvidedMenuException(weekMenu.RestaurantName);
            }

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
    }
}