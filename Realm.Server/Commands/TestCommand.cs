namespace Realm.Server.Commands;

internal class TestCommand : ICommand
{
    public string CommandName => "test";

    public void HandleCommand(string command)
    {
        Console.WriteLine("Test command executed");
    }
}
