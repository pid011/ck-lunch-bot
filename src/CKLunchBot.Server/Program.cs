// Copyright (c) Sepi. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Figgle;
using Serilog;

namespace CKLunchBot.Server
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

            AssemblyName botInfo = typeof(Program).Assembly.GetName();
            Log.Information($"[{botInfo.Name} v{botInfo.Version.ToString(3)}]");

            AssemblyName coreInfo = Assembly.LoadFrom("CKLunchBot.Core.dll").GetName();
            Log.Information($"[{coreInfo.Name} v{coreInfo.Version.ToString(3)}]");

            using var bot = new BotService();
            Log.Information("Starting bot...");
            var setupSuccess = await bot.Setup();

            if (!setupSuccess)
            {
                return;
            }

            var botCancel = new CancellationTokenSource();

            Task botTask = bot.Run(botCancel.Token);

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
