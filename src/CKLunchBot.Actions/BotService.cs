// Copyright (c) Sepi. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Threading.Tasks;

using CKLunchBot.Core.ImageProcess;
using CKLunchBot.Core.Menu;
using CKLunchBot.Core.Requester;
using CKLunchBot.Core.Utils;

using Serilog;

using Tweetinvi;
using Tweetinvi.Models;
using Tweetinvi.Parameters;

namespace CKLunchBot.Actions
{
    public record TwitterApiKeys(string ConsumerApiKey, string ConsumerSecretKey, string AccessToken, string AccessTokenSecret);
    public record BotConfig(string MealApiKey, TwitterApiKeys TwitterConfig);

    public class BotService
    {
        public enum MealTimeFlags
        {
            Breakfast, Lunch, DormLunch, Dinner
        }

        private readonly BotConfig _config;

        public BotService(BotConfig config)
        {
            _config = config;
        }

        /// <summary>
        /// Start the tweet process.
        /// </summary>
        public async Task Run(MealTimeFlags flag)
        {
            try
            {
                Log.Information("Loading bot twitter account...");
                TwitterClient twitter = await ConnectToTwitter(_config.TwitterConfig);

                Log.Information("Requesting menu list...");
                var menuRequester = new MenuRequester(_config.MealApiKey);
                RestaurantsWeekMenu menus = await GetWeekMenu(menuRequester);

                var tweetText = new StringBuilder()
                    .Append(TimeUtils.GetFormattedKoreaTime(DateTime.UtcNow))
                    .Append(" 오늘의 청강대 ")
                    .Append(flag switch
                    {
                        MealTimeFlags.Breakfast => "아침",
                        MealTimeFlags.Lunch => "점심",
                        MealTimeFlags.DormLunch => "기숙사 점심",
                        MealTimeFlags.Dinner => "저녁",
                        _ => throw new Exception()
                    })
                    .Append(" 메뉴는!")
                    .ToString();

                try
                {
                    Log.Information("Generating menu image...");
                    byte[] menuImage = await GenerateImageAsync(flag, menus);

                    Log.Information("Publishing tweet...");

#if DEBUG
                    Log.Warning("The bot didn't tweet because it's Debug mode.");
#endif

#if RELEASE
                    ITweet tweet = await PublishTweet(twitter, tweetText.ToString(), menuImage);
                    Log.Debug($"tweet: {tweet}");
                    Log.Information("Tweet publish completed.");
#endif
                }
                catch (NoProvidedMenuException e)
                {
                    Log.Information($"The bot didn't tweet because menu data doesn't exist.");

                    var sb = new StringBuilder();
                    sb.Append("No provided menu name: ");
                    for (int i = 0; i < e.RestaurantsName.Length; i++)
                    {
                        sb.Append(e.RestaurantsName[i].ToString());
                        if (i < e.RestaurantsName.Length - 1)
                        {
                            sb.Append(", ");
                        }
                    }
                    Log.Debug(sb.ToString());
                }
            }
            catch (Exception e)
            {
                Log.Fatal(e.ToString());
                Log.Warning("Bot has stopped due to an error.");
            }
        }

        private static async Task<RestaurantsWeekMenu> GetWeekMenu(MenuRequester requester)
        {
            RestaurantsWeekMenu menuList = await requester.RequestWeekMenuAsync();
            Log.Debug($"Responsed menu list count: {menuList.Count}");
            foreach (MenuItem menu in menuList.Values)
            {
                Log.Debug(menu.ToString());
            }

            return menuList;
        }

        [SuppressMessage("CodeQuality", "IDE0051", Justification = "<Pending>")]
        private static async Task<ITweet> PublishTweet(TwitterClient twitter, string tweetText, byte[] image = null)
        {
            ITweet tweet;

            if (image is null)
            {
                tweet = await twitter.Tweets.PublishTweetAsync(new PublishTweetParameters(tweetText.ToString()));
            }
            else
            {
                IMedia uploadedImage = await twitter.Upload.UploadTweetImageAsync(image);
                tweet = await twitter.Tweets.PublishTweetAsync(new PublishTweetParameters(tweetText.ToString())
                {
                    Medias = { uploadedImage }
                });
            }

            return tweet;
        }

        private static async Task<byte[]> GenerateImageAsync(MealTimeFlags flag, RestaurantsWeekMenu menuList)
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

        private static async Task<TwitterClient> ConnectToTwitter(TwitterApiKeys keys)
        {
            var client = new TwitterClient(keys.ConsumerApiKey, keys.ConsumerSecretKey, keys.AccessToken, keys.AccessTokenSecret);
            IAuthenticatedUser authenticatedUser = await client.Users.GetAuthenticatedUserAsync();

            Log.Debug("------------ Bot information ------------");
            Log.Debug($"Name: {authenticatedUser.Name} @{authenticatedUser.ScreenName}");
            Log.Debug($"ID:   {authenticatedUser.Id}");
            Log.Debug("-----------------------------------------");

            return client;
        }
    }
}
