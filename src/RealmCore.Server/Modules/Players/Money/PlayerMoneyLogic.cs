namespace RealmCore.Server.Modules.Players.Money;

internal sealed class PlayerMoneyLogic : PlayerLogic
{
    private readonly IOptionsMonitor<GameplayOptions> _gameplayOptions;
    private readonly IElementCollection _elementCollection;
    private readonly ILogger<PlayerMoneyLogic> _logger;
    private decimal _limit;
    private byte _precision;

    public PlayerMoneyLogic(MtaServer server, IOptionsMonitor<GameplayOptions> gameplayOptions, IElementCollection elementCollection, ILogger<PlayerMoneyLogic> logger) : base(server)
    {
        _gameplayOptions = gameplayOptions;
        _elementCollection = elementCollection;
        _logger = logger;
        gameplayOptions.OnChange(HandleGameplayOptionsChanged);
        var currentValue = gameplayOptions.CurrentValue;
        _limit = currentValue.MoneyLimit;
        _precision = currentValue.MoneyPrecision;
        _logger.LogInformation("Set money limit to: {moneyLimit} and money precision to: {moneyPrecision}", _limit, _precision);
    }

    private void HandleGameplayOptionsChanged(GameplayOptions gameplayOptions)
    {
        if (gameplayOptions.MoneyLimit == _limit && gameplayOptions.MoneyPrecision == _precision)
            return;

        _limit = gameplayOptions.MoneyLimit;
        _precision = gameplayOptions.MoneyPrecision;

        foreach (var player in _elementCollection.GetByType<RealmPlayer>())
            player.Money.SetMoneyLimitAndPrecision(gameplayOptions.MoneyLimit, gameplayOptions.MoneyPrecision);

        _logger.LogInformation("Set money limit to: {moneyLimit} and money precision to: {moneyPrecision}", _limit, _precision);
    }

    protected override void PlayerJoined(RealmPlayer player)
    {
        player.Money.SetMoneyLimitAndPrecision(_limit, _precision);
    }
}
