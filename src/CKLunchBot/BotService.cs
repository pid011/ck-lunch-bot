using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using CKLunchBot.Extensions;
using CKLunchBot.Menu;
using CKLunchBot.Post;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CKLunchBot;

public sealed partial class BotService(
    IMenuService menuService,
    IPostService postService,
    IHostEnvironment environment,
    ILogger<BotService> logger,
    IOptions<BotConfig> botConfig)
{
    private BotConfig BotConfig => botConfig.Value;

    public async Task ProcessBriefingAsync(CancellationToken cancellationToken)
    {
        var todayMenu = await GetTodayMenuAsync(cancellationToken);
        if (todayMenu is null) return;

        if (todayMenu.Breakfast.IsEmpty() && todayMenu.Lunch.IsEmpty() && todayMenu.Dinner.IsEmpty())
        {
            LogTodayMenuEmpty();
            return;
        }

        var koreanDateString = todayMenu.Date.GetFormattedKoreanString();
        var postContents = FormatTemplate(BotConfig.BriefingMessage, source => source switch
        {
            "date" => koreanDateString,
            "breakfast" => FormatMenu(todayMenu.Breakfast),
            "lunch" => FormatMenu(todayMenu.Lunch),
            "dinner" => FormatMenu(todayMenu.Dinner),
            _ => throw new KeyNotFoundException($"Key '{source}' not found in template.")
        });

        await PostAsync(postContents, cancellationToken);
    }

    public async Task ProcessMealAsync(MenuType mealType, CancellationToken cancellationToken)
    {
        var todayMenu = await GetTodayMenuAsync(cancellationToken);
        if (todayMenu is null) return;

        var menu = todayMenu[mealType];
        if (menu.IsEmpty())
        {
            LogMealMenuEmpty(mealType);
            return;
        }

        var template = mealType switch
        {
            MenuType.Breakfast => BotConfig.BreakfastMessage,
            MenuType.Lunch => BotConfig.LunchMessage,
            MenuType.Dinner => BotConfig.DinnerMessage,
            _ => throw new NotImplementedException()
        };

        var koreanDateString = todayMenu.Date.GetFormattedKoreanString();
        var emoji = BotConfig.Emoji[RandomNumberGenerator.GetInt32(0, BotConfig.Emoji.Count)];
        var postContents = FormatTemplate(template, source => source switch
        {
            "date" => koreanDateString,
            "emoji" => emoji,
            "menu" => FormatMenu(menu),
            _ => throw new KeyNotFoundException($"Key '{source}' not found in template.")
        });

        await PostAsync(postContents, cancellationToken);
    }

    private async Task<MenuTable?> GetTodayMenuAsync(CancellationToken cancellationToken)
    {
        IReadOnlyCollection<MenuTable> weekMenu;
        try
        {
            weekMenu = await menuService.GetWeekMenuAsync(cancellationToken);
        }
        catch (HtmlLoadException e)
        {
            LogFailedToGetHtml(e);
            return null;
        }
        catch (MenuParseException e)
        {
            LogFailedToParseMenu(e);
            return null;
        }

        var now = KST.Now.ToDateOnly();
        var todayMenu = weekMenu.FirstOrDefault(x => x.Date == now);
        if (todayMenu is null)
        {
            LogTodayMenuNotFound();
            return null;
        }

        return todayMenu;
    }

    private async Task PostAsync(string postContents, CancellationToken cancellationToken)
    {
        LogPostContents(postContents);

        if (!environment.IsProduction())
        {
            LogSkipNonProduction();
            return;
        }

        try
        {
            var post = await postService.PostMessageAsync(postContents, cancellationToken);
            LogPostSuccess(post.Id);
        }
        catch (ApiException e)
        {
            LogFailedToPostMessage(e);
        }
        catch (JsonException e)
        {
            LogFailedToParsePostingResponse(e);
        }
    }

    private string FormatTemplate(IReadOnlyCollection<string> template, Func<string, string> replacer)
    {
        var builder = new StringBuilder();
        foreach (var text in template)
        {
            builder.AppendLine(RegexParser.ReplacementTextRegex().Replace(text, match => replacer(match.Groups[1].Value)));
        }
        return builder.ToString();
    }

    private static string FormatMenu(Menu.Menu menu)
    {
        return menu.IsEmpty() ? "-" : string.Join(", ", menu.Menus);
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Today's menu is empty. Skip posting.")]
    private partial void LogTodayMenuEmpty();

    [LoggerMessage(Level = LogLevel.Warning, Message = "Today's {mealType} menu is empty. Skip posting.")]
    private partial void LogMealMenuEmpty(MenuType mealType);

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to get html.")]
    private partial void LogFailedToGetHtml(Exception ex);

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to parse menu.")]
    private partial void LogFailedToParseMenu(Exception ex);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Cannot find today's menu.")]
    private partial void LogTodayMenuNotFound();

    [LoggerMessage(Level = LogLevel.Debug, Message = "Post contents: {contents}")]
    private partial void LogPostContents(string contents);

    [LoggerMessage(Level = LogLevel.Information, Message = "Current environment is not production. Skip posting.")]
    private partial void LogSkipNonProduction();

    [LoggerMessage(Level = LogLevel.Information, Message = "Bot successfully posted: {postId}")]
    private partial void LogPostSuccess(string postId);

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to post message.")]
    private partial void LogFailedToPostMessage(Exception ex);

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to parse posting response.")]
    private partial void LogFailedToParsePostingResponse(Exception ex);
}
