
namespace RealmCore.Server.Modules.Players;

public interface IPlayerJoinedPipeline
{
    Task<bool> Next(Player player);
}

internal sealed class PlayerJoinedPipelineHostedService : IHostedService
{
    private readonly MtaServer _mtaServer;
    private readonly IEnumerable<IPlayerJoinedPipeline> _playerPipelines;
    private readonly PlayersEventManager _playersEventManager;
    private readonly ILogger<PlayerJoinedPipelineHostedService> _logger;

    public PlayerJoinedPipelineHostedService(MtaServer mtaServer, IEnumerable<IPlayerJoinedPipeline> playerPipelines, PlayersEventManager playersEventManager, ILogger<PlayerJoinedPipelineHostedService> logger)
    {
        _mtaServer = mtaServer;
        _playerPipelines = playerPipelines;
        _playersEventManager = playersEventManager;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _mtaServer.PlayerJoined += HandlePlayerJoined;
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _mtaServer.PlayerJoined -= HandlePlayerJoined;
        return Task.CompletedTask;
    }

    private async void HandlePlayerJoined(Player player)
    {
        using var activity = Activity.StartActivity(nameof(HandlePlayerJoined));

        try
        {
            bool success = true;
            foreach (var playerPipeline in _playerPipelines)
            {
                using var playerPipelineActivity = Activity.StartActivity(playerPipeline.GetType().Name);
                if (!await playerPipeline.Next(player))
                {
                    success = false;
                    break;
                }
            }

            if (success)
            {
                _playersEventManager?.RelayJoined(player);
                return;
            }
        }
        catch (Exception ex)
        {
            _logger.LogHandleError(ex);
            player.Kick("Wystąpił błąd...");
        }

        // Kick player if none of player pipeline did it already.
        if (player.Destroy())
        {
            player.Kick("Nie udało się wejść na serwer.");
        }
    }

    public static readonly ActivitySource Activity = new("RealmCore.PlayerJoinedPipeline", "1.0.0");
}
