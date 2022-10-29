namespace Realm.Server.Commands;

internal class CommandsLogic
{
    private readonly IConsoleCommands _consoleCommands;
    private readonly List<ICommand> _commands;

    public CommandsLogic(IConsoleCommands consoleCommands, IEnumerable<ICommand> commands, HelpCommand helpCommand)
    {
        _consoleCommands = consoleCommands;
        _commands = commands.ToList();
        _commands.Add(helpCommand);
        _consoleCommands.CommandExecuted += CommandExecuted;
    }

    private void CommandExecuted(string? line)
    {
        var firstWord = line.Split(' ').FirstOrDefault();
        if (firstWord == null)
            return;

        foreach (var command in _commands)
        {
            if (line.StartsWith(command.CommandName))
            {
                command.HandleCommand(line);
            }
        }
    }
}
