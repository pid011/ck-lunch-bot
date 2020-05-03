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
            var imageByte = await program.GenerateImageAsync();

            using var memorystream = new MemoryStream(imageByte);
            using var filestream = new FileStream("image_test.png", FileMode.Create);
            using var image = Image.Load(memorystream);
            image.SaveAsPng(filestream);
        }

        private async Task<byte[]> GenerateImageAsync()
        {
            byte[] image;
            switch (TimeUtils.GetKoreaNowTime(DateTime.UtcNow).DayOfWeek)
            {
                case DayOfWeek.Sunday:
                case DayOfWeek.Saturday:
                    image = await Task.Run(() =>
                    {
                        using var generator = new WeekendImageGenerator();
                        return generator.Generate();
                    });
                    break;

                default:
                    var menuList = await new MenuLoader().GetWeekMenuFromAPIAsync();

                    image = await Task.Run(() =>
                    {
                        using var generator = new MenuImageGenerator(menuList);
                        return generator.Generate();
                    });
                    break;
            }
            //Log.Information("Generate completed.");
            return image;
        }
    }
}
