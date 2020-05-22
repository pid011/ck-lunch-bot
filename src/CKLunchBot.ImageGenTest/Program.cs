// Copyright (c) Sepi. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Threading.Tasks;

using CKLunchBot.Core.ImageProcess;
using CKLunchBot.Core.Menu;
using CKLunchBot.Core.Requester;

using SixLabors.ImageSharp;

namespace CKLunchBot.ImageGenTest
{
    internal class Program
    {
        private static async Task Main(string[] _)
        {
            using var requester = new MenuRequester();
            RestaurantsWeekMenu menuList = await requester.RequestWeekMenuAsync();

            // Dorm breakfast
            //byte[] imageByte = await MenuImageGenerator.GenerateTodayDormMenuImageAsync(menuList[Restaurants.DormBreakfast]);

            // Dorm lunch
            //byte[] imageByte = await MenuImageGenerator.GenerateTodayDormMenuImageAsync(menuList[Restaurants.DormLunch]);

            // Dorm dinner
            //byte[] imageByte = await MenuImageGenerator.GenerateTodayDormMenuImageAsync(menuList[Restaurants.DormDinner]);

            // School lunch
            byte[] imageByte = await MenuImageGenerator.GenerateTodayLunchMenuImageAsync(menuList);

            // Weekend image
            //byte[] imageByte = await WeekendImageGenerator.GenerateAsync();

            using var memorystream = new MemoryStream(imageByte);
            using var filestream = new FileStream("image_test.png", FileMode.Create);
            using var image = Image.Load(memorystream);
            image.SaveAsPng(filestream);
            Console.WriteLine("Done.");
        }
    }
}
