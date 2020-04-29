using CKLunchBot.Core.Image;
using CKLunchBot.Core.Menu;

using Serilog;

using System;
using System.IO;
using System.Threading.Tasks;

namespace CKLunchBot.Twitter
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.File(Path.Combine("logs", "log-.txt"), rollingInterval: RollingInterval.Month)
                .CreateLogger();

            try
            {
                Log.Information("Hello, World!");
                var menuList = await new MenuLoader().GetWeekMenuFromAPIAsync();
                // TODO: 주말일경우 WeekendImageGenerator.GenerateImage(DayOfWeek week) 호출해서 주말 전용 이미지 생성하기
                var image = await Task.Run(() => MenuImageGenerator.GenerateImage(menuList));

                #region Test
                System.Drawing.Bitmap bmp;
                using (var ms = new MemoryStream(image))
                {
                    bmp = new System.Drawing.Bitmap(ms);
                }
                bmp.Save("test.png", System.Drawing.Imaging.ImageFormat.Png);
                #endregion

                Log.Debug("Done");
            }
            catch (Exception e)
            {
                // Log.Error(e.Message);
                Log.Error(e.ToString());
            }
        }
    }
}