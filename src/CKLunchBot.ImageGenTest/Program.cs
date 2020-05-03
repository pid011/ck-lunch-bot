using CKLunchBot.Core.ImageProcess;
using CKLunchBot.Core.Menu;
using CKLunchBot.Core.Utils;
using System;
using System.IO;
using System.Threading.Tasks;
using SixLabors.ImageSharp;

namespace CKLunchBot.ImageGenTest
{
    class Program
    {
        public object Log { get; private set; }

        static async Task Main(string[] args)
        {
            var program = new Program();
            byte[] imageByte;// = await program.GenerateImageAsync();

            // weekend
            //imageByte = await Task.Run(() =>
            //{
            //    using var generator = new WeekendImageGenerator();
            //    return generator.Generate();
            //});

            // week
            var menuList = await new MenuLoader().GetWeekMenuFromAPIAsync();

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
        }
    }
}
