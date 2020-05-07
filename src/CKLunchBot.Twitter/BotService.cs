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
        private enum MealTimeFlags
        {
            Breakfast, Lunch, Dinner
        }

        private ConfigItem m_config;
        private TwitterClient m_twitter;
        private MenuLoader m_menuLoader;

        private (MealTimeFlags flag, DateTime nextTweetTime)[] m_nextTweetTimes;
        private DateTime m_lastTweetedTime;

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

                    Log.Information("Waiting for next tweet time...");
                    var flag = await WaitForTweetTime(token);

                    Log.Information("--- Image tweet start ---");

                    Log.Information("Requesting menu list...");
                    var menus = await GetWeekMenu();

                    Log.Information("Generating menu image...");
                    byte[] menuImage = await GenerateImageAsync(flag, menus);

                    SaveLogImage(menuImage);

                    Log.Information("Publishing tweet...");
                    var tweetText = new StringBuilder();
                    tweetText.Append("청강대 ");
                    tweetText.Append(TimeUtils.GetFormattedKoreaTime(DateTime.UtcNow));
                    tweetText.Append("오늘의 ");
                    switch (flag)
                    {
                        case MealTimeFlags.Breakfast:
                            tweetText.Append("아침");
                            break;

                        case MealTimeFlags.Lunch:
                            tweetText.Append("점심");
                            break;

                        case MealTimeFlags.Dinner:
                            tweetText.Append("저녁");
                            break;
                    }
                    tweetText.Append("메뉴는...");
#if DEBUG
                    Log.Information("The bot didn't tweet because it's Debug mode.");
#endif
#if RELEASE
                    var tweet = await PublishTweet(tweetText.ToString(), menuImage);
                    Log.Debug($"tweet: {tweet}");
                    Log.Information("Tweet publish completed.");
#endif
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

        private async Task<List<MenuItem>> GetWeekMenu()
        {
            List<MenuItem> menuList = await m_menuLoader.GetWeekMenuFromAPIAsync();
            Log.Debug($"Responsed menu list count: {menuList.Count}");
            foreach (var menu in menuList)
            {
                Log.Debug(menu.ToString());
            }

            return menuList;
        }

        private async Task<ITweet> PublishTweet(string tweetText, byte[] image = null)
        {
            ITweet tweet;

            if (image is null)
            {
                tweet = await m_twitter.Tweets.PublishTweet(new PublishTweetParameters(tweetText.ToString()));
            }
            else
            {
                IMedia uploadedImage = await m_twitter.Upload.UploadTweetImage(image);
                tweet = await m_twitter.Tweets.PublishTweet(new PublishTweetParameters(tweetText.ToString())
                {
                    Medias = { uploadedImage }
                });
            }

            return tweet;
        }

        private async Task<byte[]> GenerateImageAsync(MealTimeFlags flag, List<MenuItem> menuList)
        {
            byte[] byteImage = null;

            async Task CreateWeekMenuAsync()
            {
                switch (flag)
                {
                    case MealTimeFlags.Breakfast:
                        byteImage = await MenuImageGenerator.GenerateTodayDormMenuImageAsync(Restaurants.DormBreakfast, menuList);
                        break;

                    case MealTimeFlags.Lunch:
                        byteImage = await MenuImageGenerator.GenerateTodayLunchMenuImageAsync(menuList);
                        break;

                    case MealTimeFlags.Dinner:
                        byteImage = await MenuImageGenerator.GenerateTodayDormMenuImageAsync(Restaurants.DormDinner, menuList);
                        break;
                }
            }

            async Task CreateWeekendMenuAsync()
            {
                switch (flag)
                {
                    case MealTimeFlags.Breakfast:
                        byteImage = await MenuImageGenerator.GenerateTodayDormMenuImageAsync(Restaurants.DormBreakfast, menuList);
                        break;

                    case MealTimeFlags.Lunch:
                        byteImage = await MenuImageGenerator.GenerateTodayDormMenuImageAsync(Restaurants.DormLunch, menuList);
                        break;

                    case MealTimeFlags.Dinner:
                        byteImage = await MenuImageGenerator.GenerateTodayDormMenuImageAsync(Restaurants.DormDinner, menuList);
                        break;
                }
            }

            switch (TimeUtils.GetKoreaNowTime(DateTime.UtcNow).DayOfWeek)
            {
                case DayOfWeek.Sunday:
                case DayOfWeek.Saturday:
                    await CreateWeekendMenuAsync();
                    break;

                default:
                    await CreateWeekMenuAsync();
                    break;
            }

            return byteImage;
        }

        /// <summary>
        /// Setup bot.
        /// </summary>
        /// <returns></returns>
        public async Task<bool> Setup()
        {
            try
            {
                m_config = LoadConfig();

                static DateTime GetNextDateTime(DateTime now, DateTime target)
                {
                    int t1 = target.Hour - now.Hour;
                    int t2 = target.Minute - now.Minute;
                    //만약 target이 now보다 이전이거나 같으면 AddDays(1)
                    return (t1 < 0 || (t1 == 0 && t2 <= 0)) ? target.AddDays(1) : target;
                }

                var now = TimeUtils.GetKoreaNowTime(DateTime.UtcNow);

                var nextBreakfastTweetTime = GetNextDateTime(now, new DateTime(now.Year, now.Month, now.Day,
                                                                               m_config.BreakfastTweetTime.Hour,
                                                                               m_config.BreakfastTweetTime.Minute, 0));

                var nextLunchTweetTime = GetNextDateTime(now, new DateTime(now.Year, now.Month, now.Day,
                                                                           m_config.LunchTweetTime.Hour,
                                                                           m_config.LunchTweetTime.Minute, 0));

                var nextDinnerTweetTime = GetNextDateTime(now, new DateTime(now.Year, now.Month, now.Day,
                                                                            m_config.DinnerTweetTime.Hour,
                                                                            m_config.DinnerTweetTime.Minute, 0));

                m_nextTweetTimes = new (MealTimeFlags, DateTime)[]
                {
                    (MealTimeFlags.Breakfast, nextBreakfastTweetTime),
                    (MealTimeFlags.Lunch, nextLunchTweetTime),
                    (MealTimeFlags.Dinner, nextDinnerTweetTime)
                };
                m_lastTweetedTime = now;

                m_twitter = await ConnectToTwitter(m_config.TwitterTokens);

                // test code
                //tweetTime = (TimeUtils.GetKoreaNowTime(DateTime.UtcNow).Hour, TimeUtils.GetKoreaNowTime(DateTime.UtcNow).Minute);
                m_menuLoader = new MenuLoader();

                Log.Information("Testing image generate...");
                var menus = await GetWeekMenu();
                var testImage = await GenerateImageAsync(MealTimeFlags.Lunch, menus);
                SaveLogImage(testImage);
                Log.Information("Complete.");

                Log.Information(
                    $"Dormitory breakfast menu image tweet time: {m_config.BreakfastTweetTime.Hour:D2}:{m_config.BreakfastTweetTime.Minute:D2}");
                Log.Information(
                    $"School cafeteria lunch menu image tweet time: {m_config.LunchTweetTime.Hour:D2}:{m_config.LunchTweetTime.Minute:D2}");
                Log.Information(
                    $"Dormitory dinner menu image tweet time: {m_config.DinnerTweetTime.Hour:D2}:{m_config.DinnerTweetTime.Minute:D2}");

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
                    BreakfastTweetTime = new ConfigItem.Time()
                    {
                        Hour = 7,
                        Minute = 50
                    },
                    LunchTweetTime = new ConfigItem.Time()
                    {
                        Hour = 11,
                        Minute = 50
                    },
                    DinnerTweetTime = new ConfigItem.Time()
                    {
                        Hour = 17,
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

            var faildList = new List<string>();

            #region tweet time

            if (config.BreakfastTweetTime is null)
            {
                faildList.Add(ConfigItem.BreakfastTweetTimePropertyName);
            }
            else
            {
                CheckTimeInstance(config.BreakfastTweetTime);
            }

            if (config.LunchTweetTime is null)
            {
                faildList.Add(ConfigItem.LunchTweetTimePropertyName);
            }
            else
            {
                CheckTimeInstance(config.LunchTweetTime);
            }

            if (config.DinnerTweetTime is null)
            {
                faildList.Add(ConfigItem.DinnerTweetTimePropertyName);
            }
            else
            {
                CheckTimeInstance(config.DinnerTweetTime);
            }

            static void CheckTimeInstance(ConfigItem.Time time)
            {
                if (time.Hour > 23)
                {
                    throw new JsonException("The hour value is must be 23 or less.");
                }
                if (time.Minute > 59)
                {
                    throw new JsonException("The minute value is must be 59 or less.");
                }
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

        private async Task<MealTimeFlags> WaitForTweetTime(CancellationToken token)
        {
            while (true)
            {
                await Task.Delay(1000);

                if (token.IsCancellationRequested)
                {
                    throw new TaskCanceledException();
                }
                var now = TimeUtils.GetKoreaNowTime(DateTime.UtcNow);

                if (now.Hour == m_lastTweetedTime.Hour && now.Minute == m_lastTweetedTime.Hour && now.Second != 0)
                {
                    continue;
                }

                for (int i = 0; i < m_nextTweetTimes.Length; i++)
                {
                    var (flag, nextTweetTime) = m_nextTweetTimes[i];

                    if (now.Hour != nextTweetTime.Hour || now.Minute != nextTweetTime.Minute)
                    {
                        continue;
                    }

                    m_lastTweetedTime = nextTweetTime;
                    m_nextTweetTimes[i].nextTweetTime = nextTweetTime.AddDays(1);

                    return flag;
                }
            }
        }

        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    m_config = null;
                    m_twitter = null;
                }
                if (m_menuLoader != null)
                {
                    m_menuLoader.Dispose();
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~BotService()
        {
            Dispose(false);
        }
    }
}