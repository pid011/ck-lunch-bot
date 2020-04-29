using System;
using System.Drawing;
using System.Drawing.Text;
using System.IO;

namespace CKLunchBot.Core.Image
{
    public class WeekendImageGenerator : ImageGenerator
    {
        private static readonly string weekendImagePath =
            Path.Combine(Directory.GetCurrentDirectory(), "assets", "images", "weekend_template.png");

        private readonly DayOfWeek week;

        public WeekendImageGenerator(DayOfWeek week) : base(weekendImagePath)
        {
            this.week = week;

            PrivateFontCollection fontCollection = new PrivateFontCollection();

            string contentFontPath = Path.Combine(Directory.GetCurrentDirectory(), FontsPath, "NANUMGOTHICEXTRABOLD.OTF");
            fontCollection.AddFontFile(contentFontPath);

            float contentFontEmSize = 23.0f;
            Fonts.Add("content", new Font(fontCollection.Families[0], contentFontEmSize));
        }

        public override byte[] Generate()
        {
            throw new NotImplementedException();
        }
    }
}