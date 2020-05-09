using CKLunchBot.Core.ImageProcess;
using CKLunchBot.Core.Menu;
using CKLunchBot.Core.Requester;

using SixLabors.ImageSharp;

using System;
using System.IO;
using System.Threading.Tasks;

namespace CKLunchBot.ImageGenTest
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            using var requester = new MenuRequester();
            var menuList = await requester.RequestWeekMenuAsync();

            //var jobj = JObject.Parse(await File.ReadAllTextAsync("menu.json"));
            //var menuList = MenuJsonParser(jobj);

            byte[] imageByte = await MenuImageGenerator.GenerateTodayDormMenuImageAsync(menuList[Restaurants.DormBreakfast]);
            //byte[] imageByte = await MenuImageGenerator.GenerateTodayDormMenuImageAsync(Restaurants.DormLunch, menuList);
            //byte[] imageByte = await MenuImageGenerator.GenerateTodayLunchMenuImageAsync(menuList);
            //byte[] imageByte = await WeekendImageGenerator.GenerateAsync();

            using var memorystream = new MemoryStream(imageByte);
            using var filestream = new FileStream("image_test.png", FileMode.Create);
            using var image = Image.Load(memorystream);
            image.SaveAsPng(filestream);
            Console.WriteLine("Done.");
        }
    }
}