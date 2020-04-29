using CKLunchBot.Core.Menu;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;

namespace CKLunchBot.Core.Image
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

            PrivateFontCollection fontCollection = new PrivateFontCollection();

            string titleFontPath = Path.Combine(Directory.GetCurrentDirectory(), FontsPath, "NANUMGOTHICEXTRABOLD.OTF");
            string contetsFontPath = Path.Combine(Directory.GetCurrentDirectory(), FontsPath, "NANUMGOTHICBOLD.OTF");
            fontCollection.AddFontFile(titleFontPath);
            fontCollection.AddFontFile(contetsFontPath);

            float titleFontEmSize = 23.0f;
            float contentFontEmSize = 16.0f;

            Fonts.Add("title", new Font(fontCollection.Families[1], titleFontEmSize));
            Fonts.Add("content", new Font(fontCollection.Families[0], contentFontEmSize));
        }

        public override byte[] Generate()
        {
            (int, int, int) white = (238, 238, 238);
            (int, int, int) black = (45, 45, 48);

            (float x, float y) titlePosition = (400.0f, 37.0f);

            // Korea Standard Time (UTC +9:00)
            DateTime koreaTime = DateTime.UtcNow.AddHours(9);
            var titleText = koreaTime.ToString("D", new System.Globalization.CultureInfo("ko-KR"));
            DrawText(titlePosition, Fonts["title"], white, titleText, StringAlignment.Far);

            var titleText2 = "오늘의 점심메뉴는?";
            DrawText(titlePosition, Fonts["title"], black, titleText2);

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
                    DrawText(position, Fonts["content"], black, text);
            }

            return ToByteArray(ImageFormat.Png);
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