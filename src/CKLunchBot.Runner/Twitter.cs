using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;

using RestSharp;
using RestSharp.Authenticators;

namespace CKLunchBot.Runner;

public class Twitter
{
    private const string ConsumerApiKeyName = "TWITTER_CONSUMER_API_KEY";
    private const string ConsumerSecretKeyName = "TWITTER_CONSUMER_SECRET_KEY";
    private const string AccessTokenName = "TWITTER_ACCESS_TOKEN";
    private const string AccessTokenSecretName = "TWITTER_ACCESS_TOKEN_SECRET";

    public record Credentials(string ConsumerApiKey, string ConsumerSecretKey, string AccessToken, string AccessTokenSecret);

    public readonly struct Tweet
    {
        public string Id { get; }
        public string Text { get; }

        public Tweet(string id, string text)
        {
            Id = id;
            Text = text;
        }
    }

    public readonly struct User
    {
        public string Id { get; }
        public string Name { get; }
        public string Description { get; }

        public User(string id, string username, string description)
        {
            Id = id;
            Name = username;
            Description = description;
        }
    }

    /// <summary>
    /// Get <see cref="TwitterClient"/> instance using API keys from enviroment values
    /// </summary>
    /// <returns></returns>
    /// <exception cref="EnvNotFoundException"></exception>
    public static Credentials GetCredentialsFromEnv()
    {
        var consumerApiKey = GetEnv(ConsumerApiKeyName) ?? throw new EnvNotFoundException(ConsumerApiKeyName);
        var consumerSecretKey = GetEnv(ConsumerSecretKeyName) ?? throw new EnvNotFoundException(ConsumerSecretKeyName);
        var accessToken = GetEnv(AccessTokenName) ?? throw new EnvNotFoundException(AccessTokenName);
        var accessTokenSecret = GetEnv(AccessTokenSecretName) ?? throw new EnvNotFoundException(AccessTokenSecretName);
        return new Credentials(consumerApiKey, consumerSecretKey, accessToken, accessTokenSecret);
    }

    public static async Task<User> GetUserInformationAsync(Credentials credentials)
    {
        var oauth = OAuth1Authenticator.ForAccessToken(
            credentials.ConsumerApiKey, credentials.ConsumerSecretKey, credentials.AccessToken, credentials.AccessTokenSecret);

        var client = new RestClient("https://api.twitter.com/2/users/me");
        var request = new RestRequest
        {
            Authenticator = oauth,
        };
        request.AddParameter("user.fields", "id,username,description");

        var response = await client.ExecuteAsync(request);
        if (response.StatusCode != HttpStatusCode.OK)
        {
            throw new Exception(response.Content);
        }

        var user = JsonNode.Parse(response.Content!);
        var id = user?["data"]?["id"]?.GetValue<string>() ?? throw new Exception("Faild to parse user response data!");
        var username = user?["data"]?["username"]?.GetValue<string>() ?? throw new Exception("Faild to parse user response data!");
        var description = user?["data"]?["description"]?.GetValue<string>() ?? throw new Exception("Faild to parse user response data!");

        return new User(id, username, description);
    }

    public static async Task<Tweet> PublishTweetAsync(Credentials credentials, string tweetText)
    {
        var payload = JsonSerializer.Serialize(new { text = tweetText });

        var oauth = OAuth1Authenticator.ForAccessToken(
            credentials.ConsumerApiKey, credentials.ConsumerSecretKey, credentials.AccessToken, credentials.AccessTokenSecret);

        var client = new RestClient("https://api.twitter.com/2/tweets");
        var request = new RestRequest
        {
            Authenticator = oauth,
        };
        request.AddJsonBody(payload, false);

        var response = await client.ExecuteAsync(request);
        if (response.StatusCode != HttpStatusCode.Created)
        {
            throw new Exception(response.Content);
        }

        var tweet = JsonNode.Parse(response.Content!);
        var id = tweet?["data"]?["id"]?.GetValue<string>() ?? throw new Exception("Faild to parse tweet response data!");
        var text = tweet?["data"]?["text"]?.GetValue<string>() ?? throw new Exception("Faild to parse tweet response data!");

        return new Tweet(id, text);
    }

    private static string? GetEnv(string key)
    {
        return Environment.GetEnvironmentVariable(key, EnvironmentVariableTarget.Process);
    }
}
