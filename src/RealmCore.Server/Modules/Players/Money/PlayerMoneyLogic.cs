namespace RealmCore.Server.Modules.Players.Money;

internal sealed class PlayerMoneyLogic : PlayerLogic
{
    private readonly IOptionsMonitor<GameplayOptions> _gameplayOptions;
    private readonly IElementCollection _elementCollection;

    public PlayerMoneyLogic(MtaServer server, IOptionsMonitor<GameplayOptions> gameplayOptions, IElementCollection elementCollection) : base(server)
    {
        gameplayOptions.OnChange(HandleGameplayOptionsChanged);
        _gameplayOptions = gameplayOptions;
        _elementCollection = elementCollection;
    }

    private void HandleGameplayOptionsChanged(GameplayOptions gameplayOptions)
    {
        foreach (var player in _elementCollection.GetByType<RealmPlayer>())
        {
            player.Money.SetMoneyLimitAndPrecision(gameplayOptions.MoneyLimit, gameplayOptions.MoneyPrecision);
        }
    }

    protected override void PlayerJoined(RealmPlayer player)
    {
        var gameplayOptions = _gameplayOptions.CurrentValue;
        player.Money.SetMoneyLimitAndPrecision(gameplayOptions.MoneyLimit, gameplayOptions.MoneyPrecision);
    }
}
