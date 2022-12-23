namespace Realm.Server.Commands;

internal class SaveCommand : ICommand
{
    private readonly IRPGServer _rpgServer;

    public string CommandName => "save";

    public SaveCommand(IRPGServer rpgServer)
    {
        _rpgServer = rpgServer;
    }

    public void HandleCommand(string command)
    {
        _rpgServer.Save();
    }
}
