namespace CKLunchBot.Core;

public interface IXCredentials
{
    string ConsumerApiKey { get; }
    string ConsumerSecretKey { get; }
    string AccessToken { get; }
    string AccessTokenSecret { get; }

    bool IsValid();
}

public sealed class XCredentials : IXCredentials
{
    public string ConsumerApiKey { get; set; } = string.Empty;
    public string ConsumerSecretKey { get; set; } = string.Empty;
    public string AccessToken { get; set; } = string.Empty;
    public string AccessTokenSecret { get; set; } = string.Empty;

    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(ConsumerApiKey) &&
               !string.IsNullOrWhiteSpace(ConsumerSecretKey) &&
               !string.IsNullOrWhiteSpace(AccessToken) &&
               !string.IsNullOrWhiteSpace(AccessTokenSecret);
    }
}
