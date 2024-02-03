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
            return true;
        }
        catch (Exception e)
        {
            _logger.LogDebug(e, "Failed to check X api.");
            return false;
        }
    }

    public async ValueTask<Post> PostMessageAsync(string message, CancellationToken cancellationToken = default)
    {
        return await _x.PostAsync(message, cancellationToken);
    }
}
