using Microsoft.Extensions.Options;

namespace RealmCore.Server.Logic.Defaults;

public class DefaultBanLogic
{
    private readonly MtaServer _mtaServer;
    private readonly IOptionsMonitor<GameplayOptions> _gameplayOptions;
    private readonly ILogger<DefaultBanLogic> _logger;
    private readonly IBanRepository _banRepository;
    private readonly IDateTimeProvider _dateTimeProvider;
    private int _banType;
    public DefaultBanLogic(MtaServer mtaServer, IOptionsMonitor<GameplayOptions> gameplayOptions, ILogger<DefaultBanLogic> logger, IBanRepository banRepository, IDateTimeProvider dateTimeProvider)
    {
        _mtaServer = mtaServer;
        _gameplayOptions = gameplayOptions;
        _logger = logger;
        _banRepository = banRepository;
        _dateTimeProvider = dateTimeProvider;
        _mtaServer.PlayerJoined += HandlePlayerJoined;

        _gameplayOptions.OnChange(GameplayOptionsChanged);
        _banType = _gameplayOptions.CurrentValue.BanType;
    }

    private void GameplayOptionsChanged(GameplayOptions gameplayOptions)
    {
        _banType = gameplayOptions.BanType;
        _logger.LogInformation("Changed ban type to {banType}", _banType);
    }

    private async Task HandlePlayerJoinedCore(Player player)
    {
        var serial = player.Client.Serial;
        if (serial == null)
            player.Client.FetchSerial();

        if (player.Client.Serial == null)
        {
            player.Kick("Failed to fetch serial");
            return;
        }

        var ban = await _banRepository.GetBanBySerialAndType(player.Client.Serial, _banType, _dateTimeProvider.Now);
        if (ban != null)
            player.Kick($"You are banned, reason: {ban.Reason} until: {ban.End}");
    }

    private async void HandlePlayerJoined(Player player)
    {
        try
        {
            await HandlePlayerJoinedCore(player);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Something went wrong while handling player join");
        }
    }
}
