using CKLunchBot.Core.Menu;
using CKLunchBot.Core.Utils;

using SixLabors.Fonts;

using System;
using System.Collections.Generic;
using System.IO;

namespace CKLunchBot.Core.ImageProcess
{
    public class MenuImageGenerator : ImageGenerator
    {
        private static readonly string menuTemplateImagePath =
            Path.Combine(Directory.GetCurrentDirectory(), "assets", "images", "menu_template.png");

        private readonly List<MenuItem> menus;

        public MenuImageGenerator(List<MenuItem> menus) : base(menuTemplateImagePath)
        {
            if (menus is null)
            {
                throw new ArgumentNullException(nameof(menus));
            }

            this.menus = menus;
            float titleFontEmSize = 32.0f;
            float contentFontEmSize = 24.0f;

            AddFont("title", FontPath.TitleFontPath, titleFontEmSize, FontStyle.Regular);
            AddFont("content", FontPath.ContentFontPath, contentFontEmSize, FontStyle.Regular);
        }

        public override byte[] Generate()
        {
            (float x, float y) titlePosition = (405.0f, 37.0f);

            string titleText = TimeUtils.GetFormattedKoreaTime(DateTime.UtcNow);
            DrawText(titlePosition, Fonts["title"], CKLunchBotColors.White, titleText, HorizontalAlignment.Right);

            var titleText2 = " 오늘의 점심메뉴는?";
            DrawText(titlePosition, Fonts["title"], CKLunchBotColors.Black, titleText2);

            const string daban = "다반";
            const string nankatsuNanUdong = "난카츠난우동";
            const string tangAndJjigae = "탕&찌개차림";
            const string yukHaeBab = "육해밥";

            Dictionary<string, List<string>> menuTexts = new Dictionary<string, List<string>>()
            {
                [daban] = new List<string>(),
                [nankatsuNanUdong] = new List<string>(),
                [tangAndJjigae] = new List<string>(),
                [yukHaeBab] = new List<string>(),
            };

            foreach (var weekMenu in menus)
            {
                if (weekMenu is null)
                {
                    continue;
                }

                if (!menuTexts.ContainsKey(weekMenu.RestaurantName))
                {
                    continue;
                }

                var todaysMenu = GetTodaysMenu(weekMenu);

                if(todaysMenu != null)
                {
                    var text = string.Empty;
                    foreach (var item in todaysMenu)
                    {
                        menuTexts[weekMenu.RestaurantName].Add($":: {item}");
                    }
                }
            }

            foreach (var textList in menuTexts)
            {
                (float x, float y) drawPosition = (0.0f, 193.0f);
                var doDrawMenu = true;

                switch (textList.Key)
                {
                    case daban:
                        drawPosition.x = 55.0f;
                        break;

                    case nankatsuNanUdong:
                        drawPosition.x = 350.0f;
                        break;

                    case tangAndJjigae:
                        drawPosition.x = 642.0f;
                        break;

                    case yukHaeBab:
                        drawPosition.x = 935.0f;
                        break;

                    default:
                        doDrawMenu = false;
                        break;
                }

                if (doDrawMenu)
                {
                    if (textList.Value.Count == 0)
                    {
                        DrawContentOnImage(drawPosition, "(제공한 메뉴가 없음)");
                    }
                    else
                    {
                        foreach (var text in textList.Value)
                        {
                            DrawContentOnImage(drawPosition, text);
                            drawPosition.y += 40.0f;
                        }
                    }
                }

                void DrawContentOnImage((float, float) position, string text)
                {
                    DrawText(position, Fonts["content"], CKLunchBotColors.Black, text);
                }
            }

            return ExportAsPng();
        }

        private static string[] GetTodaysMenu(MenuItem menu)
        {
            string[] todaysMenu = null;

            // TODO: Change TimeUtils.GetKoreaTime(DateTime.UtcNow)
            switch (DateTime.Now.DayOfWeek)
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