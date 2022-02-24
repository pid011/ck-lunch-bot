// Copyright (c) Sepi. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Threading.Tasks;
using CKLunchBot.Core;
using CKLunchBot.Core.Menu;
using SixLabors.ImageSharp;

namespace CKLunchBot.ImageGenTest
{
    internal class ImageGenTestApp
    {
        private static async Task Main(string[] _)
        {
            try
            {
                var outputDir = $"Test_{DateTime.Now:yyyy-MM-dd-HH-mm-ss}";

                var weekMenu = await WeekMenu.LoadAsync();

                var todayMenu = weekMenu.FindMenu(KST.Now.Day);
                await SaveMenuImageAsync(outputDir, todayMenu, MenuType.Lunch);

                /*
                foreach (var menu in weekMenu)
                {
                    Console.WriteLine(menu.ToString());
                    Console.WriteLine();

                    Console.WriteLine($"Make breakfast menu image {menu.Date}");
                    await SaveMenuImage(outputDir, menu, MenuType.Breakfast);
                    Console.WriteLine($"Make lunch menu image {menu.Date}");
                    await SaveMenuImage(outputDir, menu, MenuType.Lunch);
                    Console.WriteLine($"Make dinner menu image {menu.Date}");
                    await SaveMenuImage(outputDir, menu, MenuType.Dinner);
                    Console.WriteLine();
                }
                */
                Console.WriteLine("Done.");
            }
            catch (NoProvidedMenuException)
            {
                Console.WriteLine("제공되는 메뉴가 없음.");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private static async Task SaveMenuImageAsync(string outputDir, TodayMenu menu, MenuType type)
        {
            var imageBytes = await Task.Run(() => menu.MakeImage(type));

            if (!Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }

            using var memorystream = new MemoryStream(imageBytes);
            using var filestream = new FileStream(Path.Combine(outputDir, $"{menu.Date.ToShortDateString()}_{type}.png"), FileMode.Create);
            using var image = await Image.LoadAsync(memorystream);

            image.SaveAsPng(filestream);
        }
    }
}
