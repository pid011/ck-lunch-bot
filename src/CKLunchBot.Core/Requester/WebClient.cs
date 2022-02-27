using System.Net.Http;

namespace CKLunchBot.Core.Requester;

internal static class WebClient
{
    // https://docs.microsoft.com/en-us/dotnet/api/system.net.http.httpclient#remarks
    public static readonly HttpClient Client = new();
}
