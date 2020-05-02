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

            foreach (var weekMenu in menus)
            {
                if (weekMenu is null)
                {
                    continue;
                }

                (float x, float y) drawPosition = (0.0f, 193.0f);
                var doDrawMenu = true;

                switch (weekMenu.RestaurantName)
                {
                    case "다반":
                        drawPosition.x = 55.0f;
                        break;

                    case "난카츠난우동":
                        drawPosition.x = 350.0f;
                        break;

                    case "탕&찌개차림":
                        drawPosition.x = 642.0f;
                        break;

                    case "육해밥":
                        drawPosition.x = 935.0f;
                        break;

                    case "아침":
                    case "점심":
                    case "저녁":
                        doDrawMenu = false;
                        break;
                }

                if (doDrawMenu)
                {
                    var todaysMenu = GetTodaysMenu(weekMenu);

                    if (todaysMenu is null)
                    {
                        DrawContentOnImage(drawPosition, "(오늘 제공한 메뉴 없음)");
                    }
                    else
                    {
                        var text = string.Empty;
                        foreach (var item in todaysMenu)
                        {
                            text = $":: {item}";
                            DrawContentOnImage(drawPosition, text);
                            drawPosition.y += 40.0f;
                        }
                    }
                }

                void DrawContentOnImage((float, float) position, string text) =>
                    DrawText(position, Fonts["content"], CKLunchBotColors.Black, text);
            }

            return ExportAsPng();
        }

        private static string[] GetTodaysMenu(MenuItem menu)
        {
            string[] todaysMenu = null;

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