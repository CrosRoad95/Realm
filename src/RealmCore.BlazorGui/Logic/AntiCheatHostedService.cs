namespace RealmCore.BlazorGui.Logic;

internal sealed class AntiCheatHostedService : IHostedService
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

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _antiCheat.ViolationReported += HandleViolationReported;
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
