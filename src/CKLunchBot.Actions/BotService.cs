// Copyright (c) Sepi. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading.Tasks;

using CKLunchBot.Core.ImageProcess;
using CKLunchBot.Core.Menu;
using CKLunchBot.Core.Requester;

using Tweetinvi;
using Tweetinvi.Models;
using Tweetinvi.Parameters;

using static System.Console;

namespace CKLunchBot.Actions
{
    public record TwitterApiKeys(string ConsumerApiKey, string ConsumerSecretKey, string AccessToken, string AccessTokenSecret);
    public record BotConfig(string MealApiKey, TwitterApiKeys TwitterConfig);

    public enum MealTimeFlags
    {
        Breakfast, Lunch, DormLunch, Dinner
    }

    public class BotService
    {
        public static async Task<RestaurantsWeekMenu> GetWeekMenu(MenuRequester requester)
        {
            RestaurantsWeekMenu menuList = await requester.RequestWeekMenuAsync();
            WriteLine($"Responsed menu list count: {menuList.Count}");
            foreach (MenuItem menu in menuList.Values)
            {
                WriteLine(menu.ToString());
            }

            return menuList;
        }

        public static async Task<ITweet> PublishTweet(TwitterClient twitter, string tweetText, byte[] image = null)
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

        public static async Task<byte[]> GenerateImageAsync(MealTimeFlags flag, RestaurantsWeekMenu menuList)
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

        public static async Task<TwitterClient> ConnectToTwitter(TwitterApiKeys keys)
        {
            var client = new TwitterClient(keys.ConsumerApiKey, keys.ConsumerSecretKey, keys.AccessToken, keys.AccessTokenSecret);
            IAuthenticatedUser authenticatedUser = await client.Users.GetAuthenticatedUserAsync();

            WriteLine("------------ Bot information ------------");
            WriteLine($"Name: {authenticatedUser.Name} @{authenticatedUser.ScreenName}");
            WriteLine($"ID:   {authenticatedUser.Id}");
            WriteLine("-----------------------------------------");

            return client;
        }
    }
}
