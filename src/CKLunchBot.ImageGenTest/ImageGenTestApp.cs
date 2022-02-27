using System;
using System.IO;
using System.Threading.Tasks;
using CKLunchBot.Core;
using CKLunchBot.Core.Drawing;
using SixLabors.ImageSharp;

var outputDir = $"Test_{DateTime.Now:yyyy-MM-dd-HH-mm-ss}";

var weekMenu = await WeekMenu.LoadAsync();
Console.WriteLine(weekMenu.ToString());

// var date = KST.Now.ToDateOnly();
var date = new DateOnly(2022, 3, 2);

var todayMenu = weekMenu.Find(date);

/*
var todayMenu = new TodayMenu(date)
{
    Dinner = new Menu(CKLunchBot.Core.MenuType.Dinner)
    {
        Menus = new[] { "테스트테스트테스트테스트", "테스트테스트테스트테스트", "테스트테스트테스트테스트", "테스트테스트테스트테스트", "테스트테스트테스트테스트", "테스트테스트테스트테스트", "테스트테스트테스트테스트", },
        SelfCorner = new[] { "테스트테스트테스트테스트", "테스트테스트테스트테스트", "테스트테스트테스트테스트", "테스트테스트테스트테스트", "테스트테스트테스트테스트", "테스트테스트테스트테스트", "테스트테스트테스트테스트", }
    }
};
*/

/*
var todayMenu = new TodayMenu(date)
{
    Dinner = new Menu(CKLunchBot.Core.MenuType.Dinner)
    {
    }
};
*/

await SaveMenuImageAsync(outputDir, todayMenu, MenuType.Breakfast);
await SaveMenuImageAsync(outputDir, todayMenu, MenuType.Lunch);
await SaveMenuImageAsync(outputDir, todayMenu, MenuType.Dinner);

static async Task SaveMenuImageAsync(string outputDir, TodayMenu menu, MenuType type)
{
    var targetMenu = menu[type];
    var imageBytes = await MenuImageGenerator.GenerateAsync(menu.Date, type, targetMenu);

    if (!Directory.Exists(outputDir))
    {
        Directory.CreateDirectory(outputDir);
    }

    using var memorystream = new MemoryStream(imageBytes);
    using var filestream = new FileStream(Path.Combine(outputDir, $"{menu.Date.ToShortDateString()}_{type}.png"), FileMode.Create);
    using var image = await Image.LoadAsync(memorystream);

    image.SaveAsPng(filestream);
}
