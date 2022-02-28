using System.IO;
using System.Reflection;

namespace CKLunchBot.Core.Drawing;

internal static class AssetPath
{
    private static readonly string s_assetsPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty, "assets");
    private static readonly string s_fontsPath = Path.Combine(s_assetsPath, "fonts");
    private static readonly string s_imagesPath = Path.Combine(s_assetsPath, "images");

    public static readonly string BoldFont = Path.Combine(s_fontsPath, "MaruBuri-Bold.ttf");
    public static readonly string RegularFont = Path.Combine(s_fontsPath, "MaruBuri-Regular.ttf");

    public static readonly string MenuTemplateImage = Path.Combine(s_imagesPath, "MenuTemplate.png");
}
