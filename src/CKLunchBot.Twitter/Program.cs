using CKLunchBot.Core.Menu;

using Serilog;

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

            Log.Information("Hello, World!");
            var menuList = await new MenuLoader().GetWeekMenuFromAPIAsync();
            Log.Debug("Done");
        }
    }
}