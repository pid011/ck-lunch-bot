using CKLunchBot;
using CKLunchBot.Extensions;
using CKLunchBot.Menu;
using CKLunchBot.Post;
using Imposter.Abstractions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

[assembly: GenerateImposter(typeof(IMenuService))]
[assembly: GenerateImposter(typeof(IPostService))]
[assembly: GenerateImposter(typeof(IHostEnvironment))]

namespace CKLunchBot.Tests;

[TestClass]
public class PostingServiceTest
{
    private static readonly DateOnly s_today = KST.Now.ToDateOnly();

    private static readonly MenuTable s_todayMenuTable = new(s_today)
    {
        Breakfast = new Menu.Menu(["백미밥", "된장찌개"]),
        Lunch = new Menu.Menu(["볶음밥", "탕수육"]),
        Dinner = new Menu.Menu(["라면", "만두"]),
    };

    private static readonly MenuTable s_emptyMenuTable = new(s_today)
    {
        Breakfast = Menu.Menu.Empty,
        Lunch = Menu.Menu.Empty,
        Dinner = Menu.Menu.Empty,
    };

    private static readonly BotConfig s_testConfig = new()
    {
        BriefingMessage = ["{{date}} 조식: {{breakfast}}, 중식: {{lunch}}, 석식: {{dinner}}"],
        BreakfastMessage = ["{{emoji}} {{date}} 조식: {{menu}}"],
        LunchMessage = ["{{emoji}} {{date}} 중식: {{menu}}"],
        DinnerMessage = ["{{emoji}} {{date}} 석식: {{menu}}"],
        Emoji = ["🍚"],
    };

    private static BotService CreateService(
        IMenuServiceImposter menuService,
        IPostServiceImposter postService,
        IHostEnvironmentImposter environment)
    {
        return new BotService(
            menuService.Instance(),
            postService.Instance(),
            environment.Instance(),
            NullLogger<BotService>.Instance,
            Options.Create(s_testConfig));
    }

    private static IMenuServiceImposter CreateMenuServiceWithTodayMenu(MenuTable menuTable)
    {
        var imposter = new IMenuServiceImposter();
        imposter
            .GetWeekMenuAsync(Arg<CancellationToken>.Any())
            .ReturnsAsync((IReadOnlyCollection<MenuTable>)[menuTable]);
        return imposter;
    }

    private static IHostEnvironmentImposter CreateEnvironment(string environmentName)
    {
        var imposter = new IHostEnvironmentImposter();
        imposter.EnvironmentName.Getter().Returns(environmentName);
        return imposter;
    }

    [TestMethod]
    public async Task ProcessBriefingAsync_ShouldPostWhenMenuExists()
    {
        var menuService = CreateMenuServiceWithTodayMenu(s_todayMenuTable);
        var postService = new IPostServiceImposter();
        postService
            .PostMessageAsync(Arg<string>.Any(), Arg<CancellationToken>.Any())
            .ReturnsAsync(new CKLunchBot.Post.Post("1", "posted"));
        var environment = CreateEnvironment("Production");

        var service = CreateService(menuService, postService, environment);
        await service.ProcessBriefingAsync(CancellationToken.None);

        postService
            .PostMessageAsync(Arg<string>.Any(), Arg<CancellationToken>.Any())
            .Called(Count.Once());
    }

    [TestMethod]
    public async Task ProcessBriefingAsync_ShouldSkipWhenTodayMenuNotFound()
    {
        var yesterday = s_today.AddDays(-1);
        var menuTable = new MenuTable(yesterday)
        {
            Breakfast = new Menu.Menu(["밥"]),
            Lunch = new Menu.Menu(["밥"]),
            Dinner = new Menu.Menu(["밥"]),
        };

        var menuService = CreateMenuServiceWithTodayMenu(menuTable);
        var postService = new IPostServiceImposter();
        var environment = CreateEnvironment("Production");

        var service = CreateService(menuService, postService, environment);
        await service.ProcessBriefingAsync(CancellationToken.None);

        postService
            .PostMessageAsync(Arg<string>.Any(), Arg<CancellationToken>.Any())
            .Called(Count.Never());
    }

    [TestMethod]
    public async Task ProcessBriefingAsync_ShouldSkipWhenAllMenusEmpty()
    {
        var menuService = CreateMenuServiceWithTodayMenu(s_emptyMenuTable);
        var postService = new IPostServiceImposter();
        var environment = CreateEnvironment("Production");

        var service = CreateService(menuService, postService, environment);
        await service.ProcessBriefingAsync(CancellationToken.None);

        postService
            .PostMessageAsync(Arg<string>.Any(), Arg<CancellationToken>.Any())
            .Called(Count.Never());
    }

    [TestMethod]
    public async Task ProcessMealAsync_ShouldPostSpecificMealType()
    {
        var menuService = CreateMenuServiceWithTodayMenu(s_todayMenuTable);
        var postService = new IPostServiceImposter();
        postService
            .PostMessageAsync(Arg<string>.Any(), Arg<CancellationToken>.Any())
            .ReturnsAsync(new CKLunchBot.Post.Post("1", "posted"));
        var environment = CreateEnvironment("Production");

        var service = CreateService(menuService, postService, environment);
        await service.ProcessMealAsync(MenuType.Lunch, CancellationToken.None);

        postService
            .PostMessageAsync(Arg<string>.Any(), Arg<CancellationToken>.Any())
            .Called(Count.Once());
    }

    [TestMethod]
    public async Task ProcessMealAsync_ShouldSkipWhenMealIsEmpty()
    {
        var menuTable = new MenuTable(s_today)
        {
            Breakfast = Menu.Menu.Empty,
            Lunch = new Menu.Menu(["볶음밥"]),
            Dinner = Menu.Menu.Empty,
        };

        var menuService = CreateMenuServiceWithTodayMenu(menuTable);
        var postService = new IPostServiceImposter();
        var environment = CreateEnvironment("Production");

        var service = CreateService(menuService, postService, environment);
        await service.ProcessMealAsync(MenuType.Breakfast, CancellationToken.None);

        postService
            .PostMessageAsync(Arg<string>.Any(), Arg<CancellationToken>.Any())
            .Called(Count.Never());
    }

    [TestMethod]
    public async Task ProcessBriefingAsync_ShouldSkipPostingInNonProduction()
    {
        var menuService = CreateMenuServiceWithTodayMenu(s_todayMenuTable);
        var postService = new IPostServiceImposter();
        var environment = CreateEnvironment("Development");

        var service = CreateService(menuService, postService, environment);
        await service.ProcessBriefingAsync(CancellationToken.None);

        postService
            .PostMessageAsync(Arg<string>.Any(), Arg<CancellationToken>.Any())
            .Called(Count.Never());
    }

    [TestMethod]
    public async Task ProcessBriefingAsync_ShouldHandleHtmlLoadException()
    {
        var menuService = new IMenuServiceImposter();
        menuService
            .GetWeekMenuAsync(Arg<CancellationToken>.Any())
            .Returns((CancellationToken _) => throw new HtmlLoadException("test error"));
        var postService = new IPostServiceImposter();
        var environment = CreateEnvironment("Production");

        var service = CreateService(menuService, postService, environment);
        await service.ProcessBriefingAsync(CancellationToken.None);

        postService
            .PostMessageAsync(Arg<string>.Any(), Arg<CancellationToken>.Any())
            .Called(Count.Never());
    }

    [TestMethod]
    public async Task ProcessBriefingAsync_ShouldHandleMenuParseException()
    {
        var menuService = new IMenuServiceImposter();
        menuService
            .GetWeekMenuAsync(Arg<CancellationToken>.Any())
            .Returns((CancellationToken _) => throw new MenuParseException("parse error"));
        var postService = new IPostServiceImposter();
        var environment = CreateEnvironment("Production");

        var service = CreateService(menuService, postService, environment);
        await service.ProcessBriefingAsync(CancellationToken.None);

        postService
            .PostMessageAsync(Arg<string>.Any(), Arg<CancellationToken>.Any())
            .Called(Count.Never());
    }
}
