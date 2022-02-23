// Copyright (c) Sepi. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;

namespace CKLunchBot.Core.ImageProcess
{
    public static class AssetPath
    {
        private static readonly string s_assetsPath = Path.Combine(AppContext.BaseDirectory, "Assets");
        private static readonly string s_fontsPath = Path.Combine(s_assetsPath, "Fonts");
        private static readonly string s_imagesPath = Path.Combine(s_assetsPath, "Images");

        public static readonly string BoldFont = Path.Combine(s_fontsPath, "MaruBuri-Bold.ttf");
        public static readonly string RegularFont = Path.Combine(s_fontsPath, "MaruBuri-Regular.ttf");

        public static readonly string MenuTemplateImage = Path.Combine(s_imagesPath, "MenuTemplate.png");
    }
}
