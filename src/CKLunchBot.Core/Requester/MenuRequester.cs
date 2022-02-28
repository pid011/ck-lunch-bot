using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace CKLunchBot.Core.Requester;

internal class MenuRequester
{
    private const string MenuUrl = @"https://www.ck.ac.kr/univ-life/menu";

    private static readonly HttpClient s_client = new(new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
    });

    public async static Task<HtmlDocument> RequestMenuHtmlAsync(CancellationToken cancelToken = default)
    {
        var str = await s_client.GetStringAsync(MenuUrl, cancelToken);

        var html = new HtmlDocument();
        html.LoadHtml(str);

        return html;
    }
}
