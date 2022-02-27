#pragma warning disable IDE0079
#pragma warning disable IDE0051

using System;
using System.Text;
using System.Threading.Tasks;
using CKLunchBot.Core;
using CKLunchBot.Core.Drawing;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Tweetinvi;
using Tweetinvi.Models;
using Tweetinvi.Parameters;


namespace CKLunchBot
{
    public class TweetFunction
    {
        private record TwitterApiKeys(string ConsumerApiKey, string ConsumerSecretKey, string AccessToken, string AccessTokenSecret);

        private const string Cron = "0 0 22,2,7 * * 0-5"; // UTC 시간 고려해서 SUN-FRI
        // private const string TestCron = "0 */1 * * * *";

        private const int BreakfastHour = 8;
        private const int LunchHour = 11;
        private const int DinnerHour = 16;

        private const string ConsumerApiKeyName = "TWITTER_CONSUMER_API_KEY";
        private const string ConsumerSecretKeyName = "TWITTER_CONSUMER_SECRET_KEY";
        private const string AccessTokenName = "TWITTER_ACCESS_TOKEN";
        private const string AccessTokenSecretName = "TWITTER_ACCESS_TOKEN_SECRET";

        [FunctionName("tweet")]
        public static async Task Run([TimerTrigger(Cron)] TimerInfo timerInfo, ILogger log) // UTC
        {
            var now = KST.Now;
            log.LogInformation($"Function started: {now} KST");

            try
            {
                await ProcessAsync(log, now);
            }
            catch (ProcessFaildException e)
            {
                log.LogWarning(e.Message);
            }
            catch (Exception e)
            {
                log.LogError(e.Message);
                log.LogDebug(e.StackTrace);
            }
            finally
            {
                log.LogInformation($"Next: {timerInfo.Schedule.GetNextOccurrence(DateTime.Now)}");
            }
        }

        private static async Task ProcessAsync(ILogger log, DateTime now)
        {
            var date = now.ToDateOnly();

            var consumerApiKey = GetEnvVariable(ConsumerApiKeyName);
            var consumerSecretKey = GetEnvVariable(ConsumerSecretKeyName);
            var accessToken = GetEnvVariable(AccessTokenName);
            var accessTokenSecret = GetEnvVariable(AccessTokenSecretName);
            var twitterApiKeys = new TwitterApiKeys(consumerApiKey, consumerSecretKey, accessToken, accessTokenSecret);

            log.LogInformation($"Successfully loaded Twitter API keys.");

            MenuType? menuType = now.Hour switch
            {
                BreakfastHour => MenuType.Breakfast,
                LunchHour => MenuType.Lunch,
                DinnerHour => MenuType.Dinner,
                _ => null
            };

            if (menuType is null)
            {
                throw new ProcessFaildException("Tweet function was executed at an unspecified time.");
            }

            log.LogInformation($"Menu type = {menuType}");

            var weekMenu = await WeekMenu.LoadAsync();
            var todayMenu = weekMenu.Find(date);
            if (todayMenu is null)
            {
                throw new ProcessFaildException($"Cannot found menu of day {now.Day}.");
            }

            var menu = todayMenu[menuType.Value];
            if (menu is null || menu.IsEmpty())
            {
                throw new ProcessFaildException($"Cannot found menu of type {menuType}.");
            }

            log.LogInformation(menu.ToString());

            log.LogDebug(Environment.CurrentDirectory);
            log.LogInformation("Generating image...");
            var image = await MenuImageGenerator.GenerateAsync(date, menuType.Value, menu);

            var twitterClient = GetTwitterClient(twitterApiKeys);
            log.LogInformation("Tweeting...");

#if DEBUG
            log.LogInformation("Cannot tweet because it's debug mode.");
#else
            var tweetText = GetTweetText(date, menuType.Value);
            var tweet = await PublishTweetAsync(twitterClient, tweetText, image);
            log.LogInformation($"Done! {tweet.Url}");
#endif
        }

        private static string GetEnvVariable(string key)
        {
            return Environment.GetEnvironmentVariable(key, EnvironmentVariableTarget.Process)
                ?? throw new Exception($"Faild to get enviroment value name of {key}");
        }

        private static TwitterClient GetTwitterClient(TwitterApiKeys keys)
        {
            return new TwitterClient(keys.ConsumerApiKey, keys.ConsumerSecretKey, keys.AccessToken, keys.AccessTokenSecret);
        }

        private static string GetTweetText(DateOnly date, MenuType type)
        {
            return new StringBuilder()
                .Append("🥪")
                .Append(date.GetFormattedKoreanString())
                .Append(" 오늘의 청강대 ")
                .Append(type switch
                {
                    MenuType.Breakfast => "아침🌄",
                    MenuType.Lunch => "점심☀️",
                    MenuType.Dinner => "저녁🌇",
                    _ => string.Empty
                })
                .Append(" 메뉴는...")
                .ToString();
        }

        private static async Task<ITweet> PublishTweetAsync(TwitterClient twitter, string tweetText, byte[]? image = null)
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
