using System;
using System.IO;
using CKLunchBot.Core.Requester;
using Newtonsoft.Json.Linq;
using Serilog;
using System.Threading.Tasks;

namespace CKLunchBot.Twitter
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.File(Path.Combine("logs", "log-.txt"), rollingInterval: RollingInterval.Month)
                .CreateLogger();

            Log.Information("Hello, World!");
            Log.Debug((await new MenuRequester().RequestData()).ToString());
        }
    }
}
