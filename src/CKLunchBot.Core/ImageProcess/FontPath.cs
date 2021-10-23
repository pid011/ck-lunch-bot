// Copyright (c) Sepi. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;

namespace CKLunchBot.Core.ImageProcess
{
    public static class FontPath
    {
        private static readonly string s_fontsPath = Path.Combine(AppContext.BaseDirectory, "assets", "fonts");

        public static readonly string TitleFontPath = Path.Combine(s_fontsPath, "Maplestory Bold.ttf");
        public static readonly string ContentFontPath = Path.Combine(s_fontsPath, "Maplestory Light.ttf");
    }
}
