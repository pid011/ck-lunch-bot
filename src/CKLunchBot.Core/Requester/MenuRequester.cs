using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace CKLunchBot.Core.Requester;

internal class MenuRequester
{
    private const string MenuUrl = @"https://www.ck.ac.kr/univ-life/menu";

    private static readonly HttpClient s_client = new();

    public static async Task<HtmlDocument> RequestMenuHtmlAsync(CancellationToken cancelToken = default)
    {
        using var stream = await s_client.GetStreamAsync(MenuUrl, cancelToken);

        var html = new HtmlDocument();
        html.Load(stream);

        return html;
    }
}
