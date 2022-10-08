namespace Realm.Server.Commands;

internal class HelpCommand : ICommand
{
    public string CommandName => "help";

    private readonly IEnumerable<ICommand> _commands;

    public HelpCommand(IEnumerable<ICommand> commands)
    {
        _commands = commands;
    }

    public void HandleCommand(string command)
    {
        Console.WriteLine("Commands:");
        Console.WriteLine($"\t{string.Join('\t', _commands.Select(x => x.CommandName))}");
    }
}
