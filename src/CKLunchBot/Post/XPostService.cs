using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CKLunchBot.Post;

public sealed partial class XPostService : IPostService
{
    private readonly ILogger<XPostService> _logger;
    private readonly XClient _x;

    public XPostService(ILogger<XPostService> logger, IOptions<XCredentials> credentials)
    {
        _logger = logger;
        if (!credentials.Value.IsValid())
        {
            throw new ArgumentException("Invalid credentials!");
        }

        _x = new XClient(credentials.Value);
    }

    public async ValueTask<Account> GetAccountInfoAsync(CancellationToken cancellationToken = default)
    {
        return await _x.GetUserInformationAsync(cancellationToken);
    }

    public async ValueTask<bool> IsValidAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _ = await _x.GetUserInformationAsync(cancellationToken);
        }
        catch (ApiException e) when (e.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
        {
            LogTooManyRequests();
        }
        catch (Exception e)
        {
            LogFailedToCheckXApi(e);
            return false;
        }

        return true;
    }

    public async ValueTask<Post> PostMessageAsync(string message,
        CancellationToken cancellationToken = default)
    {
        return await _x.PostAsync(message, cancellationToken);
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Failed to get user information. Too many requests to X api.")]
    private partial void LogTooManyRequests();

    [LoggerMessage(Level = LogLevel.Debug, Message = "Failed to check X api.")]
    private partial void LogFailedToCheckXApi(Exception ex);
}
