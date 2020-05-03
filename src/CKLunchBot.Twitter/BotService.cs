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
            StartupResult startupResult;
            TwitterClient client;
            try
            {
                startupResult = await Startup();
                client = startupResult.Client;

                tweetTime = (startupResult.Config.TweetTime.Hour, startupResult.Config.TweetTime.Minute);
                // test code
                //tweetTime = (TimeUtils.GetKoreaNowTime(DateTime.UtcNow).Hour, TimeUtils.GetKoreaNowTime(DateTime.UtcNow).Minute);
            }
            catch (CreateConfigException)
            {
                Log.Error("Create a new config.json because it does not exist. Please setup the config and start again.");
                return;
            }
            catch (Exception e)
            {
                Log.Fatal(e.ToString());
                Log.Information("The bot has stopped due to a startup error.");
                return;
            }

            while (true)
            {
                try
                {
                    await WaitForTweetTime(token);
                    Log.Information("--- Image tweet start ---");
                    Log.Information("Starting image generate...");
                    var image = await GenerateImageAsync();

                    await Tweet(client, image);

                    DateTime date = TimeUtils.GetKoreaNowTime(DateTime.UtcNow);
                    int day = date.AddDays(1).Day;
                    date = new DateTime(date.Year, date.Month, day, tweetTime.hour, tweetTime.minute, 0);
                    Log.Information($"Next tweet time is {date}");
                }
                catch (TaskCanceledException)
                {
                    Log.Information("The bot has stopped.");
                    break;
                }
                catch (Exception e)
                {
                    Log.Fatal(e.ToString());
                    Log.Information("The bot has stopped due to an error. It will try again at the next tweet time.");
                }
            }
        }

        private async Task Tweet(TwitterClient client, byte[] image)
        {
            Log.Information("Uploading tweet image...");
            var uploadedImage = await client.Upload.UploadTweetImage(image);
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
            return image;
        }

        private async Task<StartupResult> Startup()
        {
            ConfigItem config = LoadConfig();
            TwitterClient client = await ConnectToTwitter(config.TwitterTokens);

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

            JsonSerializer serializer = new JsonSerializer();

            if (!File.Exists(configFileName))
            {
                var newConfig = new ConfigItem()
                {
                    TweetTime = new ConfigItem.Time()
                    {
                        Hour = 11,
                        Minute = 50
                    },
                    TwitterTokens = new ConfigItem.TwitterToken()
                    {
                        ConsumerApiKey = "consumer_api_key",
                        ConsumerSecretKey = "consumer_secret_key",
                        AccessToken = "bot_access_token",
                        AccessTokenSecret = "bot_access_token_secret"
                    }
                };

                using var stream = File.CreateText(configFileName);
                using var jsonWriter = new JsonTextWriter(stream)
                {
                    Formatting = Formatting.Indented
                };
                serializer.Serialize(jsonWriter, newConfig, typeof(ConfigItem));

                throw new CreateConfigException();
            }

            using StreamReader configReader = File.OpenText(configFileName);
            using JsonReader configJsonReader = new JsonTextReader(configReader);
            config = serializer.Deserialize<ConfigItem>(configJsonReader);

            if (config is null)
            {
                throw new JsonException("All configs are could not be retrieved.");
            }

            #region tweet time

            var time = config.TweetTime;

            var faildList = new List<string>();
            if (time is null)
            {
                faildList.Add(ConfigItem.TweetTimePropertyName);
            }

            if (time.Hour > 23)
            {
                throw new JsonException("The hour value is must be 23 or less.");
            }
            if (time.Minute > 59)
            {
                throw new JsonException("The minute value is must be 59 or less.");
            }

            #endregion tweet time

            #region twitter tokens

            var tokens = config.TwitterTokens;

            if (tokens is null)
            {
                throw new JsonException("All tokens are could not be retrieved.");
            }

            if (tokens.ConsumerApiKey is null)
            {
                faildList.Add(ConfigItem.TwitterToken.ConsumerApiKeyPropertyName);
            }
            if (tokens.ConsumerSecretKey is null)
            {
                faildList.Add(ConfigItem.TwitterToken.ConsumerSecretKeyPropertyName);
            }
            if (tokens.AccessToken is null)
            {
                faildList.Add(ConfigItem.TwitterToken.AccessTokenPropertyName);
            }
            if (tokens.AccessTokenSecret is null)
            {
                faildList.Add(ConfigItem.TwitterToken.AccessTokenSecretPropertyName);
            }

            #endregion twitter tokens

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

            return config;
        }

        private async Task<TwitterClient> ConnectToTwitter(ConfigItem.TwitterToken tokens)
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