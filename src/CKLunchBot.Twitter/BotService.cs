using CKLunchBot.Core.Image;
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
        // TODO: config.json에서 시간을 조정할 수 있도록 수정
        private readonly (int hour, int minute) tweetTime = (8, 50);
        //private (int hour, int minute) tweetTime = (TimeUtils.Time.KoreaNowTime.Hour, TimeUtils.Time.KoreaNowTime.Minute);

        private bool alreadyTweeted;

        public async Task Run(CancellationToken token)
        {
            try
            {
                var startupResult = await Startup();
                var client = startupResult.Client;

                while (true)
                {
                    await WaitForTweetTime(token);
                    Log.Information("--- Image tweet start ---");
                    Log.Information("Starting image generate...");
                    var image = await GenerateImageAsync();

                    Log.Information("Uploading tweet image...");
                    var uploadedImage = await client.Upload.UploadTweetImage(image);
                    //Log.Debug(uploadedImage.ToString());
                    Log.Information("Publishing tweet...");
                    var tweetWithImage = await client.Tweets.PublishTweet(new PublishTweetParameters("")
                    {
                        Medias = { uploadedImage }
                    });
                    Log.Debug($"tweet link: {tweetWithImage}");
                    Log.Information("Tweet publish completed.");

                    var date = TimeUtils.Time.KoreaNowTime;
                    int month = date.Month;
                    int day = date.Day + 1;

                    date = new DateTime(date.Year,
                                        date.Month,
                                        date.Day + 1,
                                        tweetTime.hour,
                                        tweetTime.minute,
                                        0);
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
                Log.Debug(e.Message);
                Log.Debug(e.Source);
                Log.Debug(e.StackTrace);
                Log.Information("The bot has stopped due to an error.");
            }
        }

        private async Task<byte[]> GenerateImageAsync()
        {
            byte[] image;
            switch (TimeUtils.Time.KoreaNowTime.DayOfWeek)
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
            const string tokensFileName = "ck_lunch_bot_twitter_tokens.json";
            TwitterTokens tokens;
            try
            {
                using StreamReader reader = File.OpenText(tokensFileName);
                var serializer = new JsonSerializer();
                tokens = (TwitterTokens)serializer.Deserialize(reader, typeof(TwitterTokens));
            }
            catch (FileNotFoundException e)
            {
                throw new FileNotFoundException($"{tokensFileName} does not exist.", e);
            }

            if (tokens is null)
            {
                throw new JsonException("All tokens are could not be retrieved.");
            }

            var faildList = new List<string>();
            if (tokens.ConsumerApiKey is null)
            {
                faildList.Add(TwitterTokens.ConsumerApiKeyName);
            }
            if (tokens.ConsumerSecretKey is null)
            {
                faildList.Add(TwitterTokens.ConsumerSecretKeyName);
            }
            if (tokens.AccessToken is null)
            {
                faildList.Add(TwitterTokens.AccessTokenName);
            }
            if (tokens.AccessTokenSecret is null)
            {
                faildList.Add(TwitterTokens.AccessTokenSecretName);
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

            // TwitterClient를 만들고 토큰이 제대로 된건지 테스트 과정 필요
            var client = new TwitterClient(tokens.ConsumerApiKey, tokens.ConsumerSecretKey, tokens.AccessToken, tokens.AccessTokenSecret);
            var authenticatedUser = await client.Users.GetAuthenticatedUser();
            var botInfo = new StringBuilder();
            botInfo.AppendLine("[Bot information]");
            botInfo.AppendLine($"Bot name: {authenticatedUser.Name}");
            botInfo.AppendLine($"Bot description: {authenticatedUser.Description}");
            Log.Debug(botInfo.ToString());

            var time = new TimeSpan(tweetTime.hour, tweetTime.minute, 0);
            Log.Information($"This bot is tweet image always {time}");

            return new StartupResult(client);
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
                var now = TimeUtils.Time.KoreaNowTime;
                if (now.Hour == tweetTime.hour && now.Minute == tweetTime.minute)
                {
                    if (alreadyTweeted)
                    {
                        continue;
                    }
                    //tweetTime.minute += 2;
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
        public TwitterClient Client { get; }

        public StartupResult(TwitterClient client)
        {
            Client = client;
        }
    }
}