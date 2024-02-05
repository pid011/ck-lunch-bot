using System.Text.Json;
using Microsoft.Extensions.Logging.Abstractions;

namespace CKLunchBot.Tests;

[TestClass]
public class BotTest
{
    [TestMethod]
    public void PostBodySerializeTest()
    {
        var postBody = new PostBody(text: "aaaaa");
        var json = JsonSerializer.Serialize(postBody, SourceGenerationContext.Default.PostBody);
        Assert.AreEqual(json, """{"text":"aaaaa"}""");
    }

    [TestMethod]
    public async Task MenuLoadTest()
    {
        var menuService = new MenuWebService(NullLogger<MenuWebService>.Instance);
        try
        {
            using var cancellation = new CancellationTokenSource(new TimeSpan(0, 0, 30));
            _ = await menuService.GetWeekMenuAsync(cancellation.Token);
        }
        catch (Exception e)
        {
            Assert.Fail(e.Message);
        }
    }
}
