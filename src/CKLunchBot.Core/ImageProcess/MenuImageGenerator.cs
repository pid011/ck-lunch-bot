// Copyright (c) Sepi. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using CKLunchBot.Core.Menu;
using CKLunchBot.Core.Utils;

using SixLabors.Fonts;

namespace CKLunchBot.Core.ImageProcess
{
    public static class MenuImageGenerator
    {
        private const float TitleFontSize = 32.0f;
        private const float ContentFontSize = 24.0f;

        private const float ContentPosXInterval = 290.0f;
        private const float ContentPosXStart = 60.0f;
        private const float ContentPosY = 193.0f;
        private const float DefaultLineHeight = 50.0f;

        private const int MaxLengthOfMenuText = 12;

        private const string NoMenuProvidedText = "(제공한 메뉴 없음)";
        private const string MenuPrefix = "::";

        private static readonly string _menuTemplateImagePath =
            Path.Combine(Directory.GetCurrentDirectory(), "assets", "images", "menu_template.png");

        private static readonly string _dormMenuTemplateImagePath =
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

            using ImageGenerator generator = new ImageGenerator(_menuTemplateImagePath)
                .AddFont("title", FontPath.TitleFontPath, TitleFontSize, FontStyle.Regular)
                .AddFont("content", FontPath.ContentFontPath, ContentFontSize, FontStyle.Regular);

            (float x, float y) titlePosition = (405.0f, 37.0f);

            var titleText1 = TimeUtils.GetFormattedKoreaTime(DateTime.UtcNow);
            generator.DrawText(titlePosition, generator.Fonts["title"], CKLunchBotColors.White, titleText1, HorizontalAlignment.Right);

            var titleText2 = " 오늘의 점심메뉴는?";
            generator.DrawText(titlePosition, generator.Fonts["title"], CKLunchBotColors.Black, titleText2);

            var menuTexts = new Dictionary<Restaurants, StringBuilder>(new RestautrantsComparer())
            {
                [Restaurants.Daban] = new StringBuilder(),
                [Restaurants.NankatsuNanUdong] = new StringBuilder(),
                [Restaurants.TangAndJjigae] = new StringBuilder(),
                [Restaurants.YukHaeBab] = new StringBuilder()
            };

            foreach (KeyValuePair<Restaurants, MenuItem> weekMenu in menus)
            {
                if (!menuTexts.ContainsKey(weekMenu.Key))
                {
                    continue;
                }

                string[] todaysMenu = weekMenu.Value[TimeUtils.GetKoreaNowTime(DateTime.UtcNow).DayOfWeek];

                foreach (var item in todaysMenu)
                {
                    if (!string.IsNullOrWhiteSpace(item))
                    {
                        menuTexts[weekMenu.Key].AppendLine(SplitLine($"{MenuPrefix} {item}\n", MaxLengthOfMenuText + MenuPrefix.Length + 1));
                    }
                }
            }

            bool allMenusWereNotProvided = false;
            foreach (KeyValuePair<Restaurants, StringBuilder> item in menuTexts)
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

            foreach (KeyValuePair<Restaurants, StringBuilder> textList in menuTexts)
            {
                (float x, float y) drawPosition = (0.0f, ContentPosY);

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
                    textList.Value.AppendLine(NoMenuProvidedText);
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

            using ImageGenerator generator = new ImageGenerator(_dormMenuTemplateImagePath)
               .AddFont("title", FontPath.TitleFontPath, TitleFontSize, FontStyle.Regular)
               .AddFont("content", FontPath.ContentFontPath, ContentFontSize, FontStyle.Regular);

            (float x, float y) titlePosition = (405.0f, 37.0f); // change

            string titleText1 = TimeUtils.GetFormattedKoreaTime(DateTime.UtcNow);
            generator.DrawText(titlePosition, generator.Fonts["title"], CKLunchBotColors.White, titleText1, HorizontalAlignment.Right);
            generator.DrawText(titlePosition, generator.Fonts["title"], CKLunchBotColors.Black, titleText2);

            List<string> menuTexts = new List<string>();

            string[] todaysMenu = weekMenu[TimeUtils.GetKoreaNowTime(DateTime.UtcNow).DayOfWeek];

            if (todaysMenu.Length == 0)
            {
                throw new NoProvidedMenuException(weekMenu.RestaurantName);
            }

            foreach (var item in todaysMenu)
            {
                if (!string.IsNullOrWhiteSpace(item))
                {
                    menuTexts.Add(SplitLine($"{MenuPrefix} {item}", MaxLengthOfMenuText + MenuPrefix.Length + 1));
                }
            }
            if (menuTexts.Count == 0)
            {
                menuTexts.Add("(제공한 메뉴가 없음)");
            }

            (float x, float y) drawPosition = (ContentPosXStart, ContentPosY);

            int i = 0;
            foreach (var text in menuTexts)
            {
                generator.DrawText(drawPosition, generator.Fonts["content"], CKLunchBotColors.Black, text);
                if (i == 3)
                {
                    drawPosition.y += DefaultLineHeight * 2;
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
            return ContentPosXStart + ContentPosXInterval * n;
        }
    }
}
