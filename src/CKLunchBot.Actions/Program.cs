// Copyright (c) Sepi. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Reflection;
using CKLunchBot.Actions;
using Figgle;
using Serilog;
using static CKLunchBot.Actions.BotService;

const string ConsumerApiKeyName = "TWITTER_CONSUMER_API_KEY";
const string ConsumerSecretKeyName = "TWITTER_CONSUMER_SECRET_KEY";
const string AccessTokenName = "TWITTER_ACCESS_TOKEN";
const string AccessTokenSecretName = "TWITTER_ACCESS_TOKEN_SECRET";
const string MealApiKeyName = "MEAL_API_KEY";

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .CreateLogger();

Log.Information($"\n{FiggleFonts.Small.Render("CKLunchBot")}");

AssemblyName botInfo = Assembly.GetExecutingAssembly().GetName();
Log.Information($"[{botInfo.Name} v{botInfo.Version.ToString(3)}]");

string coreDllPath = Path.Combine(AppContext.BaseDirectory, "CKLunchBot.Core.dll");
AssemblyName coreInfo = Assembly.LoadFrom(coreDllPath).GetName();
Log.Information($"[{coreInfo.Name} v{coreInfo.Version.ToString(3)}]");

try
{
    var consumerApiKey = Environment.GetEnvironmentVariable(ConsumerApiKeyName, EnvironmentVariableTarget.Process)
        ?? throw new Exception($"Faild to get enviroment value name of {ConsumerApiKeyName}");

    var consumerSecretKey = Environment.GetEnvironmentVariable(ConsumerSecretKeyName, EnvironmentVariableTarget.Process)
        ?? throw new Exception($"Faild to get enviroment value name of {ConsumerSecretKeyName}");

    var accessToken = Environment.GetEnvironmentVariable(AccessTokenName, EnvironmentVariableTarget.Process)
        ?? throw new Exception($"Faild to get enviroment value name of {AccessTokenName}");

    var accessTokenSecret = Environment.GetEnvironmentVariable(AccessTokenSecretName, EnvironmentVariableTarget.Process)
        ?? throw new Exception($"Faild to get enviroment value name of {AccessTokenSecretName}");

    var twitterKeys = new TwitterApiKeys(consumerApiKey, consumerSecretKey, accessToken, accessTokenSecret);

    var mealApiKey = Environment.GetEnvironmentVariable(MealApiKeyName, EnvironmentVariableTarget.Process)
        ?? throw new Exception($"Faild to get enviroment value name of {MealApiKeyName}");

    var config = new BotConfig(mealApiKey, twitterKeys);
    var bot = new BotService(config);

    MealTimeFlags flag;

    if (args.Length < 1)
    {
        Log.Warning("Need argument about meal time flag.");
        return 1;
    }

    switch (args[0])
    {
        case "breakfast":
            flag = MealTimeFlags.Breakfast;
            break;

        case "lunch":
            flag = MealTimeFlags.Lunch;
            break;

        case "dorm_lunch":
            flag = MealTimeFlags.DormLunch;
            break;

        case "dinner":
            flag = MealTimeFlags.Dinner;
            break;

        default:
            Log.Warning("Invalid flag name.");
            return 1;
    }

    await bot.Run(flag);

    return 0;
}
catch (Exception e)
{
    Log.Fatal(e.ToString());
    return 1;
}
