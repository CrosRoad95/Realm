namespace Realm.Server.Commands;

internal class CommandsLogic
{
    private readonly IConsole _consoleCommands;
    private readonly List<ICommand> _commands;

    public CommandsLogic(IConsole consoleCommands, IEnumerable<ICommand> commands, HelpCommand helpCommand)
    {
        _consoleCommands = consoleCommands;
        _commands = commands.ToList();
        _commands.Add(helpCommand);
        _consoleCommands.CommandExecuted += HandleCommandExecuted;
    }

    private void HandleCommandExecuted(string? line)
    {
        if (line == null)
            return;

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
