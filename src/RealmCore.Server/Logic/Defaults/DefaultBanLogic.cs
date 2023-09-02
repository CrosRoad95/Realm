namespace RealmCore.Server.Logic.Defaults;

public class DefaultBanLogic
{
    private readonly MtaServer _mtaServer;
    private readonly IOptions<GameplayOptions> _gameplayOptions;
    private readonly ILogger<DefaultBanLogic> _logger;
    private readonly IBanRepository _banRepository;
    private readonly IDateTimeProvider _dateTimeProvider;

    public DefaultBanLogic(MtaServer mtaServer, IOptions<GameplayOptions> options, ILogger<DefaultBanLogic> logger, IBanRepository banRepository, IDateTimeProvider dateTimeProvider)
    {
        _mtaServer = mtaServer;
        _gameplayOptions = options;
        _logger = logger;
        _banRepository = banRepository;
        _dateTimeProvider = dateTimeProvider;
        _mtaServer.PlayerJoined += HandlePlayerJoined;
    }

    private async void HandlePlayerJoined(Player player)
    {
        try
        {
            var serial = player.Client.Serial;
            if (serial == null)
                player.Client.FetchSerial();

            if (player.Client.Serial == null)
            {
                player.Kick("Failed to fetch serial");
                return;
            }

            var ban = await _banRepository.GetBanBySerialAndType(player.Client.Serial, _gameplayOptions.Value.BanType, _dateTimeProvider.Now);
            if (ban != null)
                player.Kick($"You are banned, reason: {ban.Reason} until: {ban.End}");

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Something went wrong while handling player join");
        }
    }
}
