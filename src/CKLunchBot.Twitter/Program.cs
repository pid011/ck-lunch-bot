using Figgle;

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

            Log.Information($"\n{FiggleFonts.Small.Render("CKLunchBot")}");
            var assembly = typeof(Program).Assembly.GetName();
            Log.Information($"[{assembly.Name} v{assembly.Version.ToString(3)}]");

            using var bot = new BotService();
            Log.Information("Starting bot...");
            var setupSuccess = await bot.Setup();

            if (!setupSuccess)
            {
                return;
            }

            var botCancel = new CancellationTokenSource();

            var botTask = bot.Run(botCancel.Token);

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