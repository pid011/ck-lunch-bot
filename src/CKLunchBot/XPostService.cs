using Microsoft.Extensions.Options;

namespace CKLunchBot;

public sealed class XPostService : IPostService
{
    private readonly ILogger<XPostService> _logger;
    private readonly X _x;

    public XPostService(ILogger<XPostService> logger, IOptions<X.Credentials> credentials)
    {
        _logger = logger;
        if (!credentials.Value.IsValid())
        {
            throw new ArgumentException("Invalid credentials!");
        }
        _x = new(credentials.Value);
    }

    public async ValueTask<Account> GetAccountInfoAsync(CancellationToken cancellationToken = default)
    {
        return await _x.GetUserInformationAsync(default);
    }

    public async ValueTask<bool> IsValidAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var _ = await _x.GetUserInformationAsync(cancellationToken);
        }
        catch (ApiException e) when (e.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
        {
            _logger.LogWarning("Failed to get user information. Too many requests to X api.");
        }
        catch (Exception e)
        {
            _logger.LogDebug(e, "Failed to check X api.");
            return false;
        }

        return true;
    }

    public async ValueTask<Post> PostMessageAsync(string message, CancellationToken cancellationToken = default)
    {
        return await _x.PostAsync(message, cancellationToken);
    }
}
