// Copyright (c) Sepi. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.IO;

namespace CKLunchBot.Core.ImageProcess
{
    public static class FontPath
    {
        private static readonly string _fontsPath = Path.Combine("assets", "fonts");

        public static string TitleFontPath => Path.Combine(_fontsPath, "Maplestory Bold.ttf");
        public static string ContentFontPath => Path.Combine(_fontsPath, "Maplestory Light.ttf");
    }
}
