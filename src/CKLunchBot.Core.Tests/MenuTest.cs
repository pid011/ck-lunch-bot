using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CKLunchBot.Core.Tests;

[TestClass]
public class MenuTest
{
    [TestMethod]
    public void DateFormatTextTest()
    {
        var date = new DateOnly(2022, 1, 1);
        var formatted = date.GetFormattedKoreanString();
        Assert.AreEqual(formatted, "2022.1.1 (토)");
    }
}
