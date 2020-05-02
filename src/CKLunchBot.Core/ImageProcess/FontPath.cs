using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CKLunchBot.Core.ImageProcess
{
    public static class FontPath
    {
        private static readonly string FontsPath = Path.Combine("assets", "fonts");

        public static string TitleFontPath => Path.Combine(FontsPath, "Maplestory Bold.ttf");
        public static string ContentFontPath => Path.Combine(FontsPath, "Maplestory Light.ttf");
    }
}
