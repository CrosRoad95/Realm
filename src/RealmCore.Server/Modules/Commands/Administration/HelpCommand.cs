namespace RealmCore.Server.Modules.Commands.Administration;

[CommandName("help")]
internal class HelpCommand : IInGameCommand
{
    private readonly IEnumerable<CommandTypeWrapper> _commands;
    private readonly ILogger<HelpCommand> _logger;

    public HelpCommand(IEnumerable<CommandTypeWrapper> commands, ILogger<HelpCommand> logger)
    {
        _commands = commands;
        _logger = logger;
    }

    public string[] RequiredPolicies { get; } = [];

    public Task Handle(RealmPlayer player, CommandArguments args, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Commands:");
        _logger.LogInformation("\t{commands}", string.Join('\t', _commands.Select(x => x.Type.GetCustomAttribute<CommandNameAttribute>()!.Name)));
        return Task.CompletedTask;
    }
}
