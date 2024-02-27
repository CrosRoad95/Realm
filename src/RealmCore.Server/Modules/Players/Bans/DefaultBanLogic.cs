namespace RealmCore.Server.Modules.Players.Bans;

public class DefaultBanLogic
{
    private readonly MtaServer _server;
    private readonly IOptionsMonitor<GameplayOptions> _gameplayOptions;
    private readonly ILogger<DefaultBanLogic> _logger;
    private readonly IDateTimeProvider _dateTimeProvider;

    public DefaultBanLogic(MtaServer server, IOptionsMonitor<GameplayOptions> gameplayOptions, ILogger<DefaultBanLogic> logger, IDateTimeProvider dateTimeProvider)
    {
        _server = server;
        _gameplayOptions = gameplayOptions;
        _logger = logger;
        _dateTimeProvider = dateTimeProvider;
        _server.PlayerJoined += HandlePlayerJoined;
    }

    private async Task HandlePlayerJoinedCore(RealmPlayer player)
    {
        var serial = player.Client.Serial;
        if (serial == null)
            player.Client.FetchSerial();

        if (player.Client.Serial == null)
        {
            player.Kick("Failed to fetch serial");
            return;
        }

        var now = _dateTimeProvider.Now;
        var bansRepository = player.GetRequiredService<IBanRepository>();

        var bans = await player.GetRequiredService<IBanRepository>().GetBansBySerial(player.Client.Serial, _dateTimeProvider.Now, _gameplayOptions.CurrentValue.BanType);

        var ban = bans.FirstOrDefault(x => x.IsActive(now));
        if (ban != null)
        {
            player.Kick($"You are banned, reason: {ban.Reason} until: {ban.End}");
        }
    }

    private async void HandlePlayerJoined(Player player)
    {
        try
        {
            await HandlePlayerJoinedCore((RealmPlayer)player);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Something went wrong while handling player join");
        }
    }
}
