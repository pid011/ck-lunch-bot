using SixLabors.ImageSharp.Drawing.Processing;

namespace CKLunchBot.Core.Drawing;

internal static class BaseBrushes
{
    public static SolidBrush White { get; } = Brushes.Solid(BaseColors.White);
    public static SolidBrush Black { get; } = Brushes.Solid(BaseColors.Black);
}
