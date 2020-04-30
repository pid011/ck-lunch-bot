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

                #region MenuImageTest

                var menuList = await new MenuLoader().GetWeekMenuFromAPIAsync();
                // TODO: 주말일경우 WeekendImageGenerator.GenerateImage(DayOfWeek week) 호출해서 주말 전용 이미지 생성하기
                var menuImage = await Task.Run(() =>
                {
                    using var generator = new MenuImageGenerator(menuList);
                    return generator.Generate();
                });

                System.Drawing.Bitmap menuImageBmp;
                using (var ms = new MemoryStream(menuImage))
                {
                    menuImageBmp = new System.Drawing.Bitmap(ms);
                }
                menuImageBmp.Save("test_menu.png", System.Drawing.Imaging.ImageFormat.Png);

                #endregion MenuImageTest

                #region WeekendImageTest

                var weekendImage = await Task.Run(() =>
                {
                    using var generator = new WeekendImageGenerator();
                    return generator.Generate();
                });

                System.Drawing.Bitmap weekendImageBmp;
                using (var ms = new MemoryStream(weekendImage))
                {
                    weekendImageBmp = new System.Drawing.Bitmap(ms);
                }
                weekendImageBmp.Save("test_weekend.png", System.Drawing.Imaging.ImageFormat.Png);

                #endregion WeekendImageTest

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