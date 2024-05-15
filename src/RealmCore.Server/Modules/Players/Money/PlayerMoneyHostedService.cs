namespace RealmCore.Server.Modules.Players.Money;

internal sealed class PlayerMoneyHostedService : PlayerLifecycle, IHostedService
{
    private readonly IOptionsMonitor<GameplayOptions> _gameplayOptions;
    private readonly IElementCollection _elementCollection;
    private readonly ILogger<PlayerMoneyHostedService> _logger;
    private decimal _limit;
    private byte _precision;

    public PlayerMoneyHostedService(PlayersEventManager playersEventManager, IOptionsMonitor<GameplayOptions> gameplayOptions, IElementCollection elementCollection, ILogger<PlayerMoneyHostedService> logger) : base(playersEventManager)
    {
        _gameplayOptions = gameplayOptions;
        _elementCollection = elementCollection;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _gameplayOptions.OnChange(HandleGameplayOptionsChanged);
        var currentValue = _gameplayOptions.CurrentValue;
        _limit = currentValue.MoneyLimit;
        _precision = currentValue.MoneyPrecision;
        _logger.LogInformation("Set money limit to: {moneyLimit} and money precision to: {moneyPrecision}", _limit, _precision);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private void HandleGameplayOptionsChanged(GameplayOptions gameplayOptions)
    {
        if (gameplayOptions.MoneyLimit == _limit && gameplayOptions.MoneyPrecision == _precision)
            return;

        _limit = gameplayOptions.MoneyLimit;
        _precision = gameplayOptions.MoneyPrecision;

        foreach (var player in _elementCollection.GetByType<RealmPlayer>())
            player.Money.SetLimitAndPrecision(gameplayOptions.MoneyLimit, gameplayOptions.MoneyPrecision);

        _logger.LogInformation("Set money limit to: {moneyLimit} and money precision to: {moneyPrecision}", _limit, _precision);
    }

    protected override void PlayerJoined(RealmPlayer player)
    {
        player.Money.SetLimitAndPrecision(_limit, _precision);
    }
}
