namespace RealmCore.Server.Modules.Players.Bans;

public class DefaultBanLogic
{
    private readonly MtaServer _mtaServer;
    private readonly IOptionsMonitor<GameplayOptions> _gameplayOptions;
    private readonly ILogger<DefaultBanLogic> _logger;
    private readonly IDateTimeProvider _dateTimeProvider;

    public DefaultBanLogic(MtaServer mtaServer, IOptionsMonitor<GameplayOptions> gameplayOptions, ILogger<DefaultBanLogic> logger,  IDateTimeProvider dateTimeProvider)
    {
        _mtaServer = mtaServer;
        _gameplayOptions = gameplayOptions;
        _logger = logger;
        _dateTimeProvider = dateTimeProvider;
        _mtaServer.PlayerJoined += HandlePlayerJoined;
    }

    private async Task HandlePlayerJoinedCore(Player plr)
    {
        var player = (RealmPlayer)plr;
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
            await HandlePlayerJoinedCore(player);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Something went wrong while handling player join");
        }
    }
}
