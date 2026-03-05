using System;
using System.Linq;
using System.Reflection;
using CKLunchBot.Core;
using HtmlAgilityPack;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CKLunchBot.Core.Tests;

[TestClass]
public class MenuParserTest
{
    private readonly MenuParser _parser = new();

    [TestMethod]
    public void Parse_ShouldThrowMenuParseExceptionForInvalidHtml()
    {
        var html = new HtmlDocument();
        html.LoadHtml("<html><body><p>no table here</p></body></html>");

        Assert.ThrowsExactly<MenuParseException>(() => _parser.Parse(html));
    }

    [TestMethod]
    public void ParseDateText_ShouldParseSameMonth()
    {
        var now = KST.Now;
        var result = MenuParser.ParseDateText($"{now.Month:D2}/15 (수)");
        Assert.AreEqual(new DateOnly(now.Year, now.Month, 15), result);
    }

    [TestMethod]
    public void ParseDateText_ShouldThrowForInvalidFormat()
    {
        Assert.ThrowsExactly<MenuParseException>(() => MenuParser.ParseDateText("invalid"));
    }

    [TestMethod]
    public void Parse_ShouldParseRealSampleHtml()
    {
        var html = LoadSampleHtml();
        var result = _parser.Parse(html);

        // 샘플 HTML에는 03/02 ~ 03/06 (월~금) 5일치 메뉴가 있음
        Assert.HasCount(5, result);

        var days = result.ToList();

        // 03/02(월) - 대체휴무: 조식/석식 비어있음, 중식은 "대체휴무" 텍스트
        var monday = days[0];
        Assert.AreEqual(2, monday.Date.Day);
        Assert.IsTrue(monday.Breakfast.IsEmpty());
        Assert.IsFalse(monday.Lunch.IsEmpty()); // "대체휴무" 텍스트가 있음
        Assert.IsTrue(monday.Dinner.IsEmpty());

        // 03/03(화) - 모든 식사 존재
        var tuesday = days[1];
        Assert.AreEqual(3, tuesday.Date.Day);
        Assert.IsFalse(tuesday.Breakfast.IsEmpty());
        Assert.IsFalse(tuesday.Lunch.IsEmpty());
        Assert.IsFalse(tuesday.Dinner.IsEmpty());

        // 03/05(목) - 조식에 "토스트와 수제 딸기잼" 포함 확인
        var thursday = days[3];
        Assert.AreEqual(5, thursday.Date.Day);
        Assert.Contains(m => m.Contains("토스트와 수제 딸기잼"), thursday.Breakfast.Menus);

        // 03/05(목) - 중식에 "돼지고기 수육" 포함 확인
        Assert.Contains(m => m.Contains("돼지고기 수육"), thursday.Lunch.Menus);

        // 03/05(목) - 석식에 "오징어볶음" 포함 확인
        Assert.Contains(m => m.Contains("오징어볶음"), thursday.Dinner.Menus);
    }

    [TestMethod]
    public void Parse_SampleHtml_MenuItemsShouldBeTrimmed()
    {
        var html = LoadSampleHtml();
        var result = _parser.Parse(html);

        foreach (var menuTable in result)
        {
            foreach (var menu in new[] { menuTable.Breakfast, menuTable.Lunch, menuTable.Dinner })
            {
                foreach (var item in menu.Menus)
                {
                    Assert.AreEqual(item.Trim(), item, $"Menu item '{item}' on {menuTable.Date} should be trimmed");
                    Assert.AreNotEqual(0, item.Length, $"Empty menu item found on {menuTable.Date}");
                }
            }
        }
    }

    private static HtmlDocument LoadSampleHtml()
    {
        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream("CKLunchBot.Core.Tests.TestData.menu.html")
            ?? throw new InvalidOperationException("Sample HTML resource not found");

        var html = new HtmlDocument();
        html.Load(stream);
        return html;
    }
}
