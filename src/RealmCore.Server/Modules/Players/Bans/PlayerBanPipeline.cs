namespace RealmCore.Server.Modules.Players.Bans;

public sealed class PlayerBanPipeline : IPlayerJoinedPipeline
{
    private readonly IOptionsMonitor<GameplayOptions> _gameplayOptions;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly BansService _bansService;

    public PlayerBanPipeline(IOptionsMonitor<GameplayOptions> gameplayOptions, IDateTimeProvider dateTimeProvider, BansService bansService)
    {
        _gameplayOptions = gameplayOptions;
        _dateTimeProvider = dateTimeProvider;
        _bansService = bansService;
    }

    public async Task<bool> Next(RealmPlayer player)
    {
        var serial = player.Client.Serial;
        if (serial == null)
            player.Client.FetchSerial();

        if (player.Client.Serial == null)
        {
            player.Kick("Failed to fetch serial");
            return false;
        }

        var now = _dateTimeProvider.Now;

        var bans = await _bansService.GetBySerial(player.Client.Serial, _gameplayOptions.CurrentValue.BanType);

        var ban = bans.FirstOrDefault(x => x.IsActive(now));
        if (ban != null)
        {
            player.Kick($"You are banned, reason: {ban.Reason} until: {ban.End}");
            return false;
        }
        return true;
    }
}
