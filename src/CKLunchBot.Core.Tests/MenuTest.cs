using System;
using System.Threading.Tasks;
using CKLunchBot.Core.Drawing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CKLunchBot.Core.Tests
{
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
        public async Task MenuImageGenTest()
        {
            var now = DateTime.Now;

            var menu = new TodayMenu(now.ToDateOnly())
            {
                Breakfast = new Menu(MenuType.Breakfast)
                {
                    Menus = new[] { "aaa", "bbb", "ccc", "dddd" },
                    SelfCorner = new[] { "aaa", "bbb", "ccc", "dddd" }
                },
                Lunch = new Menu(MenuType.Breakfast)
                {
                    Menus = new[] { "aaa", "bbb", "ccc", "dddd" },
                    SelfCorner = new[] { "aaa", "bbb", "ccc", "dddd" }
                },
                Dinner = new Menu(MenuType.Breakfast)
                {
                    Menus = new[] { "aaa", "bbb", "ccc", "dddd" },
                    SelfCorner = new[] { "aaa", "bbb", "ccc", "dddd" }
                }
            };

            var imageBytes = await MenuImageGenerator.GenerateAsync(menu.Date, MenuType.Lunch, menu.Lunch);
            Assert.IsNotNull(imageBytes);
        }
    }
}
