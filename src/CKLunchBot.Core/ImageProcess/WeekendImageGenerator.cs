// Copyright (c) Sepi. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Threading.Tasks;

using CKLunchBot.Core.Utils;

using SixLabors.Fonts;

namespace CKLunchBot.Core.ImageProcess
{
    public static class WeekendImageGenerator
    {
        private const float TitleFontSize = 32.0f;

        private static readonly string _weekendImagePath =
            Path.Combine(Directory.GetCurrentDirectory(), "assets", "images", "weekend_template.png");

        public static async Task<byte[]> GenerateAsync() => await Task.Run(() => Generate());

        public static byte[] Generate()
        {
            (float x, float y) titlePosition = (405.0f, 37.0f);
            string date = TimeUtils.GetFormattedKoreaTime(DateTime.UtcNow);

            byte[] imageByte;
            using (var generator = new ImageGenerator(_weekendImagePath))
            {
                imageByte = generator
                    .AddFont("title", FontPath.TitleFontPath, TitleFontSize, FontStyle.Regular)
                    .DrawText(titlePosition, generator.Fonts["title"], CKLunchBotColors.White, date, HorizontalAlignment.Right)
                    .ExportAsPng();
            }

            return imageByte;
        }
    }
}
