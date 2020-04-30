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
        // private (int hour, int minute) tweetTime = (11, 50);
        private (int hour, int minute) tweetTime = (TimeUtils.Time.KoreaNowTime.Hour, TimeUtils.Time.KoreaNowTime.Minute);

        private bool alreadyTweeted;

        public async Task Run(CancellationToken token)
        {
            try
            {
                Startup(out TwitterTokens twitterTokens);

                var client = new TwitterClient(twitterTokens.ConsumerApiKey,
                                               twitterTokens.ConsumerSecretKey,
                                               twitterTokens.AccessToken,
                                               twitterTokens.AccessTokenSecret);
                while (true)
                {
                    await WaitForTweetTime(token);

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
                }
            }
            catch (TaskCanceledException)
            {
                Log.Information("The bot has stopped.");
            }
            catch (Exception e)
            {
                Log.Error(e.ToString());
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
            Log.Information("Generate completed.");
            return image;
        }

        private void Startup(out TwitterTokens tokens)
        {
            const string tokensFileName = "ck_lunch_bot_twitter_tokens.json";

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
                    tweetTime.minute += 2;
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
}