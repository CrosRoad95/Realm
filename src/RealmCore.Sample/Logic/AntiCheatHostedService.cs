namespace RealmCore.Sample.Logic;

internal sealed class AntiCheatHostedService : BackgroundService
{
    private readonly IAntiCheat _antiCheat;
    private readonly ILogger<AntiCheatHostedService> _logger;

    public AntiCheatHostedService(IAntiCheat antiCheat, ILogger<AntiCheatHostedService> logger)
    {
        _antiCheat = antiCheat;
        _logger = logger;
    }

    private void HandleViolationReported(RealmPlayer player, int violationId, AntiCheatViolationDetails? details)
    {
        _logger.LogWarning("Violation reported by {playerName}, violation id: {violationId}", player.Name, violationId);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _antiCheat.ViolationReported += HandleViolationReported;

        return Task.CompletedTask;
    }
}
