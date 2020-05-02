using CKLunchBot.Core.Utils;

using SixLabors.Fonts;

using System;
using System.IO;

namespace CKLunchBot.Core.ImageProcess
{
    public class WeekendImageGenerator : ImageGenerator
    {
        private static readonly string weekendImagePath =
            Path.Combine(Directory.GetCurrentDirectory(), "assets", "images", "weekend_template.png");

        public WeekendImageGenerator() : base(weekendImagePath)
        {
            string titleFontPath = Path.Combine(Directory.GetCurrentDirectory(), FontsPath, "Maplestory Bold.ttf");

            float contentFontEmSize = 32.0f;
            AddFont("title", titleFontPath, contentFontEmSize, FontStyle.Regular);
        }

        public override byte[] Generate()
        {
            (float x, float y) titlePosition = (405.0f, 37.0f);

            string date = TimeUtils.GetFormattedKoreaTime(DateTime.UtcNow);
            DrawText(titlePosition, Fonts["title"], CKLunchBotColors.White, date, HorizontalAlignment.Right);

            return ExportAsPng();
        }
    }
}