using CKLunchBot.Core.ImageProcess;
using CKLunchBot.Core.Menu;
using CKLunchBot.Core.Utils;

using Newtonsoft.Json;

using Serilog;

using SixLabors.ImageSharp;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Tweetinvi;
using Tweetinvi.Models;
using Tweetinvi.Parameters;

namespace CKLunchBot.Twitter
{
    public class BotService : IDisposable
    {
        private bool alreadyTweeted;

        private ConfigItem config;
        private TwitterClient twitter;
        private MenuLoader menuLoader;

        /// <summary>
        /// Bot running task.
        /// </summary>
        /// <param name="token">task cancel token</param>
        /// <returns></returns>
        public async Task Run(CancellationToken token)
        {
            while (true)
            {
                if (disposedValue)
                {
                    return;
                }

                try
                {
                    #region test code

                    //for (int i = 0; i < 1; i++)
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

                    #endregion test code

                    await WaitForTweetTime(token, LunchTweetTime);

                    Log.Information("--- Image tweet start ---");
                    Log.Information("Starting image generate...");

                    var menuImage = await GenerateImageAsync();
                    SaveLogImage(menuImage);

#if DEBUG
                    Log.Information("The bot didn't tweet because it's Debug mode.");
#endif

#if RELEASE
                    await Tweet(menuImage);
#endif

                    DateTime date = TimeUtils.GetKoreaNowTime(DateTime.UtcNow);
                    int day = date.AddDays(1).Day;
                    date = new DateTime(date.Year, date.Month, day, LunchTweetTime.hour, LunchTweetTime.minute, 0);
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

        private async Task Tweet(byte[] image)
        {
            Log.Information("Uploading tweet image...");
            IMedia uploadedImage = await twitter.Upload.UploadTweetImage(image);
            Log.Information("Publishing tweet...");

            var tweetText = new StringBuilder();
            tweetText.Append("[청강대 학식] ");
            tweetText.Append(TimeUtils.GetFormattedKoreaTime(DateTime.UtcNow));
            tweetText.Append(" 오늘은...");
            ITweet tweetWithImage = await twitter.Tweets.PublishTweet(new PublishTweetParameters(tweetText.ToString())
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
                    using (var weekendImgGenerator = new WeekendImageGenerator())
                    {
                        image = await weekendImgGenerator.GenerateAsync();
                    }
                    break;

                default:
                    Log.Information("Requesting menu list...");
                    var menuList = await menuLoader.GetWeekMenuFromAPIAsync();
                    Log.Debug($"Responsed menu list count: {menuList.Count}");

                    try
                    {
                        // Tostring()이 검증되지 않음. 만약을 위해 예외처리
                        foreach (var menu in menuList)
                        {
                            Log.Debug(menu.ToString());
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Error(e.ToString());
                    }

                    Log.Information("Generating menu image...");
                    using (var menuImgGenerator = new MenuImageGenerator())
                    {
                        menuImgGenerator.SetMenu(menuList);
                        image = await menuImgGenerator.GenerateAsync();
                    }
                    break;
            }

            return image;
        }

        /// <summary>
        /// Setup bot. If setup succeeds, return true and other setup results.
        /// </summary>
        /// <returns></returns>
        public async Task<bool> Setup()
        {
            try
            {
                config = LoadConfig();
                twitter = await ConnectToTwitter(config.TwitterTokens);

                // test code
                //tweetTime = (TimeUtils.GetKoreaNowTime(DateTime.UtcNow).Hour, TimeUtils.GetKoreaNowTime(DateTime.UtcNow).Minute);
                menuLoader = new MenuLoader();

                Log.Information("Testing image generate...");
                var testImage = await GenerateImageAsync();
                SaveLogImage(testImage);
                Log.Information("Complete.");

                var ampm = LunchTweetTime.hour < 12 ? "a.m." : "p.m.";
                var hour = LunchTweetTime.hour > 12 ? LunchTweetTime.hour - 12 : LunchTweetTime.hour < 1 ? 12 : LunchTweetTime.hour;

                Log.Information($"This bot is tweet image always {hour:D2}:{LunchTweetTime.minute:D2} {ampm}");

                return true;
            }
            catch (ConfigCreatedException e)
            {
                string message = "Create a new config.json because ";
                switch (e.Reason)
                {
                    case ConfigCreatedException.Reasons.ConfigDoesNotExist:
                        message += "it does not exist. Please setup the config and start again.";
                        break;
                }

                Log.Error(message);
            }
            catch (Exception e)
            {
                Log.Fatal(e.ToString());
                Log.Information("The bot has stopped due to a startup error.");
            }

            return false;
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

                throw new ConfigCreatedException(ConfigCreatedException.Reasons.ConfigDoesNotExist);
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
                faildList.Add(ConfigItem.LunchTweetTimePropertyName);
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

        private void SaveLogImage(byte[] imageByte)
        {
            const string logImagesDirName = "log_images";
            Directory.CreateDirectory(logImagesDirName);
            using var memorystream = new MemoryStream(imageByte);
            using var filestream = new FileStream(Path.Combine(logImagesDirName, $"{TimeUtils.GetKoreaNowTime(DateTime.UtcNow):yyyy-MM-dd-HH-mm-ss}.png"), FileMode.Create);
            using var image = Image.Load(memorystream);
            image.SaveAsPng(filestream);
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

        private async Task WaitForTweetTime(CancellationToken token, (int hour, int minute) tweetTime)
        {
            while (true)
            {
                await Task.Delay(500);
                if (token.IsCancellationRequested)
                {
                    throw new TaskCanceledException();
                }
                var now = TimeUtils.GetKoreaNowTime(DateTime.UtcNow);
                if (now.Hour == tweetTime.hour && now.Minute == tweetTime.minute)
                {
                    if (alreadyTweeted)
                    {
                        continue;
                    }

                    alreadyTweeted = true;
                    break;
                }
                else
                {
                    alreadyTweeted = false;
                }
            }
        }

        #region IDisposable Support

        private bool disposedValue = false; // 중복 호출을 검색하려면

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    config = null;
                    twitter = null;
                }
                if (menuLoader != null)
                {
                    menuLoader.Dispose();
                }
                disposedValue = true;
            }
        }

        ~BotService()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable Support
    }
}