// Copyright (c) Sepi. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using CKLunchBot.Core.ImageProcess;
using CKLunchBot.Core.Menu;
using CKLunchBot.Core.Requester;
using CKLunchBot.Core.Utils;

using Newtonsoft.Json;

using Serilog;

using SixLabors.ImageSharp;

using Tweetinvi;
using Tweetinvi.Models;
using Tweetinvi.Parameters;

namespace CKLunchBot.Twitter
{
    public class BotService : IDisposable
    {
        private enum MealTimeFlags
        {
            Breakfast, Lunch, DormLunch, Dinner
        }

        private TwitterClient _twitter;

        private MenuRequester _menuRequester;

        private (MealTimeFlags flag, DateTime nextTweetTime)[] _nextTweetTimes;
        private DateTime _lastTweetedTime;

        private readonly StringBuilder _tmpStrBuilder = new StringBuilder();

        /// <summary>
        /// Bot running task.
        /// </summary>
        /// <param name="token">task cancel token</param>
        /// <returns></returns>
        public async Task Run(CancellationToken token)
        {
            while (true)
            {
                if (_disposedValue)
                {
                    return;
                }

                try
                {
                    _tmpStrBuilder.AppendLine("Next tweet time list:");
                    foreach ((MealTimeFlags flag, DateTime nextTweetTime) tweetTime in _nextTweetTimes)
                    {
                        _tmpStrBuilder.AppendLine($"{tweetTime.flag}: {tweetTime.nextTweetTime}");
                    }
                    Log.Debug(_tmpStrBuilder.ToString());
                    _tmpStrBuilder.Clear();

                    Log.Information("Waiting for next tweet time...");
                    MealTimeFlags flag = await WaitForTweetTime(token);
                    Log.Information($"Tweet target: {flag}");
                    Log.Information("--- Image tweet start ---");

                    Log.Information("Requesting menu list...");
                    RestaurantsWeekMenu menus = await GetWeekMenu();

                    _tmpStrBuilder.Append(TimeUtils.GetFormattedKoreaTime(DateTime.UtcNow));
                    _tmpStrBuilder.Append(" 오늘의 청강대 ");
                    switch (flag)
                    {
                        case MealTimeFlags.Breakfast:
                            _tmpStrBuilder.Append("아침");
                            break;

                        case MealTimeFlags.Lunch:
                            _tmpStrBuilder.Append("점심");
                            break;

                        case MealTimeFlags.Dinner:
                            _tmpStrBuilder.Append("저녁");
                            break;
                    }
                    _tmpStrBuilder.Append("메뉴는...");
                    string tweetText = _tmpStrBuilder.ToString();
                    _tmpStrBuilder.Clear();

                    try
                    {
                        Log.Information("Generating menu image...");
                        byte[] menuImage = await GenerateImageAsync(flag, menus);
                        SaveLogImage(menuImage);

                        Log.Information("Publishing tweet...");
#if DEBUG
                        Log.Information("The bot didn't tweet because it's Debug mode.");
#endif
#if RELEASE
                        var tweet = await PublishTweet(tweetText.ToString(), menuImage);
                        Log.Debug($"tweet: {tweet}");
                        Log.Information("Tweet publish completed.");
#endif
                    }
                    catch (NoProvidedMenuException e)
                    {
                        Log.Information($"The bot didn't tweet because menu data doesn't exist.");
                        _tmpStrBuilder.Append("No provided menu name: ");
                        for (int i = 0; i < e.RestaurantsName.Length; i++)
                        {
                            _tmpStrBuilder.Append(e.RestaurantsName[i].ToString());
                            if (i < e.RestaurantsName.Length - 1)
                            {
                                _tmpStrBuilder.Append(", ");
                            }
                        }
                        Log.Debug(_tmpStrBuilder.ToString());
                        _tmpStrBuilder.Clear();
                        continue;
                    }
                }
                catch (TaskCanceledException)
                {
                    Log.Information("The bot has stopped.");
                    break;
                }
                catch (Exception e)
                {
                    Log.Fatal(e.ToString());
                    Log.Information("Bot has stopped due to an error.");
                    break;
                }
            }
        }

        private async Task<RestaurantsWeekMenu> GetWeekMenu()
        {
            RestaurantsWeekMenu menuList = await _menuRequester.RequestWeekMenuAsync();
            Log.Debug($"Responsed menu list count: {menuList.Count}");
            foreach (MenuItem menu in menuList.Values)
            {
                Log.Debug(menu.ToString());
            }

            return menuList;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
        private async Task<ITweet> PublishTweet(string tweetText, byte[] image = null)
        {
            ITweet tweet;

            if (image is null)
            {
                tweet = await _twitter.Tweets.PublishTweet(new PublishTweetParameters(tweetText.ToString()));
            }
            else
            {
                IMedia uploadedImage = await _twitter.Upload.UploadTweetImage(image);
                tweet = await _twitter.Tweets.PublishTweet(new PublishTweetParameters(tweetText.ToString())
                {
                    Medias = { uploadedImage }
                });
            }

            return tweet;
        }

        private async Task<byte[]> GenerateImageAsync(MealTimeFlags flag, RestaurantsWeekMenu menuList)
        {
            byte[] byteImage = null;

            switch (flag)
            {
                case MealTimeFlags.Breakfast:
                    byteImage = await MenuImageGenerator.GenerateTodayDormMenuImageAsync(menuList[Restaurants.DormBreakfast]);
                    break;

                case MealTimeFlags.Lunch:
                    byteImage = await MenuImageGenerator.GenerateTodayLunchMenuImageAsync(menuList);
                    break;

                case MealTimeFlags.DormLunch:
                    byteImage = await MenuImageGenerator.GenerateTodayDormMenuImageAsync(menuList[Restaurants.DormLunch]);
                    break;

                case MealTimeFlags.Dinner:
                    byteImage = await MenuImageGenerator.GenerateTodayDormMenuImageAsync(menuList[Restaurants.DormDinner]);
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
                ConfigItem config = LoadConfig();

                static DateTime GetNextDateTime(DateTime now, DateTime target)
                {
                    int t1 = target.Hour - now.Hour;
                    int t2 = target.Minute - now.Minute;
                    //만약 target이 now보다 이전이거나 같으면 AddDays(1)
                    return (t1 < 0 || (t1 == 0 && t2 <= 0)) ? target.AddDays(1) : target;
                }

                DateTime now = TimeUtils.GetKoreaNowTime(DateTime.UtcNow);

                DateTime nextBreakfastTweetTime = GetNextDateTime(now, new DateTime(now.Year, now.Month, now.Day,
                                                                               config.BreakfastTweetTime.Hour,
                                                                               config.BreakfastTweetTime.Minute, 0));

                DateTime nextLunchTweetTime = GetNextDateTime(now, new DateTime(now.Year, now.Month, now.Day,
                                                                           config.LunchTweetTime.Hour,
                                                                           config.LunchTweetTime.Minute, 0));

                DateTime nextDinnerTweetTime = GetNextDateTime(now, new DateTime(now.Year, now.Month, now.Day,
                                                                            config.DinnerTweetTime.Hour,
                                                                            config.DinnerTweetTime.Minute, 0));

                _nextTweetTimes = new (MealTimeFlags, DateTime)[]
                {
                    (MealTimeFlags.Breakfast, nextBreakfastTweetTime),
                    (MealTimeFlags.Lunch, nextLunchTweetTime),
                    // Tweet dorm lunch menu after 10 seconds tweeting the school cafeteria menu.
                    (MealTimeFlags.DormLunch, nextLunchTweetTime.AddSeconds(10)),
                    (MealTimeFlags.Dinner, nextDinnerTweetTime)
                };

                // bot test code
                //m_nextTweetTimes = new (MealTimeFlags, DateTime)[]
                //{
                //    (MealTimeFlags.Breakfast, now.AddSeconds(30)),
                //    (MealTimeFlags.Lunch, now.AddSeconds(40)),
                //    (MealTimeFlags.DormLunch, now.AddSeconds(50)),
                //    (MealTimeFlags.Dinner, now.AddSeconds(60))
                //};

                _lastTweetedTime = now;

                _twitter = await ConnectToTwitter(config.TwitterTokens);

                _menuRequester = new MenuRequester();

                Log.Information("Testing image generate...");
                try
                {
                    RestaurantsWeekMenu menus = await GetWeekMenu();
                    var testImage = await GenerateImageAsync(MealTimeFlags.Lunch, menus);
                    SaveLogImage(testImage);
                }
                catch (NoProvidedMenuException)
                {
                }
                Log.Information("Complete.");

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

                using StreamWriter stream = File.CreateText(configFileName);
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

            static void CheckTimeInstance(ConfigItem.Time time)
            {
                if (time is null)
                {
                    throw new ArgumentNullException(nameof(time));
                }

                if (time.Hour > 23)
                {
                    throw new JsonException("The hour value is must be 23 or less.");
                }
                if (time.Minute > 59)
                {
                    throw new JsonException("The minute value is must be 59 or less.");
                }
            }

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

            ConfigItem.TwitterToken tokens = config.TwitterTokens;

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

            if (faildList.Count != 0)
            {
                _tmpStrBuilder.Append("Faild to loaded tokens: ");
                for (int i = 0; i < faildList.Count; i++)
                {
                    _tmpStrBuilder.Append(faildList[i]);
                    if (i < faildList.Count - 1)
                    {
                        _tmpStrBuilder.Append(", ");
                    }
                }
                var error = _tmpStrBuilder.ToString();
                _tmpStrBuilder.Clear();
                throw new JsonException(error);
            }

            if (config.BreakfastTweetTime == config.LunchTweetTime
                || config.BreakfastTweetTime == config.DinnerTweetTime
                || config.LunchTweetTime == config.DinnerTweetTime)
            {
                throw new JsonException("Each tweet time value should not be the same.");
            }

            return config;
        }

        private void SaveLogImage(byte[] imageByte)
        {
            if (imageByte is null)
            {
                throw new ArgumentNullException(nameof(imageByte));
            }

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
            IAuthenticatedUser authenticatedUser = await client.Users.GetAuthenticatedUser();
            _tmpStrBuilder.AppendLine("---Bot information---");
            _tmpStrBuilder.AppendLine($"Bot name: {authenticatedUser.Name}");
            _tmpStrBuilder.AppendLine($"Bot description: {authenticatedUser.Description}");
            Log.Debug(_tmpStrBuilder.ToString());
            _tmpStrBuilder.Clear();
            return client;
        }

        private async Task<MealTimeFlags> WaitForTweetTime(CancellationToken token)
        {
            DateTime now;
            MealTimeFlags flag;
            DateTime nextTweetTime;

            while (true)
            {
                await Task.Delay(1000);

                if (token.IsCancellationRequested)
                {
                    throw new TaskCanceledException();
                }
                now = TimeUtils.GetKoreaNowTime(DateTime.UtcNow);

                if (now - _lastTweetedTime == TimeSpan.Zero)
                {
                    continue;
                }

                for (int i = 0; i < _nextTweetTimes.Length; i++)
                {
                    (flag, nextTweetTime) = _nextTweetTimes[i];

                    if (now.Hour != nextTweetTime.Hour || now.Minute != nextTweetTime.Minute || now.Second != nextTweetTime.Second)
                    {
                        continue;
                    }

                    _lastTweetedTime = nextTweetTime;
                    _nextTweetTimes[i].nextTweetTime = nextTweetTime.AddDays(1);

                    return flag;
                }
            }
        }

        private bool _disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    //m_config = null;
                    _twitter = null;
                }
                if (_menuRequester != null)
                {
                    _menuRequester.Dispose();
                }
                _disposedValue = true;
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
