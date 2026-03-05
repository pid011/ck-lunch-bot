using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using CKLunchBot.Core;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CKLunchBot;

public sealed class PostingHelper(
    IMenuService menuService,
    IPostService postService,
    IMessageFormatter messageFormatter,
    IHostEnvironment environment,
    ILogger<PostingHelper> logger,
    IOptions<BotConfig> config)
{
    private readonly IBotConfig _config = config.Value;

    public async Task ProcessBriefingAsync(CancellationToken cancellationToken)
    {
        var weekMenu = await GetWeekMenuAsync(cancellationToken);
        if (weekMenu is null) return;

        if (!TryFindTodayMenu(weekMenu, out var todayMenu))
        {
            logger.LogWarning("Cannot found today's menu.");
            return;
        }

        if (todayMenu!.Breakfast.IsEmpty() && todayMenu.Lunch.IsEmpty() && todayMenu.Dinner.IsEmpty())
        {
            logger.LogWarning("Today's menu is empty. Skip posting.");
            return;
        }

        var postContents = CreateBriefingContents(_config.BriefingMessage, todayMenu);
        await PostAsync(postContents, cancellationToken);
    }

    public async Task ProcessMealAsync(MenuType mealType, CancellationToken cancellationToken)
    {
        var weekMenu = await GetWeekMenuAsync(cancellationToken);
        if (weekMenu is null) return;

        if (!TryFindTodayMenu(weekMenu, out var todayMenu))
        {
            logger.LogWarning("Cannot found today's menu.");
            return;
        }

        var menu = todayMenu![mealType];
        if (menu.IsEmpty())
        {
            logger.LogWarning("Today's {mealType} menu is empty. Skip posting.", mealType);
            return;
        }

        var template = mealType switch
        {
            MenuType.Breakfast => _config.BreakfastMessage,
            MenuType.Lunch => _config.LunchMessage,
            MenuType.Dinner => _config.DinnerMessage,
            _ => throw new NotImplementedException()
        };

        var postContents = CreatePostContentsFromTemplate(template, todayMenu.Date, menu);
        await PostAsync(postContents, cancellationToken);
    }

    private async Task<IReadOnlyCollection<MenuTable>?> GetWeekMenuAsync(CancellationToken cancellationToken)
    {
        try
        {
            return await menuService.GetWeekMenuAsync(cancellationToken);
        }
        catch (HtmlLoadException e)
        {
            logger.LogError(e, "Failed to get html.");
            return null;
        }
        catch (MenuParseException e)
        {
            logger.LogError(e, "Failed to parse menu.");
            return null;
        }
    }

    private async Task PostAsync(string postContents, CancellationToken cancellationToken)
    {
        logger.LogDebug("Post contents{newline}{contents}", Environment.NewLine, postContents);

        if (environment.IsProduction())
        {
            try
            {
                var post = await postService.PostMessageAsync(postContents, cancellationToken);
                logger.LogInformation("Bot successfully posted: {post}", post.Id);
            }
            catch (ApiException e)
            {
                logger.LogError(e, "Failed to post message.");
            }
            catch (JsonException e)
            {
                logger.LogError(e, "Failed to parse posting response.");
            }
        }
        else
        {
            logger.LogInformation("Current environment is not production. Skip posting.");
        }
    }

    private static bool TryFindTodayMenu(IReadOnlyCollection<MenuTable> menuTables, out MenuTable? menuTable)
    {
        var now = KST.Now.ToDateOnly();
        var todayMenu = menuTables.FirstOrDefault(x => x.Date == now);
        if (todayMenu is null)
        {
            menuTable = null;
            return false;
        }
        menuTable = todayMenu;
        return true;
    }

    private string CreateBriefingContents(IReadOnlyCollection<string> template, MenuTable menuTable)
    {
        var koreanDateString = menuTable.Date.GetFormattedKoreanString();

        var builder = new StringBuilder();
        foreach (var text in template)
        {
            var replaced = messageFormatter.Format(text, source => source switch
            {
                "date" => koreanDateString,
                "breakfast" => menuTable.Breakfast.IsEmpty() ? "-" : string.Join(", ", menuTable.Breakfast.Menus),
                "lunch" => menuTable.Lunch.IsEmpty() ? "-" : string.Join(", ", menuTable.Lunch.Menus),
                "dinner" => menuTable.Dinner.IsEmpty() ? "-" : string.Join(", ", menuTable.Dinner.Menus),
                _ => throw new KeyNotFoundException($"Key '{source}' not found in template.")
            });
            builder.AppendLine(replaced);
        }

        return builder.ToString();
    }

    private string CreatePostContentsFromTemplate(IReadOnlyCollection<string> template, DateOnly date, Menu menu)
    {
        var koreanDateString = date.GetFormattedKoreanString();
        var emoji = _config.Emoji[RandomNumberGenerator.GetInt32(0, _config.Emoji.Count)];

        var builder = new StringBuilder();
        foreach (var text in template)
        {
            var replaced = messageFormatter.Format(text, source => source switch
            {
                "date" => koreanDateString,
                "emoji" => emoji,
                "menu" => menu.IsEmpty() ? "-" : string.Join(", ", menu.Menus),
                _ => throw new KeyNotFoundException($"Key '{source}' not found in template.")
            });
            builder.AppendLine(replaced);
        }

        return builder.ToString();
    }
}
