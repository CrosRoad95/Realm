namespace Realm.Server.Commands;

public class HelpCommand : ICommand
{
    public string CommandName => "help";

    private readonly ICommand[] _commands;
    private readonly ILogger<HelpCommand> _logger;

    public HelpCommand(IEnumerable<ICommand> commands, ILogger<HelpCommand> logger)
    {
        _logger = logger;
        _commands = commands.ToArray();
    }

    public Task HandleCommand(string command)
    {
        _logger.LogInformation("Commands:");
        _logger.LogInformation($"\t{string.Join('\t', _commands.Select(x => x.CommandName))}");
        return Task.CompletedTask;
    }
}
