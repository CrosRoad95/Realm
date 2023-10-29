namespace RealmCore.Server.Logic.Defaults;

public class DefaultBanLogic
{
    private readonly MtaServer _mtaServer;
    private readonly IOptionsMonitor<GameplayOptions> _gameplayOptions;
    private readonly ILogger<DefaultBanLogic> _logger;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IServiceProvider _serviceProvider;

    public DefaultBanLogic(MtaServer mtaServer, IOptionsMonitor<GameplayOptions> gameplayOptions, ILogger<DefaultBanLogic> logger, IBanRepository banRepository, IDateTimeProvider dateTimeProvider, IServiceProvider serviceProvider)
    {
        _mtaServer = mtaServer;
        _gameplayOptions = gameplayOptions;
        _logger = logger;
        _dateTimeProvider = dateTimeProvider;
        _serviceProvider = serviceProvider;
        _mtaServer.PlayerJoined += HandlePlayerJoined;
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

        using var scope = _serviceProvider.CreateScope();
        var bans = await scope.ServiceProvider.GetRequiredService<IBanRepository>().GetBansBySerial(player.Client.Serial, _dateTimeProvider.Now, _gameplayOptions.CurrentValue.BanType);
        if (bans != null && bans.Count > 0)
        {
            var ban = bans[0];
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
