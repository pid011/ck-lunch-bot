using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace CKLunchBot.Core.Requester;

internal class MenuRequester
{
    private const string MenuUrl = @"https://www.ck.ac.kr/univ-life/menu";

    private static readonly HtmlWeb s_web = new();

    public async static Task<HtmlDocument> RequestMenuHtmlAsync(CancellationToken cancelToken = default)
    {
        return await s_web.LoadFromWebAsync(MenuUrl, cancelToken);
    }
}
