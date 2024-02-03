using System.Diagnostics;
using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using RestSharp;
using RestSharp.Authenticators;

namespace CKLunchBot;

public sealed partial class X(X.Credentials credentials)
{
    private readonly IAuthenticator _authenticator = GetAuthenticatorFromCredentials(credentials);

    public async Task<Account> GetUserInformationAsync(CancellationToken cancellationToken = default)
    {
        using var client = new RestClient("https://api.twitter.com/2/users/me");
        var request = new RestRequest
        {
            Authenticator = _authenticator
        };
        request.AddParameter("user.fields", "id,username,description");

        var response = await client.ExecuteAsync(request, cancellationToken);
        if (response.StatusCode != HttpStatusCode.OK)
        {
            throw new Exception(response.Content);
        }

        var user = JsonNode.Parse(response.Content!);
        return new Account(
            id: user?["data"]?["id"]?.GetValue<string>() ?? throw new JsonException("Faild to parse user response data!"),
            name: user?["data"]?["username"]?.GetValue<string>() ?? throw new JsonException("Faild to parse user response data!"),
            description: user?["data"]?["description"]?.GetValue<string>() ?? throw new JsonException("Faild to parse user response data!"));
    }

    public async Task<Post> PostAsync(string tweetText, CancellationToken cancellationToken = default)
    {
        using var client = new RestClient("https://api.twitter.com/2/tweets");
        var request = new RestRequest
        {
            Authenticator = _authenticator
        };
        var body = new PostBody(tweetText);
        request.AddBody(JsonSerializer.Serialize(body, SourceGenerationContext.Default.PostBody));
        Debug.WriteLine(request.Authenticator.ToString());

        var response = await client.ExecutePostAsync(request, cancellationToken: cancellationToken);
        if (response.StatusCode != HttpStatusCode.Created)
        {
            throw new Exception(response.Content);
        }

        var post = JsonNode.Parse(response.Content!);
        return new Post(
            id: post?["data"]?["id"]?.GetValue<string>() ?? throw new JsonException("Faild to parse tweet response data!"),
            text: post?["data"]?["text"]?.GetValue<string>() ?? throw new JsonException("Faild to parse tweet response data!"));
    }

    private static OAuth1Authenticator GetAuthenticatorFromCredentials(Credentials credentials)
    {
        return OAuth1Authenticator.ForAccessToken(
            credentials.ConsumerApiKey, credentials.ConsumerSecretKey, credentials.AccessToken, credentials.AccessTokenSecret);
    }
}

public readonly struct PostBody(string text)
{
    public string Text { get; } = text;
}

[JsonSourceGenerationOptions(DictionaryKeyPolicy = JsonKnownNamingPolicy.SnakeCaseLower)]
[JsonSerializable(typeof(PostBody))]
public partial class SourceGenerationContext : JsonSerializerContext
{
}
