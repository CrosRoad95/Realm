using Microsoft.Extensions.Options;
using RealmCore.Server.Options;

namespace RealmCore.Console.Commands;

[CommandName("currency")]
public sealed class CurrencyCommand : IInGameCommand
{
    private readonly ILogger<CurrencyCommand> _logger;
    private readonly IOptions<GameplayOptions> _gameplayOptions;
    private readonly ChatBox _chatBox;

    public CurrencyCommand(ILogger<CurrencyCommand> logger, IOptions<GameplayOptions> gameplayOptions, ChatBox chatBox)
    {
        _logger = logger;
        _gameplayOptions = gameplayOptions;
        _chatBox = chatBox;
    }

    public Task Handle(Entity entity, CommandArguments args)
    {
        var money = 123.456m;
        _chatBox.OutputTo(entity, money.FormatMoney(_gameplayOptions.Value.CurrencyCulture));
        return Task.CompletedTask;
    }
}
