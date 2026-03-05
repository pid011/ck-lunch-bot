using Microsoft.Extensions.Options;

namespace CKLunchBot.Functions.Tests;

[TestClass]
public class XPostServiceTest
{
    [TestMethod]
    public void Constructor_ShouldThrowOnInvalidCredentials()
    {
        var credentials = new XCredentials
        {
            ConsumerApiKey = "",
            ConsumerSecretKey = "",
            AccessToken = "",
            AccessTokenSecret = "",
        };

        Assert.ThrowsExactly<ArgumentException>(
            () => new XPostService(
                Microsoft.Extensions.Logging.Abstractions.NullLogger<XPostService>.Instance,
                Options.Create(credentials)));
    }

    [TestMethod]
    public void Constructor_ShouldSucceedWithValidCredentials()
    {
        var credentials = new XCredentials
        {
            ConsumerApiKey = "test-key",
            ConsumerSecretKey = "test-secret",
            AccessToken = "test-token",
            AccessTokenSecret = "test-token-secret",
        };

        var service = new XPostService(
            Microsoft.Extensions.Logging.Abstractions.NullLogger<XPostService>.Instance,
            Options.Create(credentials));

        Assert.IsNotNull(service);
    }
}
