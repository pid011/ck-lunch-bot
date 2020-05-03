using CKLunchBot.Core.ImageProcess;
using CKLunchBot.Core.Menu;
using CKLunchBot.Core.Utils;

using Newtonsoft.Json;

using Serilog;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Tweetinvi;
using Tweetinvi.Parameters;

namespace CKLunchBot.Twitter
{
    public class BotService
    {
        private (int hour, int minute) tweetTime;

        private bool alreadyTweeted;

        public async Task Run(CancellationToken token)
        {
            // TODO: 실행 중 오류가 발생했을 경우 강제종료가 아닌 다음 트윗 시간에 다시 시도하도록 수정하기
            try
            {
                var startupResult = await Startup();
                var client = startupResult.Client;

                tweetTime = (startupResult.Config.TweetTime.Hour, startupResult.Config.TweetTime.Minute);
                //tweetTime = (TimeUtils.GetKoreaNowTime(DateTime.UtcNow).Hour, TimeUtils.GetKoreaNowTime(DateTime.UtcNow).Minute); // test code

                while (true)
                {
                    await WaitForTweetTime(token);
                    Log.Information("--- Image tweet start ---");
                    Log.Information("Starting image generate...");
                    var image = await GenerateImageAsync();

                    await Tweet(client, image);

                    var date = TimeUtils.GetKoreaNowTime(DateTime.UtcNow);
                    int day = date.AddDays(1).Day;
                    date = new DateTime(date.Year, date.Month, day, tweetTime.hour, tweetTime.minute, 0);
                    Log.Information($"Next tweet time is {date}");
                }
            }
            catch (TaskCanceledException)
            {
                Log.Information("The bot has stopped.");
            }
            catch (Exception e)
            {
                Log.Fatal(e.ToString());
                Log.Information("The bot has stopped due to an error.");
            }
        }

        private async Task Tweet(TwitterClient client, byte[] image)
        {
            Log.Information("Uploading tweet image...");
            var uploadedImage = await client.Upload.UploadTweetImage(image);
            //Log.Debug(uploadedImage.ToString());
            Log.Information("Publishing tweet...");
            var tweetText = TimeUtils.GetFormattedKoreaTime(DateTime.UtcNow) + " 오늘은...";
            tweetText = "[청강대 학식] " + tweetText;
            var tweetWithImage = await client.Tweets.PublishTweet(new PublishTweetParameters(tweetText)
            {
                Medias = { uploadedImage }
            });
            Log.Debug($"tweet: {tweetWithImage}");
            Log.Information("Tweet publish completed.");
        }

        private async Task<byte[]> GenerateImageAsync()
        {
            byte[] image;
            switch (TimeUtils.GetKoreaNowTime(DateTime.UtcNow).DayOfWeek)
            {
                case DayOfWeek.Sunday:
                case DayOfWeek.Saturday:
                    Log.Information("Generating weekend image...");
                    image = await Task.Run(() =>
                    {
                        using var generator = new WeekendImageGenerator();
                        return generator.Generate();
                    });
                    break;

                default:
                    Log.Information("Requesting menu list...");
                    var menuList = await new MenuLoader().GetWeekMenuFromAPIAsync();
                    Log.Debug("Responsed menu list:");
                    Log.Debug(menuList.ToString()); // foreach문으로 ToString 구현

                    Log.Information("Generating menu image...");
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

        private async Task<StartupResult> Startup()
        {
            ConfigItem config = LoadConfig();
            TwitterTokens tokens = LoadTwitterToken();
            TwitterClient client = await ConnectToTwitter(tokens);

            tweetTime = (config.TweetTime.Hour, config.TweetTime.Minute);

            var ampm = tweetTime.hour < 12 ? "a.m." : "p.m.";
            var hour = tweetTime.hour > 12 ? tweetTime.hour - 12 : tweetTime.hour < 1 ? 12 : tweetTime.hour;

            Log.Information($"This bot is tweet image always {hour:D2}:{tweetTime.minute:D2} {ampm}");

            return new StartupResult(config, client);
        }

        private ConfigItem LoadConfig()
        {
            const string configFileName = "config.json";
            ConfigItem config;
            using StreamReader configReader = File.OpenText(configFileName);
            using JsonReader configJsonReader = new JsonTextReader(configReader);
            config = new JsonSerializer().Deserialize<ConfigItem>(configJsonReader);

            if (config is null)
            {
                throw new JsonException("All configs are could not be retrieved.");
            }

            var faildList = new List<string>();
            if (config.TweetTime is null)
            {
                faildList.Add(ConfigItem.TweetTimePropertyName);
            }

            if (faildList.Count != 0)
            {
                var errorMessage = new StringBuilder("Faild to loaded configs: ");
                for (int i = 0; i < faildList.Count; i++)
                {
                    errorMessage.Append(faildList[i]);
                    if (i < faildList.Count - 1)
                    {
                        errorMessage.Append(", ");
                    }
                }
                throw new JsonException(errorMessage.ToString());
            }

            if (config.TweetTime.Hour > 23)
            {
                throw new JsonException("The hour value is must be 23 or less.");
            }
            if (config.TweetTime.Minute > 59)
            {
                throw new JsonException("The minute value is must be 59 or less.");
            }

            return config;
        }

        private TwitterTokens LoadTwitterToken()
        {
            const string tokensFileName = "twitter-tokens.json";
            TwitterTokens tokens;
            using StreamReader tokenReader = File.OpenText(tokensFileName);
            using JsonReader tokenJsonReader = new JsonTextReader(tokenReader);
            tokens = new JsonSerializer().Deserialize<TwitterTokens>(tokenJsonReader);

            if (tokens is null)
            {
                throw new JsonException("All tokens are could not be retrieved.");
            }

            var faildList = new List<string>();
            if (tokens.ConsumerApiKey is null)
            {
                faildList.Add(TwitterTokens.ConsumerApiKeyPropertyName);
            }
            if (tokens.ConsumerSecretKey is null)
            {
                faildList.Add(TwitterTokens.ConsumerSecretKeyPropertyName);
            }
            if (tokens.AccessToken is null)
            {
                faildList.Add(TwitterTokens.AccessTokenPropertyName);
            }
            if (tokens.AccessTokenSecret is null)
            {
                faildList.Add(TwitterTokens.AccessTokenSecretPropertyName);
            }

            if (faildList.Count != 0)
            {
                var errorMessage = new StringBuilder("Faild to loaded tokens: ");
                for (int i = 0; i < faildList.Count; i++)
                {
                    errorMessage.Append(faildList[i]);
                    if (i < faildList.Count - 1)
                    {
                        errorMessage.Append(", ");
                    }
                }
                throw new JsonException(errorMessage.ToString());
            }

            return tokens;
        }

        private async Task<TwitterClient> ConnectToTwitter(TwitterTokens tokens)
        {
            // TwitterClient를 만들고 토큰이 제대로 된건지 테스트 과정 필요
            var client = new TwitterClient(tokens.ConsumerApiKey, tokens.ConsumerSecretKey, tokens.AccessToken, tokens.AccessTokenSecret);
            var authenticatedUser = await client.Users.GetAuthenticatedUser();
            var botInfo = new StringBuilder();
            botInfo.AppendLine("---Bot information---");
            botInfo.AppendLine($"Bot name: {authenticatedUser.Name}");
            botInfo.AppendLine($"Bot description: {authenticatedUser.Description}");
            Log.Debug(botInfo.ToString());

            return client;
        }

        private async Task WaitForTweetTime(CancellationToken token)
        {
            while (true)
            {
                if (token.IsCancellationRequested)
                {
                    throw new TaskCanceledException();
                }
                await Task.Delay(500);
                var now = TimeUtils.GetKoreaNowTime(DateTime.UtcNow);
                if (now.Hour == tweetTime.hour && now.Minute == tweetTime.minute)
                {
                    if (alreadyTweeted)
                    {
                        continue;
                    }

                    //#region test code

                    //for (int i = 0; i < 2; i++)
                    //{
                    //    tweetTime.minute++;
                    //    if (tweetTime.minute > 59)
                    //    {
                    //        tweetTime.minute = 0;
                    //        tweetTime.hour++;
                    //        if (tweetTime.hour > 23)
                    //        {
                    //            tweetTime.hour = 0;
                    //        }
                    //    }
                    //}

                    //#endregion test code

                    alreadyTweeted = true;
                    break;
                }
                else
                {
                    alreadyTweeted = false;
                }
            }
        }
    }

    public class StartupResult
    {
        public ConfigItem Config { get; }

        public TwitterClient Client { get; }

        public StartupResult(ConfigItem config, TwitterClient client)
        {
            Config = config;
            Client = client;
        }
    }
}