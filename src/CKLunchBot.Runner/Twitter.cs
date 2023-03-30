using Tweetinvi;
using Tweetinvi.Models;
using Tweetinvi.Parameters;

namespace CKLunchBot.Runner;

public class Twitter
{
    private const string ConsumerApiKeyName = "TWITTER_CONSUMER_API_KEY";
    private const string ConsumerSecretKeyName = "TWITTER_CONSUMER_SECRET_KEY";
    private const string AccessTokenName = "TWITTER_ACCESS_TOKEN";
    private const string AccessTokenSecretName = "TWITTER_ACCESS_TOKEN_SECRET";

    /// <summary>
    /// Get <see cref="TwitterClient"/> instance using API keys from enviroment values
    /// </summary>
    /// <returns></returns>
    /// <exception cref="EnvNotFoundException"></exception>
    public static TwitterClient GetClientUsingKeys()
    {
        var consumerApiKey = GetEnv(ConsumerApiKeyName) ?? throw new EnvNotFoundException(ConsumerApiKeyName);
        var consumerSecretKey = GetEnv(ConsumerSecretKeyName) ?? throw new EnvNotFoundException(ConsumerSecretKeyName);
        var accessToken = GetEnv(AccessTokenName) ?? throw new EnvNotFoundException(AccessTokenName);
        var accessTokenSecret = GetEnv(AccessTokenSecretName) ?? throw new EnvNotFoundException(AccessTokenSecretName);

        var credentials = new TwitterCredentials
        {
            ConsumerKey = consumerApiKey,
            ConsumerSecret = consumerSecretKey,
            AccessToken = accessToken,
            AccessTokenSecret = accessTokenSecret
        };
        return new TwitterClient(credentials);
    }

    public static async Task<ITweet> PublishTweetAsync(TwitterClient twitter, string tweetText, byte[]? image = null)
    {
        if (image is null)
        {
            return await twitter.Tweets.PublishTweetAsync(new PublishTweetParameters(tweetText.ToString()));
        }

        IMedia uploadedImage = await twitter.Upload.UploadTweetImageAsync(image);
        return await twitter.Tweets.PublishTweetAsync(new PublishTweetParameters(tweetText.ToString())
        {
            Medias = { uploadedImage }
        });
    }

    private static string? GetEnv(string key)
    {
        return Environment.GetEnvironmentVariable(key, EnvironmentVariableTarget.Process);
    }
}
