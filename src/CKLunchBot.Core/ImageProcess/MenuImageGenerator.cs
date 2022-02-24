// Copyright (c) Sepi. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using CKLunchBot.Core.Menu;

using SixLabors.Fonts;

namespace CKLunchBot.Core.ImageProcess
{
    internal static class MenuImageGenerator
    {
        private const float TitleFontSize = 32.0f;
        private const float ContentFontSize = 24.0f;

        private const float ContentPosXInterval = 290.0f;
        private const float ContentPosXStart = 60.0f;
        private const float ContentPosY = 193.0f;
        private const float DefaultLineHeight = 50.0f;

        private const int MaxLengthOfMenuText = 12;

        // private const string NoMenuProvidedText = "(메뉴 없음)";
        private const string MenuPrefix = "::";

        /// <summary>
        /// Generate menu image
        /// </summary>
        /// <param name="menu"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        /// <exception cref="MenuImageGenerateException"></exception>
        public static byte[] GenerateMenuImage(TodayMenu menu, MenuType type)
        {
            using ImageGenerator generator = new ImageGenerator(AssetPath.MenuTemplateImage)
               .AddFont("title", AssetPath.BoldFont, TitleFontSize, FontStyle.Regular)
               .AddFont("content", AssetPath.RegularFont, ContentFontSize, FontStyle.Regular);

            var titlePosition = new Vector2(405.0f, 37.0f); // changeable

            string titleText1 = menu.Date.GetFormattedKoreanTimeString();
            string titleText2 = " 오늘의 ";
            titleText2 += type switch
            {
                MenuType.Breakfast => "아침메뉴는?",
                MenuType.Lunch => "점심메뉴는?",
                MenuType.Dinner => "저녁메뉴는?",
                _ => "메뉴는?",
            };

            generator
                .DrawText(generator.Fonts["title"], CKLunchBotColors.White, titlePosition, titleText1, HorizontalAlignment.Right)
                .DrawText(generator.Fonts["title"], CKLunchBotColors.Black, titlePosition, titleText2);

            var items = type switch
            {
                MenuType.Breakfast => menu.Breakfast,
                MenuType.Lunch => menu.Lunch,
                MenuType.Dinner => menu.Dinner,
                _ => throw new MenuImageGenerateException($"No data found matching {type}")
            };

            var drawPosition = new Vector2(ContentPosXStart, ContentPosY);

            IEnumerable<string>? texts = null;
            if (items is not null)
            {
                texts = items.Menus?
                    .SkipWhile(item => string.IsNullOrWhiteSpace(item))
                    .Select(item => SplitLine($"{MenuPrefix} {item}", MaxLengthOfMenuText + MenuPrefix.Length + 1)) ?? null;
            }

            if (texts is null || !texts.Any())
            {
                // generator.DrawText(drawPosition, generator.Fonts["content"], CKLunchBotColors.Black, NoMenuProvidedText);
                throw new MenuImageGenerateException($"No menu found matching {type}");
            }
            else
            {
                int i = 0;
                foreach (var text in texts)
                {
                    if (string.IsNullOrWhiteSpace(text))
                    {
                        continue;
                    }

                    generator.DrawText(generator.Fonts["content"], CKLunchBotColors.Black, drawPosition, text);
                    if (i == 3)
                    {
                        drawPosition.Y += DefaultLineHeight * 2;
                        i = 0;
                    }
                    else
                    {
                        i++;
                    }
                    drawPosition.X = GetContentPositionX(i);
                }
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

        private static float GetContentPositionX(int n)
        {
            return ContentPosXStart + (ContentPosXInterval * n);
        }
    }
}
