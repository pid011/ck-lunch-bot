
using CKLunchBot.Core;
using CKLunchBot.Runner;
using Microsoft.Extensions.Options;

namespace CKLunchBot;

public sealed class BotService(
    IHostApplicationLifetime lifetime,
    ILogger<BotService> logger,
    IHostEnvironment environment,
    IOptions<ApiKey> apiKey,
    IOptions<BotConfig> config) : BackgroundService, IAsyncDisposable
{
    private readonly IHostApplicationLifetime _lifetime = lifetime;
    private readonly ILogger<BotService> _logger = logger;
    private readonly IHostEnvironment _environment = environment;
    private readonly ApiKey _apiKey = apiKey.Value;
    private readonly IBotConfig _config = config.Value;

    public MenuType Next { get; set; }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("BotService is starting. ({enviroment})", _environment.EnvironmentName);
        _logger.LogInformation("API Key: {key}", _apiKey.ConsumerApiKey);
        _logger.LogInformation("briefing: {briefing}", _config.Briefing);
        _lifetime.StopApplication();
    }

    private async ValueTask Init(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Initializing BotService...");


    }

    private async ValueTask CheckApiKey(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Checking API Key...");
    }

    private int GetNextTimeDelay()
    {
        var now = KST.Now;

        return TimeSpan.Zero.Milliseconds;
    }

    public async ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
    }
}
