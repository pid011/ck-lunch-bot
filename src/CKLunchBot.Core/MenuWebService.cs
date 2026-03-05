using CKLunchBot.Core;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;

namespace CKLunchBot;

public interface IMenuService
{
    Task<IReadOnlyCollection<MenuTable>> GetWeekMenuAsync(CancellationToken cancellationToken);
}

public sealed partial class MenuWebService(
    IMenuParser menuParser,
    HttpClient httpClient,
    ILogger<MenuWebService> logger) : IMenuService
{
    public static readonly string RequestUri = @"https://www.ck.ac.kr/univ-life/menu";

    public async Task<IReadOnlyCollection<MenuTable>> GetWeekMenuAsync(CancellationToken cancellationToken)
    {
        HtmlDocument html;
        try
        {
            html = await GetHtmlAsync(cancellationToken);
        }
        catch (Exception e)
        {
            throw new HtmlLoadException($"Failed to get html from {RequestUri}.", e);
        }

        try
        {
            return menuParser.Parse(html);
        }
        catch (MenuParseException)
        {
            throw;
        }
        catch (Exception e)
        {
            LogFailedToParseHtml(e);
            throw;
        }
    }

    private async Task<HtmlDocument> GetHtmlAsync(CancellationToken cancelToken = default)
    {
        using var stream = await httpClient.GetStreamAsync(RequestUri, cancelToken);

        var html = new HtmlDocument();
        html.Load(stream);

        return html;
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to parse html.")]
    private partial void LogFailedToParseHtml(Exception ex);
}
