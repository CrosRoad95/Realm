using Microsoft.Extensions.Options;
using RealmCore.Server.Extensions;
using RealmCore.Server.Options;

namespace RealmCore.Console.Commands;

[CommandName("currency")]
public sealed class CurrencyCommand : IIngameCommand
{
    private readonly ILogger<CurrencyCommand> _logger;
    private readonly IOptions<GameplayOptions> _gameplayOptions;

    public CurrencyCommand(ILogger<CurrencyCommand> logger, IOptions<GameplayOptions> gameplayOptions)
    {
        _logger = logger;
        _gameplayOptions = gameplayOptions;
    }

    public Task Handle(Entity entity, string[] args)
    {
        var money = 123.456m;
        entity.GetRequiredComponent<PlayerElementComponent>().SendChatMessage(money.FormatMoney(_gameplayOptions.Value.CurrencyCulture));
        return Task.CompletedTask;
    }
}
