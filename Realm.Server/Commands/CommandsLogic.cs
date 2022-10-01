namespace Realm.Server.Commands;

internal class CommandsLogic
{
    private readonly IConsoleCommands _consoleCommands;
    private readonly IEnumerable<ICommand> _commands;
    private readonly ICommand _helpCommand;

    public CommandsLogic(IConsoleCommands consoleCommands, IEnumerable<ICommand> commands, HelpCommand helpCommand)
    {
        _consoleCommands = consoleCommands;
        _commands = commands;
        _helpCommand = helpCommand;
        _consoleCommands.CommandExecuted += CommandExecuted;
    }

    private void CommandExecuted(string? line)
    {
        if (line == null)
            return;

        if(line.StartsWith(_helpCommand.CommandName))
        {
            _helpCommand.HandleCommand(line);
            return;
        }

        foreach (var command in _commands)
        {
            if(line.StartsWith(command.CommandName))
            {
                command.HandleCommand(line);
            }
        }
    }
}
