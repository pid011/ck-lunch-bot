using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CKLunchBot.Core.Tests;

[TestClass]
public class MenuTest
{
    //[TestMethod]
    //public async Task MenuLoadTest()
    //{
    //    var menu = await WeekMenu.LoadAsync();
    //    Assert.IsNotNull(menu);
    //}

    //[TestMethod]
    //public void MenuInstanceTest()
    //{
    //    var now = DateTime.Now;
    //    var date = now.ToDateOnly();
    //    var weekMenu = CreateTestWeekMenu(date);

    //    Assert.IsNotNull(weekMenu.Find(date.AddDays(1)));
    //}

    //private static WeekMenu CreateTestWeekMenu(DateOnly date)
    //{
    //    var menu1 = new TodayMenu(date)
    //    {
    //        Breakfast = new Menu(MenuType.Breakfast)
    //        {
    //            Menus = new[] { "aaa", "bbb", "ccc", "dddd" },
    //            SpecialMenus = new[] { "aaa", "bbb", "ccc", "dddd" }
    //        },
    //        Lunch = new Menu(MenuType.Breakfast)
    //        {
    //            Menus = new[] { "aaa", "bbb", "ccc", "dddd" },
    //            SpecialMenus = new[] { "aaa", "bbb", "ccc", "dddd" }
    //        },
    //        Dinner = new Menu(MenuType.Breakfast)
    //        {
    //            Menus = new[] { "aaa", "bbb", "ccc", "dddd" },
    //            SpecialMenus = new[] { "aaa", "bbb", "ccc", "dddd" }
    //        }
    //    };

    //    var menu2 = menu1 with
    //    {
    //        Date = date.AddDays(1),
    //        Lunch = new Menu(MenuType.Breakfast)
    //        {
    //            Menus = new[] { "aaa", "bbb" }
    //        }
    //    };

    //    return new WeekMenu(new[] { menu1, menu2 });
    //}
}
