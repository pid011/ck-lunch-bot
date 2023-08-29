using System;
using System.Threading.Tasks;

using CKLunchBot.Core.Drawing;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CKLunchBot.Core.Tests;

[TestClass]
public class MenuTest
{
    [TestMethod]
    public async Task MenuLoadTest()
    {
        var menu = await WeekMenu.LoadAsync();
        Assert.IsNotNull(menu);

        if (menu.Count > 0)
        {
            foreach (var todayMenu in menu)
            {
                if (!todayMenu.Lunch.IsEmpty())
                {
                    var imageBytes = await MenuImageGenerator.GenerateAsync(todayMenu.Date, MenuType.Lunch, todayMenu.Lunch);
                    Assert.IsNotNull(imageBytes);
                }
            }
        }
    }

    [TestMethod]
    public void MenuInstanceTest()
    {
        var now = DateTime.Now;
        var date = now.ToDateOnly();
        var weekMenu = CreateTestWeekMenu(date);

        Assert.IsNotNull(weekMenu.Find(date.AddDays(1)));
    }

    [TestMethod]
    public async Task MenuImageGenTest()
    {
        var now = DateTime.Now;
        var date = now.ToDateOnly();
        var weekMenu = CreateTestWeekMenu(date);

        foreach (var menu in weekMenu)
        {
            var imageBytes = await MenuImageGenerator.GenerateAsync(menu.Date, MenuType.Lunch, menu.Lunch);
            Assert.IsNotNull(imageBytes);
        }
    }

    private static WeekMenu CreateTestWeekMenu(DateOnly date)
    {
        var menu1 = new TodayMenu(date)
        {
            Breakfast = new Menu(MenuType.Breakfast)
            {
                Menus = new[] { "aaa", "bbb", "ccc", "dddd" },
                SpecialMenus = new[] { "aaa", "bbb", "ccc", "dddd" }
            },
            Lunch = new Menu(MenuType.Breakfast)
            {
                Menus = new[] { "aaa", "bbb", "ccc", "dddd" },
                SpecialMenus = new[] { "aaa", "bbb", "ccc", "dddd" }
            },
            Dinner = new Menu(MenuType.Breakfast)
            {
                Menus = new[] { "aaa", "bbb", "ccc", "dddd" },
                SpecialMenus = new[] { "aaa", "bbb", "ccc", "dddd" }
            }
        };

        var menu2 = menu1 with
        {
            Date = date.AddDays(1),
            Lunch = new Menu(MenuType.Breakfast)
            {
                Menus = new[] { "aaa", "bbb" }
            }
        };

        return new WeekMenu(new[] { menu1, menu2 });
    }
}
