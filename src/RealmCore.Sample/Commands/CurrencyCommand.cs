namespace RealmCore.Sample.Commands;

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

    public Task Handle(RealmPlayer player, CommandArguments args, CancellationToken cancellationToken)
    {
        var money = 123.456m;
        _chatBox.OutputTo(player, money.FormatMoney(_gameplayOptions.Value.CurrencyCulture));
        return Task.CompletedTask;
    }
}
