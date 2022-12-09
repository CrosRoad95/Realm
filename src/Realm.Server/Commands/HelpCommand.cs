namespace Realm.Server.Commands;

public class HelpCommand : ICommand
{
    public string CommandName => "help";

    private readonly ICommand[] _commands;
    private readonly ILogger _logger;

    public HelpCommand(IEnumerable<ICommand> commands, ILogger logger)
    {
        _commands = commands.ToArray();
        _logger = logger.ForContext<HelpCommand>();
    }

    public void HandleCommand(string command)
    {
        _logger.Information("Commands:");
        _logger.Information($"\t{string.Join('\t', _commands.Select(x => x.CommandName))}");
    }
}
