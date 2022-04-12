using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using SixLabors.Fonts;

using PointF = SixLabors.ImageSharp.PointF;

namespace CKLunchBot.Core.Drawing;

public static class MenuImageGenerator
{
    private const float TitleFontSize = 38f;
    private const float SubTitleFontSize = 34f;
    private const float ContentFontSize = 32f;

    private const float TitlePosYStart = 35f;
    private const float ContentPosYStart = 220f;

    private const float ContentPosXInterval = 390f;
    private const float ContentPosXStart = 60f;

    private const float SubTitleContentInterval = 90f;
    private const float LineHeight = 135f;
    private const int MaxLengthOfMenuText = 10;

    private const int MaxCountInLine = 4;

    private const string NoMenuProvidedText = "음... 없네요";
    private const string MenuPrefix = "-";

    public static async Task<byte[]> GenerateAsync(DateOnly date, MenuType type, Menu menu)
    {
        using var drawingBoard = new DrawingBoard(AssetPath.MenuTemplateImage);

        drawingBoard
           .AddFont("title", AssetPath.BoldFont, TitleFontSize, FontStyle.Regular)
           .AddFont("subtitle", AssetPath.BoldFont, SubTitleFontSize, FontStyle.Regular)
           .AddFont("content", AssetPath.RegularFont, ContentFontSize, FontStyle.Regular);

        var titleFont = drawingBoard.Fonts["title"];
        var subTitleFont = drawingBoard.Fonts["subtitle"];
        var contentFont = drawingBoard.Fonts["content"];

        var titlePosition = new PointF(ContentPosXStart, TitlePosYStart);

        var titleText = new StringBuilder()
            .AppendLine(date.GetFormattedKoreanString())
            .Append($"오늘의 청강대 ")
            .Append(type switch
            {
                MenuType.Breakfast => "아침",
                MenuType.Lunch => "점심",
                MenuType.Dinner => "저녁",
                _ => string.Empty,
            })
            .Append("메뉴는...")
            .ToString();
        drawingBoard.DrawText(titleFont, BaseBrushes.Black, titlePosition, titleText);

        var contentPos = new PointF(ContentPosXStart, ContentPosYStart);

        if (menu.IsEmpty())
        {
            drawingBoard.DrawText(subTitleFont, BaseBrushes.Black, new PointF(ContentPosXStart, ContentPosYStart), NoMenuProvidedText);
            return await drawingBoard.Mutate().ExportAsPngAsync();
        }

        if (menu.Menus.Count > 0)
        {
            DrawMenus(drawingBoard, subTitleFont, menu.Menus, contentPos);
        }

        contentPos = GetNextLinePos(contentPos);

        if (menu.SpecialMenus.Count > 0)
        {
            drawingBoard.DrawText(titleFont, BaseBrushes.Black, contentPos, "<셀프코너>");
            contentPos.Y += SubTitleContentInterval;

            contentPos = DrawMenus(drawingBoard, subTitleFont, menu.SpecialMenus, contentPos);
        }

        return await drawingBoard.Mutate().ExportAsJpegAsync(quality: 95);

        ///////////////////////////////////////////////////////////////////

        static PointF DrawMenus(DrawingBoard drawingBoard, Font font, IReadOnlyCollection<string> menus, PointF startPos)
        {
            var pos = startPos;

            int i = 0;
            foreach (var item in menus)
            {
                var txt = SplitLine($"{MenuPrefix} {item}", MaxLengthOfMenuText + MenuPrefix.Length + 1);
                drawingBoard.DrawText(font, BaseBrushes.Black, pos, txt);
                if (i == MaxCountInLine - 1)
                {
                    pos = GetNextLinePos(pos);
                    i = 0;
                }
                else
                {
                    ++i;
                }
                pos = GetNewPosByIdx(pos, i);
            }

            if (i > 0)
            {
                pos = GetNextLinePos(pos);
            }

            return pos;
        }

        static string SplitLine(string text, int maxLineLength)
        {
            if (text.Length > maxLineLength)
            {
                return $"{text[..maxLineLength]}\n{SplitLine(text[maxLineLength..], maxLineLength)}";
            }

            return text;
        }

        static PointF GetNewPosByIdx(PointF point, int idx) => new(ContentPosXStart + (ContentPosXInterval * idx), point.Y);

        static PointF GetNextLinePos(PointF point) => new(ContentPosXStart, point.Y + LineHeight);
    }
}
