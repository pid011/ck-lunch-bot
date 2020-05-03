using Serilog;

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace CKLunchBot.Twitter
{
    internal class Program
    {
        private static async Task Main()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.File(Path.Combine("logs", ".log"), rollingInterval: RollingInterval.Month)
                .CreateLogger();

            var bot = new BotService();
            Log.Information("Starting bot...");
            var setup = await bot.Setup();

            if (!setup.setupSuccess)
            {
                return;
            }

            using var botCancel = new CancellationTokenSource();

            var botTask = new BotService().Run(botCancel.Token, setup.config, setup.twitter);

            var stopCommandTask = Task.Run(() =>
            {
                string answer;
                while (true)
                {
                    Console.WriteLine("If you want to stop the bot, enter 'stop'");
                    answer = Console.ReadLine();
                    if (answer is "stop")
                    {
                        botCancel.Cancel();

                        var check = false;
                        while (!check)
                        {
                            check = botTask.IsCompleted;
                        }

                        break;
                    }
                }
            });

            await Task.WhenAny(botTask, stopCommandTask);
        }
    }
}