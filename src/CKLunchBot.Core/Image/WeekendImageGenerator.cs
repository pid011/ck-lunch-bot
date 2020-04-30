using CKLunchBot.Core.Utils;

using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;

namespace CKLunchBot.Core.Image
{
    public class WeekendImageGenerator : ImageGenerator
    {
        private static readonly string weekendImagePath =
            Path.Combine(Directory.GetCurrentDirectory(), "assets", "images", "weekend_template.png");

        public WeekendImageGenerator() : base(weekendImagePath)
        {
            PrivateFontCollection fontCollection = new PrivateFontCollection();

            string titleFontPath = Path.Combine(Directory.GetCurrentDirectory(), FontsPath, "NANUMGOTHICEXTRABOLD.OTF");
            fontCollection.AddFontFile(titleFontPath);

            float contentFontEmSize = 23.0f;
            Fonts.Add("title", new Font(fontCollection.Families[0], contentFontEmSize));
        }

        public override byte[] Generate()
        {
            (float x, float y) titlePosition = (400.0f, 37.0f);

            string date = TimeUtils.Time.FormattedKoreaNowTime;
            DrawText(titlePosition, Fonts["title"], CKLunchBotColors.White, date, StringAlignment.Far);

            return ToByteArray(ImageFormat.Png);
        }
    }
}