// Copyright (c) Sepi. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading.Tasks;
using CKLunchBot.Core.Menu;
using CKLunchBot.Core.Utils;
using Serilog;
using Tweetinvi;
using Tweetinvi.Models;
using Tweetinvi.Parameters;

namespace CKLunchBot.Actions
{
    public record TwitterApiKeys(string ConsumerApiKey, string ConsumerSecretKey, string AccessToken, string AccessTokenSecret);
    public record BotConfig(TwitterApiKeys TwitterConfig);

    public static class BotService
    {
        public static async Task<WeekMenu> GetWeekMenuAsync()
        {
            return await WeekMenu.LoadAsync();
        }

        public static TodayMenu SelectTodayMenu(WeekMenu weekMenu)
        {
            var today = TimeUtils.KSTNow;
            return SelectMenu(weekMenu, today.Day);
        }

        public static TodayMenu SelectMenu(WeekMenu weekMenu, int day)
        {
            var todayMenu = weekMenu.FindMenu(day);
            return todayMenu;
        }

        public static async Task<byte[]> GenerateImageAsync(TodayMenu menu, MenuType type)
        {
            return await menu.MakeImageAsync(type);
        }

        public static async Task<TwitterClient> ConnectToTwitterAsync(TwitterApiKeys keys)
        {
            var client = new TwitterClient(keys.ConsumerApiKey, keys.ConsumerSecretKey, keys.AccessToken, keys.AccessTokenSecret);
            IAuthenticatedUser authenticatedUser = await client.Users.GetAuthenticatedUserAsync();

            Log.Debug("------------ Bot information ------------");
            Log.Debug($"Name: {authenticatedUser.Name} @{authenticatedUser.ScreenName}");
            Log.Debug($"ID:   {authenticatedUser.Id}");
            Log.Debug("-----------------------------------------");

            return client;
        }

        public static async Task<ITweet> PublishTweetAsync(TwitterClient twitter, string tweetText, byte[] image = null)
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
    }
}
