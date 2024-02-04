
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using CKLunchBot.Core;
using Microsoft.Extensions.Options;

namespace CKLunchBot;

public sealed class BotService(
    IHostApplicationLifetime lifetime,
    IServiceProvider serviceProvider,
    IHostEnvironment environment,
    ILogger<BotService> logger,
    IOptions<BotConfig> config) : BackgroundService, IAsyncDisposable
{
    private enum PostType
    {
        Briefing,
        Breakfast,
        Lunch,
        Dinner
    }

    private readonly IHostApplicationLifetime _lifetime = lifetime;
    private readonly IHostEnvironment _environment = environment;
    private readonly ILogger<BotService> _logger = logger;
    private readonly IBotConfig _config = config.Value;

    private readonly IMenuService _menuService = serviceProvider.GetRequiredService<IMenuService>();
    private readonly IPostService _postService = serviceProvider.GetRequiredService<IPostService>();
    private readonly IMessageFormatter _messageFormatter = serviceProvider.GetRequiredService<IMessageFormatter>();

    private readonly PostTimeComparer _postTimeComparer = new();

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            await InitAsync(cancellationToken);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Initialize failed.");
            _lifetime.StopApplication();
            return;
        }

        await base.StartAsync(cancellationToken);
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping BotService...");
        return base.StopAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield();

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var (nextPostType, nextPostTime) = GetNextPostTime();
                _logger.LogInformation("Next post: {nextPostType}", nextPostType);
                _logger.LogInformation("Bot will sleep until {nextPostTime}", nextPostTime);

                var now = KST.Now;
                var delay = nextPostTime - now;
                if (delay < TimeSpan.Zero)
                {
                    _logger.LogWarning("Next post time is already passed. Skip posting.");
                    continue;
                }

                _logger.LogDebug("Delay: {delay}", delay);
                await Task.Delay(delay, stoppingToken);

                _logger.LogInformation("Start posting.");
                await ProcessAsync(nextPostType, stoppingToken);
            }
        }
        catch (TaskCanceledException)
        {
            _logger.LogDebug("BotService was canceled.");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Unhandled exception.");
        }
        finally
        {
            _lifetime.StopApplication();
        }
    }

    private async ValueTask ProcessAsync(PostType postType, CancellationToken cancellationToken)
    {
        IReadOnlyCollection<MenuTable> weekMenu;

        try
        {
            weekMenu = await _menuService.GetWeekMenuAsync(cancellationToken);
        }
        catch (HtmlLoadException e)
        {
            _logger.LogError(e, "Failed to get html.");
            return;
        }
        catch (MenuParseException e)
        {
            _logger.LogError(e, "Failed to parse menu.");
            return;
        }

        if (!TryFindTodayMenu(weekMenu, out var todayMenu))
        {
            _logger.LogWarning("Cannot found today's menu.");
            return;
        }

        Debug.Assert(todayMenu is not null);
        var postContents = postType switch
        {
            PostType.Briefing => CreateBriefingContents(_config.BriefingMessage, todayMenu),
            PostType.Breakfast => CreatePostContentsFromTemplate(_config.BreakfastMessage, todayMenu.Date, todayMenu.Breakfast),
            PostType.Lunch => CreatePostContentsFromTemplate(_config.LunchMessage, todayMenu.Date, todayMenu.Lunch),
            PostType.Dinner => CreatePostContentsFromTemplate(_config.DinnerMessage, todayMenu.Date, todayMenu.Dinner),
            _ => throw new NotImplementedException()
        };
        _logger.LogDebug("Post contents{newline}{contents}", Environment.NewLine, postContents);

        if (_environment.IsProduction())
        {
            try
            {
                var post = await _postService.PostMessageAsync(postContents, cancellationToken);
                _logger.LogInformation("Bot successfully posted: {post}", post.Id);
            }
            catch (ApiException e)
            {
                _logger.LogError(e, "Failed to post message.");
            }
            catch (JsonException e)
            {
                _logger.LogError(e, "Failed to parse poting response.");
            }
        }
        else
        {
            _logger.LogInformation("Current enviroment is not production. Skip posting.");
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

    private async ValueTask InitAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Initializing BotService...");

        ConfigurationValidationCheck();

        if (_environment.IsProduction()) // API 과도 호출 상황을 방지하기 위해 Production 환경에서만 실행
        {
            try
            {
                if (!await _postService.IsValidAsync(cancellationToken))
                {
                    throw new InvalidOperationException("PostService is not valid.");
                }
            }
            catch (ApiException e) when (e.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            {
                // API가 한도에 도달했더라도 일단 실행 중은 유지하기
                // X API는 24시간 단위로 체크함
                _logger.LogWarning("X API rate limit exceeded. But BotService will running.");
            }
            catch (ApiException e)
            {
                // 이외의 경우라면 예외를 던지고 종료
                _logger.LogError("Post API is not valid. ({StatusCode})", e.StatusCode);
                throw;
            }
        }

        _logger.LogInformation("Briefing time: {BriefingTime}", _config.BriefingTime.ToString("c"));
        _logger.LogInformation("Breakfast time: {BreakfastTime}", _config.BreakfastTime.ToString("c"));
        _logger.LogInformation("Lunch time: {LunchTime}", _config.LunchTime.ToString("c"));
        _logger.LogInformation("Dinner time: {DinnerTime}", _config.DinnerTime.ToString("c"));
    }

    private void ConfigurationValidationCheck()
    {
        if (_config.BriefingTime.Ticks <= 0)
        {
            throw new InvalidOperationException("BriefingTime must be greater than 0.");
        }
        if (_config.BreakfastTime.Ticks <= 0)
        {
            throw new InvalidOperationException("BreakfastTime must be greater than 0.");
        }
        if (_config.LunchTime.Ticks <= 0)
        {
            throw new InvalidOperationException("LunchTime must be greater than 0.");
        }
        if (_config.DinnerTime.Ticks <= 0)
        {
            throw new InvalidOperationException("DinnerTime must be greater than 0.");
        }
        if (_config.BriefingMessage.Count is 0)
        {
            throw new InvalidOperationException("BriefingMessage must not be empty.");
        }
        if (_config.BreakfastMessage.Count is 0)
        {
            throw new InvalidOperationException("BreakfastMessage must not be empty.");
        }
        if (_config.LunchMessage.Count is 0)
        {
            throw new InvalidOperationException("LunchMessage must not be empty.");
        }
        if (_config.DinnerMessage.Count is 0)
        {
            throw new InvalidOperationException("DinnerMessage must not be empty.");
        }
        if (_config.Emoji.Count is 0)
        {
            throw new InvalidOperationException("Emoji must not be empty.");
        }

        try
        {
            var now = KST.Now.ToDateOnly();
            var emptyMenuTable = new MenuTable(now)
            {
                Breakfast = Menu.Empty,
                Lunch = Menu.Empty,
                Dinner = Menu.Empty
            };
            _ = CreateBriefingContents(_config.BriefingMessage, emptyMenuTable);
            _ = CreatePostContentsFromTemplate(_config.BreakfastMessage, now, emptyMenuTable.Breakfast);
            _ = CreatePostContentsFromTemplate(_config.LunchMessage, now, emptyMenuTable.Lunch);
            _ = CreatePostContentsFromTemplate(_config.DinnerMessage, now, emptyMenuTable.Dinner);
        }
        catch
        {
            _logger.LogError("Message template check failed.");
            throw;
        }
    }

    private string CreateBriefingContents(IReadOnlyCollection<string> template, MenuTable menuTable)
    {
        var koreanDateString = menuTable.Date.GetFormattedKoreanString();

        var builder = new StringBuilder();
        foreach (var text in template)
        {
            var replaced = _messageFormatter.Format(text, source => source switch
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
            var replaced = _messageFormatter.Format(text, source => source switch
            {
                "date" => koreanDateString,
                "emoji" => emoji,
                "menu" => menu.IsEmpty() ? "-" : string.Join(", ", menu.Menus),
                _ => throw new KeyNotFoundException($"Key '{source}' not found in template.")
            }); ;
            builder.AppendLine(replaced);
        }

        return builder.ToString();
    }

    private (PostType nextPostType, DateTime nextPostTime) GetNextPostTime()
    {
        // TODO: Need refactoring
        var now = KST.Now;
        var nowDiff = now - new DateTime(now.Year, now.Month, now.Day, 0, 0, 0);

        Span<(PostType? PostType, long Ticks)> times =
        [
            (PostType.Briefing, _config.BriefingTime.Ticks),
            (PostType.Breakfast, _config.BreakfastTime.Ticks),
            (PostType.Lunch, _config.LunchTime.Ticks),
            (PostType.Dinner, _config.DinnerTime.Ticks),
            (null, nowDiff.Ticks),
        ];
        times.Sort(_postTimeComparer);

        var nowIndex = times.IndexOf((null, nowDiff.Ticks));
        var nextIndex = nowIndex + 1;

        var nextTime = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0);
        if (nextIndex > times.Length - 1)
        {
            // 현재 시간이 설정된 시간들보다 늦은 시간일 때 다음 날의 설정된 시간 중 가장 빠른 시간으로 설정
            nextTime = nextTime.AddDays(1).AddTicks(times[0].Ticks);
            var nextPostType = times[0].PostType;
            Debug.Assert(nextPostType is not null);
            return (nextPostType.Value, nextTime);
        }
        else
        {
            // 현재 시간 다음의 시간으로 설정
            nextTime = nextTime.AddTicks(times[nextIndex].Ticks);
            var nextPostType = times[nextIndex].PostType;
            Debug.Assert(nextPostType is not null);
            return (nextPostType.Value, nextTime);
        }
    }

    public ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        return ValueTask.CompletedTask;
    }

    private class PostTimeComparer : IComparer<(PostType?, long)>
    {
        public int Compare((PostType?, long) x, (PostType?, long) y)
        {
            return x.Item2.CompareTo(y.Item2);
        }
    }
}
