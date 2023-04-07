namespace Realm.Server.Commands;

[CommandName("help")]
internal class HelpCommand : ICommand
{
    private readonly IEnumerable<CommandTypeWrapper> _commands;
    private readonly ILogger<HelpCommand> _logger;

    public HelpCommand(IEnumerable<CommandTypeWrapper> commands, ILogger<HelpCommand> logger)
    {
        _commands = commands;
        _logger = logger;
    }

    public Task HandleCommand(string command)
    {
        _logger.LogInformation("Commands:");
        _logger.LogInformation($"\t{string.Join('\t', _commands.Select(x => x.Type.GetCustomAttribute<CommandNameAttribute>().Name))}");
        return Task.CompletedTask;
    }
}
