using CKLunchBot.Core.ImageProcess;
using CKLunchBot.Core.Menu;
using CKLunchBot.Core.Utils;
using System;
using System.IO;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace CKLunchBot.ImageGenTest
{
    class Program
    {
        static async Task Main(string[] args)
        {
            //var program = new Program();
            byte[] imageByte;// = await program.GenerateImageAsync();

            // weekend
            //imageByte = await Task.Run(() =>
            //{
            //    using var generator = new WeekendImageGenerator();
            //    return generator.Generate();
            //});

            // week
            using var menuLoader = new MenuLoader();
            var menuList = await menuLoader.GetWeekMenuFromAPIAsync();

            //var jobj = JObject.Parse(await File.ReadAllTextAsync("menu.json"));
            //var menuList = MenuJsonParser(jobj);

            imageByte = await Task.Run(() =>
            {
                using var generator = new MenuImageGenerator();
                generator.SetMenu(menuList);
                return generator.Generate();
            });

            using var memorystream = new MemoryStream(imageByte);
            using var filestream = new FileStream("image_test.png", FileMode.Create);
            using var image = Image.Load(memorystream);
            image.SaveAsPng(filestream);
            Console.WriteLine("Done.");
        }

        private static List<MenuItem> MenuJsonParser(JObject jsonObject)
        {
            var success = (bool)jsonObject["success"];
            if (!success)
            {
                var message = (string)jsonObject["result"];
                throw new Exception($"API request fail message: {message}");
            }

            List<MenuItem> menuList = jsonObject["result"]
                .Select(a => new MenuItem(a.ToObject<RawMenuItem>()))
                .ToList();

            return menuList;
        }
    }
}
