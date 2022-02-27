using System.Net.Http;

namespace CKLunchBot.Core.Requester;

internal static class WebClient
{
    // https://docs.microsoft.com/en-us/dotnet/api/system.net.http.httpclient#remarks
    public static readonly HttpClient Client = new(new HttpClientHandler
    {
        // https://stackoverflow.com/questions/52939211/the-ssl-connection-could-not-be-established
        ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
    });
}
